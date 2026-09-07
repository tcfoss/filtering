using TcfOss.Filtering.Contracts;

namespace TcfOss.Filtering.Linq.FilterMapping;

public class DictFilterMapper(Dictionary<string, Func<string, object>> valueParsers, Dictionary<string, Type>? propertyTypes = null) : BasicFilterMapper
{
    private readonly Dictionary<string, Func<string, object>> _valueParsers = valueParsers;
    private readonly Dictionary<string, Type>? _propertyTypes = propertyTypes;

    public override SimpleFilter ToFilter(Contracts.SimpleFilter dto)
    {
        var op = QueryOperator.ParseContractKey(dto.Operator);

        Func<string, object> parser = GetValueParser(dto.Field);

        if (dto.Operator is FilterOperators.IsNull or FilterOperators.IsNotNull)
        {
            return new SimpleFilter()
            {
                Field = dto.Field,
                Operator = op,
                Value = null!
            };
        }
        else if (dto.Value == null)
        {
            throw new FilterMappingException(string.Format(FilterMappingException.NullValueNotAllowedPattern, dto.Operator));
        }

        try
        {
            object value = parser(dto.Value);
            return new SimpleFilter()
            {
                Field = dto.Field,
                Operator = op,
                Value = value
            };
        }
        catch (FormatException ex)
        {
            throw new FilterMappingException(string.Format(FilterMappingException.InvalidValuePattern, dto.Field, dto.Value), ex);
        }
    }

    public override SetFilter ToFilter(Contracts.SetFilter dto)
    {
        if (dto.Values.Length == 0)
        {
            throw new FilterMappingException(string.Format(FilterMappingException.EmptyValuesPattern, dto.Field));
        }

        Func<string, object> parser = GetValueParser(dto.Field);
        object[] values = new object[dto.Values.Length];
        for (int i = 0; i < dto.Values.Length; i++)
        {
            try
            {
                values[i] = parser(dto.Values[i]);
            }
            catch (FormatException ex)
            {
                throw new FilterMappingException(string.Format(FilterMappingException.InvalidValuePattern, dto.Field, dto.Values[i]), ex);
            }
        }

        Type? elementType = _propertyTypes?.GetValueOrDefault(dto.Field);
        return new SetFilter(dto.Field, values, dto.Negated, elementType);
    }

    public override RangeFilter ToFilter(Contracts.RangeFilter dto)
    {
        Func<string, object> parser = GetValueParser(dto.Field);

        object valueFrom;
        object valueTo;

        try
        {
            valueFrom = parser(dto.ValueFrom);
        }
        catch (FormatException ex)
        {
            throw new FilterMappingException(string.Format(FilterMappingException.InvalidValuePattern, dto.Field, dto.ValueFrom), ex);
        }

        try
        {
            valueTo = parser(dto.ValueTo);
        }
        catch (FormatException ex)
        {
            throw new FilterMappingException(string.Format(FilterMappingException.InvalidValuePattern, dto.Field, dto.ValueTo), ex);
        }

        return new RangeFilter(dto.Field, valueFrom, valueTo, dto.Exclusive, dto.Negated);
    }

    public override string ToRequestedField(string field)
    {
        _ = GetValueParser(field); // Validate field exists and navigation path
        return field;
    }

    private Func<string, object> GetValueParser(string fieldName)
    {
        if (_valueParsers.TryGetValue(fieldName, out Func<string, object>? parser))
        {
            return parser;
        }

        if (fieldName.Contains('.'))
        {
            string[] components = fieldName.Split('.');
            if (components.Length != 2
                || (!components[1].Equals("Count", StringComparison.OrdinalIgnoreCase)
                    && !components[1].Equals("Length", StringComparison.OrdinalIgnoreCase)))
            {
                throw new FilterMappingException(string.Format(FilterMappingException.UnknownFieldPattern, fieldName));
            }

            if (!_valueParsers.ContainsKey(components[0]))
            {
                throw new FilterMappingException(string.Format(FilterMappingException.UnknownFieldPattern, fieldName));
            }

            return v => int.Parse(v);
        }

        throw new FilterMappingException(string.Format(FilterMappingException.UnknownFieldPattern, fieldName));
    }

    public override SortComponent ToSortComponent(Contracts.SortComponent dto)
    {
        _ = GetValueParser(dto.Field); // Validate field exists and navigation path
        return base.ToSortComponent(dto);
    }
}
