using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace TcfOss.Filtering.XlsxOutput;

public class ExcelWriter : IDisposable
{
    protected static readonly OpenXmlAttribute[] s_stringAttributes =
    [
        new("t", string.Empty, "s")
    ];

    protected static readonly OpenXmlAttribute[] s_numberAttributes =
    [
        new("t", string.Empty, "n")
    ];

    protected static readonly OpenXmlAttribute[] s_booleanAttributes =
    [
        new("t", string.Empty, "b")
    ];

    protected static readonly OpenXmlAttribute[] s_dateAttributes =
    [
        new("s", string.Empty, "2")
    ];

    protected static readonly OpenXmlAttribute[] s_dateTimeAttributes =
    [
        new("s", string.Empty, "3")
    ];

    private bool _disposed;
    private readonly Dictionary<string, uint> _sharedStringTable = [];
    private uint _totalStringReferences;
    private protected readonly SerializationOptions _options;

    private readonly SpreadsheetDocument _spreadsheetDocument;
    private readonly List<WorksheetPart> _worksheetParts = [];
    private protected OpenXmlWriter? _writer;
    private protected uint _currentRow = 1;

    public ExcelWriter(Stream stream, SerializationOptions options)
    {
        _options = options;

        _spreadsheetDocument = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, autoSave: true);
        _spreadsheetDocument.AddWorkbookPart();

