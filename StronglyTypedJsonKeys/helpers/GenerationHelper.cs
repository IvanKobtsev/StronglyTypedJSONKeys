using System.Text;
using StronglyTypedJsonKeys.generators;

namespace StronglyTypedJsonKeys.helpers;

public static class GenerationHelper
{
    public static void AddIndent(StringBuilder sb, int nestingLevel)
    {
        for (var i = 0; i < nestingLevel; i++)
        {
            sb.Append("    ");
        }
    }

    public static string CapitalizeFirst(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToUpper(input[0]) + input.Substring(1);
    }

    internal static string ConfigToString(LocalizationKeysGenerator.AdditionalFileConfig config)
    {
        var sb = new StringBuilder();
        sb.Append(config.ClassName);
        sb.Append("___");
        sb.Append(config.KeysPrefix);
        sb.Append("___");
        sb.Append(config.CapitalizeGeneratedKeys.ToString());
        sb.Append("___");
        sb.Append(config.RootKeyPath);
        return sb.ToString();
    }
}