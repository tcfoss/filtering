using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TcfOss.Filtering.Contracts.Newtonsoft;
using TcfOss.Filtering.EntityFrameworkCore.Queryable;
using TcfOss.Filtering.Linq.FilterMapping;
using TcfOss.Filtering.Linq.IntegrationTests.Database;
using TcfOss.Filtering.Linq.IntegrationTests.Database.Entities;
using TcfOss.Filtering.XlsxOutput;
using STJ = System.Text.Json;

namespace TcfOss.Filtering.Linq.IntegrationTests.MySql;

public class MariaDbQueryTests(MariaDbFixture_11_08_06 fixture)
    : IClassFixture<MariaDbFixture_11_08_06>
{
    private readonly MariaDbFixture_11_08_06 _fixture = fixture;

    private LibraryDbContext CreateContext()
    {
        string connectionString = _fixture.Container.GetConnectionString();
        DbContextOptions<LibraryDbContext> options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            .Options;

        return new LibraryDbContext(options);
    }


    private static readonly STJ.JsonSerializerOptions s_stjOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly JsonSerializerSettings s_nsSettings = new()
    {
        Converters = { new FilterConverter() },
    };

    private record BookSummary
    {
        public required string Title { get; init; }
        public required string SortableTitle { get; init; }
        public DateOnly? PurchaseDate { get; init; }
        public IEnumerable<string>? Authors { get; init; }
    }

    private const string TestJson = """
    {
        "filter": {
            "filterType": "composite",
            "logicalOperator": "and",
            "filters": [
                {
                    "filterType": "simple",
                    "field": "Title",
                    "operator": "cn",
                    "value": "the"
                },
                {
                    "filterType": "simple",
                    "field": "purchaseDate",
                    "operator": "gt",
                    "value": "2024-01-01"
                }
            ]
        },
        "sorts": [
            {
                "field": "SortableTitle",
                "direction": "desc"
            }
        ],
        "page": 1,
        "pageSize": 10
    }
    """;


    [Fact]
    public async Task SimpleTest_NoNavProp()
    {
        LibraryDbContext context = CreateContext();

        IQueryable<Book> query = context.Books.AsNoTracking();

        var dataFilter = new DataRequest
        {
            Filter = new SimpleFilter("Title", QueryOperator.Contains, "clean"),
            Sorts = [new SortComponent("Title", SortDirection.Ascending)],
            Page = 1,
            PageSize = 10
        };

        Contracts.DataResult<Book> books = await query.ToDataResultAsync(dataFilter, TestContext.Current.CancellationToken);

        Assert.Equal(1, books.TotalCount);
        Book singleBook = books.Data.Single();
        Assert.Equal("Clean Code", singleBook.Title);
        Assert.Empty(singleBook.Authors);
    }

    [Fact]
    public async Task SimpleTest_WithNavProps()
    {
        LibraryDbContext context = CreateContext();

        IQueryable<Book> query = context.Books
            .Include(b => b.Authors)
            .Include(b => b.FullGenres)
            .Include(b => b.Genres)
            .AsNoTracking();

        var dataFilter = new DataRequest
        {
            Filter = new SimpleFilter("Title", QueryOperator.Contains, "clean"),
            Sorts = [new SortComponent("Title", SortDirection.Ascending)],
            Page = 1,
            PageSize = 10
        };

        Contracts.DataResult<Book> books = await query.ToDataResultAsync(dataFilter, TestContext.Current.CancellationToken);

        Assert.Equal(1, books.TotalCount);
        Book firstBook = books.Data.Single();
        Assert.Equal("Clean Code", firstBook.Title);
        Assert.Single(firstBook.Authors);
        Assert.Equal("Robert C. Martin", firstBook.Authors.First().Name);

        Assert.Single(firstBook.Genres);
        Assert.Equal("Computer Science", firstBook.Genres.First().Name);

        Assert.Single(firstBook.FullGenres);
        Assert.Equal("Science & Technology › Computer Science", firstBook.FullGenres.First().Name);
    }

    [Fact]
    public async Task SimpleTest_Dynamic()
    {
        LibraryDbContext context = CreateContext();

        IQueryable<Book> query = context.Books
            .AsNoTracking();

        var dataFilter = new DynamicDataRequest
        {
            RequestedFields = ["Title", "Subtitle"],
            Filter = new SimpleFilter("Title", QueryOperator.Contains, "ea"),
            Sorts = [new SortComponent("Title", SortDirection.Ascending)],
            Page = 1,
            PageSize = 10
        };

        Contracts.DataResult<dynamic> books = await query.ToDataResultAsync(dataFilter, TestContext.Current.CancellationToken);

        Assert.Equal(2, books.TotalCount);
        Assert.Equal("Clean Code", books.Data[0].Title);
        Assert.Equal("A Handbook of Agile Software Craftsmanship", books.Data[0].Subtitle);
        Assert.False(((IDictionary<string, object>)books.Data[0]).ContainsKey("PurchasePrice"));

        Assert.Equal("The Lean Startup", books.Data[1].Title);
        Assert.Null(books.Data[1].Subtitle);
        Assert.False(((IDictionary<string, object>)books.Data[1]).ContainsKey("PurchasePrice"));
    }

    [Fact]
    public async Task Filter_Like()
    {
        LibraryDbContext context = CreateContext();

        IQueryable<Book> query = context.Books
            .Include(b => b.Authors)
            .Include(b => b.FullGenres)
            .Include(b => b.Genres)
            .AsNoTracking();

        var dataFilter = new DataRequest
        {
            Filter = new SimpleFilter("Title", QueryOperator.Like, "the%of%wind"),
            Sorts = [new SortComponent("Title", SortDirection.Ascending)],
            Page = 1,
            PageSize = 10
        };

        Contracts.DataResult<Book> books = await query.ToDataResultAsync(dataFilter, TestContext.Current.CancellationToken);

        Assert.Equal(2, books.TotalCount);
        Assert.Equal("The Name of the Wind", books.Data[0].Title);
        Assert.Equal("The Shadow of the Wind", books.Data[1].Title);
    }

    [Fact]
    public async Task FilterLike_Dynamic()
    {
        LibraryDbContext context = CreateContext();

        IQueryable<Book> query = context.Books
            .AsNoTracking();

        var dataFilter = new DynamicDataRequest
        {
            RequestedFields = ["Title", "Subtitle"],
            Filter = new SimpleFilter("Title", QueryOperator.Like, "%ea%"),
            Sorts = [new SortComponent("Title", SortDirection.Ascending)],
            Page = 1,
            PageSize = 10
        };

        Contracts.DataResult<dynamic> books = await query.ToDataResultAsync(dataFilter, TestContext.Current.CancellationToken);

        Assert.Equal(2, books.TotalCount);
        Assert.Equal("Clean Code", books.Data[0].Title);
        Assert.Equal("A Handbook of Agile Software Craftsmanship", books.Data[0].Subtitle);
        Assert.False(((IDictionary<string, object>)books.Data[0]).ContainsKey("PurchasePrice"));

        Assert.Equal("The Lean Startup", books.Data[1].Title);
        Assert.Null(books.Data[1].Subtitle);
        Assert.False(((IDictionary<string, object>)books.Data[1]).ContainsKey("PurchasePrice"));
    }

    [Fact]
    public async Task Filter_NavProp()
    {
        LibraryDbContext context = CreateContext();

        IQueryable<Book> query = context.Books
            .Include(b => b.Authors)
            .Include(b => b.FullGenres)
            .Include(b => b.Genres)
            .AsNoTracking();

        var dataFilter = new DataRequest
        {
            Filter = new QuantifiedFilter()
            {
                FieldName = "FullGenres",
                QuantifiedOperator = QuantifiedOperator.Any,
                SubFilter = new SimpleFilter("Name", QueryOperator.Contains, "Computer Science")
            },
            Sorts = [new SortComponent("Title", SortDirection.Ascending)],
            Page = 1,
            PageSize = 10
        };

        Contracts.DataResult<Book> books = await query.ToDataResultAsync(dataFilter, TestContext.Current.CancellationToken);

        Assert.Equal(4, books.TotalCount);

        Assert.All(books.Data, b => Assert.Contains(b.Genres, g => g.Name == "Computer Science"));

        Assert.Equal("An Introduction to Algorithms", books.Data[0].Title);
        Assert.Equal("Clean Code", books.Data[1].Title);
        Assert.Equal("Designing Data-Intensive Applications", books.Data[2].Title);
        Assert.Equal("The Pragmatic Programmer", books.Data[3].Title);
    }

    [Fact]
    public async Task OrderBy_NavProp()
    {
        LibraryDbContext context = CreateContext();

        IQueryable<Book> query = context.Books
            .Include(b => b.Authors)
            .Include(b => b.FullGenres)
            .Include(b => b.Genres)
            .AsNoTracking();

        var dataFilter = new DataRequest
        {
            Sorts = [new SortComponent("Authors.Count", SortDirection.Descending)],
            Page = 1,
            PageSize = 3
        };

        Contracts.DataResult<Book> books = await query.ToDataResultAsync(dataFilter, TestContext.Current.CancellationToken);

        Assert.Equal(30, books.TotalCount);

        Assert.Equal(3, books.Data.Length);

        Book firstBook = books.Data[0];
        Assert.Equal("An Introduction to Algorithms", firstBook.Title);
        Assert.Equal(4, firstBook.Authors.Count);

        Book secondBook = books.Data[1];
        Assert.Equal("The Pragmatic Programmer", secondBook.Title);
        Assert.Equal(2, secondBook.Authors.Count);

        Book thirdBook = books.Data[2];
        Assert.Equal("Good Omens", thirdBook.Title);
        Assert.Equal(2, thirdBook.Authors.Count);
    }

    [Fact]
    public async Task CompositeFilter_OrderBy()
    {
        LibraryDbContext context = CreateContext();

        IQueryable<Book> query = context.Books
            .Include(b => b.FullGenres)
            .AsNoTracking();

        var dataFilter = new DataRequest
        {
            Filter = new CompositeFilter()
            {
                LogicalOperator = LogicalOperator.Or,
                Filters = [
                    new QuantifiedFilter()
                    {
                        FieldName = "FullGenres",
                        QuantifiedOperator = QuantifiedOperator.Any,
                        SubFilter = new SimpleFilter("Name", QueryOperator.EndsWith, "Physics")
                    },
                    new SimpleFilter("PurchaseDate", QueryOperator.GreaterThan, new DateOnly(2024, 1, 1))
                ]
            },
            Sorts = [new SortComponent("SortableTitle", SortDirection.Ascending)],
            Page = 1,
            PageSize = 10
        };

        Contracts.DataResult<Book> books = await query.ToDataResultAsync(dataFilter, TestContext.Current.CancellationToken);

        Assert.Equal(5, books.TotalCount);

        Assert.Equal("The Art of War", books.Data[0].Title);
        Assert.Single(books.Data[0].FullGenres);
        Assert.Equal("Non-Fiction › Philosophy", books.Data[0].FullGenres.First().Name);

        Assert.Equal("A Brief History of Time", books.Data[1].Title);
        Assert.Equal(2, books.Data[1].FullGenres.Count);
        Assert.Equal("Science & Technology › Physics", books.Data[1].FullGenres.First(g => g.Name.Contains("Physics")).Name);
        Assert.Equal("Non-Fiction › Science", books.Data[1].FullGenres.First(g => g.Name.Contains("Fiction")).Name);

        Assert.Equal("Flowers for Algernon", books.Data[2].Title);
        Assert.Single(books.Data[2].FullGenres);
        Assert.Equal("Fiction › Science Fiction", books.Data[2].FullGenres.First().Name);

        Assert.Equal("An Introduction to Algorithms", books.Data[3].Title);
        Assert.Equal(2, books.Data[3].FullGenres.Count);
        Assert.Equal("Science & Technology › Computer Science", books.Data[3].FullGenres.First(g => g.Name.Contains("Computer Science")).Name);
        Assert.Equal("Science & Technology › Mathematics", books.Data[3].FullGenres.First(g => g.Name.Contains("Mathematics")).Name);

        Assert.Equal("The Lean Startup", books.Data[4].Title);
        Assert.Single(books.Data[4].FullGenres);
        Assert.Equal("Non-Fiction", books.Data[4].FullGenres.First().Name);
    }

    [Fact]
    public async Task SecondPage()
    {
        LibraryDbContext context = CreateContext();

        IQueryable<Book> query = context.Books
            .AsNoTracking();

        var dataFilter = new DataRequest
        {
            Sorts = [new SortComponent("SortableTitle", SortDirection.Ascending)],
            Page = 2,
            PageSize = 5
        };

        Contracts.DataResult<Book> books = await query.ToDataResultAsync(dataFilter, TestContext.Current.CancellationToken);

        Assert.Equal(30, books.TotalCount);

        Assert.Equal(5, books.Data.Length);

        Assert.Equal("Crime and Punishment", books.Data[0].Title);
        Assert.Equal("Designing Data-Intensive Applications", books.Data[1].Title);
        Assert.Equal("Dune", books.Data[2].Title);
        Assert.Equal("A Fire Upon the Deep", books.Data[3].Title);
        Assert.Equal("Flowers for Algernon", books.Data[4].Title);
    }

    [Fact]
    public async Task Projection()
    {
        LibraryDbContext context = CreateContext();

        IQueryable<Book> query = context.Books
            .AsNoTracking();

        var dataFilter = new DataRequest
        {
            Filter = new SimpleFilter("Title", QueryOperator.Contains, "clean"),
            Sorts = [new SortComponent("Title", SortDirection.Ascending)],
            Page = 1,
            PageSize = 10
        };

        var bookTitles = await query
            .Select(b => new { b.Title, b.Subtitle })
            .ApplyDataFilter(dataFilter)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Single(bookTitles);
        Assert.Equal("Clean Code", bookTitles[0].Title);
        Assert.Equal("A Handbook of Agile Software Craftsmanship", bookTitles[0].Subtitle);
    }

    [Fact]
    public async Task NestedFilter_WithSetFilter_NoNullableType()
    {
        var subFilter1 = new Contracts.SimpleFilter("BookId", Contracts.FilterOperators.EqualTo, "11");
        var subFilter2 = new Contracts.SetFilter("GenreId", ["1", "2"]);
        var rawFilter = new Contracts.CompositeFilter(Contracts.LogicalOperators.Or, [subFilter1, subFilter2]);

        var rawDataRequest = new Contracts.DataRequest
        {
            Filter = rawFilter,
            Sorts = [new Contracts.SortComponent("BookId", Contracts.SortDirections.Ascending)],
            Page = 1,
            PageSize = 10
        };

        var mapper = new ReflectionFilterMapper<BookGenre>();

        var dataRequest = mapper.ToDataRequest(rawDataRequest);

        LibraryDbContext context = CreateContext();

        IQueryable<BookGenre> query = context.BookGenres
            .AsNoTracking();

        Contracts.DataResult<BookGenre> result = await query.ToDataResultAsync(dataRequest, TestContext.Current.CancellationToken);

        Assert.Equal(4, result.TotalCount);

        Assert.Equal(10u, result.Data[0].BookId);
        Assert.Equal(1u, result.Data[0].GenreId);
        Assert.Equal(11u, result.Data[1].BookId);
        Assert.Equal(6u, result.Data[1].GenreId);
        Assert.Equal(25u, result.Data[2].BookId);
        Assert.Equal(2u, result.Data[2].GenreId);
        Assert.Equal(29u, result.Data[3].BookId);
        Assert.Equal(2u, result.Data[3].GenreId);
    }


    [Fact]
    public async Task NestedFilter_WithSetFilter_NullableType()
    {
        var subFilter1 = new Contracts.SimpleFilter("BookId", Contracts.FilterOperators.EqualTo, "11");
        var subFilter2 = new Contracts.SetFilter("GenreId", ["1", "2"]);
        var rawFilter = new Contracts.CompositeFilter(Contracts.LogicalOperators.Or, [subFilter1, subFilter2]);

        var rawDataRequest = new Contracts.DataRequest
        {
            Filter = rawFilter,
            Sorts = [new Contracts.SortComponent("BookId", Contracts.SortDirections.Ascending)],
            Page = 1,
            PageSize = 10
        };

        var mapper = new ReflectionFilterMapper<BookOptionalGenre>();

        var dataRequest = mapper.ToDataRequest(rawDataRequest);

        LibraryDbContext context = CreateContext();

        IQueryable<BookOptionalGenre> query = context.BookOptionalGenres
            .AsNoTracking();

        Contracts.DataResult<BookOptionalGenre> result = await query.ToDataResultAsync(dataRequest, TestContext.Current.CancellationToken);

        Assert.Equal(4, result.TotalCount);

        Assert.Equal(10u, result.Data[0].BookId);
        Assert.Equal(1u, result.Data[0].GenreId);
        Assert.Equal(11u, result.Data[1].BookId);
        Assert.Equal(6u, result.Data[1].GenreId);
        Assert.Equal(25u, result.Data[2].BookId);
        Assert.Equal(2u, result.Data[2].GenreId);
        Assert.Equal(29u, result.Data[3].BookId);
        Assert.Equal(2u, result.Data[3].GenreId);
    }

    [Fact]
    public async Task Complex_Filter_SystemTextJson()
    {
        LibraryDbContext context = CreateContext();

        IQueryable<BookSummary> query = context.Books
            .Select(b => new BookSummary
            {
                Title = b.Title,
                SortableTitle = b.SortableTitle,
                PurchaseDate = b.PurchaseDate,
                Authors = b.Authors.OrderBy(a => a.DisplayOrder).Select(a => a.Name)
            });



        Contracts.DataRequest? dto = STJ.JsonSerializer.Deserialize<Contracts.DataRequest>(TestJson, s_stjOptions);
        Assert.NotNull(dto);

        var mapper = new ReflectionFilterMapper<BookSummary>();
        DataRequest filter = mapper.ToDataRequest(dto);

        Contracts.DataResult<BookSummary> result = await query.ToDataResultAsync(filter, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(2, result.Data.Length);

        Assert.Equal("The Lean Startup", result.Data[0].Title);
        Assert.NotNull(result.Data[0].Authors);
        var authors1 = result.Data[0].Authors!.ToList();
        Assert.Single(authors1);
        Assert.Equal("Eric Ries", authors1[0]);

        Assert.Equal("The Art of War", result.Data[1].Title);
        Assert.NotNull(result.Data[1].Authors);
        var authors2 = result.Data[1].Authors!.ToList();
        Assert.Single(authors2);
        Assert.Equal("Sun Tzu", authors2[0]);
    }

    [Fact]
    public async Task Complex_Filter_NewtonsoftJson()
    {
        LibraryDbContext context = CreateContext();

        IQueryable<BookSummary> query = context.Books
            .Select(b => new BookSummary
            {
                Title = b.Title,
                SortableTitle = b.SortableTitle,
                PurchaseDate = b.PurchaseDate,
                Authors = b.Authors.OrderBy(a => a.DisplayOrder).Select(a => a.Name)
            });

        Contracts.DataRequest? dto = JsonConvert.DeserializeObject<Contracts.DataRequest>(TestJson, s_nsSettings);
        Assert.NotNull(dto);

        var mapper = new ReflectionFilterMapper<BookSummary>();
        DataRequest filter = mapper.ToDataRequest(dto);

        Contracts.DataResult<BookSummary> result = await query.ToDataResultAsync(filter, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(2, result.Data.Length);

        Assert.Equal("The Lean Startup", result.Data[0].Title);
        Assert.NotNull(result.Data[0].Authors);
        var authors1 = result.Data[0].Authors!.ToList();
        Assert.Single(authors1);
        Assert.Equal("Eric Ries", authors1[0]);

        Assert.Equal("The Art of War", result.Data[1].Title);
        Assert.NotNull(result.Data[1].Authors);
        var authors2 = result.Data[1].Authors!.ToList();
        Assert.Single(authors2);
        Assert.Equal("Sun Tzu", authors2[0]);
    }

    [Fact]
    public void EfWrapper_ApplySortingAndPagingOverloads_WorkAsExpected()
    {
        using LibraryDbContext context = CreateContext();

        IQueryable<Book> query = context.Books.AsNoTracking();
        SortComponent[] sorts = [new("SortableTitle", SortDirection.Ascending)];

        string[] sortedTop3 = [.. query
            .ApplySorting(sorts)
            .Take(3)
            .Select(b => b.Title)];

        string[] expectedTop3 = [.. query
            .OrderBy(b => b.SortableTitle)
            .Take(3)
            .Select(b => b.Title)];

        Assert.Equal(expectedTop3, sortedTop3);

        string[] secondPage = [.. query
            .ApplyPagingAndSorting(sorts, page: 2, pageSize: 3)
            .Select(b => b.Title)];

        string[] expectedSecondPage = [.. query
            .OrderBy(b => b.SortableTitle)
            .Skip(3)
            .Take(3)
            .Select(b => b.Title)];

        Assert.Equal(3, secondPage.Length);
        Assert.Equal(expectedSecondPage, secondPage);
    }

    [Fact]
    public void EfWrapper_ApplyFilteringOverloads_WorkAsExpected()
    {
        using LibraryDbContext context = CreateContext();

        IQueryable<Book> query = context.Books.AsNoTracking();
        var filter = new SimpleFilter("Title", QueryOperator.Contains, "clean");

        string[] viaDefaultManager = [.. query
            .ApplyFiltering(filter)
            .Select(b => b.Title)];

        string[] viaExplicitManager = [.. query
            .ApplyFiltering(filter, new ValueManager())
            .Select(b => b.Title)];

        Assert.Single(viaDefaultManager);
        Assert.Equal("Clean Code", viaDefaultManager[0]);
        Assert.Equal(viaDefaultManager, viaExplicitManager);
    }

    [Fact]
    public void EfWrapper_ApplyDataFilterAndToDataResultSyncOverloads_WorkAsExpected()
    {
        using LibraryDbContext context = CreateContext();

        IQueryable<Book> query = context.Books.AsNoTracking();

        var dataFilter = new DataRequest
        {
            Filter = new SimpleFilter("Title", QueryOperator.Contains, "the"),
            Sorts = [new SortComponent("SortableTitle", SortDirection.Ascending)],
            Page = 1,
            PageSize = 2
        };

        string[] projectedTitles = [.. query
            .Select(b => new { b.Title, b.SortableTitle })
            .ApplyDataFilter(dataFilter, new ValueManager())
            .Select(b => b.Title)];

        string[] expectedProjectedTitles = [.. query
            .Where(b => b.Title.Contains("the"))
            .OrderBy(b => b.SortableTitle)
            .Take(2)
            .Select(b => b.Title)];

        Assert.Equal(2, projectedTitles.Length);
        Assert.Equal(expectedProjectedTitles, projectedTitles);

        Contracts.DataResult<Book> viaDefaultManager = query.ToDataResult(dataFilter);
        Contracts.DataResult<Book> viaExplicitManager = query.ToDataResult(dataFilter, new ValueManager());

        Assert.Equal(viaDefaultManager.TotalCount, viaExplicitManager.TotalCount);
        Assert.Equal(viaDefaultManager.Data.Select(b => b.Title), viaExplicitManager.Data.Select(b => b.Title));
        Assert.Equal(expectedProjectedTitles, viaDefaultManager.Data.Select(b => b.Title));
    }

    [Fact]
    public void EfWrapper_ToDataResultDynamicSyncOverloads_WorkAsExpected()
    {
        using LibraryDbContext context = CreateContext();

        IQueryable<Book> query = context.Books.AsNoTracking();

        var dynamicFilter = new DynamicDataRequest
        {
            RequestedFields = ["Title", "Subtitle"],
            Filter = new SimpleFilter("Title", QueryOperator.Contains, "ea"),
            Sorts = [new SortComponent("Title", SortDirection.Ascending)],
            Page = 1,
            PageSize = 10
        };

        Contracts.DataResult<dynamic> viaDefaultManager = query.ToDataResult(dynamicFilter);
        Contracts.DataResult<dynamic> viaExplicitManager = query.ToDataResult(dynamicFilter, new ValueManager());

        Assert.Equal(2, viaDefaultManager.TotalCount);
        Assert.Equal(viaDefaultManager.TotalCount, viaExplicitManager.TotalCount);
        Assert.Equal("Clean Code", viaDefaultManager.Data[0].Title);
        Assert.Equal("The Lean Startup", viaDefaultManager.Data[1].Title);
    }

    [Fact]
    public void ExcelExport()
    {
        string fileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".xlsx";

        try
        {
            LibraryDbContext context = CreateContext();

            IQueryable<Book> query = context.Books
                .Include(b => b.FullGenres)
                .AsNoTracking();

            var filter = new CompositeFilter()
            {
                LogicalOperator = LogicalOperator.Or,
                Filters = [
                    new QuantifiedFilter()
                    {
                        FieldName = "FullGenres",
                        QuantifiedOperator = QuantifiedOperator.Any,
                        SubFilter = new SimpleFilter("Name", QueryOperator.EndsWith, "Physics")
                    },
                    new SimpleFilter("PurchaseDate", QueryOperator.GreaterThan, new DateOnly(2024, 1, 1))
                ]
            };

            SortComponent[] sorts = [new("SortableTitle", SortDirection.Ascending)];

            SerializationOptions options = new()
            {
                BlankValue = "N/A",
                IncludeHeaders = false
            };

            using (FileStream stream = new(fileName, FileMode.Create, FileAccess.ReadWrite))
            {
                query.ToExcelStream(stream, filter, sorts, maxRows: null, options: options);
            }

            using SpreadsheetDocument doc = SpreadsheetDocument.Open(fileName, isEditable: false);
            WorkbookPart workbookPart = doc.WorkbookPart!;
            SheetData sheetData = workbookPart.WorksheetParts.First().Worksheet!.GetFirstChild<SheetData>()
                ?? throw new InvalidOperationException("SheetData not found.");
            string[] sharedStrings = [..
                (workbookPart.SharedStringTablePart ?? throw new InvalidOperationException("SharedStringTablePart not found."))
                .SharedStringTable!
                .Elements<SharedStringItem>()
                .Select(i => i.Text?.Text ?? "")];

            List<Row> rows = [.. sheetData.Elements<Row>()];
            Assert.Equal(5, rows.Count);

            // Title is the 2nd property on Book (index 1), so the 2nd cell in each row
            string GetTitle(Row row) =>
                sharedStrings[int.Parse(row.Elements<Cell>().ElementAt(1).CellValue!.Text)];

            Assert.Equal("The Art of War", GetTitle(rows[0]));
            Assert.Equal("A Brief History of Time", GetTitle(rows[1]));
            Assert.Equal("Flowers for Algernon", GetTitle(rows[2]));
            Assert.Equal("An Introduction to Algorithms", GetTitle(rows[3]));
            Assert.Equal("The Lean Startup", GetTitle(rows[4]));
        }
        finally
        {
            File.Delete(fileName);
        }
    }
}
