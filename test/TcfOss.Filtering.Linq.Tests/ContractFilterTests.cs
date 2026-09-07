using Newtonsoft.Json;
using TcfOss.Filtering.Contracts.Newtonsoft;
using STJ = System.Text.Json;

namespace TcfOss.Filtering.Linq.Tests;

public class ContractFilterTests
{
    [Fact]
    public void SimpleFilter_FilterType()
    {
        var filter = new Contracts.SimpleFilter("Field", "eq", "Value");
        Assert.Equal(Contracts.FilterTypes.Simple, filter.FilterType);
    }

    [Fact]
    public void CompositeFilter_FilterType()
    {
        var filter = new Contracts.CompositeFilter
        {
            LogicalOperator = Contracts.LogicalOperators.And,
            Filters =
            [
                new Contracts.SimpleFilter("Field1", "eq", "Value1"),
                new Contracts.SimpleFilter("Field2", "eq", "Value2")
            ]
        };
        Assert.Equal(Contracts.FilterTypes.Composite, filter.FilterType);
    }

    // -------------------------------------------------------------------------
    // Shared JSON test data
    // -------------------------------------------------------------------------

    private static readonly STJ.JsonSerializerOptions s_stjOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly JsonSerializerSettings s_nsSettings = new()
    {
        Converters = { new FilterConverter() },
    };

    private const string SimpleFilterJson = """
    {
        "filter": {
             "filterType": "simple",
             "field": "Name",
             "operator": "eq",
             "value": "Alice"
        },
        "sorts": [
            {
                "field": "Age",
                "direction": "asc"
            }
        ],
        "page": 2,
        "pageSize": 50
    }
    """;

    private const string CompositeFilterJson = """
    {
        "filter": {
            "filterType": "composite",
            "logicalOperator": "and",
            "filters": [
                {
                    "filterType": "simple",
                    "field": "Name",
                    "operator": "eq",
                    "value": "Alice"
                },
                {
                    "filterType": "simple",
                    "field": "Age",
                    "operator": "gt",
                    "value": "30"
                }
            ]
        },
        "sorts": [
            {
                "field": "Age",
                "direction": "desc"
            }
        ],
        "page": 2,
        "pageSize": 50
    }
    """;

    // -------------------------------------------------------------------------
    // System.Text.Json round-trip
    // -------------------------------------------------------------------------

    [Fact]
    public void SystemTextJson_SimpleFilter_DeserializesCorrectly()
    {
        Contracts.DataRequest? dto = STJ.JsonSerializer.Deserialize<Contracts.DataRequest>(SimpleFilterJson, s_stjOptions);

        Assert.NotNull(dto);
        Contracts.SimpleFilter filter = Assert.IsType<Contracts.SimpleFilter>(dto.Filter);
        Assert.Equal("Name", filter.Field);
        Assert.Equal(Contracts.FilterOperators.EqualTo, filter.Operator);
        Assert.Equal("Alice", filter.Value);
        Assert.Single(dto.Sorts);
        Assert.Equal(2, dto.Page);
        Assert.Equal(50, dto.PageSize);
    }

    [Fact]
    public void SystemTextJson_SimpleFilter_SerializesFilterType()
    {
        Contracts.Filter filter = new Contracts.SimpleFilter("Name", Contracts.FilterOperators.EqualTo, "Alice");

        string json = STJ.JsonSerializer.Serialize(filter, s_stjOptions);

        Assert.Contains("\"filterType\":\"simple\"", json);
        Assert.DoesNotContain("\"type\":", json);
    }

    [Fact]
    public void SystemTextJson_CompositeFilter_DeserializesCorrectly()
    {
        Contracts.DataRequest? dto = STJ.JsonSerializer.Deserialize<Contracts.DataRequest>(CompositeFilterJson, s_stjOptions);

        Assert.NotNull(dto);
        Contracts.CompositeFilter filter = Assert.IsType<Contracts.CompositeFilter>(dto.Filter);
        Assert.Equal(Contracts.LogicalOperators.And, filter.LogicalOperator);
        Assert.Equal(2, filter.Filters.Length);
        Assert.Single(dto.Sorts);
        Assert.Equal(2, dto.Page);
        Assert.Equal(50, dto.PageSize);
    }

    [Fact]
    public void SystemTextJson_UnknownFilterType_Throws()
    {
        string json = """{"filter":{"filterType":"bogus","field":"X","operator":"eq"}}""";
        Assert.Throws<Contracts.FilterDeserializationException>(
            () => STJ.JsonSerializer.Deserialize<Contracts.DataRequest>(json, s_stjOptions));
    }

