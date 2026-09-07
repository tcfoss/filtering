using TcfOss.Filtering.Contracts;
using TcfOss.Filtering.Programmatic.DataTypes;

namespace TcfOss.Filtering.Programmatic;

public abstract record SearchFilter(string Field)
{
    public abstract Filter? ToContractFilter(MappingOptions options);
}

public record SearchFilter<T>(string Field) : SearchFilter(Field)
{
    public T? Value { get; set; }

    public SearchFilter(string field, T? value) : this(field)
    {
        Value = value;
    }

    public override Filter? ToContractFilter(MappingOptions options)
    {
        if (Value == null)
        {
            if (options.NullValueToIsNullFilter)
            {
                return new SimpleFilter
                {
                    Field = Field,
                    Operator = FilterOperators.IsNull
                };
            }
            return null;
        }

        if (Value is ITypeRange typeRange)
        {
            Filter? filter = typeRange.ToContractFilter(Field, options);
            if (filter == null && options.NullValueToIsNullFilter)
            {
                return new SimpleFilter
                {
                    Field = Field,
                    Operator = FilterOperators.IsNull
                };
            }
            return filter;
        }

        if (Value is string stringValue)
        {
            return ConstructStringFilter(Field, stringValue, options);
        }

        return new SimpleFilter
        {
            Field = Field,
            Operator = FilterOperators.EqualTo,
            Value = Value.ToString()
        };
    }

    private static SimpleFilter ConstructStringFilter(string field, string value, MappingOptions options)
    {
        if (options.HandleStringWildcards)
        {
            int wildcardCount = value.Count(c => c == '%');
            if (wildcardCount == 2 && value.StartsWith('%') && value.EndsWith('%'))
            {
                return new SimpleFilter
                {
                    Field = field,
                    Operator = FilterOperators.Contains,
                    Value = value.Substring(1, value.Length - 2)
                };
            }
            if (wildcardCount == 1)
            {
                if (value.StartsWith('%'))
                {
                    return new SimpleFilter
                    {
                        Field = field,
                        Operator = FilterOperators.EndsWith,
                        Value = value.Substring(1)
                    };
                }
                if (value.EndsWith('%'))
                {
                    return new SimpleFilter
                    {
                        Field = field,
                        Operator = FilterOperators.StartsWith,
                        Value = value.Substring(0, value.Length - 1)
                    };
                }
            }
            if (options.SupportLikeOperator && wildcardCount > 0)
            {
                return new SimpleFilter
                {
                    Field = field,
                    Operator = FilterOperators.Like,
                    Value = value
                };
            }
        }

        return new SimpleFilter
        {
            Field = field,
            Operator = FilterOperators.EqualTo,
            Value = value
        };
    }
}
