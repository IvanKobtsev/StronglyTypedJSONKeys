using System.Text;

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
}