    [Fact]
    public void SystemTextJson_MissingOperator_Throws()
    {
        string json = """{"filter":{"filterType":"simple","field":"X","value":"Y"}}""";
        Assert.Throws<Contracts.FilterDeserializationException>(
            () => STJ.JsonSerializer.Deserialize<Contracts.DataRequest>(json, s_stjOptions));
    }

    // -------------------------------------------------------------------------
    // Newtonsoft.Json round-trip
    // -------------------------------------------------------------------------

    [Fact]
    public void Newtonsoft_SimpleFilter_DeserializesCorrectly()
    {
        Contracts.DataRequest? dto = JsonConvert.DeserializeObject<Contracts.DataRequest>(SimpleFilterJson, s_nsSettings);

        Assert.NotNull(dto);
        Contracts.SimpleFilter filter = Assert.IsType<Contracts.SimpleFilter>(dto.Filter);
        Assert.Equal("Name", filter.Field);
        Assert.Equal(Contracts.FilterOperators.EqualTo, filter.Operator);
        Assert.Equal("Alice", filter.Value);
        Assert.Single(dto.Sorts);
        Assert.Equal(2, dto.Page);
        Assert.Equal(50, dto.PageSize);
    }

    [Fact]
    public void Newtonsoft_CompositeFilter_DeserializesCorrectly()
    {
        Contracts.DataRequest? dto = JsonConvert.DeserializeObject<Contracts.DataRequest>(CompositeFilterJson, s_nsSettings);

        Assert.NotNull(dto);
        Contracts.CompositeFilter filter = Assert.IsType<Contracts.CompositeFilter>(dto.Filter);
        Assert.Equal(Contracts.LogicalOperators.And, filter.LogicalOperator);
        Assert.Equal(2, filter.Filters.Length);
        Assert.Single(dto.Sorts);
        Assert.Equal(2, dto.Page);
        Assert.Equal(50, dto.PageSize);
    }

    [Fact]
    public void Newtonsoft_UnknownFilterType_Throws()
    {
        string json = """{"filter":{"filterType":"bogus","field":"X","operator":"eq"}}""";
        Assert.Throws<Contracts.FilterDeserializationException>(
            () => JsonConvert.DeserializeObject<Contracts.DataRequest>(json, s_nsSettings));
    }

    [Fact]
    public void Newtonsoft_MissingOperator_Throws()
    {
        string json = """{"filter":{"filterType":"simple","field":"X","value":"Y"}}""";
        Assert.Throws<Contracts.FilterDeserializationException>(
            () => JsonConvert.DeserializeObject<Contracts.DataRequest>(json, s_nsSettings));
    }

    // -------------------------------------------------------------------------
    // SetFilter serialisation
    // -------------------------------------------------------------------------

    private const string SetFilterJson = """
    {
        "filter": {
            "filterType": "set",
            "field": "Name",
            "values": ["Alice", "Bob", "Charlie"]
        },
        "sorts": [],
        "page": 1,
        "pageSize": 25
    }
    """;

    private const string SetFilterNegatedJson = """
    {
        "filter": {
            "filterType": "set",
            "field": "Name",
            "values": ["Alice"],
            "negated": true
        },
        "sorts": [],
        "page": 1,
        "pageSize": 25
    }
    """;

    [Fact]
    public void SystemTextJson_SetFilter_DeserializesCorrectly()
    {
        Contracts.DataRequest? dto = STJ.JsonSerializer.Deserialize<Contracts.DataRequest>(SetFilterJson, s_stjOptions);

        Assert.NotNull(dto);
        Contracts.SetFilter filter = Assert.IsType<Contracts.SetFilter>(dto.Filter);
        Assert.Equal("Name", filter.Field);
        Assert.Equal(["Alice", "Bob", "Charlie"], filter.Values);
        Assert.False(filter.Negated);
    }

    [Fact]
    public void SystemTextJson_SetFilter_Negated_DeserializesCorrectly()
    {
        Contracts.DataRequest? dto = STJ.JsonSerializer.Deserialize<Contracts.DataRequest>(SetFilterNegatedJson, s_stjOptions);

        Assert.NotNull(dto);
        Contracts.SetFilter filter = Assert.IsType<Contracts.SetFilter>(dto.Filter);
        Assert.True(filter.Negated);
    }

