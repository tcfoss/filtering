using TcfOss.Filtering.Contracts;

namespace TcfOss.Filtering.Linq.Tests;

public class DataResultTests
{
    // ReSharper disable NotAccessedPositionalProperty.Local
    private record Nested(string Name);
    private record Outer(string Name, Nested Child);
    // ReSharper restore NotAccessedPositionalProperty.Local

    [Fact]
    public void ApplyDataFilter_WideOpen()
    {
        IQueryable<TestData.Person> query = TestData.People.AsQueryable();

        var filter = new DataRequest
        {
            Filter = null,
            Sorts = [],
            Page = 1,
            PageSize = 10
        };

        var actualResult = query.ApplyDataFilter(filter).ToList();

        var expectedResult = TestData.People
            .Take(10)
            .ToList();

        Assert.Equal(10, actualResult.Count);
        Assert.Equal(expectedResult.Count, actualResult.Count);
        Assert.Equal(expectedResult, actualResult);
    }

    [Fact]
    public void ApplyDataFilter_Simple()
    {
        IQueryable<TestData.Person> query = TestData.People.AsQueryable();

        var filter = new DataRequest
        {
            Filter = new SimpleFilter
            {
                Field = "City",
                Operator = QueryOperator.Contains,
                Value = "New"
            },
            Sorts =
            [
                new SortComponent("Age", SortDirection.Ascending)
            ],
            Page = 1,
            PageSize = 10
        };

        var actualResult = query.ApplyDataFilter(filter).ToList();

        var expectedResult = TestData.People
            .Where(p => p.City.Contains("New"))
            .OrderBy(p => p.Age)
            .Take(10)
            .ToList();

        Assert.NotEmpty(actualResult);
        Assert.Equal(3, actualResult.Count);
        Assert.Equal(expectedResult, actualResult);
    }

    [Fact]
    public void ApplyDataFilter_Complex()
    {
        IQueryable<TestData.Person> query = TestData.People.AsQueryable();

        var filter = new DataRequest
        {
            Filter = new CompositeFilter
            {
                LogicalOperator = LogicalOperator.Or,
                Filters =
                [
                    new SimpleFilter
                    {
                        Field = "Name",
                        Operator = QueryOperator.StartsWith,
                        Value = "A"
                    },
                    new SimpleFilter
                    {
                        Field = "City",
                        Operator = QueryOperator.EqualTo,
                        Value = "Los Angeles"
                    }
                ]
            },
            Sorts =
            [
                new SortComponent("Age", SortDirection.Descending)
            ],
            Page = 1,
            PageSize = 10
        };

        var actualResult = query.ApplyDataFilter(filter).ToList();

        var expectedResult = TestData.People
            .Where(p => p.Name.StartsWith('A') || p.City == "Los Angeles")
            .OrderByDescending(p => p.Age)
            .Take(10)
            .ToList();

        Assert.Equal(2, actualResult.Count);
        Assert.Equal(expectedResult.Count, actualResult.Count);
        Assert.Equal(expectedResult, actualResult);
    }

    [Fact]
    public void ToDataResult_WideOpen()
    {
        IQueryable<TestData.Person> query = TestData.People.AsQueryable();

        var filter = new DataRequest
        {
            Filter = null,
            Sorts = [],
            Page = 1,
            PageSize = 10
        };

        var actual = query.ToDataResult(filter);

        var expected = new DataResult<TestData.Person>
        {
            Data = [.. TestData.People.Take(10)],
            TotalCount = 26,
            Page = 1,
            PageSize = 10
        };

        Assert.Equal(expected.Data, actual.Data);
        Assert.Equal(expected.TotalCount, actual.TotalCount);
        Assert.Equal(expected.Page, actual.Page);
        Assert.Equal(expected.PageSize, actual.PageSize);
    }

