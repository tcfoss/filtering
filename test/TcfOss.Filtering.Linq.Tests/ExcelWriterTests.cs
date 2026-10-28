using System.Globalization;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using TcfOss.Filtering.XlsxOutput;

namespace TcfOss.Filtering.Linq.Tests;

public class ExcelWriterTests
{
    private sealed class IndexLike
    {
        public override string ToString() => "0";
    }

    private sealed class NullFormattable : IFormattable
    {
        public string ToString(string? format, IFormatProvider? formatProvider) => null!;
        public override string ToString() => null!;
    }

    private sealed class NullTextValue
    {
        public override string ToString() => null!;
    }

    private sealed class TestableExcelWriter(Stream stream, SerializationOptions options) : ExcelWriter(stream, options)
    {
        public void WriteCellDirect(object value) => WriteCell(value);
        public void WriteOtherValueDirect(object value) => WriteCellOther(value);
    }

    private static string[] GetSharedStrings(WorkbookPart workbookPart)
    {
        SharedStringTablePart? part = workbookPart.SharedStringTablePart;
        if (part?.SharedStringTable is null)
        {
            return [];
        }

        return [.. part.SharedStringTable.Elements<SharedStringItem>().Select(i => i.Text?.Text ?? string.Empty)];
    }

    private static List<Cell> GetCells(WorkbookPart workbookPart, int worksheetIndex, uint rowIndex)
    {
        WorksheetPart worksheet = workbookPart.WorksheetParts.ElementAt(worksheetIndex);
        SheetData sheetData = (worksheet.Worksheet ?? throw new InvalidOperationException("Worksheet not found."))
            .GetFirstChild<SheetData>()
            ?? throw new InvalidOperationException("SheetData not found.");

        Row row = sheetData.Elements<Row>().Single(r => r.RowIndex?.Value == rowIndex);
        return [.. row.Elements<Cell>()];
    }

    [Fact]
    public void OpenWorksheet_WhenCalledAgain_CreatesSecondSheet_AndCloseIsIdempotent()
    {
        using MemoryStream stream = new();
        var writer = new ExcelWriter(stream, new SerializationOptions());

        writer.WriteRow(["first"]);
        writer.OpenWorksheet();
        writer.WriteRow(["second"]);

        writer.Close();
        writer.Close();

        stream.Position = 0;
        using SpreadsheetDocument doc = SpreadsheetDocument.Open(stream, false);

        WorkbookPart workbookPart = doc.WorkbookPart ?? throw new InvalidOperationException("WorkbookPart not found.");
        Workbook workbook = workbookPart.Workbook ?? throw new InvalidOperationException("Workbook not found.");
        Sheets sheetsElement = workbook.Sheets ?? throw new InvalidOperationException("Sheets not found.");
        List<Sheet> sheets = [.. sheetsElement.Elements<Sheet>()];
        Assert.Equal(2, sheets.Count);
        Assert.Equal("Sheet1", sheets[0].Name?.Value);
        Assert.Equal("Sheet2", sheets[1].Name?.Value);
    }

