using TcfOss.Filtering.Contracts;

namespace TcfOss.Filtering.Programmatic.Tests;

public class MapSearchFiltersTests
{
    [Fact]
    public void ConstructContractFilter_NoEmittedFilters_ReturnsNull()
    {
        SearchFilter[] filters =
        [
            new SearchFilter<string>("Name"),
            new SearchFilter<int?>("Age")
        ];

        Filter? result = MapSearchFilters.ConstructContractFilter(filters, new MappingOptions());

        Assert.Null(result);
    }

    [Fact]
    public void ConstructContractFilter_SingleFilter_ReturnsFilterDirectly()
    {
        SearchFilter[] filters =
        [
            new SearchFilter<string>("Name") { Value = "Alice" }
        ];

        Filter? result = MapSearchFilters.ConstructContractFilter(filters, new MappingOptions());

        SimpleFilter simple = Assert.IsType<SimpleFilter>(result);
        Assert.Equal("Name", simple.Field);
    }

    [Fact]
    public void ConstructContractFilter_MultipleFilters_ReturnsAndComposite()
    {
        SearchFilter[] filters =
        [
            new SearchFilter<string>("Name") { Value = "Alice" },
            new SearchFilter<int>("Age") { Value = 30 }
        ];

        Filter? result = MapSearchFilters.ConstructContractFilter(filters, new MappingOptions());

        CompositeFilter composite = Assert.IsType<CompositeFilter>(result);
        Assert.Equal(LogicalOperators.And, composite.LogicalOperator);
        Assert.Equal(2, composite.Filters.Length);
    }

    [Fact]
    public void ConstructDataRequest_UsesDefaultsWhenNullInputsProvided()
    {
        SearchFilter[] filters =
        [
            new SearchFilter<string>("Name") { Value = "Alice" }
        ];
        MappingOptions options = new() { DefaultPage = 3, DefaultPageSize = 99 };

        DataRequest request = MapSearchFilters.ConstructDataRequest(filters, sortComponents: null, page: null, pageSize: null, options);

        Assert.Equal(3, request.Page);
        Assert.Equal(99, request.PageSize);
        Assert.Empty(request.Sorts);
        Assert.NotNull(request.Filter);
    }

    [Fact]
    public void ConstructDataRequest_UsesProvidedPagingAndSorts()
    {
        SearchFilter[] filters =
        [
            new SearchFilter<string>("Name") { Value = "Alice" }
        ];

        SortComponent[] sorts =
        [
            new("Name", SortDirections.Descending)
        ];

        DataRequest request = MapSearchFilters.ConstructDataRequest(filters, sorts, page: 2, pageSize: 25, new MappingOptions());

        Assert.Equal(2, request.Page);
        Assert.Equal(25, request.PageSize);
        Assert.Single(request.Sorts);
        Assert.Equal("Name", request.Sorts[0].Field);
        Assert.Equal(SortDirections.Descending, request.Sorts[0].Direction);
    }

    [Fact]
    public void ConstructDynamicDataRequest_MapsRequestedFieldsAndDefaults()
    {
        string[] requested = ["Name", "Department"];
        SearchFilter[] filters =
        [
            new SearchFilter<string>("Department") { Value = "Engineering" }
        ];
        MappingOptions options = new() { DefaultPage = 5, DefaultPageSize = 10 };

        DynamicDataRequest request = MapSearchFilters.ConstructDynamicDataRequest(
            requested,
            filters,
            sortComponents: null,
            page: null,
            pageSize: null,
            options);

        Assert.Equal(requested, request.RequestedFields);
        Assert.Equal(5, request.Page);
        Assert.Equal(10, request.PageSize);
        Assert.NotNull(request.Filter);
        Assert.IsType<SimpleFilter>(request.Filter);
        var simpleFilter = (SimpleFilter)request.Filter!;
        Assert.Equal("Department", simpleFilter.Field);
        Assert.Equal(FilterOperators.EqualTo, simpleFilter.Operator);
        Assert.Equal("Engineering", simpleFilter.Value);
    }
}
