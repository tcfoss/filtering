using System.Text.RegularExpressions;
using TcfOss.Filtering.Contracts;

namespace TcfOss.Filtering.Linq.FilterMapping;

public partial class BasicFilterMapper : IMapFilters
{
    [GeneratedRegex(@"^[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)*$", RegexOptions.CultureInvariant)]
    private static partial Regex FieldNameRegex();

    protected static void ValidateFieldName(string field)
    {
        if (string.IsNullOrEmpty(field) || !FieldNameRegex().IsMatch(field))
        {
            throw new FilterMappingException(string.Format(FilterMappingException.InvalidFieldNamePattern, field));
        }
    }

    public IFilter ToFilter(Filter dto)
    {
        return dto switch
        {
            Contracts.SimpleFilter simple => ToFilter(simple),
            Contracts.CompositeFilter composite => ToFilter(composite),
            Contracts.SetFilter set => ToFilter(set),
            Contracts.RangeFilter range => ToFilter(range),
            Contracts.QuantifiedFilter quantified => ToFilter(quantified),
            _ => throw new FilterMappingException(string.Format(FilterMappingException.UnknownFilterTypePattern, dto.FilterType))
        };
    }

    public virtual SimpleFilter ToFilter(Contracts.SimpleFilter dto)
    {
        ValidateFieldName(dto.Field);
        QueryOperator op = QueryOperator.ParseContractKey(dto.Operator);
        return new SimpleFilter(dto.Field, op, dto.Value ?? string.Empty);
    }

    public CompositeFilter ToFilter(Contracts.CompositeFilter dto)
    {
        LogicalOperator logicalOperator = dto.LogicalOperator switch
        {
            LogicalOperators.And => LogicalOperator.And,
            LogicalOperators.Or => LogicalOperator.Or,
            _ => throw new FilterMappingException(string.Format(FilterMappingException.UnknownLogicalOperatorPattern, dto.LogicalOperator))
        };

        IFilter[] filters = [.. dto.Filters.Select(ToFilter)];
        return new CompositeFilter(filters, logicalOperator);
    }

    public virtual SetFilter ToFilter(Contracts.SetFilter dto)
    {
        ValidateFieldName(dto.Field);
        if (dto.Values.Length == 0)
        {
            throw new FilterMappingException(string.Format(FilterMappingException.EmptyValuesPattern, dto.Field));
        }

        object[] values = [.. dto.Values.Select(v => (object)v)];
        return new SetFilter(dto.Field, values, dto.Negated);
    }

    public virtual QuantifiedFilter ToFilter(Contracts.QuantifiedFilter dto)
    {
        ValidateFieldName(dto.Field);
        IFilter subFilter = ToFilter(dto.SubFilter);
        QuantifiedOperator op = dto.Operator switch
        {
            QuantifiedOperators.Any => QuantifiedOperator.Any,
            QuantifiedOperators.All => QuantifiedOperator.All,
            _ => throw new FilterMappingException(string.Format(FilterMappingException.UnknownQuantifiedOperatorPattern, dto.Operator))
        };
        return new QuantifiedFilter(dto.Field, subFilter, op)
        {
            IsNegated = dto.IsNegated,
            IsNullable = dto.IsNullable
        };
    }

    public virtual RangeFilter ToFilter(Contracts.RangeFilter dto)
    {
        ValidateFieldName(dto.Field);
        return new RangeFilter(dto.Field, dto.ValueFrom, dto.ValueTo, dto.Exclusive, dto.Negated);
    }

    public virtual SortComponent ToSortComponent(Contracts.SortComponent dto)
    {
        ValidateFieldName(dto.Field);
        SortDirection direction = dto.Direction switch
        {
            SortDirections.Ascending => SortDirection.Ascending,
            SortDirections.Descending => SortDirection.Descending,
            _ => throw new FilterMappingException(string.Format(FilterMappingException.UnknownSortDirectionPattern, dto.Direction))
        };

        return new SortComponent(dto.Field, direction);
    }

    public virtual string ToRequestedField(string field)
    {
        ValidateFieldName(field);
        return field;
    }

    public DataRequest ToDataRequest(Contracts.DataRequest dto)
    {
        ValidatePaging(dto.Page, dto.PageSize);
        IFilter? filter = dto.Filter is not null ? ToFilter(dto.Filter) : null;
        SortComponent[] sorts = [.. dto.Sorts.Select(ToSortComponent)];
        return new DataRequest
        {
            Filter = filter,
            Sorts = sorts,
            Page = dto.Page,
            PageSize = dto.PageSize
        };
    }

    public DynamicDataRequest ToDataRequest(Contracts.DynamicDataRequest dto)
    {
        ValidatePaging(dto.Page, dto.PageSize);
        if (dto.RequestedFields.Length == 0)
        {
            throw new FilterMappingException(FilterMappingException.EmptyRequestedFieldsPattern);
        }
        IFilter? filter = dto.Filter is not null ? ToFilter(dto.Filter) : null;
        SortComponent[] sorts = [.. dto.Sorts.Select(ToSortComponent)];
        string[] requestedFields = [.. dto.RequestedFields.Select(ToRequestedField)];
        return new DynamicDataRequest
        {
            RequestedFields = requestedFields,
            Filter = filter,
            Sorts = sorts,
            Page = dto.Page,
            PageSize = dto.PageSize
        };
    }

    private static void ValidatePaging(int page, int pageSize)
    {
        if (page < 1)
        {
            throw new FilterMappingException(string.Format(FilterMappingException.InvalidPagePattern, page));
        }
        if (pageSize < 1)
        {
            throw new FilterMappingException(string.Format(FilterMappingException.InvalidPageSizePattern, pageSize));
        }
    }
}
