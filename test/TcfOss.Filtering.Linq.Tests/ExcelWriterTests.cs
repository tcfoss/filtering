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

    private sealed class TestableExcelWriter(Stream stream, SerializationOptions options) : ExcelWriter(stream, options)
    {
        public void WriteCellDirect(object value) => WriteCell(value);
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

        writer.WriteRow(["repeat", 123, true, dateTime, dateOnly, new IndexLike()]);
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
        Assert.Equal(dateTime.ToOADate().ToString(System.Globalization.CultureInfo.InvariantCulture), firstRow[3].CellValue?.Text);

        Assert.Equal((uint)2, firstRow[4].StyleIndex?.Value);
        Assert.Equal(dateOnly.ToDateTime(TimeOnly.MinValue).ToOADate().ToString(System.Globalization.CultureInfo.InvariantCulture), firstRow[4].CellValue?.Text);

        Assert.Equal(CellValues.SharedString, firstRow[5].DataType?.Value);
        Assert.Equal("0", firstRow[5].CellValue?.Text);

        Assert.Equal(firstRow[0].CellValue?.Text, secondRow[0].CellValue?.Text);
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
