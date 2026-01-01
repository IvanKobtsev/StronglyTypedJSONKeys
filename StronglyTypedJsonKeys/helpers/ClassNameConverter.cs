using System.IO;
using System.Text.RegularExpressions;

namespace StronglyTypedJsonKeys.helpers;

public static class ClassNameConverter
{
    public static string FileNameToClassName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "GeneratedClass";

        var name = Path.GetFileNameWithoutExtension(fileName);

        name = Regex.Replace(name, @"[^a-zA-Z0-9_]", "_");
        name = Regex.Replace(name, @"^[0-9]+", "");

        if (string.IsNullOrEmpty(name))
            name = "GeneratedClass";

        if (!char.IsLetter(name[0]) && name[0] != '_')
            name = "_" + name;

        if (CSharpKeywords.IsCSharpKeyword(name))
            name = "_" + name;

        return name;
    }
}
