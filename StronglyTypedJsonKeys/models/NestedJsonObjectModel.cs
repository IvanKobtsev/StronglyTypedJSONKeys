using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using StronglyTypedJsonKeys.generators;

namespace StronglyTypedJsonKeys.models;

internal sealed class NestedJsonObjectModel : IEquatable<NestedJsonObjectModel>
{
    public NestedJsonObjectModel? Parent { get; }
    public string KeyName { get; }
    public IReadOnlyList<NestedJsonObjectModel> Properties { get; }
    public LocalizationKeysGenerator.GenerationOptions? GenerationOptions { get; set; }

    public NestedJsonObjectModel(NestedJsonObjectModel? parent, string keyName, JsonObject json)
    {
        Parent = parent;
        KeyName = keyName;

        var properties = new List<NestedJsonObjectModel>();

        foreach (var kvp in json)
        {
            if (kvp.Value is JsonObject obj)
            {
                properties.Add(new NestedJsonObjectModel(this, kvp.Key, obj));
            }
            else
            {
                properties.Add(new NestedJsonObjectModel(this, kvp.Key, []));
            }
        }

        Properties = properties;
    }

    public string GetPath(string? childPath)
    {
        if (Parent != null) return Parent.GetPath(KeyName + "." + childPath);
        
        var sb = new StringBuilder();
        sb.Append(GenerationOptions!.KeysPrefix);
            
        if (GenerationOptions?.RootKeyPath != string.Empty)
        {
            sb.Append(GenerationOptions!.RootKeyPath);
            sb.Append(".");
        }
            
        sb.Append(childPath);
            
        return sb.ToString();

    }

    public bool Equals(NestedJsonObjectModel? secondObject)
    {
        if (secondObject is null)
            return false;

        if (!string.Equals(KeyName, secondObject.KeyName, StringComparison.Ordinal))
            return false;

        if (Properties.Count != secondObject.Properties.Count)
            return false;

        return !Properties.Where((t, i) => !t.Equals(secondObject.Properties[i])).Any();
    }

    public override bool Equals(object? obj) => Equals(obj as NestedJsonObjectModel);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = KeyName.GetHashCode();

            var current = hash;

            foreach (var child in Properties)
            {
                current = (current * 397) ^ child.GetHashCode();
            }

            return current;
        }
    }
}