    [Fact]
    public void SystemTextJson_SetFilter_MissingField_Throws()
    {
        string json = """{"filter":{"filterType":"set","values":["A"]}}""";
        Assert.Throws<Contracts.FilterDeserializationException>(
            () => STJ.JsonSerializer.Deserialize<Contracts.DataRequest>(json, s_stjOptions));
    }

    [Fact]
    public void Newtonsoft_SetFilter_DeserializesCorrectly()
    {
        Contracts.DataRequest? dto = JsonConvert.DeserializeObject<Contracts.DataRequest>(SetFilterJson, s_nsSettings);

        Assert.NotNull(dto);
        Contracts.SetFilter filter = Assert.IsType<Contracts.SetFilter>(dto.Filter);
        Assert.Equal("Name", filter.Field);
        Assert.Equal(["Alice", "Bob", "Charlie"], filter.Values);
        Assert.False(filter.Negated);
    }

    [Fact]
    public void Newtonsoft_SetFilter_Negated_DeserializesCorrectly()
    {
        Contracts.DataRequest? dto = JsonConvert.DeserializeObject<Contracts.DataRequest>(SetFilterNegatedJson, s_nsSettings);

        Assert.NotNull(dto);
        Contracts.SetFilter filter = Assert.IsType<Contracts.SetFilter>(dto.Filter);
        Assert.True(filter.Negated);
    }

    [Fact]
    public void Newtonsoft_SetFilter_MissingField_Throws()
    {
        string json = """{"filter":{"filterType":"set","values":["A"]}}""";
        Assert.Throws<Contracts.FilterDeserializationException>(
            () => JsonConvert.DeserializeObject<Contracts.DataRequest>(json, s_nsSettings));
    }

    [Fact]
    public void Newtonsoft_SetFilter_MissingValues_Throws()
    {
        string json = """{"filter":{"filterType":"set","field":"Name"}}""";
        Assert.Throws<Contracts.FilterDeserializationException>(
            () => JsonConvert.DeserializeObject<Contracts.DataRequest>(json, s_nsSettings));
    }

    // -------------------------------------------------------------------------
    // RangeFilter serialisation
    // -------------------------------------------------------------------------

    private const string RangeFilterJson = """
    {
        "filter": {
            "filterType": "range",
            "field": "Age",
            "valueFrom": "27",
            "valueTo": "31"
        },
        "sorts": [],
        "page": 1,
        "pageSize": 25
    }
    """;

    private const string RangeFilterExclusiveNegatedJson = """
    {
        "filter": {
            "filterType": "range",
            "field": "Age",
            "valueFrom": "27",
            "valueTo": "31",
            "exclusive": true,
            "negated": true
        },
        "sorts": [],
        "page": 1,
        "pageSize": 25
    }
    """;

    [Fact]
    public void SystemTextJson_RangeFilter_DeserializesCorrectly()
    {
        Contracts.DataRequest? dto = STJ.JsonSerializer.Deserialize<Contracts.DataRequest>(RangeFilterJson, s_stjOptions);

        Assert.NotNull(dto);
        Contracts.RangeFilter filter = Assert.IsType<Contracts.RangeFilter>(dto.Filter);
        Assert.Equal("Age", filter.Field);
        Assert.Equal("27", filter.ValueFrom);
        Assert.Equal("31", filter.ValueTo);
        Assert.False(filter.Exclusive);
        Assert.False(filter.Negated);
    }

    [Fact]
    public void SystemTextJson_RangeFilter_ExclusiveNegated_DeserializesCorrectly()
    {
        Contracts.DataRequest? dto = STJ.JsonSerializer.Deserialize<Contracts.DataRequest>(RangeFilterExclusiveNegatedJson, s_stjOptions);

        Assert.NotNull(dto);
        Contracts.RangeFilter filter = Assert.IsType<Contracts.RangeFilter>(dto.Filter);
        Assert.True(filter.Exclusive);
        Assert.True(filter.Negated);
    }

    [Fact]
    public void Newtonsoft_RangeFilter_DeserializesCorrectly()
    {
        Contracts.DataRequest? dto = JsonConvert.DeserializeObject<Contracts.DataRequest>(RangeFilterJson, s_nsSettings);

        Assert.NotNull(dto);
        Contracts.RangeFilter filter = Assert.IsType<Contracts.RangeFilter>(dto.Filter);
        Assert.Equal("Age", filter.Field);
        Assert.Equal("27", filter.ValueFrom);
        Assert.Equal("31", filter.ValueTo);
        Assert.False(filter.Exclusive);
        Assert.False(filter.Negated);
    }

