using System.Globalization;
using System.Reflection;
using TcfOss.Filtering.Contracts;

namespace TcfOss.Filtering.Linq.FilterMapping;

public static class TypeHelpers
{
    private static bool IsCollectionType(this Type t)
        => t != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(t);

    public static Type GetPropertyTypePreserveNullability(this Type parentType, string fieldName)
    {
        if (fieldName.Contains('.'))
        {
            string[] components = fieldName.Split('.');
            Type currentType;
            try
            {
                // Intermediate traversal still uses simple types for parser-compatible navigation.
                currentType = GetPropertyType(parentType, components[0]);
            }
            catch (FilterMappingException)
            {
                throw new FilterMappingException(string.Format(FilterMappingException.UnknownFieldPattern, fieldName));
            }

            string rest = string.Join('.', components.Skip(1));

            if (currentType.IsCollectionType()
                && !rest.Equals("Count", StringComparison.OrdinalIgnoreCase)
                && !rest.Equals("Length", StringComparison.OrdinalIgnoreCase))
            {
                throw new FilterMappingException(string.Format(FilterMappingException.UnsupportedCollectionNavigationPattern, components[0]));
            }

            try
            {
                return GetPropertyTypePreserveNullability(currentType, rest);
            }
            catch (FilterMappingException)
            {
                throw new FilterMappingException(string.Format(FilterMappingException.UnknownFieldPattern, fieldName));
            }
        }

        PropertyInfo prop = parentType.GetProperty(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) ?? throw new FilterMappingException(string.Format(FilterMappingException.UnknownFieldPattern, fieldName));
        return prop.PropertyType;
    }

    public static Type GetPropertyType(this Type parentType, string fieldName)
    {
        if (fieldName.Contains('.'))
        {
            string[] components = fieldName.Split('.');
            Type currentType;
            try
            {
                currentType = GetPropertyType(parentType, components[0]);
            }
            catch (FilterMappingException)
            {
                throw new FilterMappingException(string.Format(FilterMappingException.UnknownFieldPattern, fieldName));
            }

            string rest = string.Join('.', components.Skip(1));

            if (currentType.IsCollectionType()
                && !rest.Equals("Count", StringComparison.OrdinalIgnoreCase)
                && !rest.Equals("Length", StringComparison.OrdinalIgnoreCase))
            {
                throw new FilterMappingException(string.Format(FilterMappingException.UnsupportedCollectionNavigationPattern, components[0]));
            }

            try
            {
                return GetPropertyType(currentType, rest);
            }
            catch (FilterMappingException)
            {
                throw new FilterMappingException(string.Format(FilterMappingException.UnknownFieldPattern, fieldName));
            }
        }

        PropertyInfo prop = parentType.GetProperty(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) ?? throw new FilterMappingException(string.Format(FilterMappingException.UnknownFieldPattern, fieldName));

        Type propType = prop.PropertyType;
        if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            propType = Nullable.GetUnderlyingType(prop.PropertyType)!;
        }

        return propType;
    }

    public static Func<string, object> ConstructValueParser(this Type t, string? fieldName)
    {
        if (t == typeof(string))
        {
            return GetValueString;
        }

        if (t.IsEnum)
        {
            return value => GetValueEnum(value, t);
        }

        if (t == typeof(bool))
        {
            return GetValueBool;
        }

        if (t == typeof(sbyte))
        {
            return GetValueSByte;
        }

        if (t == typeof(byte))
        {
            return GetValueByte;
        }

        if (t == typeof(int))
        {
            return GetValueInt;
        }

        if (t == typeof(uint))
        {
            return GetValueUInt;
        }

        if (t == typeof(short))
        {
            return GetValueShort;
        }

        if (t == typeof(ushort))
        {
            return GetValueUShort;
        }

        if (t == typeof(long))
        {
            return GetValueLong;
        }

        if (t == typeof(ulong))
        {
            return GetValueULong;
        }

        if (t == typeof(char))
        {
            return GetValueChar;
        }

        if (t == typeof(float))
        {
            return GetValueFloat;
        }

        if (t == typeof(double))
        {
            return GetValueDouble;
        }

        if (t == typeof(decimal))
        {
            return GetValueDecimal;
        }

        if (t == typeof(DateTime))
        {
            return GetValueDateTime;
        }

        if (t == typeof(DateOnly))
        {
            return GetValueDateOnly;
        }

        if (t == typeof(Guid))
        {
            return GetValueGuid;
        }

        throw new FilterMappingException(string.Format(FilterMappingException.UnsupportedFieldTypePattern, fieldName ?? "unknown", t.Name));
    }

    public static string GetValueString(string value)
    {
        return value;
    }

    public static object GetValueEnum(string value, Type t)
    {
        return Enum.Parse(t, value, ignoreCase: true);
    }

    public static object GetValueBool(string value)
    {
        return bool.Parse(value);
    }

    public static object GetValueSByte(string value)
    {
        return sbyte.Parse(value);
    }

    public static object GetValueByte(string value)
    {
        return byte.Parse(value);
    }

    public static object GetValueInt(string value)
    {
        return int.Parse(value);
    }

    public static object GetValueUInt(string value)
    {
        return uint.Parse(value);
    }

    public static object GetValueShort(string value)
    {
        return short.Parse(value);
    }

    public static object GetValueUShort(string value)
    {
        return ushort.Parse(value);
    }

    public static object GetValueLong(string value)
    {
        return long.Parse(value);
    }

    public static object GetValueULong(string value)
    {
        return ulong.Parse(value);
    }

    public static object GetValueChar(string value)
    {
        if (value.Length != 1)
        {
            throw new FormatException("Value \"" + value + "\" is not a valid char.");
        }
        return value[0];
    }

    public static object GetValueFloat(string value)
    {
        return float.Parse(value, CultureInfo.InvariantCulture);
    }

    public static object GetValueDouble(string value)
    {
        return double.Parse(value, CultureInfo.InvariantCulture);
    }

    public static object GetValueDecimal(string value)
    {
        return decimal.Parse(value, CultureInfo.InvariantCulture);
    }

    public static object GetValueDateTime(string value)
    {
        return DateTime.Parse(value, CultureInfo.InvariantCulture);
    }

    public static object GetValueDateOnly(string value)
    {
        return DateOnly.Parse(value, CultureInfo.InvariantCulture);
    }

    public static object GetValueGuid(string value)
    {
        return Guid.Parse(value);
    }
}
