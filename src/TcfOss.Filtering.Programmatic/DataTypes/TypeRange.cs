using TcfOss.Filtering.Contracts;

namespace TcfOss.Filtering.Programmatic.DataTypes;

public abstract record TypeRange<T> : ITypeRange
    where T : struct
{
    public T? Min { get; set; }
    public T? Max { get; set; }

    public abstract string ConvertValueToString(T value, MappingOptions options);

    public Filter? ToContractFilter(string field, MappingOptions options)
    {
        if (Min != null && Max != null)
        {
            return new RangeFilter
            {
                Field = field,
                ValueFrom = ConvertValueToString(Min.Value, options),
                ValueTo = ConvertValueToString(Max.Value, options)
            };
        }
        if (Min != null)
        {
            return new SimpleFilter
            {
                Field = field,
                Operator = FilterOperators.GreaterThanOrEqualTo,
                Value = ConvertValueToString(Min.Value, options)
            };
        }
        if (Max != null)
        {
            return new SimpleFilter
            {
                Field = field,
                Operator = FilterOperators.LessThanOrEqualTo,
                Value = ConvertValueToString(Max.Value, options)
            };
        }
        return null;
    }
}
