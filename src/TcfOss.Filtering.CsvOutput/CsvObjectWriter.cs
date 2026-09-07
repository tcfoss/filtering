using System.Globalization;
using System.Reflection;

namespace TcfOss.Filtering.CsvOutput;

public class CsvObjectWriter<T>(Stream stream, SerializationOptions options) : CsvWriter(stream, options)
{
    // Columns are eagerly built when T exposes static properties (the common POCO case).
    // For `dynamic`/object items (no static properties) we defer column discovery until
    // the first object is written and derive columns from its runtime shape.
    private (string Header, Func<object, string> GetValue)[]? _columns =
        HasStaticProperties(typeof(T)) ? BuildColumnsFromType(typeof(T), options.BlankValue) : null;
    private bool _headersPending;
    private bool _headersWritten;

    public void WriteHeaders()
    {
        if (!_options.IncludeHeaders)
        {
            return;
        }

        if (_options.Headers != null)
        {
            WriteRow(_options.Headers);
            _headersWritten = true;
            return;
        }

        if (_columns != null)
        {
            WriteRow(_columns.Select(c => c.Header));
            _headersWritten = true;
            return;
        }

        // Defer until we see the first object and can infer columns from it.
        _headersPending = true;
    }

    public void WriteObject(T obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        _columns ??= BuildColumnsFromObject(obj, _options.BlankValue);

        if (_headersPending && !_headersWritten)
        {
            WriteRow(_columns.Select(c => c.Header));
            _headersWritten = true;
            _headersPending = false;
        }

        WriteRow(_columns.Select(c => c.GetValue(obj)));
    }

    private static bool HasStaticProperties(Type type)
    {
        if (type == typeof(object))
        {
            return false;
        }
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Any(p => p.GetIndexParameters().Length == 0);
    }

    private static (string Header, Func<object, string> GetValue)[] BuildColumnsFromObject(object obj, string blankValue)
    {
        if (obj is IDictionary<string, object?> dict)
        {
            return [.. dict.Keys.Select(key => (
                key,
                (Func<object, string>)(o => FormatValue(((IDictionary<string, object?>)o).TryGetValue(key, out object? v) ? v : null, blankValue))
            ))];
        }

        return BuildColumnsFromType(obj.GetType(), blankValue);
    }

    private static (string Header, Func<object, string> GetValue)[] BuildColumnsFromType(Type type, string blankValue)
    {
        return [.. type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetIndexParameters().Length == 0)
            .Select(property => (property.Name, BuildGetter(property, blankValue)))];
    }

    private static Func<object, string> BuildGetter(PropertyInfo property, string blankValue)
    {
        Type propertyType = property.PropertyType;

        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            propertyType = Nullable.GetUnderlyingType(propertyType)!;
        }

        if (propertyType == typeof(DateTime))
        {
            return obj => property.GetValue(obj) is DateTime dt
                ? dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                : blankValue;
        }

        if (propertyType == typeof(DateOnly))
        {
            return obj => property.GetValue(obj) is DateOnly d
                ? d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                : blankValue;
        }

        if (propertyType == typeof(bool))
        {
            return obj => property.GetValue(obj) is bool b ? (b ? "True" : "False") : blankValue;
        }

        if (propertyType == typeof(int)
            || propertyType == typeof(uint)
            || propertyType == typeof(long)
            || propertyType == typeof(ulong)
            || propertyType == typeof(short)
            || propertyType == typeof(ushort)
            || propertyType == typeof(byte)
            || propertyType == typeof(sbyte)
            || propertyType == typeof(float)
            || propertyType == typeof(double)
            || propertyType == typeof(decimal))
        {
            return obj => property.GetValue(obj) is IFormattable n
                ? n.ToString(null, CultureInfo.InvariantCulture)
                : blankValue;
        }

        return obj => property.GetValue(obj)?.ToString() ?? blankValue;
    }

    private static string FormatValue(object? value, string blankValue)
    {
        return value switch
        {
            null => blankValue,
            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            DateOnly d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            bool b => b ? "True" : "False",
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? blankValue,
        };
    }
}
