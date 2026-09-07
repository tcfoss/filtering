using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using TcfOss.Filtering.XlsxOutput;

namespace TcfOss.Filtering.Linq.Tests;

public class ExcelObjectWriterTests
{
    // ReSharper disable NotAccessedPositionalProperty.Local
    private record TestRow(
        string Name,
        int Age,
        decimal Balance,
        bool IsActive,
        DateTime CreatedAt,
        DateOnly BirthDate,
        string? Notes);
    // ReSharper restore NotAccessedPositionalProperty.Local

    private static readonly TestRow[] s_rows =
    [
        new("Alice",   30, 19.99m, true,  new DateTime(2024, 1, 15, 10, 30, 0), new DateOnly(2000, 5,  20), "Some notes"),
        new("Bob",     25, 0m,     false, new DateTime(2024, 2,  1,  8,  0, 0), new DateOnly(1995, 10, 10), null),
        new("Charlie", 35, 42.5m,  true,  new DateTime(2023, 6,  1, 12,  0, 0), new DateOnly(1988, 3,  15), "Notes"),
    ];

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static string? ExtractValue(Cell cell, string[] sharedStrings)
    {
        if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
        {
            return sharedStrings[int.Parse(cell.CellValue!.Text)];
        }
        return cell.CellValue?.Text;
    }

    /// <summary>
    /// Opens the written stream and returns the header row (if hasHeaders) and
    /// all data rows as lists of raw cell values.
    /// </summary>
    private static (List<string>? headers, List<List<string>> data) ReadWrittenData(Stream stream, bool hasHeaders)
    {
        using SpreadsheetDocument doc = SpreadsheetDocument.Open(stream, isEditable: false);
        WorkbookPart workbookPart = doc.WorkbookPart!;
        SheetData sheetData = workbookPart
            .WorksheetParts.First()
            .Worksheet!
            .GetFirstChild<SheetData>()
            ?? throw new InvalidOperationException("SheetData not found.");

        string[] sharedStrings = [.. (workbookPart.SharedStringTablePart
            ?? throw new InvalidOperationException("SharedStringTablePart not found."))
            .SharedStringTable!
            .Elements<SharedStringItem>()
            .Select(i => i.Text?.Text ?? "")];

        List<Row> rows = [.. sheetData.Elements<Row>()];
        int dataStartIndex = 0;
        List<string>? headers = null;

        if (hasHeaders && rows.Count > 0)
        {
            headers = [.. rows[0].Elements<Cell>().Select(c => ExtractValue(c, sharedStrings) ?? "")];
            dataStartIndex = 1;
        }

        List<List<string>> data = [.. rows.Skip(dataStartIndex).Select(row =>
            row.Elements<Cell>().Select(c => ExtractValue(c, sharedStrings) ?? "").ToList()
        )];

        return (headers, data);
    }

    private static MemoryStream Write<T>(IQueryable<T> data, SerializationOptions? options = null)
    {
        options ??= new SerializationOptions { IncludeHeaders = false };
        MemoryStream stream = new();
        data.ToExcelStream(stream, options);
        return stream;
    }

    // -------------------------------------------------------------------------
    // Field type serialization
    // -------------------------------------------------------------------------

    [Fact]
    public void WriteObject_WritesAllFields()
    {
        using Stream content = Write(s_rows.Take(1).AsQueryable());
        (_, List<List<string>> data) = ReadWrittenData(content, hasHeaders: false);

        Assert.Single(data);
        Assert.Equal("Alice", data[0][0]);  // string
        Assert.Equal("30", data[0][1]);  // int
        Assert.Equal("19.99", data[0][2]);  // decimal
        Assert.Equal("1", data[0][3]);  // bool (written as "1"/"0")
        // DateTime and DateOnly are stored as OADate doubles, not formatted strings
        Assert.Equal(new DateTime(2024, 1, 15, 10, 30, 0), DateTime.FromOADate(double.Parse(data[0][4])));
        Assert.Equal(new DateOnly(2000, 5, 20), DateOnly.FromDateTime(DateTime.FromOADate(double.Parse(data[0][5]))));
        Assert.Equal("Some notes", data[0][6]);  // nullable string with value
    }

