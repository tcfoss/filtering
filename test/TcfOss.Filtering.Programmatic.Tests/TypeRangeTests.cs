using TcfOss.Filtering.Contracts;
using TcfOss.Filtering.Programmatic.DataTypes;

namespace TcfOss.Filtering.Programmatic.Tests;

public class TypeRangeTests
{
    [Fact]
    public void IntTypeRange_MinAndMax_ReturnsRangeFilter()
    {
        var range = new IntTypeRange { Min = 10, Max = 20 };

        Filter? result = range.ToContractFilter("Age", new MappingOptions());

        RangeFilter filter = Assert.IsType<RangeFilter>(result);
        Assert.Equal("Age", filter.Field);
        Assert.Equal("10", filter.ValueFrom);
        Assert.Equal("20", filter.ValueTo);
    }

    [Fact]
    public void IntTypeRange_MinOnly_ReturnsGreaterThanOrEqualTo()
    {
        var range = new IntTypeRange { Min = 10 };

        Filter? result = range.ToContractFilter("Age", new MappingOptions());

        SimpleFilter filter = Assert.IsType<SimpleFilter>(result);
        Assert.Equal(FilterOperators.GreaterThanOrEqualTo, filter.Operator);
        Assert.Equal("10", filter.Value);
    }

    [Fact]
    public void IntTypeRange_MaxOnly_ReturnsLessThanOrEqualTo()
    {
        var range = new IntTypeRange { Max = 20 };

        Filter? result = range.ToContractFilter("Age", new MappingOptions());

        SimpleFilter filter = Assert.IsType<SimpleFilter>(result);
        Assert.Equal(FilterOperators.LessThanOrEqualTo, filter.Operator);
        Assert.Equal("20", filter.Value);
    }

    [Fact]
    public void IntTypeRange_NoBounds_ReturnsNull()
    {
        var range = new IntTypeRange();

        Filter? result = range.ToContractFilter("Age", new MappingOptions());

        Assert.Null(result);
    }

    [Fact]
    public void DateTypeRange_UsesConfiguredDateFormat()
    {
        var range = new DateTypeRange { Min = new DateOnly(2026, 6, 28), Max = new DateOnly(2026, 7, 1) };
        MappingOptions options = new() { DateFormat = "dd/MM/yyyy" };

        Filter? result = range.ToContractFilter("PurchaseDate", options);

        RangeFilter filter = Assert.IsType<RangeFilter>(result);
        Assert.Equal("28/06/2026", filter.ValueFrom);
        Assert.Equal("01/07/2026", filter.ValueTo);
    }

    [Fact]
    public void DateTimeTypeRange_UsesConfiguredDateTimeFormat()
    {
        var range = new DateTimeTypeRange
        {
            Min = new DateTime(2026, 6, 28, 13, 45, 0, DateTimeKind.Utc),
            Max = new DateTime(2026, 6, 29, 9, 15, 0, DateTimeKind.Utc)
        };
        MappingOptions options = new() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" };

        Filter? result = range.ToContractFilter("UpdatedAt", options);

        RangeFilter filter = Assert.IsType<RangeFilter>(result);
        Assert.Equal("2026-06-28 13:45:00", filter.ValueFrom);
        Assert.Equal("2026-06-29 09:15:00", filter.ValueTo);
    }

    [Fact]
    public void DecimalTypeRange_MinAndMax_ReturnsRangeFilter()
    {
        var range = new DecimalTypeRange { Min = 10m, Max = 20m };

        Filter? result = range.ToContractFilter("Amount", new MappingOptions());

        RangeFilter filter = Assert.IsType<RangeFilter>(result);
        Assert.Equal("Amount", filter.Field);
        Assert.Equal("10", filter.ValueFrom);
        Assert.Equal("20", filter.ValueTo);
    }

    [Fact]
    public void DoubleTypeRange_MinOnly_ReturnsGreaterThanOrEqualTo()
    {
        var range = new DoubleTypeRange { Min = 42d };

        Filter? result = range.ToContractFilter("Score", new MappingOptions());

        SimpleFilter filter = Assert.IsType<SimpleFilter>(result);
        Assert.Equal("Score", filter.Field);
        Assert.Equal(FilterOperators.GreaterThanOrEqualTo, filter.Operator);
        Assert.Equal("42", filter.Value);
    }

    [Fact]
    public void FloatTypeRange_MaxOnly_ReturnsLessThanOrEqualTo()
    {
        var range = new FloatTypeRange { Max = 99f };

        Filter? result = range.ToContractFilter("Progress", new MappingOptions());

        SimpleFilter filter = Assert.IsType<SimpleFilter>(result);
        Assert.Equal("Progress", filter.Field);
        Assert.Equal(FilterOperators.LessThanOrEqualTo, filter.Operator);
        Assert.Equal("99", filter.Value);
    }
}