    [Fact]
    public void Newtonsoft_RangeFilter_ExclusiveNegated_DeserializesCorrectly()
    {
        Contracts.DataRequest? dto = JsonConvert.DeserializeObject<Contracts.DataRequest>(RangeFilterExclusiveNegatedJson, s_nsSettings);

        Assert.NotNull(dto);
        Contracts.RangeFilter filter = Assert.IsType<Contracts.RangeFilter>(dto.Filter);
        Assert.True(filter.Exclusive);
        Assert.True(filter.Negated);
    }

    [Fact]
    public void Newtonsoft_RangeFilter_MissingField_Throws()
    {
        string json = """{"filter":{"filterType":"range","valueFrom":"1","valueTo":"10"}}""";
        Assert.Throws<Contracts.FilterDeserializationException>(
            () => JsonConvert.DeserializeObject<Contracts.DataRequest>(json, s_nsSettings));
    }

    [Fact]
    public void Newtonsoft_RangeFilter_MissingValueFrom_Throws()
    {
        string json = """{"filter":{"filterType":"range","field":"Age","valueTo":"10"}}""";
        Assert.Throws<Contracts.FilterDeserializationException>(
            () => JsonConvert.DeserializeObject<Contracts.DataRequest>(json, s_nsSettings));
    }

    [Fact]
    public void Newtonsoft_RangeFilter_MissingValueTo_Throws()
    {
        string json = """{"filter":{"filterType":"range","field":"Age","valueFrom":"1"}}""";
        Assert.Throws<Contracts.FilterDeserializationException>(
            () => JsonConvert.DeserializeObject<Contracts.DataRequest>(json, s_nsSettings));
    }

    [Fact]
    public void RangeFilter_FilterType()
    {
        var filter = new Contracts.RangeFilter("Age", "27", "31");
        Assert.Equal(Contracts.FilterTypes.Range, filter.FilterType);
    }

    // -------------------------------------------------------------------------
    // QuantifiedFilter serialisation
    // -------------------------------------------------------------------------

    private const string QuantifiedFilterJson = """
    {
        "filter": {
            "filterType": "quantified",
            "field": "Tags",
            "operator": "any",
            "subFilter": {
                "filterType": "simple",
                "field": "Value",
                "operator": "eq",
                "value": "csharp"
            }
        },
        "sorts": [],
        "page": 1,
        "pageSize": 25
    }
    """;

    private const string QuantifiedFilterNegatedNullableJson = """
    {
        "filter": {
            "filterType": "quantified",
            "field": "Tags",
            "operator": "all",
            "subFilter": {
                "filterType": "simple",
                "field": "Value",
                "operator": "eq",
                "value": "csharp"
            },
            "isNegated": true,
            "isNullable": true
        },
        "sorts": [],
        "page": 1,
        "pageSize": 25
    }
    """;

    [Fact]
    public void QuantifiedFilter_FilterType()
    {
        var filter = new Contracts.QuantifiedFilter
        {
            Field = "Tags",
            Operator = Contracts.QuantifiedOperators.Any,
            SubFilter = new Contracts.SimpleFilter("Value", Contracts.FilterOperators.EqualTo, "csharp"),
        };
        Assert.Equal(Contracts.FilterTypes.Quantified, filter.FilterType);
    }

    [Fact]
    public void SystemTextJson_QuantifiedFilter_DeserializesCorrectly()
    {
        Contracts.DataRequest? dto = STJ.JsonSerializer.Deserialize<Contracts.DataRequest>(QuantifiedFilterJson, s_stjOptions);

        Assert.NotNull(dto);
        Contracts.QuantifiedFilter filter = Assert.IsType<Contracts.QuantifiedFilter>(dto.Filter);
        Assert.Equal("Tags", filter.Field);
        Assert.Equal(Contracts.QuantifiedOperators.Any, filter.Operator);
        Assert.IsType<Contracts.SimpleFilter>(filter.SubFilter);
        Assert.False(filter.IsNegated);
        Assert.False(filter.IsNullable);
    }