    [Fact]
    public void WriteObject_BoolFalse_WritesZero()
    {
        using Stream content = Write(s_rows.Skip(1).Take(1).AsQueryable(),
            new SerializationOptions { IncludeHeaders = false }); // Bob, IsActive=false
        (_, List<List<string>> data) = ReadWrittenData(content, hasHeaders: false);

        Assert.Equal("0", data[0][3]);
    }

    [Fact]
    public void WriteObject_NullNullable_UsesBlankValue()
    {
        using Stream content = Write(
            s_rows.Skip(1).Take(1).AsQueryable(), // Bob has null Notes
            new SerializationOptions { BlankValue = "N/A", IncludeHeaders = false });

        (_, List<List<string>> data) = ReadWrittenData(content, hasHeaders: false);

        Assert.Equal("N/A", data[0][6]);
    }

    [Fact]
    public void WriteObject_MultipleRows_AllPresent()
    {
        using Stream content = Write(s_rows.AsQueryable());
        (_, List<List<string>> data) = ReadWrittenData(content, hasHeaders: false);

        Assert.Equal(3, data.Count);
        Assert.Equal("Alice", data[0][0]);
        Assert.Equal("Bob", data[1][0]);
        Assert.Equal("Charlie", data[2][0]);
    }

    // -------------------------------------------------------------------------
    // Headers
    // -------------------------------------------------------------------------

    [Fact]
    public void WriteHeaders_WritesPropertyNamesAsFirstRow()
    {
        using Stream content = Write(s_rows.AsQueryable(), new SerializationOptions { IncludeHeaders = true });
        (List<string>? headers, List<List<string>> data) = ReadWrittenData(content, hasHeaders: true);

        Assert.Equal(["Name", "Age", "Balance", "IsActive", "CreatedAt", "BirthDate", "Notes"], headers);
        Assert.Equal(3, data.Count); // data rows unaffected
    }

    [Fact]
    public void WriteHeaders_CustomHeaders_UsesProvidedNames()
    {
        string[] customHeaders = ["A", "B", "C", "D", "E", "F", "G"];
        using Stream content = Write(s_rows.Take(1).AsQueryable(),
            new SerializationOptions { IncludeHeaders = true, Headers = customHeaders });

        (List<string>? headers, _) = ReadWrittenData(content, hasHeaders: true);

        Assert.Equal(customHeaders, headers);
    }

    [Fact]
    public void WriteHeaders_Disabled_FirstRowIsData()
    {
        using Stream content = Write(s_rows.Take(1).AsQueryable(),
            new SerializationOptions { IncludeHeaders = false });

        (_, List<List<string>> data) = ReadWrittenData(content, hasHeaders: false);

        Assert.Single(data);
        Assert.Equal("Alice", data[0][0]); // first row is data, not a header
    }

    // -------------------------------------------------------------------------
    // QueryableExtensions: filter / sort / maxRows overload
    // -------------------------------------------------------------------------

    [Fact]
    public void Filter_ReducesRows()
    {
        var filter = new SimpleFilter("Age", QueryOperator.GreaterThan, 28);
        using Stream content = s_rows.AsQueryable().ToExcelStream(filter, sorts: null, maxRows: null,
            options: new SerializationOptions { IncludeHeaders = false });
        (_, List<List<string>> data) = ReadWrittenData(content, hasHeaders: false);

        Assert.Equal(2, data.Count); // Alice (30) and Charlie (35)
        Assert.All(data, row => Assert.True(int.Parse(row[1]) > 28));
    }

