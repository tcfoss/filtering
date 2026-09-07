using TcfOss.Filtering.CsvOutput;

namespace TcfOss.Filtering.Linq.Tests;

public class CsvObjectWriterTests
{
    // ReSharper disable NotAccessedPositionalProperty.Local
    private record Row(
        string Name,
        int Age,
        decimal Balance,
        bool IsActive,
        DateTime CreatedAt,
        DateOnly BirthDate,
        string? Notes);

    private record NullableRow(
        DateTime? UpdatedAt,
        DateOnly? Anniversary,
        bool? IsActive,
        int? Count,
        string? Notes);

    private record RuntimeShape(
        string Name,
        DateOnly Date,
        bool IsEnabled);
    // ReSharper restore NotAccessedPositionalProperty.Local

    private static string Write(Action<CsvObjectWriter<Row>> action, string blankValue = "", bool writeHeaders = false)
    {
        var opts = new SerializationOptions { BlankValue = blankValue, IncludeHeaders = writeHeaders };
        using MemoryStream stream = new();
        using (CsvObjectWriter<Row> writer = new(stream, opts))
        {
            action(writer);
        }
        stream.Position = 0;
        return new StreamReader(stream).ReadToEnd();
    }

    private static string[] Lines(string content)
        => content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

    private static string ReadAll(Stream stream)
    {
        stream.Position = 0;
        return new StreamReader(stream).ReadToEnd();
    }

    [Fact]
    public void WriteHeaders_WritesPropertyNamesInOrder()
    {
        string content = Write(w => w.WriteHeaders(), writeHeaders: true);
        Assert.Equal("Name,Age,Balance,IsActive,CreatedAt,BirthDate,Notes", Lines(content)[0]);
    }

    [Fact]
    public void WriteHeaders_UsesConfiguredCustomHeaders()
    {
        string[] headers = ["A", "B", "C", "D", "E", "F", "G"];
        var opts = new SerializationOptions { IncludeHeaders = true, Headers = headers };

        using MemoryStream stream = new();
        using (CsvObjectWriter<Row> writer = new(stream, opts))
        {
            writer.WriteHeaders();
        }

        string content = ReadAll(stream);
        Assert.Equal("A,B,C,D,E,F,G", Lines(content)[0]);
    }

    [Fact]
    public void WriteObject_WritesAllFields()
    {
        var row = new Row("Alice", 30, 19.99m, true,
            new DateTime(2024, 1, 15, 10, 30, 0), new DateOnly(2000, 5, 20), "Some notes");

        string content = Write(w => w.WriteObject(row));

        Assert.Equal("Alice,30,19.99,True,2024-01-15 10:30:00,2000-05-20,Some notes", Lines(content)[0]);
    }

    [Fact]
    public void WriteObject_NullNullable_UsesBlankValue()
    {
        var row = new Row("Alice", 30, 0m, false,
            new DateTime(2024, 1, 1), new DateOnly(2000, 1, 1), null);

        string[] fields = Lines(Write(w => w.WriteObject(row), blankValue: "N/A"))[0].Split(',');

        Assert.Equal("N/A", fields[^1]);
    }

    [Fact]
    public void WriteObject_DecimalUsesInvariantCulture()
    {
        var row = new Row("Alice", 30, 1234.56m, false,
            new DateTime(2024, 1, 1), new DateOnly(2000, 1, 1), null);

        Assert.Contains(",1234.56,", Write(w => w.WriteObject(row)));
    }

    [Fact]
    public void WriteObject_DateTimeUsesIsoFormat()
    {
        var row = new Row("Alice", 30, 0m, false,
            new DateTime(2024, 6, 15, 8, 5, 3), new DateOnly(2000, 1, 1), null);

        Assert.Contains(",2024-06-15 08:05:03,", Write(w => w.WriteObject(row)));
    }

