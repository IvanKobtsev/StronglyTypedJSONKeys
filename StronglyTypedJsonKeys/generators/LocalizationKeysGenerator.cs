using System;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using StronglyTypedJsonKeys.helpers;
using StronglyTypedJsonKeys.models;

namespace StronglyTypedJsonKeys.generators;

[Generator]
public sealed class LocalizationKeysGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var jsonFiles = context
            .AdditionalTextsProvider.Combine(context.AnalyzerConfigOptionsProvider)
            .Select(
                (pair, _) =>
                {
                    var (additionalText, optionsProvider) = pair;

                    var options = optionsProvider.GetOptions(additionalText);

                    options.TryGetValue(
                        "build_metadata.additionalfiles.generatekeys",
                        out var generateKeys
                    );
                    options.TryGetValue(
                        "build_metadata.additionalfiles.rootkeypath",
                        out var rootKeyPath
                    );
                    options.TryGetValue(
                        "build_metadata.additionalfiles.keysprefix",
                        out var keysPrefix
                    );
                    options.TryGetValue(
                        "build_metadata.additionalfiles.classname",
                        out var className
                    );
                    options.TryGetValue(
                        "build_metadata.additionalfiles.capitalizegeneratedkeys",
                        out var capitalizeGeneratedKeys
                    );

                    return new
                    {
                        File = additionalText,
                        Enabled = 
                            bool.TryParse(generateKeys, out var enabled) && enabled,
                        RootKeyPath = rootKeyPath ?? string.Empty,
                        KeysPrefix = keysPrefix ?? string.Empty,
                        ClassName = className
                                    ?? ClassNameConverter.FileNameToClassName(additionalText.Path),
                        CapitalizeGeneratedKeys = bool.TryParse(capitalizeGeneratedKeys, out var cgk) && cgk
                    };
                }
            )
            .Where(f => f.Enabled);

        var serverErrorsProvider = jsonFiles.Select(
            static (fileWithConfig, ct) =>
            {
                var jsonFile = fileWithConfig.File;

                var jsonText = jsonFile.GetText(ct)!.ToString();

                if (jsonText is null)
                    throw new InvalidOperationException(
                        $"File '{jsonFile.Path}' could not be read."
                    );

                var pathKeys =
                    fileWithConfig.RootKeyPath != string.Empty ? fileWithConfig.RootKeyPath.Split('.') : [];
                var json =
                    JsonNode.Parse(jsonText)
                    ?? throw new InvalidOperationException($"Invalid JSON in '{jsonFile.Path}'.");

                if (json is not JsonObject currentObject)
                    throw new InvalidOperationException(
                        $"Root is not a JSON object in '{jsonFile.Path}'."
                    );

                foreach (var pathKey in pathKeys)
                {
                    if (
                        !currentObject.TryGetPropertyValue(pathKey, out var node)
                        || node is not JsonObject childObject
                    )
                    {
                        throw new InvalidOperationException($"'{pathKey}' key was not found.");
                    }
                    currentObject = childObject;
                }

                return new NestedJsonObjectModel(null, string.Empty, currentObject)
                {
                    GenerationOptions = new GenerationOptions(fileWithConfig.RootKeyPath, fileWithConfig.KeysPrefix, fileWithConfig.ClassName, fileWithConfig.CapitalizeGeneratedKeys)
                };
            }
        );

        context.RegisterSourceOutput(
            serverErrorsProvider,
            static (spc, jsonObject) =>
            {
                var sb = new StringBuilder();
                sb.Append("public static class " + jsonObject.GenerationOptions!.ClassName + " {");

                EmitObject(sb, jsonObject, 1, jsonObject.GenerationOptions);

                sb.Append("\n}");

                spc.AddSource(
                    jsonObject.GenerationOptions!.ClassName + ".g.cs",
                    SourceText.From(sb.ToString(), Encoding.UTF8)
                );
            }
        );
    }

    internal sealed record GenerationOptions(
        string RootKeyPath,
        string KeysPrefix,
        string ClassName,
        bool CapitalizeGeneratedKeys
    )
    {
        public string RootKeyPath { get; } = RootKeyPath;
        public string KeysPrefix { get; } = KeysPrefix;
        public string ClassName { get; } = ClassName;
        public bool CapitalizeGeneratedKeys { get; } = CapitalizeGeneratedKeys;
    }

    private static void EmitObject(
        StringBuilder sb,
        NestedJsonObjectModel jsonObject,
        int nestingLevel,
        GenerationOptions generationOptions
    )
    {
        foreach (var node in jsonObject.Properties)
        {
            if (node.Properties.Count > 0) // When it's a nested object
            {
                sb.Append("\n\n");
                GenerationHelper.AddIndent(sb, nestingLevel);
                sb.Append("public static class ");
                sb.Append(
                    generationOptions.CapitalizeGeneratedKeys
                        ? GenerationHelper.CapitalizeFirst(node.KeyName)
                        : node.KeyName
                );
                sb.Append(" {");

                EmitObject(sb, node, nestingLevel + 1, generationOptions);

                sb.Append("\n");
                GenerationHelper.AddIndent(sb, nestingLevel);
                sb.Append("}");
            }
            else // When it's a primitive type
            {
                sb.Append("\n");
                GenerationHelper.AddIndent(sb, nestingLevel);
                sb.Append("public const string ");
                sb.Append(
                    generationOptions.CapitalizeGeneratedKeys
                        ? GenerationHelper.CapitalizeFirst(node.KeyName)
                        : node.KeyName
                );
                sb.Append(" = \"");
                sb.Append(node.Parent!.GetPath(node.KeyName));
                sb.Append("\";");
            }
        }
    }
}