    [Fact]
    public void Sort_OrdersRows()
    {
        SortComponent[] sorts = [new("Age", SortDirection.Descending)];
        using Stream content = s_rows.AsQueryable().ToExcelStream(filter: null, sorts: sorts, maxRows: null,
            options: new SerializationOptions { IncludeHeaders = false });
        (_, List<List<string>> data) = ReadWrittenData(content, hasHeaders: false);

        Assert.Equal(3, data.Count);
        Assert.Equal("Charlie", data[0][0]); // 35
        Assert.Equal("Alice", data[1][0]); // 30
        Assert.Equal("Bob", data[2][0]); // 25
    }

    [Fact]
    public void MaxRows_LimitsOutput()
    {
        using Stream content = s_rows.AsQueryable().ToExcelStream(filter: null, sorts: null, maxRows: 2,
            options: new SerializationOptions { IncludeHeaders = false });
        (_, List<List<string>> data) = ReadWrittenData(content, hasHeaders: false);

        Assert.Equal(2, data.Count);
    }

    [Fact]
    public void FilterSortMaxRows_AppliedInCorrectOrder()
    {
        // Filter Age > 24 (all 3 pass), sort by Age asc, take top 2 → Bob (25), Alice (30)
        var filter = new SimpleFilter("Age", QueryOperator.GreaterThan, 24);
        SortComponent[] sorts = [new("Age", SortDirection.Ascending)];

        using Stream content = s_rows.AsQueryable().ToExcelStream(filter, sorts, maxRows: 2,
            options: new SerializationOptions { IncludeHeaders = false });
        (_, List<List<string>> data) = ReadWrittenData(content, hasHeaders: false);

        Assert.Equal(2, data.Count);
        Assert.Equal("Bob", data[0][0]); // 25
        Assert.Equal("Alice", data[1][0]); // 30
    }

    // -------------------------------------------------------------------------
    // QueryableExtensions: stream overload
    // -------------------------------------------------------------------------

    [Fact]
    public void StreamOverload_WritesToProvidedStream()
    {
        using MemoryStream stream = new();
        s_rows.AsQueryable().ToExcelStream(stream, new SerializationOptions { IncludeHeaders = false });

        stream.Position = 0;
        (_, List<List<string>> data) = ReadWrittenData(stream, hasHeaders: false);

        Assert.Equal(3, data.Count);
    }

    [Fact]
    public void StreamOverload_WithFilterAndSort_WritesCorrectly()
    {
        var filter = new SimpleFilter("Age", QueryOperator.LessThan, 35);
        SortComponent[] sorts = [new("Name", SortDirection.Ascending)];

        using MemoryStream stream = new();
        s_rows.AsQueryable().ToExcelStream(stream, filter, sorts, maxRows: null,
            options: new SerializationOptions { IncludeHeaders = false });

        stream.Position = 0;
        (_, List<List<string>> data) = ReadWrittenData(stream, hasHeaders: false);

        Assert.Equal(2, data.Count);   // Alice and Bob (Charlie age=35 excluded)
        Assert.Equal("Alice", data[0][0]);
        Assert.Equal("Bob", data[1][0]);
    }

    // -------------------------------------------------------------------------
    // Dynamic / dictionary-backed rows (DataResult<dynamic> path)
    // -------------------------------------------------------------------------

    [Fact]
    public void WritesDynamicResults_HeadersDerivedFromDictionaryKeys_PreservesDottedNames()
    {
        IDictionary<string, object?> row = new System.Dynamic.ExpandoObject();
        row["Name"] = "Alice";
        row["Department.Name"] = "Engineering";
        row["Age"] = 30;

        var opts = new SerializationOptions { IncludeHeaders = true };
        using MemoryStream stream = new();
        using (ExcelObjectWriter<dynamic> writer = new(stream, opts))
        {
            writer.WriteHeaders();
            writer.WriteObject(row);
        }
        stream.Position = 0;

        (List<string>? headers, List<List<string>> data) = ReadWrittenData(stream, hasHeaders: true);

        Assert.NotNull(headers);
        Assert.Equal(["Name", "Department.Name", "Age"], headers);
        Assert.Single(data);
        Assert.Equal("Alice", data[0][0]);
        Assert.Equal("Engineering", data[0][1]);
        Assert.Equal("30", data[0][2]);
    }