    [Fact]
    public void WriteObject_DateOnlyUsesIsoFormat()
    {
        var row = new Row("Alice", 30, 0m, false,
            new DateTime(2024, 1, 1), new DateOnly(1995, 12, 3), null);

        Assert.Contains(",1995-12-03,", Write(w => w.WriteObject(row)));
    }

    [Fact]
    public void WriteObject_BoolWritesTrueOrFalse()
    {
        var baseRow = new Row("A", 0, 0m, true, new DateTime(2024, 1, 1), new DateOnly(2000, 1, 1), null);

        Assert.Contains(",True,", Write(w => w.WriteObject(baseRow)));
        Assert.Contains(",False,", Write(w => w.WriteObject(baseRow with { IsActive = false })));
    }

    [Fact]
    public void WriteObject_FieldWithComma_IsQuoted()
    {
        var row = new Row("Smith, Alice", 30, 0m, false,
            new DateTime(2024, 1, 1), new DateOnly(2000, 1, 1), null);

        Assert.StartsWith("\"Smith, Alice\",", Write(w => w.WriteObject(row)));
    }

    [Fact]
    public void WriteObject_FieldWithDoubleQuote_IsDoubled()
    {
        var row = new Row("Alice \"A\" Smith", 30, 0m, false,
            new DateTime(2024, 1, 1), new DateOnly(2000, 1, 1), null);

        Assert.StartsWith("\"Alice \"\"A\"\" Smith\",", Write(w => w.WriteObject(row)));
    }

    [Fact]
    public void WriteObject_FieldWithNewline_IsQuoted()
    {
        var row = new Row("Line1\nLine2", 30, 0m, false,
            new DateTime(2024, 1, 1), new DateOnly(2000, 1, 1), null);

        Assert.StartsWith("\"Line1\nLine2\",", Write(w => w.WriteObject(row)));
    }

    [Fact]
    public void WriteObject_MultipleRows_EachOnOwnLine()
    {
        var row1 = new Row("Alice", 30, 0m, false, new DateTime(2024, 1, 1), new DateOnly(2000, 1, 1), null);
        var row2 = new Row("Bob", 25, 0m, false, new DateTime(2024, 1, 1), new DateOnly(2000, 1, 1), null);

        string[] lines = Lines(Write(w => { w.WriteObject(row1); w.WriteObject(row2); }));

        Assert.Equal(2, lines.Length);
        Assert.StartsWith("Alice,", lines[0]);
        Assert.StartsWith("Bob,", lines[1]);
    }

    // -------------------------------------------------------------------------
    // Dynamic / dictionary-backed rows (DataResult<dynamic> path)
    // -------------------------------------------------------------------------

    private static string WriteDynamic(IEnumerable<IDictionary<string, object?>> rows, bool writeHeaders = true)
    {
        var opts = new SerializationOptions { IncludeHeaders = writeHeaders };
        using MemoryStream stream = new();
        using (CsvObjectWriter<dynamic> writer = new(stream, opts))
        {
            writer.WriteHeaders();
            foreach (IDictionary<string, object?> row in rows)
            {
                writer.WriteObject(row);
            }
        }
        stream.Position = 0;
        return new StreamReader(stream).ReadToEnd();
    }

    [Fact]
    public void WritesDynamicResults_HeadersDerivedFromDictionaryKeys_PreservesDottedNames()
    {
        IDictionary<string, object?> row = new System.Dynamic.ExpandoObject();
        row["Name"] = "Alice";
        row["Department.Name"] = "Engineering";
        row["Age"] = 30;

        string[] lines = Lines(WriteDynamic([row]));

        Assert.Equal("Name,Department.Name,Age", lines[0]);
        Assert.Equal("Alice,Engineering,30", lines[1]);
    }

