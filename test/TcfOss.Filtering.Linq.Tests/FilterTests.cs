using System.Linq.Expressions;
using TcfOss.Filtering.Contracts;

namespace TcfOss.Filtering.Linq.Tests;

public class FilterTests
{
    private record PersonProjection(string Name, int Age);

    [Theory]
    [InlineData("EqualTo", "Name == @0")]
    [InlineData("NotEqualTo", "Name != @0")]
    [InlineData("IsNull", "Name == null")]
    [InlineData("IsNotNull", "Name != null")]
    [InlineData("GreaterThan", "Name > @0")]
    [InlineData("GreaterThanOrEqualTo", "Name >= @0")]
    [InlineData("LessThan", "Name < @0")]
    [InlineData("LessThanOrEqualTo", "Name <= @0")]
    [InlineData("StartsWith", "Name.StartsWith(@0)")]
    [InlineData("DoesNotStartWith", "!Name.StartsWith(@0)")]
    [InlineData("EndsWith", "Name.EndsWith(@0)")]
    [InlineData("DoesNotEndWith", "!Name.EndsWith(@0)")]
    [InlineData("Contains", "Name.Contains(@0)")]
    [InlineData("DoesNotContain", "!Name.Contains(@0)")]
    [InlineData("Like", "DbFunctionsExtensions.Like(EF.Functions, Name, @0)")]
    [InlineData("NotLike", "!DbFunctionsExtensions.Like(EF.Functions, Name, @0)")]
    public void SimpleFilter_ToDynamicLinq(string operatorName, string expected)
    {
        var valueManager = new ValueManager();

        var op = QueryOperator.Parse(operatorName);
        var filter = new SimpleFilter("Name", op, "John");
        string result = filter.ToDynamicLinq(valueManager);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void UnknownOperator_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => QueryOperator.Parse("UnknownOperator"));
    }

    [Fact]
    public void BlankOperator_Throws()
    {
        Assert.Throws<ArgumentException>(() => QueryOperator.Parse(""));
        Assert.Throws<ArgumentException>(() => QueryOperator.Parse(null!));
    }

    [Fact]
    public void ParseContractKey_LikeOperators()
    {
        Assert.Same(QueryOperator.Like, QueryOperator.ParseContractKey(FilterOperators.Like));
        Assert.Same(QueryOperator.NotLike, QueryOperator.ParseContractKey(FilterOperators.NotLike));
    }

    [Fact]
    public void LikeOperator_WithoutParsingConfig_ThrowsClearException()
    {
        var filter = new SimpleFilter("Name", QueryOperator.Like, "%ohn%");
        var valueManager = new ValueManager();

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            TestData.People.AsQueryable().ApplyFiltering(filter, valueManager).ToList());

        Assert.Contains("registers EF and DbFunctionsExtensions", ex.Message);
    }

    [Fact]
    public void CompositeFilter_ToDynamicLinq()
    {
        var valueManager = new ValueManager();

        var filter1 = new SimpleFilter("Name", QueryOperator.EqualTo, "John");
        var filter2 = new SimpleFilter("Age", QueryOperator.GreaterThan, 30);
        var filter3 = new SimpleFilter("City", QueryOperator.StartsWith, "New");


        var compositeFilter = new CompositeFilter([filter1, filter2, filter3], LogicalOperator.And);

        string result = compositeFilter.ToDynamicLinq(valueManager);

        Assert.Equal("(Name == @0 and Age > @1 and City.StartsWith(@2))", result);
    }

    [Fact]
    public void CompositeFilter_Nested_ToDynamicLinq()
    {
        var valueManager = new ValueManager();

        var filter1 = new SimpleFilter("Name", QueryOperator.EqualTo, "John");
        var filter2 = new SimpleFilter("Age", QueryOperator.GreaterThan, 30);
        var filter3 = new SimpleFilter("City", QueryOperator.StartsWith, "New");

        var innerComposite = new CompositeFilter([filter2, filter3], LogicalOperator.Or);
        var outerComposite = new CompositeFilter([filter1, innerComposite], LogicalOperator.And);

        string result = outerComposite.ToDynamicLinq(valueManager);
        Assert.Equal("(Name == @0 and (Age > @1 or City.StartsWith(@2)))", result);

        ConstantExpression[] actualValues = valueManager.GetValueExpressions();

        ConstantExpression[] expectedValues =
        [
            Expression.Constant("John"),
            Expression.Constant(30),
            Expression.Constant("New")
        ];

        Assert.Equal(expectedValues.Length, actualValues.Length);
        for (int i = 0; i < expectedValues.Length; i++)
        {
            Assert.IsType<ConstantExpression>(actualValues[i]);
            ConstantExpression constant = actualValues[i];
            Assert.Equal(expectedValues[i].Value, constant.Value);
        }
    }

    [Fact]
    public void QuantifiedFilter_ToDynamicLinq()
    {
        var valueManager = new ValueManager();

        var subFilter = new SimpleFilter("Name", QueryOperator.StartsWith, "J");
        var filter = new QuantifiedFilter("Friends", subFilter, QuantifiedOperator.Any);

        string result = filter.ToDynamicLinq(valueManager);
        Assert.Equal("Friends.Any(Name.StartsWith(@0))", result);

        ConstantExpression[] actualValues = valueManager.GetValueExpressions();
        Assert.Single(actualValues);
        Assert.IsType<ConstantExpression>(actualValues[0]);
        ConstantExpression constant = actualValues[0];
        Assert.Equal("J", constant.Value);
    }

    [Fact]
    public void SimpleFilter_GreaterThan_FiltersData()
    {
        var filter = new SimpleFilter("Age", QueryOperator.GreaterThan, 30);
        var valueManager = new ValueManager();

        var result = TestData.People
            .AsQueryable()
            .ApplyFiltering(filter, valueManager)
            .ToList();

        Assert.Equal(10, result.Count);
        Assert.Equal("Charlie", result[0].Name);
    }

    [Fact]
    public void CompositeFilter_And_FiltersData()
    {
        var filter1 = new SimpleFilter("Age", QueryOperator.GreaterThan, 30);
        var filter2 = new SimpleFilter("City", QueryOperator.StartsWith, "New");
        var compositeFilter = new CompositeFilter([filter1, filter2], LogicalOperator.And);

        var valueManager = new ValueManager();

        var result = TestData.People
            .AsQueryable()
            .ApplyFiltering(compositeFilter, valueManager)
            .ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Judy", result[0].Name);
    }

    [Fact]
    public void QuantifiedFilter_Any_FiltersData()
    {
        var subFilter = new SimpleFilter("Name", QueryOperator.EqualTo, "Alice");
        var filter = new QuantifiedFilter("Friends", subFilter, QuantifiedOperator.Any)
        {
            IsNullable = true
        };

        var valueManager = new ValueManager();

        var result = TestData.People
            .AsQueryable()
            .ApplyFiltering(filter, valueManager)
            .ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Bob", result[0].Name);
        Assert.Equal("Grace", result[1].Name);
    }

    [Fact]
    public void QuantifiedFilter_All_FiltersData()
    {
        var subFilter = new SimpleFilter("Age", QueryOperator.LessThan, 27);
        var filter = new QuantifiedFilter("Friends", subFilter, QuantifiedOperator.All)
        {
            IsNullable = true
        };

        var valueManager = new ValueManager();

        var result = TestData.People
            .AsQueryable()
            .ApplyFiltering(filter, valueManager)
            .ToList();

        Assert.Single(result);
        Assert.Equal("Ivan", result[0].Name);
    }

    [Fact]
    public void SimpleFilter_Projection_FiltersData()
    {
        var filter = new SimpleFilter("Name", QueryOperator.GreaterThan, "Xavier");
        var valueManager = new ValueManager();

        var result = TestData.People
            .AsQueryable()
            .Select(p => new PersonProjection(p.Name, p.Age))
            .ApplyFiltering(filter, valueManager)
            .ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Yvonne", result[0].Name);
        Assert.Equal("Zara", result[1].Name);
    }


    [Fact]
    public void SetFilter_In_ToDynamicLinq()
    {
        var valueManager = new ValueManager();
        var filter = new SetFilter("Name", ["Alice", "Bob"]);

        string result = filter.ToDynamicLinq(valueManager);

        Assert.Equal("@0.Contains(Name)", result);
    }

    [Fact]
    public void SetFilter_NotIn_ToDynamicLinq()
    {
        var valueManager = new ValueManager();
        var filter = new SetFilter("Name", ["Alice", "Bob"], negated: true);

        string result = filter.ToDynamicLinq(valueManager);

        Assert.Equal("!(@0.Contains(Name))", result);
    }

    [Fact]
    public void SetFilter_In_FiltersData()
    {
        var filter = new SetFilter("Name", ["Alice", "Charlie", "Eve"]);
        var valueManager = new ValueManager();

        var result = TestData.People
            .AsQueryable()
            .ApplyFiltering(filter, valueManager)
            .ToList();

        Assert.Equal(3, result.Count);
        Assert.Contains(result, p => p.Name == "Alice");
        Assert.Contains(result, p => p.Name == "Charlie");
        Assert.Contains(result, p => p.Name == "Eve");
    }

    [Fact]
    public void SetFilter_NotIn_FiltersData()
    {
        string[] excluded = ["Alice", "Bob", "Charlie"];
        var filter = new SetFilter("Name", excluded, negated: true);
        var valueManager = new ValueManager();

        var result = TestData.People
            .AsQueryable()
            .ApplyFiltering(filter, valueManager)
            .ToList();

        int expectedCount = TestData.People.Count(p => !excluded.Contains(p.Name));
        Assert.Equal(expectedCount, result.Count);
        Assert.DoesNotContain(result, p => excluded.Contains(p.Name));
    }

    [Theory]
    [InlineData(false, false, "(Age >= @0 && Age <= @1)")]
    [InlineData(true, false, "(Age > @0 && Age < @1)")]
    [InlineData(false, true, "!(Age >= @0 && Age <= @1)")]
    [InlineData(true, true, "!(Age > @0 && Age < @1)")]
    public void RangeFilter_ToDynamicLinq(bool exclusive, bool negated, string expected)
    {
        var valueManager = new ValueManager();
        var filter = new RangeFilter("Age", 27, 31, exclusive, negated);

        string result = filter.ToDynamicLinq(valueManager);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void RangeFilter_Inclusive_FiltersData()
    {
        var filter = new RangeFilter("Age", 27, 31);
        var valueManager = new ValueManager();

        var result = TestData.People
            .AsQueryable()
            .ApplyFiltering(filter, valueManager)
            .ToList();

        Assert.Equal(15, result.Count);
        Assert.All(result, p => Assert.InRange(p.Age, 27, 31));
    }

    [Fact]
    public void RangeFilter_Exclusive_FiltersData()
    {
        var filter = new RangeFilter("Age", 27, 31, exclusive: true);
        var valueManager = new ValueManager();

        var result = TestData.People
            .AsQueryable()
            .ApplyFiltering(filter, valueManager)
            .ToList();

        Assert.Equal(9, result.Count);
        Assert.All(result, p => Assert.InRange(p.Age, 28, 30));
    }

    [Fact]
    public void RangeFilter_Inclusive_Negated_FiltersData()
    {
        var filter = new RangeFilter("Age", 27, 31, negated: true);
        var valueManager = new ValueManager();

        var result = TestData.People
            .AsQueryable()
            .ApplyFiltering(filter, valueManager)
            .ToList();

        Assert.Equal(11, result.Count);
        Assert.All(result, p => Assert.False(p.Age is >= 27 and <= 31));
    }

    [Fact]
    public void NestedFilter()
    {
        var subFilter1 = new SimpleFilter("Name", QueryOperator.EqualTo, "Alice");
        var subFilter2 = new SetFilter("Age", [24, 25, 26]);
        var filter = new CompositeFilter([subFilter1, subFilter2], LogicalOperator.Or);

        var result = TestData.People
            .AsQueryable()
            .ApplyFiltering(filter, new ValueManager())
            .ToList();

        result = [.. result.OrderBy(p => p.Name)];

        Assert.Equal(5, result.Count);

        Assert.Equal("Alice", result[0].Name);
        Assert.Equal("Bob", result[1].Name);
        Assert.Equal("Ivan", result[2].Name);
        Assert.Equal("Karl", result[3].Name);
        Assert.Equal("Sybil", result[4].Name);
    }
}
