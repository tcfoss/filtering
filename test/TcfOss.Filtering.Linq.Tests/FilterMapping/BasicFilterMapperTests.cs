using TcfOss.Filtering.Contracts;
using TcfOss.Filtering.Linq.FilterMapping;

namespace TcfOss.Filtering.Linq.Tests.FilterMapping;

public class BasicFilterMapperTests
{
    private readonly BasicFilterMapper _mapper = new();

    private record FakeFilter : Filter
    {
        public override string FilterType => "mock";
    }

    [Theory]
    [InlineData(FilterOperators.EqualTo)]
    [InlineData(FilterOperators.NotEqualTo)]
    [InlineData(FilterOperators.GreaterThan)]
    [InlineData(FilterOperators.GreaterThanOrEqualTo)]
    [InlineData(FilterOperators.LessThan)]
    [InlineData(FilterOperators.LessThanOrEqualTo)]
    [InlineData(FilterOperators.StartsWith)]
    [InlineData(FilterOperators.DoesNotStartWith)]
    [InlineData(FilterOperators.EndsWith)]
    [InlineData(FilterOperators.DoesNotEndWith)]
    [InlineData(FilterOperators.Contains)]
    [InlineData(FilterOperators.DoesNotContain)]
    public void SimpleFilter_AllValueOperators_PreservesStringValue(string op)
    {
        var dto = new Contracts.SimpleFilter("Name", op, "Alice");
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal("Name", result.Field);
        Assert.Equal("Alice", result.Value);
    }

    [Theory]
    [InlineData(FilterOperators.IsNull)]
    [InlineData(FilterOperators.IsNotNull)]
    public void SimpleFilter_NullOperators_NullValueAllowed(string op)
    {
        var dto = new Contracts.SimpleFilter("Name", op);
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal("Name", result.Field);
    }

    [Fact]
    public void SimpleFilter_NullValue_FallsBackToEmptyString()
    {
        var dto = new Contracts.SimpleFilter("Name", FilterOperators.EqualTo);
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal(string.Empty, result.Value);
    }

    [Fact]
    public void CompositeFilter_And_MapsCorrectly()
    {
        var dto = new Contracts.CompositeFilter(LogicalOperators.And,
        [
            new Contracts.SimpleFilter("Name", FilterOperators.EqualTo, "Alice"),
                new Contracts.SimpleFilter("Age", FilterOperators.GreaterThan, "30"),
            ]);

        CompositeFilter result = _mapper.ToFilter(dto);

        Assert.Equal(LogicalOperator.And, result.LogicalOperator);
        Assert.Equal(2, result.Filters.Length);
    }

    [Fact]
    public void CompositeFilter_Or_MapsCorrectly()
    {
        var dto = new Contracts.CompositeFilter(LogicalOperators.Or,
        [
            new Contracts.SimpleFilter("Name", FilterOperators.EqualTo, "Alice"),
                new Contracts.SimpleFilter("Name", FilterOperators.EqualTo, "Bob"),
            ]);

        CompositeFilter result = _mapper.ToFilter(dto);

        Assert.Equal(LogicalOperator.Or, result.LogicalOperator);
    }

    [Fact]
    public void CompositeFilter_Nested_MapsRecursively()
    {
        var dto = new Contracts.CompositeFilter(LogicalOperators.And,
        [
            new Contracts.SimpleFilter("Name", FilterOperators.EqualTo, "Alice"),
                new Contracts.CompositeFilter(LogicalOperators.Or,
                [
                    new Contracts.SimpleFilter("Age", FilterOperators.GreaterThan, "25"),
                    new Contracts.SimpleFilter("Age", FilterOperators.LessThan, "10"),
                ]),
            ]);

        IFilter result = _mapper.ToFilter(dto);

        CompositeFilter outer = Assert.IsType<CompositeFilter>(result);
        Assert.Equal(2, outer.Filters.Length);
        CompositeFilter inner = Assert.IsType<CompositeFilter>(outer.Filters[1]);
        Assert.Equal(LogicalOperator.Or, inner.LogicalOperator);
    }