    [Fact]
    public void ToDataResult_Simple()
    {
        IQueryable<TestData.Person> query = TestData.People.AsQueryable();

        var filter = new DataRequest
        {
            Filter = new SimpleFilter
            {
                Field = "City",
                Operator = QueryOperator.Contains,
                Value = "New"
            },
            Sorts =
            [
                new SortComponent("Age", SortDirection.Ascending)
            ],
            Page = 1,
            PageSize = 10
        };

        var actual = query.ToDataResult(filter);

        TestData.Person[] expectedData = [.. TestData.People
            .Where(p => p.City.Contains("New"))
            .OrderBy(p => p.Age)
            .Take(10)];

        int expectedTotalCount = TestData.People.Count(p => p.City.Contains("New"));

        var expected = new DataResult<TestData.Person>
        {
            Data = expectedData,
            TotalCount = expectedTotalCount,
            Page = 1,
            PageSize = 10
        };

        Assert.Equal(expected.Data, actual.Data);
        Assert.Equal(expected.TotalCount, actual.TotalCount);
        Assert.Equal(expected.Page, actual.Page);
        Assert.Equal(expected.PageSize, actual.PageSize);
    }

    [Fact]
    public void ToDataResult_Complex()
    {
        IQueryable<TestData.Person> query = TestData.People.AsQueryable();

        var filter = new DataRequest
        {
            Filter = new CompositeFilter
            {
                LogicalOperator = LogicalOperator.Or,
                Filters =
                [
                    new SimpleFilter
                    {
                        Field = "Name",
                        Operator = QueryOperator.StartsWith,
                        Value = "A"
                    },
                    new SimpleFilter
                    {
                        Field = "City",
                        Operator = QueryOperator.EqualTo,
                        Value = "Los Angeles"
                    }
                ]
            },
            Sorts =
            [
                new SortComponent("Age", SortDirection.Descending)
            ],
            Page = 1,
            PageSize = 10
        };

        var actual = query.ToDataResult(filter);

        TestData.Person[] expectedData = [.. TestData.People
            .Where(p => p.Name.StartsWith('A') || p.City == "Los Angeles")
            .OrderByDescending(p => p.Age)
            .Take(10)];

        int expectedTotalCount = TestData.People.Count(p => p.Name.StartsWith('A') || p.City == "Los Angeles");

        var expected = new DataResult<TestData.Person>
        {
            Data = expectedData,
            TotalCount = expectedTotalCount,
            Page = 1,
            PageSize = 10
        };

        Assert.Equal(expected.Data, actual.Data);
        Assert.Equal(expected.TotalCount, actual.TotalCount);
        Assert.Equal(expected.Page, actual.Page);
        Assert.Equal(expected.PageSize, actual.PageSize);
    }

