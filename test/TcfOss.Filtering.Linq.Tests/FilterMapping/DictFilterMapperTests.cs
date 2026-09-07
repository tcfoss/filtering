using TcfOss.Filtering.Contracts;
using TcfOss.Filtering.Linq.FilterMapping;

namespace TcfOss.Filtering.Linq.Tests.FilterMapping;

public class DictFilterMapperTests
{
    private static DictFilterMapper MakeMapper() => new(new Dictionary<string, Func<string, object>>
    {
        ["Name"] = v => v,
        ["Age"] = v => int.Parse(v),
        ["Salary"] = v => decimal.Parse(v, System.Globalization.CultureInfo.InvariantCulture),
        ["Friends"] = v => v,
    });

    [Fact]
    public void KnownStringField_MapsCorrectly()
    {
        DictFilterMapper mapper = MakeMapper();
        var dto = new Contracts.SimpleFilter("Name", FilterOperators.EqualTo, "Alice");
        SimpleFilter result = mapper.ToFilter(dto);

        Assert.Equal("Alice", result.Value);
        Assert.Equal(QueryOperator.EqualTo, result.Operator);
    }

    [Fact]
    public void KnownIntField_ParsesValue()
    {
        DictFilterMapper mapper = MakeMapper();
        var dto = new Contracts.SimpleFilter("Age", FilterOperators.GreaterThan, "30");
        SimpleFilter result = mapper.ToFilter(dto);

        Assert.Equal(30, result.Value);
    }

    [Fact]
    public void KnownDecimalField_ParsesValue()
    {
        DictFilterMapper mapper = MakeMapper();
        var dto = new Contracts.SimpleFilter("Salary", FilterOperators.GreaterThanOrEqualTo, "1234.56");
        SimpleFilter result = mapper.ToFilter(dto);

        Assert.Equal(1234.56m, result.Value);
    }

    [Fact]
    public void IsNull_UnknownField_ThrowsFilterMappingException()
    {
        DictFilterMapper mapper = MakeMapper();
        var dto = new Contracts.SimpleFilter("UnknownField", FilterOperators.IsNull);
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("UnknownField", ex.Message);
    }

    [Fact]
    public void IsNotNull_UnknownField_ThrowsFilterMappingException()
    {
        DictFilterMapper mapper = MakeMapper();
        var dto = new Contracts.SimpleFilter("UnknownField", FilterOperators.IsNotNull);
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("UnknownField", ex.Message);
    }

    [Fact]
    public void IsNotNull_KnownField_NullValue_DoesNotThrow()
    {
        DictFilterMapper mapper = MakeMapper();
        var dto = new Contracts.SimpleFilter("Name", FilterOperators.IsNotNull);
        SimpleFilter result = mapper.ToFilter(dto);

        Assert.Equal(QueryOperator.IsNotNull, result.Operator);
    }

    [Fact]
    public void NullValue_NonNullOperator_ThrowsFilterMappingException()
    {
        DictFilterMapper mapper = MakeMapper();
        var dto = new Contracts.SimpleFilter("Age", FilterOperators.EqualTo);
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains(FilterOperators.EqualTo, ex.Message);
    }

    [Fact]
    public void UnknownField_ThrowsFilterMappingException()
    {
        DictFilterMapper mapper = MakeMapper();
        var dto = new Contracts.SimpleFilter("UnknownField", FilterOperators.EqualTo, "x");
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("UnknownField", ex.Message);
    }

    [Fact]
    public void CompositeFilter_InnerFiltersUseDictParsers()
    {
        DictFilterMapper mapper = MakeMapper();
        var dto = new Contracts.CompositeFilter(LogicalOperators.And,
        [
            new Contracts.SimpleFilter("Name", FilterOperators.EqualTo, "Alice"),
                new Contracts.SimpleFilter("Age", FilterOperators.GreaterThan, "25"),
            ]);

        CompositeFilter result = mapper.ToFilter(dto);

        SimpleFilter nameFilter = Assert.IsType<SimpleFilter>(result.Filters[0]);
        SimpleFilter ageFilter = Assert.IsType<SimpleFilter>(result.Filters[1]);
        Assert.Equal("Alice", nameFilter.Value);
        Assert.Equal(25, ageFilter.Value);
    }

