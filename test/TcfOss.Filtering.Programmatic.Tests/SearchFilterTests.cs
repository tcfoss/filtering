using TcfOss.Filtering.Contracts;
using TcfOss.Filtering.Programmatic.DataTypes;

namespace TcfOss.Filtering.Programmatic.Tests;

public class SearchFilterTests
{
    [Fact]
    public void ToContractFilter_NullValueWithoutNullOption_ReturnsNull()
    {
        var filter = new SearchFilter<string>("Name");
        MappingOptions options = new();

        Filter? result = filter.ToContractFilter(options);

        Assert.Null(result);
    }

    [Fact]
    public void ToContractFilter_NullValueWithNullOption_ReturnsIsNullFilter()
    {
        var filter = new SearchFilter<string>("Name");
        MappingOptions options = new() { NullValueToIsNullFilter = true };

        Filter? result = filter.ToContractFilter(options);

        SimpleFilter simple = Assert.IsType<SimpleFilter>(result);
        Assert.Equal("Name", simple.Field);
        Assert.Equal(FilterOperators.IsNull, simple.Operator);
        Assert.Null(simple.Value);
    }

    [Fact]
    public void ToContractFilter_StringNoWildcard_ReturnsEqualTo()
    {
        var filter = new SearchFilter<string>("Name") { Value = "Alice" };

        Filter? result = filter.ToContractFilter(new MappingOptions());

        SimpleFilter simple = Assert.IsType<SimpleFilter>(result);
        Assert.Equal(FilterOperators.EqualTo, simple.Operator);
        Assert.Equal("Alice", simple.Value);
    }

    [Theory]
    [InlineData("%Ali%", FilterOperators.Contains, "Ali")]
    [InlineData("%Ali", FilterOperators.EndsWith, "Ali")]
    [InlineData("Ali%", FilterOperators.StartsWith, "Ali")]
    public void ToContractFilter_StringBoundaryWildcards_UsesStringOperators(string value, string op, string expectedValue)
    {
        var filter = new SearchFilter<string>("Name") { Value = value };

        Filter? result = filter.ToContractFilter(new MappingOptions());

        SimpleFilter simple = Assert.IsType<SimpleFilter>(result);
        Assert.Equal(op, simple.Operator);
        Assert.Equal(expectedValue, simple.Value);
    }

    [Fact]
    public void ToContractFilter_StringInteriorWildcardWithLikeEnabled_UsesLike()
    {
        var filter = new SearchFilter<string>("Name") { Value = "A%ce" };
        MappingOptions options = new() { SupportLikeOperator = true };

        Filter? result = filter.ToContractFilter(options);

        SimpleFilter simple = Assert.IsType<SimpleFilter>(result);
        Assert.Equal(FilterOperators.Like, simple.Operator);
        Assert.Equal("A%ce", simple.Value);
    }

    [Fact]
    public void ToContractFilter_StringSingleWildcardNotAtBoundary_WithLikeEnabled_UsesLike()
    {
        var filter = new SearchFilter<string>("Name") { Value = "Ali%ce" };
        MappingOptions options = new() { SupportLikeOperator = true };

        Filter? result = filter.ToContractFilter(options);

        SimpleFilter simple = Assert.IsType<SimpleFilter>(result);
        Assert.Equal(FilterOperators.Like, simple.Operator);
        Assert.Equal("Ali%ce", simple.Value);
    }

    [Fact]
    public void ToContractFilter_StringInteriorWildcardWithLikeDisabled_FallsBackToEqualTo()
    {
        var filter = new SearchFilter<string>("Name") { Value = "A%ce" };
        MappingOptions options = new() { SupportLikeOperator = false };

        Filter? result = filter.ToContractFilter(options);

        SimpleFilter simple = Assert.IsType<SimpleFilter>(result);
        Assert.Equal(FilterOperators.EqualTo, simple.Operator);
        Assert.Equal("A%ce", simple.Value);
    }

    [Fact]
    public void ToContractFilter_StringWildcardHandlingDisabled_FallsBackToEqualTo()
    {
        var filter = new SearchFilter<string>("Name") { Value = "%Ali%" };
        MappingOptions options = new() { HandleStringWildcards = false };

        Filter? result = filter.ToContractFilter(options);

        SimpleFilter simple = Assert.IsType<SimpleFilter>(result);
        Assert.Equal(FilterOperators.EqualTo, simple.Operator);
        Assert.Equal("%Ali%", simple.Value);
    }

    [Fact]
    public void ToContractFilter_NonStringScalar_UsesEqualToWithStringValue()
    {
        var filter = new SearchFilter<int>("Age") { Value = 42 };

        Filter? result = filter.ToContractFilter(new MappingOptions());

        SimpleFilter simple = Assert.IsType<SimpleFilter>(result);
        Assert.Equal(FilterOperators.EqualTo, simple.Operator);
        Assert.Equal("42", simple.Value);
    }

    [Fact]
    public void ToContractFilter_EmptyTypeRangeWithNullOption_ReturnsIsNull()
    {
        var filter = new SearchFilter<IntTypeRange>("Age") { Value = new IntTypeRange() };
        MappingOptions options = new() { NullValueToIsNullFilter = true };

        Filter? result = filter.ToContractFilter(options);

        SimpleFilter simple = Assert.IsType<SimpleFilter>(result);
        Assert.Equal(FilterOperators.IsNull, simple.Operator);
    }
}