    [Fact]
    public void WritesDynamicResults_NullValueUsesBlank()
    {
        IDictionary<string, object?> row = new System.Dynamic.ExpandoObject();
        row["Name"] = "Alice";
        row["Notes"] = null;

        var opts = new SerializationOptions { IncludeHeaders = false, BlankValue = "N/A" };
        using MemoryStream stream = new();
        using (CsvObjectWriter<dynamic> writer = new(stream, opts))
        {
            writer.WriteObject(row);
        }
        stream.Position = 0;
        string content = new StreamReader(stream).ReadToEnd();

        Assert.Equal("Alice,N/A", Lines(content)[0]);
    }

    [Fact]
    public void WriteHeaders_ObjectType_DefersAndUsesRuntimeShapeOnFirstWrite()
    {
        var opts = new SerializationOptions { IncludeHeaders = true };

        using MemoryStream stream = new();
        using (CsvObjectWriter<object> writer = new(stream, opts))
        {
            writer.WriteHeaders();
            writer.WriteObject(new RuntimeShape("Alice", new DateOnly(2026, 7, 14), true));
        }

        string[] lines = Lines(ReadAll(stream));
        Assert.Equal("Name,Date,IsEnabled", lines[0]);
        Assert.Equal("Alice,2026-07-14,True", lines[1]);
    }

    [Fact]
    public void WriteObject_NullableTypedProperties_WhenNull_UseBlankValue()
    {
        var row = new NullableRow(null, null, null, null, null);

        var opts = new SerializationOptions { IncludeHeaders = false, BlankValue = "N/A" };
        using MemoryStream stream = new();
        using (CsvObjectWriter<NullableRow> writer = new(stream, opts))
        {
            writer.WriteObject(row);
        }

        Assert.Equal("N/A,N/A,N/A,N/A,N/A", Lines(ReadAll(stream))[0]);
    }