    [Fact]
    public void DynamicDataFilter_WithFilter_Maps()
    {
        DictFilterMapper mapper = MakeMapper();
        var dto = new Contracts.DynamicDataRequest
        {
            RequestedFields = ["Name", "Age"],
            Filter = new Contracts.SimpleFilter("Name", FilterOperators.Contains, "Smith"),
            Sorts = [new Contracts.SortComponent("Age", SortDirections.Descending)],
            Page = 2,
            PageSize = 50,
        };

        DynamicDataRequest result = mapper.ToDataRequest(dto);

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
        DictFilterMapper mapper = MakeMapper();
        var dto = new Contracts.DynamicDataRequest
        {
            RequestedFields = ["Salary"],
            Filter = null,
            Sorts = [],
            Page = 1,
            PageSize = 25,
        };

        DynamicDataRequest result = mapper.ToDataRequest(dto);

        Assert.Equal(["Salary"], result.RequestedFields);
        Assert.Null(result.Filter);
        Assert.Empty(result.Sorts);
        Assert.Equal(1, result.Page);
        Assert.Equal(25, result.PageSize);
    }

    // -------------------------------------------------------------------------
    // Invalid value throws parsing error tests
    // -------------------------------------------------------------------------

    [Fact]
    public void InvalidInt_ThrowsFilterMappingException()
    {
        var mapper = new DictFilterMapper(new Dictionary<string, Func<string, object>>
        {
            ["Age"] = v => int.Parse(v),
        });

        var dto = new Contracts.SimpleFilter("Age", FilterOperators.EqualTo, "not-a-number");
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("Age", ex.Message);
        Assert.Contains("not-a-number", ex.Message);
        Assert.IsType<FormatException>(ex.InnerException);
    }

    [Fact]
    public void InvalidDecimal_ThrowsFilterMappingException()
    {
        var mapper = new DictFilterMapper(new Dictionary<string, Func<string, object>>
        {
            ["Salary"] = v => decimal.Parse(v, System.Globalization.CultureInfo.InvariantCulture),
        });

        var dto = new Contracts.SimpleFilter("Salary", FilterOperators.EqualTo, "abc");
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("Salary", ex.Message);
        Assert.IsType<FormatException>(ex.InnerException);
    }

    // -------------------------------------------------------------------------
    // ToFilter dotted-field navigation
    // -------------------------------------------------------------------------

    [Fact]
    public void DottedCountField_ParsesValueAsInt()
    {
        var mapper = new DictFilterMapper(new Dictionary<string, Func<string, object>>
        {
            ["Friends"] = v => v,
        });
        var dto = new Contracts.SimpleFilter("Friends.Count", FilterOperators.GreaterThan, "5");
        SimpleFilter result = mapper.ToFilter(dto);

        Assert.Equal(5, result.Value);
        Assert.IsType<int>(result.Value);
    }

    [Fact]
    public void DottedLengthField_ParsesValueAsInt()
    {
        var mapper = new DictFilterMapper(new Dictionary<string, Func<string, object>>
        {
            ["Tags"] = v => v,
        });
        var dto = new Contracts.SimpleFilter("Tags.Length", FilterOperators.EqualTo, "3");
        SimpleFilter result = mapper.ToFilter(dto);

        Assert.Equal(3, result.Value);
        Assert.IsType<int>(result.Value);
    }

    [Fact]
    public void DottedCountField_UnknownRoot_ThrowsFilterMappingException()
    {
        var mapper = new DictFilterMapper(new Dictionary<string, Func<string, object>>
        {
            ["Name"] = v => v,
        });
        var dto = new Contracts.SimpleFilter("Enemies.Count", FilterOperators.GreaterThan, "0");
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("Enemies.Count", ex.Message);
    }

    [Fact]
    public void DottedCountField_IsNull_KnownRoot_Passes()
    {
        var mapper = new DictFilterMapper(new Dictionary<string, Func<string, object>>
        {
            ["Friends"] = v => v,
        });
        var dto = new Contracts.SimpleFilter("Friends.Count", FilterOperators.IsNull);
        SimpleFilter result = mapper.ToFilter(dto);

        Assert.Equal(QueryOperator.IsNull, result.Operator);
    }