    [Fact]
    public void SystemTextJson_QuantifiedFilter_NegatedNullable_DeserializesCorrectly()
    {
        Contracts.DataRequest? dto = STJ.JsonSerializer.Deserialize<Contracts.DataRequest>(QuantifiedFilterNegatedNullableJson, s_stjOptions);

        Assert.NotNull(dto);
        Contracts.QuantifiedFilter filter = Assert.IsType<Contracts.QuantifiedFilter>(dto.Filter);
        Assert.Equal(Contracts.QuantifiedOperators.All, filter.Operator);
        Assert.True(filter.IsNegated);
        Assert.True(filter.IsNullable);
    }

    [Fact]
    public void SystemTextJson_QuantifiedFilter_MissingField_Throws()
    {
        string json = """{"filter":{"filterType":"quantified","operator":"any","subFilter":{"filterType":"simple","field":"Value","operator":"eq","value":"x"}}}""";
        Assert.Throws<Contracts.FilterDeserializationException>(
            () => STJ.JsonSerializer.Deserialize<Contracts.DataRequest>(json, s_stjOptions));
    }

    [Fact]
    public void SystemTextJson_QuantifiedFilter_MissingOperator_Throws()
    {
        string json = """{"filter":{"filterType":"quantified","field":"Tags","subFilter":{"filterType":"simple","field":"Value","operator":"eq","value":"x"}}}""";
        Assert.Throws<Contracts.FilterDeserializationException>(
            () => STJ.JsonSerializer.Deserialize<Contracts.DataRequest>(json, s_stjOptions));
    }

    [Fact]
    public void SystemTextJson_QuantifiedFilter_MissingSubFilter_Throws()
    {
        string json = """{"filter":{"filterType":"quantified","field":"Tags","operator":"any"}}""";
        Assert.Throws<Contracts.FilterDeserializationException>(
            () => STJ.JsonSerializer.Deserialize<Contracts.DataRequest>(json, s_stjOptions));
    }

    [Fact]
    public void Newtonsoft_QuantifiedFilter_DeserializesCorrectly()
    {
        Contracts.DataRequest? dto = JsonConvert.DeserializeObject<Contracts.DataRequest>(QuantifiedFilterJson, s_nsSettings);

        Assert.NotNull(dto);
        Contracts.QuantifiedFilter filter = Assert.IsType<Contracts.QuantifiedFilter>(dto.Filter);
        Assert.Equal("Tags", filter.Field);
        Assert.Equal(Contracts.QuantifiedOperators.Any, filter.Operator);
        Assert.IsType<Contracts.SimpleFilter>(filter.SubFilter);
        Assert.False(filter.IsNegated);
        Assert.False(filter.IsNullable);
    }

    [Fact]
    public void Newtonsoft_QuantifiedFilter_NegatedNullable_DeserializesCorrectly()
    {
        Contracts.DataRequest? dto = JsonConvert.DeserializeObject<Contracts.DataRequest>(QuantifiedFilterNegatedNullableJson, s_nsSettings);

        Assert.NotNull(dto);
        Contracts.QuantifiedFilter filter = Assert.IsType<Contracts.QuantifiedFilter>(dto.Filter);
        Assert.Equal(Contracts.QuantifiedOperators.All, filter.Operator);
        Assert.True(filter.IsNegated);
        Assert.True(filter.IsNullable);
    }

    [Fact]
    public void Newtonsoft_QuantifiedFilter_MissingField_Throws()
    {
        string json = """{"filter":{"filterType":"quantified","operator":"any","subFilter":{"filterType":"simple","field":"Value","operator":"eq","value":"x"}}}""";
        Assert.Throws<Contracts.FilterDeserializationException>(
            () => JsonConvert.DeserializeObject<Contracts.DataRequest>(json, s_nsSettings));
    }

    [Fact]
    public void Newtonsoft_QuantifiedFilter_MissingOperator_Throws()
    {
        string json = """{"filter":{"filterType":"quantified","field":"Tags","subFilter":{"filterType":"simple","field":"Value","operator":"eq","value":"x"}}}""";
        Assert.Throws<Contracts.FilterDeserializationException>(
            () => JsonConvert.DeserializeObject<Contracts.DataRequest>(json, s_nsSettings));
    }

    [Fact]
    public void Newtonsoft_QuantifiedFilter_MissingSubFilter_Throws()
    {
        string json = """{"filter":{"filterType":"quantified","field":"Tags","operator":"any"}}""";
        Assert.Throws<Contracts.FilterDeserializationException>(
            () => JsonConvert.DeserializeObject<Contracts.DataRequest>(json, s_nsSettings));
    }
}