    [Fact]
    public void WritesDynamicResults_TypedValues_AndMissingKeys_AreFormattedOrBlank()
    {
        IDictionary<string, object?> row1 = new System.Dynamic.ExpandoObject();
        row1["Name"] = "Alice";
        row1["UpdatedAt"] = new DateTime(2026, 7, 14, 9, 30, 0);
        row1["Anniversary"] = new DateOnly(2026, 7, 1);
        row1["IsActive"] = true;
        row1["Amount"] = 1234.56m;

        IDictionary<string, object?> row2 = new System.Dynamic.ExpandoObject();
        row2["Name"] = "Bob";

        var opts = new SerializationOptions { IncludeHeaders = true, BlankValue = "N/A" };
        using MemoryStream stream = new();
        using (CsvObjectWriter<dynamic> writer = new(stream, opts))
        {
            writer.WriteHeaders();
            writer.WriteObject(row1);
            writer.WriteObject(row2);
        }

        string[] lines = Lines(ReadAll(stream));
        Assert.Equal("Name,UpdatedAt,Anniversary,IsActive,Amount", lines[0]);
        Assert.Equal("Alice,2026-07-14 09:30:00,2026-07-01,True,1234.56", lines[1]);
        Assert.Equal("Bob,N/A,N/A,N/A,N/A", lines[2]);
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
    public async Task ToCsvStreamAsync_StreamsAllItems()
    {
        Row[] rows =
        [
            new("Alice", 30, 0m, true, new DateTime(2024, 1, 1), new DateOnly(2000, 1, 1), null),
            new("Bob", 25, 0m, false, new DateTime(2024, 1, 1), new DateOnly(2000, 1, 1), null),
        ];

        using var resultStream = (MemoryStream)await ToAsync(rows).ToCsvStreamAsync(
            new SerializationOptions { IncludeHeaders = true }, TestContext.Current.CancellationToken);

        string content = await new StreamReader(resultStream).ReadToEndAsync(TestContext.Current.CancellationToken);
        string[] lines = Lines(content);

        Assert.Equal("Name,Age,Balance,IsActive,CreatedAt,BirthDate,Notes", lines[0]);
        Assert.StartsWith("Alice,", lines[1]);
        Assert.StartsWith("Bob,", lines[2]);
    }

    [Fact]
    public async Task ToCsvStreamAsync_StreamOverload_StreamsAllItems()
    {
        Row[] rows =
        [
            new("Alice", 30, 0m, true, new DateTime(2024, 1, 1), new DateOnly(2000, 1, 1), null),
            new("Bob", 25, 0m, false, new DateTime(2024, 1, 1), new DateOnly(2000, 1, 1), null),
        ];

        using MemoryStream stream = new();
        await ToAsync(rows).ToCsvStreamAsync(stream, options: null, TestContext.Current.CancellationToken);

        string[] lines = Lines(ReadAll(stream));
        Assert.Equal("Name,Age,Balance,IsActive,CreatedAt,BirthDate,Notes", lines[0]);
        Assert.StartsWith("Alice,", lines[1]);
        Assert.StartsWith("Bob,", lines[2]);
    }

    [Fact]
    public void ToCsvStream_DefaultOverloads_WriteToProvidedAndReturnedStreams()
    {
        Row[] rows =
        [
            new("Alice", 30, 0m, true, new DateTime(2024, 1, 1), new DateOnly(2000, 1, 1), null),
            new("Bob", 25, 0m, false, new DateTime(2024, 1, 1), new DateOnly(2000, 1, 1), null),
        ];

        using MemoryStream provided = new();
        rows.AsQueryable().ToCsvStream(provided);
        Assert.Equal(3, Lines(ReadAll(provided)).Length);

        using Stream returned = rows.AsQueryable().ToCsvStream();
        Assert.Equal(3, Lines(ReadAll(returned)).Length);
    }

    [Fact]
    public void ToCsvStream_ConvenienceOverloads_WithFilterSortAndTake_WriteExpectedRows()
    {
        Row[] rows =
        [
            new("Alice", 30, 0m, true, new DateTime(2024, 1, 1), new DateOnly(2000, 1, 1), null),
            new("Bob", 25, 0m, false, new DateTime(2024, 1, 1), new DateOnly(2000, 1, 1), null)
        ];

        var filter = new SimpleFilter("Age", QueryOperator.GreaterThan, 24);
        SortComponent[] sorts = [new("Age", SortDirection.Ascending)];

        using Stream returnedStream = rows.AsQueryable().ToCsvStream(
            filter,
            sorts,
            maxRows: 1,
            System.Linq.Dynamic.Core.ParsingConfig.Default,
            new SerializationOptions { IncludeHeaders = false });

        string[] returnedLines = Lines(ReadAll(returnedStream));
        Assert.Single(returnedLines);
        Assert.StartsWith("Bob,", returnedLines[0]);

        using MemoryStream providedStream = new();
        rows.AsQueryable().ToCsvStream(
            providedStream,
            filter,
            sorts,
            maxRows: 1,
            System.Linq.Dynamic.Core.ParsingConfig.Default,
            new SerializationOptions { IncludeHeaders = false });

        string[] providedLines = Lines(ReadAll(providedStream));
        Assert.Single(providedLines);
        Assert.StartsWith("Bob,", providedLines[0]);
    }

    [Fact]
    public void ToCsvStream_MainOverload_NullArguments_Throws()
    {
        Row[] rows = [new("Alice", 30, 0m, true, new DateTime(2024, 1, 1), new DateOnly(2000, 1, 1), null)];
        IQueryable<Row> query = rows.AsQueryable();

        Assert.Throws<ArgumentNullException>(() =>
            query.ToCsvStream(new MemoryStream(), null, null, null, null!, System.Linq.Dynamic.Core.ParsingConfig.Default, new SerializationOptions()));

        Assert.Throws<ArgumentNullException>(() =>
            query.ToCsvStream(new MemoryStream(), null, null, null, new ValueManager(), null!, new SerializationOptions()));

        Assert.Throws<ArgumentNullException>(() =>
            query.ToCsvStream(new MemoryStream(), null, null, null, new ValueManager(), System.Linq.Dynamic.Core.ParsingConfig.Default, null!));
    }
}