    [Fact]
    public void WriteRow_MixedValueTypes_WritesExpectedAttributesAndValues()
    {
        DateTime dateTime = new(2026, 7, 14, 11, 0, 0, DateTimeKind.Utc);
        DateOnly dateOnly = new(2026, 7, 14);

        using MemoryStream stream = new();
        var writer = new ExcelWriter(stream, new SerializationOptions());

        writer.WriteRow(["repeat", 123, true, dateTime, dateOnly, new IndexLike(), false]);
        writer.WriteRow(["repeat"]);
        writer.Close();

        stream.Position = 0;
        using SpreadsheetDocument doc = SpreadsheetDocument.Open(stream, false);
        WorkbookPart workbookPart = doc.WorkbookPart!;
        string[] sharedStrings = GetSharedStrings(workbookPart);

        List<Cell> firstRow = GetCells(workbookPart, worksheetIndex: 0, rowIndex: 1);
        List<Cell> secondRow = GetCells(workbookPart, worksheetIndex: 0, rowIndex: 2);

        Assert.Equal(CellValues.SharedString, firstRow[0].DataType?.Value);
        Assert.Equal("repeat", sharedStrings[int.Parse(firstRow[0].CellValue!.Text)]);

        Assert.Equal(CellValues.Number, firstRow[1].DataType?.Value);
        Assert.Equal("123", firstRow[1].CellValue?.Text);

        Assert.Equal(CellValues.Boolean, firstRow[2].DataType?.Value);
        Assert.Equal("1", firstRow[2].CellValue?.Text);

        Assert.Equal((uint)3, firstRow[3].StyleIndex?.Value);
        Assert.Equal(dateTime.ToOADate().ToString(CultureInfo.InvariantCulture), firstRow[3].CellValue?.Text);

        Assert.Equal((uint)2, firstRow[4].StyleIndex?.Value);
        Assert.Equal(dateOnly.ToDateTime(TimeOnly.MinValue).ToOADate().ToString(CultureInfo.InvariantCulture), firstRow[4].CellValue?.Text);

        Assert.Equal(CellValues.SharedString, firstRow[5].DataType?.Value);
        Assert.Equal("0", sharedStrings[int.Parse(firstRow[5].CellValue!.Text)]);

        Assert.Equal(CellValues.Boolean, firstRow[6].DataType?.Value);
        Assert.Equal("0", firstRow[6].CellValue?.Text);

        Assert.Equal(firstRow[0].CellValue?.Text, secondRow[0].CellValue?.Text);
    }

    [Fact]
    public void WriteRow_DecimalUsesInvariantCulture()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            using MemoryStream stream = new();
            var writer = new ExcelWriter(stream, new SerializationOptions());
            writer.WriteRow([19.99m]);
            writer.Close();

            stream.Position = 0;
            using SpreadsheetDocument doc = SpreadsheetDocument.Open(stream, false);
            Cell cell = GetCells(doc.WorkbookPart!, worksheetIndex: 0, rowIndex: 1)[0];

            Assert.Equal("19.99", cell.CellValue?.Text);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void WriteRow_NullCustomFormatting_UsesBlankValue()
    {
        using MemoryStream stream = new();
        var writer = new TestableExcelWriter(stream, new SerializationOptions { BlankValue = "N/A" });
        writer.WriteRow([new NullFormattable(), new NullTextValue()]);
        writer.Close();

        stream.Position = 0;
        using SpreadsheetDocument doc = SpreadsheetDocument.Open(stream, false);
        string[] sharedStrings = GetSharedStrings(doc.WorkbookPart!);
        List<Cell> cells = GetCells(doc.WorkbookPart!, worksheetIndex: 0, rowIndex: 1);

        Assert.All(cells, cell => Assert.Equal("N/A", sharedStrings[int.Parse(cell.CellValue!.Text)]));
    }

    [Fact]
    public void WriteOtherValueDirect_NullToString_WritesConfiguredBlankValue()
    {
        using MemoryStream stream = new();
        using (var writer = new TestableExcelWriter(stream, new SerializationOptions { BlankValue = "N/A" }))
        {
            writer.WriteOtherValueDirect(new NullTextValue());
        }

        stream.Position = 0;
        using SpreadsheetDocument doc = SpreadsheetDocument.Open(stream, false);
        using Stream worksheetStream = doc.WorkbookPart!.WorksheetParts.Single().GetStream();
        var worksheet = System.Xml.Linq.XDocument.Load(worksheetStream);
        var value = Assert.Single(worksheet.Descendants(System.Xml.Linq.XName.Get("v", "http://schemas.openxmlformats.org/spreadsheetml/2006/main")));

        Assert.Equal("N/A", value.Value);
    }

    [Fact]
    public void WriteRow_AfterClose_ThrowsInvalidOperationException()
    {
        using MemoryStream stream = new();
        var writer = new ExcelWriter(stream, new SerializationOptions());
        writer.Close();

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => writer.WriteRow(["x"]));

        Assert.Equal("Cannot write to a closed worksheet.", ex.Message);
    }

    [Fact]
    public void WriteCellDirect_AfterClose_ThrowsInvalidOperationException()
    {
        using MemoryStream stream = new();
        var writer = new TestableExcelWriter(stream, new SerializationOptions());
        writer.Close();

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => writer.WriteCellDirect("x"));

        Assert.Equal("Cannot write to a closed worksheet.", ex.Message);
    }
}