    // -------------------------------------------------------------------------
    // Sorting Tests
    // -------------------------------------------------------------------------

    [Fact]
    public void Sort_KnownField_Passes()
    {
        DictFilterMapper mapper = MakeMapper();
        var dto = new Contracts.SortComponent("Name", SortDirections.Ascending);
        SortComponent result = mapper.ToSortComponent(dto);

        Assert.Equal("Name", result.Field);
        Assert.Equal(SortDirection.Ascending, result.Direction);
    }

    [Fact]
    public void Sort_UnknownField_ThrowsFilterMappingException()
    {
        DictFilterMapper mapper = MakeMapper();
        var dto = new Contracts.SortComponent("UnknownField", SortDirections.Ascending);
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToSortComponent(dto));
        Assert.Contains("UnknownField", ex.Message);
    }

    [Fact]
    public void Sort_DottedField_Count_KnownRoot_Passes()
    {
        DictFilterMapper mapper = MakeMapper();
        var dto = new Contracts.SortComponent("Friends.Count", SortDirections.Ascending);
        SortComponent result = mapper.ToSortComponent(dto);

        Assert.Equal("Friends.Count", result.Field);
    }

    [Fact]
    public void Sort_DottedField_Length_KnownRoot_Passes()
    {
        DictFilterMapper mapper = MakeMapper();
        var dto = new Contracts.SortComponent("Friends.Length", SortDirections.Ascending);
        SortComponent result = mapper.ToSortComponent(dto);

        Assert.Equal("Friends.Length", result.Field);
    }

    [Fact]
    public void Sort_DottedField_UnknownRoot_ThrowsFilterMappingException()
    {
        DictFilterMapper mapper = MakeMapper();
        var dto = new Contracts.SortComponent("Enemies.Count", SortDirections.Ascending);
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToSortComponent(dto));
        Assert.Contains("Enemies.Count", ex.Message);
    }

    [Fact]
    public void Sort_DottedField_InvalidSuffix_ThrowsFilterMappingException()
    {
        DictFilterMapper mapper = MakeMapper();
        var dto = new Contracts.SortComponent("Friends.Name", SortDirections.Ascending);
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToSortComponent(dto));
        Assert.Contains("Friends.Name", ex.Message);
    }

    [Fact]
    public void Sort_DottedField_TooManyParts_ThrowsFilterMappingException()
    {
        DictFilterMapper mapper = MakeMapper();
        var dto = new Contracts.SortComponent("A.B.Count", SortDirections.Ascending);
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToSortComponent(dto));
        Assert.Contains("A.B.Count", ex.Message);
    }


    // -------------------------------------------------------------------------
    // SetFilter Mapping
    // -------------------------------------------------------------------------

    [Fact]
    public void SetFilter_StringValues_MapsCorrectly()
    {
        var mapper = new DictFilterMapper(new Dictionary<string, Func<string, object>>
        {
            ["Name"] = v => v,
        });
        var dto = new Contracts.SetFilter("Name", ["Alice", "Bob"]);

        SetFilter result = mapper.ToFilter(dto);

        Assert.Equal("Alice", result.Values[0]);
        Assert.Equal("Bob", result.Values[1]);
        Assert.IsType<string>(result.Values[0]);
    }

    [Fact]
    public void SetFilter_IntValues_ParsesElements()
    {
        var mapper = new DictFilterMapper(new Dictionary<string, Func<string, object>>
        {
            ["Age"] = v => int.Parse(v),
        });
        var dto = new Contracts.SetFilter("Age", ["25", "30", "35"]);

        SetFilter result = mapper.ToFilter(dto);

        Assert.Equal(3, result.Values.Length);
        Assert.Equal(25, result.Values[0]);
        Assert.Equal(30, result.Values[1]);
        Assert.Equal(35, result.Values[2]);
    }

    [Fact]
    public void SetFilter_InvalidValue_ThrowsFilterMappingException()
    {
        var mapper = new DictFilterMapper(new Dictionary<string, Func<string, object>>
        {
            ["Age"] = v => int.Parse(v),
        });
        var dto = new Contracts.SetFilter("Age", ["25", "not-a-number"]);

        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("Age", ex.Message);
        Assert.Contains("not-a-number", ex.Message);
        Assert.IsType<FormatException>(ex.InnerException);
    }

    [Fact]
    public void SetFilter_UnknownField_Throws()
    {
        var mapper = new DictFilterMapper(new Dictionary<string, Func<string, object>>
        {
            ["Name"] = v => v,
        });
        var dto = new Contracts.SetFilter("Nonexistent", ["A", "B"]);

        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("Nonexistent", ex.Message);
    }

    [Fact]
    public void SetFilter_EmptyValues_Throws()
    {
        var mapper = new DictFilterMapper(new Dictionary<string, Func<string, object>>
        {
            ["Name"] = v => v,
        });
        var dto = new Contracts.SetFilter("Name", []);

        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("Name", ex.Message);
    }

    [Fact]
    public void SetFilter_NullableIntField_WithTypeMapping_StoresElementType()
    {
        var mapper = new DictFilterMapper(
            new Dictionary<string, Func<string, object>>
            {
                ["Score"] = v => int.Parse(v),
            },
            new Dictionary<string, Type>
            {
                ["Score"] = typeof(int?),
            }
        );
        var dto = new Contracts.SetFilter("Score", ["85", "90", "95"]);

        SetFilter result = mapper.ToFilter(dto);

        Assert.Equal(3, result.Values.Length);
        Assert.Equal(85, result.Values[0]);
        Assert.Equal(90, result.Values[1]);
        Assert.Equal(95, result.Values[2]);
        Assert.NotNull(result.ElementType);
        Assert.Equal(typeof(int?), result.ElementType);
    }

    [Fact]
    public void SetFilter_NullableIntField_WithoutTypeMapping_NoElementType()
    {
        var mapper = new DictFilterMapper(new Dictionary<string, Func<string, object>>
        {
            ["Score"] = v => int.Parse(v),
        });
        var dto = new Contracts.SetFilter("Score", ["80", "85"]);

        SetFilter result = mapper.ToFilter(dto);

        Assert.Equal(2, result.Values.Length);
        Assert.Null(result.ElementType);
    }

    // -------------------------------------------------------------------------
    // RangeFilter Mapping
    // -------------------------------------------------------------------------

    [Fact]
    public void RangeFilter_ParsesBothBounds()
    {
        var mapper = new DictFilterMapper(new Dictionary<string, Func<string, object>>
        {
            ["Age"] = v => int.Parse(v),
        });
        var dto = new Contracts.RangeFilter("Age", "27", "31");

        RangeFilter result = mapper.ToFilter(dto);

        Assert.Equal(27, result.ValueFrom);
        Assert.Equal(31, result.ValueTo);
    }

    [Fact]
    public void RangeFilter_InvalidValueFrom_ThrowsFilterMappingException()
    {
        var mapper = new DictFilterMapper(new Dictionary<string, Func<string, object>>
        {
            ["Age"] = v => int.Parse(v),
        });
        var dto = new Contracts.RangeFilter("Age", "not-a-number", "31");

        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("Age", ex.Message);
        Assert.Contains("not-a-number", ex.Message);
        Assert.IsType<FormatException>(ex.InnerException);
    }

    [Fact]
    public void RangeFilter_InvalidValueTo_ThrowsFilterMappingException()
    {
        var mapper = new DictFilterMapper(new Dictionary<string, Func<string, object>>
        {
            ["Age"] = v => int.Parse(v),
        });
        var dto = new Contracts.RangeFilter("Age", "27", "not-a-number");

        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("Age", ex.Message);
        Assert.Contains("not-a-number", ex.Message);
        Assert.IsType<FormatException>(ex.InnerException);
    }

    [Fact]
    public void RangeFilter_UnknownField_Throws()
    {
        var mapper = new DictFilterMapper(new Dictionary<string, Func<string, object>>
        {
            ["Age"] = v => int.Parse(v),
        });
        var dto = new Contracts.RangeFilter("Salary", "1000", "5000");

        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("Salary", ex.Message);
    }
}
