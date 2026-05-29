using System.Collections;
using System.Reflection;
using DocumentIntelligence.Rag.Models;

namespace DocumentIntelligence.Rag.Text;

public sealed class ReflectionRagTextBuilder : IRagTextBuilder
{
    public string BuildText<T>(RagDocument<T> document)
    {
        if (document.Value is null)
        {
            return string.Empty;
        }

        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        List<string> lines = [];

        foreach (PropertyInfo property in properties)
        {
            if (property.GetIndexParameters().Length <= 0 && property.GetValue(document.Value) is { } value)
            {
                lines.Add($"{property.Name}: {FormatValue(value)}");
            }
        }

        return string.Join('\n', lines).Trim();
    }

    private static string FormatValue(object value)
    {
        if (value is string str)
        {
            return str;
        }

        if (value is IEnumerable<string> stringEnumerable)
        {
            return string.Join(", ", stringEnumerable);
        }

        if (value is IEnumerable enumerable)
        {
            List<string> items = [];
            foreach (object? item in enumerable)
            {
                items.Add(item?.ToString() ?? string.Empty);
            }
            return string.Join(", ", items);
        }

        return value.ToString() ?? string.Empty;
    }
}
