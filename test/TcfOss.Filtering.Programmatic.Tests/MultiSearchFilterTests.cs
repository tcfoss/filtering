using TcfOss.Filtering.Contracts;

namespace TcfOss.Filtering.Programmatic.Tests;

public class MultiSearchFilterTests
{
    [Fact]
    public void ToContractFilter_EmptyValues_ReturnsNull()
    {
        var filter = new MultiSearchFilter<string>("Tags");

        Filter? result = filter.ToContractFilter(new MappingOptions());

        Assert.Null(result);
    }

    [Fact]
    public void ToContractFilter_ConstructorValues_MapsAllValuesToStrings()
    {
        var filter = new MultiSearchFilter<int>("Age", [10, 20, 30]);

        Filter? result = filter.ToContractFilter(new MappingOptions());

        SetFilter setFilter = Assert.IsType<SetFilter>(result);
        Assert.Equal("Age", setFilter.Field);
        Assert.Equal(["10", "20", "30"], setFilter.Values);
    }

    [Fact]
    public void ToContractFilter_DefaultOptions_OmitNullAndEmptyValues()
    {
        var filter = new MultiSearchFilter<string?>("Name") { Values = ["Alice", null, string.Empty, "Bob"] };

        Filter? result = filter.ToContractFilter(new MappingOptions());

        SetFilter setFilter = Assert.IsType<SetFilter>(result);
        Assert.Equal(["Alice", "Bob"], setFilter.Values);
    }

    [Fact]
    public void ToContractFilter_OmitNullsDisabledAndOmitEmptyStringsDisabled_MapsNullToEmptyString()
    {
        var filter = new MultiSearchFilter<string?>("Name") { Values = ["Alice", null, "Bob"] };
        MappingOptions options = new()
        {
            MultiSearchOmitNulls = false,
            MultiSearchOmitEmptyStrings = false
        };

        Filter? result = filter.ToContractFilter(options);

        SetFilter setFilter = Assert.IsType<SetFilter>(result);
        Assert.Equal(["Alice", string.Empty, "Bob"], setFilter.Values);
    }

    [Fact]
    public void ToContractFilter_OmitEmptyStringsEnabled_RemovesEmptyStringValues()
    {
        var filter = new MultiSearchFilter<string>("Name") { Values = ["Alice", string.Empty, "Bob"] };
        MappingOptions options = new()
        {
            MultiSearchOmitNulls = false,
            MultiSearchOmitEmptyStrings = true
        };

        Filter? result = filter.ToContractFilter(options);

        SetFilter setFilter = Assert.IsType<SetFilter>(result);
        Assert.Equal(["Alice", "Bob"], setFilter.Values);
    }

    [Fact]
    public void ToContractFilter_AllValuesOmittedByOptions_ReturnsNull()
    {
        var filter = new MultiSearchFilter<string?>("Name") { Values = [null, string.Empty] };

        Filter? result = filter.ToContractFilter(new MappingOptions());

        Assert.Null(result);
    }

    [Fact]
    public void ToContractFilter_Negated_MapsNegatedToSetFilter()
    {
        var filter = new MultiSearchFilter<string>("Department")
        {
            Values = ["Engineering", "Sales"],
            Negated = true
        };

        Filter? result = filter.ToContractFilter(new MappingOptions());

        SetFilter setFilter = Assert.IsType<SetFilter>(result);
        Assert.True(setFilter.Negated);
        Assert.Equal(["Engineering", "Sales"], setFilter.Values);
    }
}
