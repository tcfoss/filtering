namespace TcfOss.Filtering.Linq.Tests;

public class SortingPagingTests
{

    // ReSharper disable NotAccessedPositionalProperty.Local
    private record Widget(string Name, int CustomerId, int Quantity);
    private record NoIdRecord(string Title, int Count);
    // ReSharper restore NotAccessedPositionalProperty.Local

    [Fact]
    public void SortComponent_ToDynamicLinq_Ascending()
    {
        var sort = new SortComponent("Name", SortDirection.Ascending);
        string result = sort.ToDynamicLinq();
        Assert.Equal("Name asc", result);
    }

    [Fact]
    public void SortComponent_ToDynamicLinq_Descending()
    {
        var sort = new SortComponent("Age", SortDirection.Descending);
        string result = sort.ToDynamicLinq();
        Assert.Equal("Age desc", result);
    }

    [Fact]
    public void SortComponent_SortsData_Ascending()
    {
        IQueryable<TestData.Person> query = TestData.People.AsQueryable();

        var sort = new SortComponent("Age", SortDirection.Ascending);

        var sorted = query.ApplyPagingAndSorting([sort], 1, 100).ToList();

        Assert.Equal(26, sorted.Count);

        Assert.Equal("Karl", sorted[0].Name);
        Assert.Equal(24, sorted[0].Age);

        Assert.Equal("Bob", sorted[1].Name);
        Assert.Equal(25, sorted[1].Age);

        TestData.Person secondLast = sorted[^2];
        Assert.Equal("Charlie", secondLast.Name);
        Assert.Equal(35, secondLast.Age);

        TestData.Person last = sorted[^1];
        Assert.Equal("Zara", last.Name);
        Assert.Equal(35, last.Age);
    }

    [Fact]
    public void SortComponent_SortsData_Descending()
    {
        IQueryable<TestData.Person> query = TestData.People.AsQueryable();

        var sort = new SortComponent("Age", SortDirection.Descending);
        var sorted = query.ApplyPagingAndSorting([sort], 1, 100).ToList();

        Assert.Equal(26, sorted.Count);

        Assert.Equal("Charlie", sorted[0].Name);
        Assert.Equal(35, sorted[0].Age);

        Assert.Equal("Zara", sorted[1].Name);
        Assert.Equal(35, sorted[1].Age);

        TestData.Person secondLast = sorted[^2];
        Assert.Equal("Bob", secondLast.Name);
        Assert.Equal(25, secondLast.Age);

        TestData.Person last = sorted[^1];
        Assert.Equal("Karl", last.Name);
        Assert.Equal(24, last.Age);
    }

    [Fact]
    public void ApplyPaging_DefaultOrdering()
    {
        IQueryable<TestData.Person> query = TestData.People.AsQueryable();

        var paged = query.ApplyPagingAndSorting([], 2, 10).ToList();

        Assert.Equal(10, paged.Count);

        Assert.Equal("Karl", paged[0].Name);
        Assert.Equal("Leo", paged[1].Name);
        Assert.Equal("Mallory", paged[2].Name);
        Assert.Equal("Nina", paged[3].Name);
        Assert.Equal("Oscar", paged[4].Name);
        Assert.Equal("Peggy", paged[5].Name);
        Assert.Equal("Quentin", paged[6].Name);
        Assert.Equal("Rupert", paged[7].Name);
        Assert.Equal("Sybil", paged[8].Name);
        Assert.Equal("Trent", paged[9].Name);
    }

    [Fact]
    public void ApplyPaging_WithSorting()
    {
        IQueryable<TestData.Person> query = TestData.People.AsQueryable();

        var sort = new SortComponent("Name", SortDirection.Descending);
        var paged = query.ApplyPagingAndSorting([sort], 2, 5).ToList();

        Assert.Equal(5, paged.Count);

        Assert.Equal("Uma", paged[0].Name);
        Assert.Equal("Trent", paged[1].Name);
        Assert.Equal("Sybil", paged[2].Name);
        Assert.Equal("Rupert", paged[3].Name);
        Assert.Equal("Quentin", paged[4].Name);
    }


    [Fact]
    public void ApplySorting_NoSorts_PrefersPropertyEndingInId()
    {
        Widget[] data =
        [
            new("Cog", 3, 10),
            new("Bolt", 1, 20),
            new("Axle", 2, 30),
        ];

        List<Widget> result = [.. data.AsQueryable().ApplySorting(null)];

        Assert.Equal([1, 2, 3], result.Select(w => w.CustomerId));
    }


    [Fact]
    public void ApplySorting_NoSorts_NoIdProperty_FallsBackToFirstDeclaredProperty()
    {
        NoIdRecord[] data =
        [
            new("Charlie", 3),
            new("Alice", 1),
            new("Bob", 2),
        ];

        List<NoIdRecord> result = [.. data.AsQueryable().ApplySorting(null)];

        Assert.Equal(["Alice", "Bob", "Charlie"], result.Select(r => r.Title));
    }
}