        OpenWorksheet();
    }

    [MemberNotNull(nameof(_writer))]
    public void OpenWorksheet()
    {
        if (_writer is not null)
        {
            CloseWorksheet();
        }
        WorksheetPart worksheetPart = _spreadsheetDocument.WorkbookPart!.AddNewPart<WorksheetPart>();
        _worksheetParts.Add(worksheetPart);
        _writer = OpenXmlWriter.Create(worksheetPart);
        _writer.WriteStartElement(new Worksheet());
        _writer.WriteStartElement(new SheetData());
    }

    private void CloseWorksheet()
    {
        _writer!.WriteEndElement(); // SheetData
        _writer.WriteEndElement(); // Worksheet
        _writer.Close();
        _writer = null;
    }

    public void Close()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            if (_writer is not null)
            {
                CloseWorksheet();
            }

            _writer = OpenXmlWriter.Create(_spreadsheetDocument.WorkbookPart!);
            _writer.WriteStartElement(new Workbook());
            _writer.WriteStartElement(new Sheets());

            for (int i = 0; i < _worksheetParts.Count; i++)
            {
                WorksheetPart worksheetPart = _worksheetParts[i];
                string sheetId = _spreadsheetDocument.WorkbookPart!.GetIdOfPart(worksheetPart);
                _writer.WriteElement(new Sheet()
                {
                    Name = $"Sheet{i + 1}",
                    SheetId = (uint)(i + 1),
                    Id = sheetId
                });
            }

            _writer.WriteEndElement(); // Sheets
            _writer.WriteEndElement(); // Workbook
            _writer.Close();
            _writer = null;

            SharedStringTablePart sharedStringTablePart = _spreadsheetDocument.WorkbookPart!.AddNewPart<SharedStringTablePart>();
            using (OpenXmlWriter sharedStringWriter = OpenXmlWriter.Create(sharedStringTablePart))
            {
                sharedStringWriter.WriteStartElement(new SharedStringTable()
                {
                    Count = _totalStringReferences,
                    UniqueCount = (uint)_sharedStringTable.Count
                });

                foreach (string entry in _sharedStringTable.OrderBy(kvp => kvp.Value).Select(kvp => kvp.Key))
                {
                    sharedStringWriter.WriteStartElement(new SharedStringItem());
                    sharedStringWriter.WriteElement(new Text(entry));
                    sharedStringWriter.WriteEndElement(); // SharedStringItem
                }
                sharedStringWriter.WriteEndElement(); // SharedStringTable
            }

            WorkbookStylesPart stylesPart = _spreadsheetDocument.WorkbookPart!.AddNewPart<WorkbookStylesPart>();

            stylesPart.Stylesheet = new Stylesheet()
            {
                Fonts = new Fonts(new Font()) { Count = 1 },
                Fills = new Fills(new Fill(new PatternFill { PatternType = PatternValues.None })) { Count = 1 },
                Borders = new Borders(new Border()) { Count = 1 },
                CellStyleFormats = new CellStyleFormats(new CellFormat()) { Count = 1 },
                NumberingFormats = new NumberingFormats(
                    new NumberingFormat()
                    {
                        NumberFormatId = 164,
                        FormatCode = "yyyy-mm-dd hh:mm:ss"
                    }
                )
                {
                    Count = 1
                },
                CellFormats = GetCellFormats()
            };
        }
        finally
        {
            _spreadsheetDocument.Dispose();
            _disposed = true;
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Close();
    }

    public void WriteRow(IEnumerable<object> values)
    {
        if (_writer is null)
        {
            throw new InvalidOperationException("Cannot write to a closed worksheet.");
        }

        _writer.WriteStartElement(new Row { RowIndex = _currentRow });

        foreach (object value in values)
        {
            WriteCell(value);
        }

        _writer.WriteEndElement(); // Row
        _currentRow++;
    }

    protected void WriteCell(object value)
    {
        if (_writer is null)
        {
            throw new InvalidOperationException("Cannot write to a closed worksheet.");
        }

        _writer.WriteStartElement(new Cell(), GetAttributesForValue(value));

        if (value is string stringValue)
        {
            WriteCellString(stringValue);
        }
        else if (value is bool boolValue)
        {
            WriteCellOther(boolValue ? "1" : "0");
        }
        else if (value is DateTime dateTimeValue)
        {
            WriteCellDateTime(dateTimeValue);
        }
        else if (value is DateOnly dateOnlyValue)
        {
            WriteCellDateOnly(dateOnlyValue);
        }
        else
        {
            WriteCellOther(value);
        }

        _writer.WriteEndElement(); // Cell
    }

    protected void WriteCellString(string value)
    {
        uint sharedStringIndex = GetSharedStringIndex(value);
        _writer!.WriteElement(new CellValue(sharedStringIndex.ToString()));
    }

    protected void WriteCellDateTime(DateTime value)
    {
        _writer!.WriteElement(new CellValue(value.ToOADate().ToString(CultureInfo.InvariantCulture)));
    }

    protected void WriteCellDateOnly(DateOnly value)
    {
        _writer!.WriteElement(new CellValue(value.ToDateTime(TimeOnly.MinValue).ToOADate().ToString(CultureInfo.InvariantCulture)));
    }

    protected void WriteCellOther(object value)
    {
        _writer!.WriteElement(new CellValue(value.ToString() ?? _options.BlankValue));
    }

    private static OpenXmlAttribute[] GetAttributesForValue(object value)
    {
        return value switch
        {
            string => s_stringAttributes,
            int or uint or long or ulong or short or ushort or byte or sbyte or float or double or decimal => s_numberAttributes,
            bool => s_booleanAttributes,
            DateTime => s_dateTimeAttributes,
            DateOnly => s_dateAttributes,
            _ => s_stringAttributes
        };
    }

    private uint GetSharedStringIndex(string value)
    {
        _totalStringReferences++;
        if (_sharedStringTable.TryGetValue(value, out uint index))
        {
            return index;
        }

        index = (uint)_sharedStringTable.Count;
        _sharedStringTable[value] = index;
        return index;
    }

    private static CellFormats GetCellFormats()
    {
        var cellFormats = new CellFormats();
        cellFormats.Append(new CellFormat { NumberFormatId = 0, ApplyNumberFormat = true }); // String
        cellFormats.Append(new CellFormat { NumberFormatId = 0, ApplyNumberFormat = true }); // Number
        cellFormats.Append(new CellFormat { NumberFormatId = 14, ApplyNumberFormat = true }); // Date
        cellFormats.Append(new CellFormat { NumberFormatId = 164, ApplyNumberFormat = true }); // DateTime

        return cellFormats;
    }
}