    [Fact]
    public void FilterDto_Dispatch_SimpleFilterDto_CallsSimpleOverload()
    {
        Filter dto = new Contracts.SimpleFilter("Name", FilterOperators.EqualTo, "Alice");
        IFilter result = _mapper.ToFilter(dto);
        Assert.IsType<SimpleFilter>(result);
    }

    [Fact]
    public void FilterDto_Dispatch_CompositeFilterDto_CallsCompositeOverload()
    {
        Filter dto = new Contracts.CompositeFilter(LogicalOperators.And,
        [
            new Contracts.SimpleFilter("Name", FilterOperators.EqualTo, "Alice"),
            ]);
        IFilter result = _mapper.ToFilter(dto);
        Assert.IsType<CompositeFilter>(result);
    }

    [Fact]
    public void UnknownOperator_ThrowsFilterMappingException()
    {
        var dto = new Contracts.SimpleFilter("Name", "bogus", "Alice");
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => _mapper.ToFilter(dto));
        Assert.Contains("bogus", ex.Message);
    }

    [Fact]
    public void UnknownLogicalOperator_ThrowsFilterMappingException()
    {
        var dto = new Contracts.CompositeFilter("bogus",
        [
            new Contracts.SimpleFilter("Name", FilterOperators.EqualTo, "Alice"),
            ]);
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => _mapper.ToFilter(dto));
        Assert.Contains("bogus", ex.Message);
    }

    [Fact]
    public void UnknownFilterType_ThrowsFilterMappingException()
    {
        var dto = new FakeFilter();
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => _mapper.ToFilter(dto));
        Assert.Contains("mock", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(QuantifiedOperators.Any)]
    [InlineData(QuantifiedOperators.All)]
    public void QuantifiedFilter_AnyAll_MapsCorrectly(string op)
    {
        var subFilterDto = new Contracts.SimpleFilter("Value", FilterOperators.EqualTo, "csharp");
        var dto = new Contracts.QuantifiedFilter
        {
            Field = "Tags",
            Operator = op,
            SubFilter = subFilterDto,
        };

        QuantifiedFilter result = _mapper.ToFilter(dto);

        Assert.Equal("Tags", result.FieldName);
        QuantifiedOperator expected = op == QuantifiedOperators.Any ? QuantifiedOperator.Any : QuantifiedOperator.All;
        Assert.Equal(expected, result.QuantifiedOperator);
        Assert.IsType<SimpleFilter>(result.SubFilter);
    }

    [Fact]
    public void QuantifiedFilter_IsNegated_PreservesFlag()
    {
        var dto = new Contracts.QuantifiedFilter
        {
            Field = "Tags",
            Operator = QuantifiedOperators.Any,
            SubFilter = new Contracts.SimpleFilter("Value", FilterOperators.EqualTo, "x"),
            IsNegated = true,
        };

        QuantifiedFilter result = _mapper.ToFilter(dto);

        Assert.True(result.IsNegated);
    }

    [Fact]
    public void QuantifiedFilter_IsNullable_PreservesFlag()
    {
        var dto = new Contracts.QuantifiedFilter
        {
            Field = "Tags",
            Operator = QuantifiedOperators.Any,
            SubFilter = new Contracts.SimpleFilter("Value", FilterOperators.EqualTo, "x"),
            IsNullable = true,
        };

        QuantifiedFilter result = _mapper.ToFilter(dto);

        Assert.True(result.IsNullable);
    }

    [Fact]
    public void QuantifiedFilter_UnknownOperator_ThrowsFilterMappingException()
    {
        var dto = new Contracts.QuantifiedFilter
        {
            Field = "Tags",
            Operator = "bogus",
            SubFilter = new Contracts.SimpleFilter("Value", FilterOperators.EqualTo, "x"),
        };

        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => _mapper.ToFilter(dto));
        Assert.Contains("bogus", ex.Message);
    }

    [Fact]
    public void FilterDto_Dispatch_QuantifiedFilterDto_CallsQuantifiedOverload()
    {
        Filter dto = new Contracts.QuantifiedFilter
        {
            Field = "Tags",
            Operator = QuantifiedOperators.Any,
            SubFilter = new Contracts.SimpleFilter("Value", FilterOperators.EqualTo, "x"),
        };

        IFilter result = _mapper.ToFilter(dto);
        Assert.IsType<QuantifiedFilter>(result);
    }

    [Fact]
    public void DataFilter_WithFilter_Maps()
    {
        var dto = new Contracts.DataRequest
        {
            Filter = new Contracts.SimpleFilter("Name", FilterOperators.Contains, "Smith"),
            Sorts = [new Contracts.SortComponent("Age", SortDirections.Descending)],
            Page = 2,
            PageSize = 50,
        };

        DataRequest result = _mapper.ToDataRequest(dto);

        Assert.NotNull(result.Filter);

        Assert.IsType<SimpleFilter>(result.Filter);
        SimpleFilter simpleFilter = (SimpleFilter)result.Filter!;
        Assert.Equal("Name", simpleFilter.Field);
        Assert.Equal("Smith", simpleFilter.Value);

        Assert.Single(result.Sorts);
        Assert.Equal("Age", result.Sorts[0].Field);
        Assert.Equal(SortDirection.Descending, result.Sorts[0].Direction);

        Assert.Equal(2, result.Page);
        Assert.Equal(50, result.PageSize);
    }

    [Fact]
    public void DataFilter_NoFilter_Maps()
    {
        var dto = new Contracts.DataRequest
        {
            Filter = null,
            Sorts = [new Contracts.SortComponent("Age", SortDirections.Descending)],
            Page = 1,
            PageSize = 20,
        };

        DataRequest result = _mapper.ToDataRequest(dto);

        Assert.Null(result.Filter);

        Assert.Single(result.Sorts);
        Assert.Equal("Age", result.Sorts[0].Field);
        Assert.Equal(SortDirection.Descending, result.Sorts[0].Direction);

        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
    }

    [Fact]
    public void DynamicDataFilter_WithFilter_Maps()
    {
        var dto = new Contracts.DynamicDataRequest
        {
            RequestedFields = ["Name", "Age"],
            Filter = new Contracts.SimpleFilter("Name", FilterOperators.Contains, "Smith"),
            Sorts = [new Contracts.SortComponent("Age", SortDirections.Descending)],
            Page = 2,
            PageSize = 50,
        };

        DynamicDataRequest result = _mapper.ToDataRequest(dto);

        Assert.Equal(["Name", "Age"], result.RequestedFields);

        Assert.NotNull(result.Filter);
        SimpleFilter simpleFilter = Assert.IsType<SimpleFilter>(result.Filter);
        Assert.Equal("Name", simpleFilter.Field);
        Assert.Equal("Smith", simpleFilter.Value);

        Assert.Single(result.Sorts);
        Assert.Equal("Age", result.Sorts[0].Field);
        Assert.Equal(SortDirection.Descending, result.Sorts[0].Direction);

        Assert.Equal(2, result.Page);
        Assert.Equal(50, result.PageSize);
    }

    [Fact]
    public void DynamicDataFilter_NoFilter_Maps()
    {
        var dto = new Contracts.DynamicDataRequest
        {
            RequestedFields = ["City"],
            Filter = null,
            Sorts = [],
            Page = 1,
            PageSize = 25,
        };

        DynamicDataRequest result = _mapper.ToDataRequest(dto);

        Assert.Equal(["City"], result.RequestedFields);
        Assert.Null(result.Filter);
        Assert.Empty(result.Sorts);
        Assert.Equal(1, result.Page);
        Assert.Equal(25, result.PageSize);
    }

    // -------------------------------------------------------------------------
    // ToSortComponent — tested via BasicFilterMapper (shared implementation)
    // -------------------------------------------------------------------------

    [Fact]
    public void Sort_Ascending_MapsCorrectly()
    {
        var dto = new Contracts.SortComponent("Name", SortDirections.Ascending);
        SortComponent result = _mapper.ToSortComponent(dto);

        Assert.Equal("Name", result.Field);
        Assert.Equal(SortDirection.Ascending, result.Direction);
    }

    [Fact]
    public void Sort_Descending_MapsCorrectly()
    {
        var dto = new Contracts.SortComponent("Age", SortDirections.Descending);
        SortComponent result = _mapper.ToSortComponent(dto);

        Assert.Equal("Age", result.Field);
        Assert.Equal(SortDirection.Descending, result.Direction);
    }

    [Fact]
    public void Sort_UnknownDirection_ThrowsFilterMappingException()
    {
        var dto = new Contracts.SortComponent("Name", "sideways");
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => _mapper.ToSortComponent(dto));
        Assert.Contains("sideways", ex.Message);
    }

    // -------------------------------------------------------------------------
    // SetFilter Mapping
    // -------------------------------------------------------------------------

    [Fact]
    public void SetFilter_StringValues_MapsCorrectly()
    {
        var mapper = new BasicFilterMapper();
        var dto = new Contracts.SetFilter("Name", ["Alice", "Bob"]);

        SetFilter result = mapper.ToFilter(dto);

        Assert.Equal("Name", result.Field);
        Assert.Equal(2, result.Values.Length);
        Assert.Equal("Alice", result.Values[0]);
        Assert.Equal("Bob", result.Values[1]);
        Assert.False(result.Negated);
    }

    [Fact]
    public void SetFilter_Negated_MapsCorrectly()
    {
        var mapper = new BasicFilterMapper();
        var dto = new Contracts.SetFilter("Name", ["Alice"], negated: true);

        SetFilter result = mapper.ToFilter(dto);

        Assert.True(result.Negated);
    }

    [Fact]
    public void SetFilter_EmptyValues_Throws()
    {
        var mapper = new BasicFilterMapper();
        var dto = new Contracts.SetFilter("Name", []);

        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("Name", ex.Message);
    }

    [Fact]
    public void SetFilter_Dispatch_SetFilterDto_CallsSetOverload()
    {
        var mapper = new BasicFilterMapper();
        Filter dto = new Contracts.SetFilter("Name", ["Alice"]);

        IFilter result = mapper.ToFilter(dto);

        Assert.IsType<SetFilter>(result);
    }

    // -------------------------------------------------------------------------
    // RangeFilter Mapping
    // -------------------------------------------------------------------------

    [Fact]
    public void RangeFilter_MapsDefaultFlags()
    {
        var mapper = new BasicFilterMapper();
        var dto = new Contracts.RangeFilter("Age", "27", "31");

        RangeFilter result = mapper.ToFilter(dto);

        Assert.Equal("Age", result.Field);
        Assert.Equal("27", result.ValueFrom);
        Assert.Equal("31", result.ValueTo);
        Assert.False(result.Exclusive);
        Assert.False(result.Negated);
    }

    [Fact]
    public void RangeFilter_ExclusiveNegate_Preserved()
    {
        var mapper = new BasicFilterMapper();
        var dto = new Contracts.RangeFilter("Age", "27", "31", exclusive: true, negated: true);

        RangeFilter result = mapper.ToFilter(dto);

        Assert.True(result.Exclusive);
        Assert.True(result.Negated);
    }

    [Fact]
    public void RangeFilter_Dispatch_RangeFilterDto_CallsRangeOverload()
    {
        var mapper = new BasicFilterMapper();
        Filter dto = new Contracts.RangeFilter("Age", "1", "100");

        IFilter result = mapper.ToFilter(dto);

        Assert.IsType<RangeFilter>(result);
    }
}