    // -------------------------------------------------------------------------
    // Async streaming
    // -------------------------------------------------------------------------

    private static async IAsyncEnumerable<T> ToAsync<T>(IEnumerable<T> items)
    {
        foreach (T item in items)
        {
            await Task.Yield();
            yield return item;
        }
    }

    [Fact]
    public async Task ToExcelStreamAsync_StreamsAllItems()
    {
        using var resultStream = (MemoryStream)await ToAsync(s_rows).ToExcelStreamAsync(
            new SerializationOptions { IncludeHeaders = true }, TestContext.Current.CancellationToken);

        (List<string>? headers, List<List<string>> data) = ReadWrittenData(resultStream, hasHeaders: true);

        Assert.NotNull(headers);
        Assert.Equal(3, data.Count);
        Assert.Equal("Alice", data[0][0]);
        Assert.Equal("Bob", data[1][0]);
        Assert.Equal("Charlie", data[2][0]);
    }

    [Fact]
    public async Task ToExcelStreamAsync_StreamOverload_StreamsAllItems()
    {
        using MemoryStream stream = new();

        await ToAsync(s_rows).ToExcelStreamAsync(stream, options: null, TestContext.Current.CancellationToken);

        stream.Position = 0;
        (List<string>? headers, List<List<string>> data) = ReadWrittenData(stream, hasHeaders: true);

        Assert.NotNull(headers);
        Assert.Equal(3, data.Count);
        Assert.Equal("Alice", data[0][0]);
        Assert.Equal("Bob", data[1][0]);
        Assert.Equal("Charlie", data[2][0]);
    }

    [Fact]
    public void ToExcelStream_MainOverload_NullArguments_Throws()
    {
        IQueryable<TestRow> query = s_rows.AsQueryable();

        Assert.Throws<ArgumentNullException>(() =>
            query.ToExcelStream(new MemoryStream(), null, null, null, null!, System.Linq.Dynamic.Core.ParsingConfig.Default, new SerializationOptions()));

        Assert.Throws<ArgumentNullException>(() =>
            query.ToExcelStream(new MemoryStream(), null, null, null, new ValueManager(), null!, new SerializationOptions()));

        Assert.Throws<ArgumentNullException>(() =>
            query.ToExcelStream(new MemoryStream(), null, null, null, new ValueManager(), System.Linq.Dynamic.Core.ParsingConfig.Default, null!));
    }

    [Fact]
    public void ToExcelStream_ParsingConfigOverload_WithFilterSortAndTake_WritesExpectedRows()
    {
        var filter = new SimpleFilter("Age", QueryOperator.GreaterThan, 24);
        SortComponent[] sorts = [new("Age", SortDirection.Ascending)];

        using Stream returned = s_rows.AsQueryable().ToExcelStream(
            filter,
            sorts,
            maxRows: 1,
            System.Linq.Dynamic.Core.ParsingConfig.Default,
            new SerializationOptions { IncludeHeaders = false });

        (_, List<List<string>> returnedData) = ReadWrittenData(returned, hasHeaders: false);
        Assert.Single(returnedData);
        Assert.Equal("Bob", returnedData[0][0]);

        using MemoryStream provided = new();
        s_rows.AsQueryable().ToExcelStream(
            provided,
            filter,
            sorts,
            maxRows: 1,
            new ValueManager(),
            System.Linq.Dynamic.Core.ParsingConfig.Default,
            new SerializationOptions { IncludeHeaders = false });

        provided.Position = 0;
        (_, List<List<string>> providedData) = ReadWrittenData(provided, hasHeaders: false);
        Assert.Single(providedData);
        Assert.Equal("Bob", providedData[0][0]);
    }
}