    [Fact]
    public void ApplyFieldSelection_ProjectsOnlyRequestedFields()
    {
        IQueryable<TestData.Person> query = TestData.People.AsQueryable();

        var projected = query.ApplyFieldSelection(["Name", "Age"]).ToList();

        Assert.Equal(TestData.People.Length, projected.Count);

        dynamic first = projected[0];
        Assert.Equal(TestData.People[0].Name, (string)first.Name);
        Assert.Equal(TestData.People[0].Age, (int)first.Age);

        string[] propertyNames = [.. ((object)first).GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0).Select(p => p.Name)];
        Assert.DoesNotContain("City", propertyNames);
        Assert.DoesNotContain("Friends", propertyNames);
    }

    [Fact]
    public void ToDataResult_DynamicDataFilter_WideOpen()
    {
        IQueryable<TestData.Person> query = TestData.People.AsQueryable();

        var filter = new DynamicDataRequest
        {
            RequestedFields = ["Name", "Age"],
            Filter = null,
            Sorts = [],
            Page = 1,
            PageSize = 10
        };

        DataResult<dynamic> actual = query.ToDataResult(filter);

        Assert.Equal(26, actual.TotalCount);
        Assert.Equal(1, actual.Page);
        Assert.Equal(10, actual.PageSize);
        Assert.Equal(10, actual.Data.Length);

        dynamic first = actual.Data[0];
        Assert.Equal(TestData.People[0].Name, (string)first.Name);
        Assert.Equal(TestData.People[0].Age, (int)first.Age);
    }

    [Fact]
    public void ToDataResult_DynamicDataFilter_WithFilterAndSort()
    {
        IQueryable<TestData.Person> query = TestData.People.AsQueryable();

        var filter = new DynamicDataRequest
        {
            RequestedFields = ["Name", "City"],
            Filter = new SimpleFilter("City", QueryOperator.Contains, "New"),
            Sorts = [new SortComponent("Name", SortDirection.Ascending)],
            Page = 1,
            PageSize = 10
        };

        DataResult<dynamic> actual = query.ToDataResult(filter);

        string[] expectedNames = [.. TestData.People
            .Where(p => p.City.Contains("New"))
            .OrderBy(p => p.Name)
            .Select(p => p.Name)];

        int expectedTotalCount = TestData.People.Count(p => p.City.Contains("New"));

        Assert.Equal(expectedTotalCount, actual.TotalCount);
        Assert.Equal(expectedNames.Length, actual.Data.Length);

        for (int i = 0; i < actual.Data.Length; i++)
        {
            dynamic item = actual.Data[i];
            Assert.Equal(expectedNames[i], (string)item.Name);

            string[] keys = [.. ((IDictionary<string, object>)item).Keys];
            Assert.DoesNotContain("Age", keys);
        }
    }

    [Fact]
    public void ApplyFieldSelection_DottedField_AliasedWithUnderscore()
    {
        IQueryable<Outer> items = new[] { new Outer("Alice", new Nested("ChildName")) }.AsQueryable();

        dynamic result = items.ApplyFieldSelection(["Name", "Child.Name"]).ToList()[0];

        string[] props = [.. ((object)result).GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0).Select(p => p.Name)];
        Assert.Contains("Name", props);
        Assert.Contains("Child_Name", props);
        Assert.DoesNotContain("Child.Name", props);
    }

    [Fact]
    public void ApplyFieldSelection_ConflictingLastSegments_NoConflict()
    {
        // "Name" and "Child.Name" both end with "Name" — without aliasing this
        // would produce two properties both called "Name".
        IQueryable<Outer> items = new[] { new Outer("Alice", new Nested("ChildName")) }.AsQueryable();

        var resultList = items.ApplyFieldSelection(["Name", "Child.Name"]).ToList();
        Assert.Single(resultList);

        dynamic first = resultList[0];
        Assert.Equal("Alice", (string)first.Name);
        Assert.Equal("ChildName", (string)first.Child_Name);
    }

    [Fact]
    public void ApplyFieldSelection_SimpleField_NotAliased()
    {
        IQueryable<Outer> items = new[] { new Outer("Alice", new Nested("ChildName")) }.AsQueryable();

        dynamic result = items.ApplyFieldSelection(["Name"]).ToList()[0];

        string[] props = [.. ((object)result).GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0).Select(p => p.Name)];
        Assert.Contains("Name", props);
        Assert.Single(props);
    }

    [Fact]
    public void ToDataResult_DottedRequestedField_JsonKeyMatchesRequestedName()
    {
        IQueryable<Outer> query = new[] { new Outer("Alice", new Nested("ChildName")) }.AsQueryable();

        var filter = new DynamicDataRequest
        {
            RequestedFields = ["Name", "Child.Name"],
            Filter = null,
            Sorts = [],
            Page = 1,
            PageSize = 10
        };

        DataResult<dynamic> result = query.ToDataResult(filter);

        dynamic item = result.Data[0];
        var dict = (IDictionary<string, object>)item;

        // Keys must match the requested names exactly — no underscore aliasing
        Assert.Contains("Name", dict.Keys);
        Assert.Contains("Child.Name", dict.Keys);
        Assert.DoesNotContain("Child_Name", dict.Keys);

        Assert.Equal("Alice", (string)item.Name);
        Assert.Equal("ChildName", (string)dict["Child.Name"]);
    }
}
