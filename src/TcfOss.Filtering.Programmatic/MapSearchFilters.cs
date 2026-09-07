using TcfOss.Filtering.Contracts;

namespace TcfOss.Filtering.Programmatic;

public static class MapSearchFilters
{
    public static Filter? ConstructContractFilter(IEnumerable<SearchFilter> filters, MappingOptions options)
    {
        Filter[] contractFilters = [.. filters
            .Select(f => f.ToContractFilter(options))
            .OfType<Filter>()
        ];

        if (contractFilters.Length == 0)
        {
            return null;
        }

        if (contractFilters.Length == 1)
        {
            return contractFilters[0];
        }

        return new CompositeFilter
        {
            LogicalOperator = LogicalOperators.And,
            Filters = contractFilters
        };
    }

    public static DataRequest ConstructDataRequest(IEnumerable<SearchFilter> filters, IEnumerable<SortComponent>? sortComponents, int? page, int? pageSize, MappingOptions options)
    {
        return new DataRequest
        {
            Filter = ConstructContractFilter(filters, options),
            Sorts = sortComponents?.ToArray() ?? [],
            Page = page ?? options.DefaultPage,
            PageSize = pageSize ?? options.DefaultPageSize
        };
    }

    public static DynamicDataRequest ConstructDynamicDataRequest(IEnumerable<string> requestedFields, IEnumerable<SearchFilter> filters, IEnumerable<SortComponent>? sortComponents, int? page, int? pageSize, MappingOptions options)
    {
        return new DynamicDataRequest
        {
            RequestedFields = [.. requestedFields],
            Filter = ConstructContractFilter(filters, options),
            Sorts = sortComponents?.ToArray() ?? [],
            Page = page ?? options.DefaultPage,
            PageSize = pageSize ?? options.DefaultPageSize
        };
    }
}
