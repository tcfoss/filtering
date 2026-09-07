using System.Reflection;
using DocumentFormat.OpenXml.Spreadsheet;

namespace TcfOss.Filtering.XlsxOutput;

public class ExcelObjectWriter<T> : ExcelWriter
{
    // Columns are eagerly built when T exposes static properties (the common POCO case).
    // For `dynamic`/object items (no static properties) we defer column discovery until
    // the first object is written and derive columns from its runtime shape.
    private ColumnWritingConfig<T>[]? _columnWritingConfigs;
    private bool _headersPending;
    private bool _headersWritten;

    public ExcelObjectWriter(Stream stream, SerializationOptions options)
        : base(stream, options)
    {
        if (HasStaticProperties(typeof(T)))
        {
            _columnWritingConfigs = GetColumnWritingConfigsFromType(typeof(T));
        }
    }

    public void WriteHeaders()
    {
        if (!_options.IncludeHeaders)
        {
            return;
        }

        if (_writer is null)
        {
            throw new InvalidOperationException("Worksheet is not open.");
        }

        if (_options.Headers != null)
        {
            EmitHeaderRow(_options.Headers);
            _headersWritten = true;
            return;
        }

        if (_columnWritingConfigs != null)
        {
            EmitHeaderRow(_columnWritingConfigs.Select(c => c.Header));
            _headersWritten = true;
            return;
        }

        // Defer until we see the first object and can infer columns from it.
        _headersPending = true;
    }

    public void WriteObject(T obj)
    {
        if (_writer is null)
        {
            throw new InvalidOperationException("Worksheet is not open.");
        }

        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        _columnWritingConfigs ??= GetColumnWritingConfigsFromObject(obj);

        if (_headersPending && !_headersWritten)
        {
            EmitHeaderRow(_columnWritingConfigs.Select(c => c.Header));
            _headersWritten = true;
            _headersPending = false;
        }

        _writer.WriteStartElement(new Row { RowIndex = _currentRow });

        foreach (ColumnWritingConfig<T> columnConfig in _columnWritingConfigs)
        {
            columnConfig.WriteCell(this, obj);
        }

        _writer.WriteEndElement(); // Row
        _currentRow++;
    }

    private void EmitHeaderRow(IEnumerable<string> headers)
    {
        _writer!.WriteStartElement(new Row { RowIndex = _currentRow });
        foreach (string header in headers)
        {
            _writer.WriteStartElement(new Cell(), s_stringAttributes);
            WriteCellString(header);
            _writer.WriteEndElement(); // Cell
        }
        _writer.WriteEndElement(); // Row
        _currentRow++;
    }

    private static bool HasStaticProperties(Type type)
    {
        if (type == typeof(object))
        {
            return false;
        }
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Any(p => p.GetIndexParameters().Length == 0);
    }

    private ColumnWritingConfig<T>[] GetColumnWritingConfigsFromObject(T obj)
    {
        if (obj is IDictionary<string, object?> dict)
        {
            // Columns derived from dictionary keys. Cell formatting is inferred per-value
            // at write time via the base ExcelWriter.WriteCell helper.
            string blankValue = _options.BlankValue;
            return [.. dict.Keys.Select(key =>
                new ColumnWritingConfig<T>(
                    key,
                    (writer, item) =>
                    {
                        var d = (IDictionary<string, object?>)item!;
                        object value = (d.TryGetValue(key, out object? v) ? v : null) ?? blankValue;
                        writer.WriteCellViaBase(value);
                    }))];
        }

        return GetColumnWritingConfigsFromType(obj!.GetType());
    }

    private ColumnWritingConfig<T>[] GetColumnWritingConfigsFromType(Type type)
    {
        PropertyInfo[] properties = [.. type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetIndexParameters().Length == 0)];

        return [.. properties.Select(BuildPropertyColumn)];
    }

    private ColumnWritingConfig<T> BuildPropertyColumn(PropertyInfo property)
    {
        Type propertyType = property.PropertyType;

        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            propertyType = Nullable.GetUnderlyingType(propertyType)!;
        }

        string blankValue = _options.BlankValue;

        if (propertyType == typeof(string))
        {
            return new ColumnWritingConfig<T>(property.Name, (w, obj) =>
            {
                w._writer!.WriteStartElement(new Cell(), s_stringAttributes);
                w.WriteCellString(property.GetValue(obj)?.ToString() ?? blankValue);
                w._writer.WriteEndElement();
            });
        }
        if (propertyType == typeof(int) || propertyType == typeof(uint)
            || propertyType == typeof(long) || propertyType == typeof(ulong)
            || propertyType == typeof(short) || propertyType == typeof(ushort)
            || propertyType == typeof(sbyte) || propertyType == typeof(byte)
            || propertyType == typeof(float) || propertyType == typeof(double)
            || propertyType == typeof(decimal))
        {
            return new ColumnWritingConfig<T>(property.Name, (w, obj) =>
            {
                w._writer!.WriteStartElement(new Cell(), s_numberAttributes);
                object? value = property.GetValue(obj);
                w.WriteCellOther(value ?? blankValue);
                w._writer.WriteEndElement();
            });
        }
        if (propertyType == typeof(bool))
        {
            return new ColumnWritingConfig<T>(property.Name, (w, obj) =>
            {
                w._writer!.WriteStartElement(new Cell(), s_booleanAttributes);
                object? value = property.GetValue(obj);
                if (value is bool b)
                {
                    w.WriteCellOther(b ? "1" : "0");
                }
                else
                {
                    w.WriteCellOther(blankValue);
                }
                w._writer.WriteEndElement();
            });
        }
        if (propertyType == typeof(DateTime))
        {
            return new ColumnWritingConfig<T>(property.Name, (w, obj) =>
            {
                w._writer!.WriteStartElement(new Cell(), s_dateTimeAttributes);
                object? value = property.GetValue(obj);
                if (value is DateTime dt)
                {
                    w.WriteCellDateTime(dt);
                }
                else
                {
                    w.WriteCellOther(blankValue);
                }
                w._writer.WriteEndElement();
            });
        }
        if (propertyType == typeof(DateOnly))
        {
            return new ColumnWritingConfig<T>(property.Name, (w, obj) =>
            {
                w._writer!.WriteStartElement(new Cell(), s_dateAttributes);
                object? value = property.GetValue(obj);
                if (value is DateOnly d)
                {
                    w.WriteCellDateOnly(d);
                }
                else
                {
                    w.WriteCellOther(blankValue);
                }
                w._writer.WriteEndElement();
            });
        }

        return new ColumnWritingConfig<T>(property.Name, (w, obj) =>
        {
            w._writer!.WriteStartElement(new Cell(), s_stringAttributes);
            w.WriteCellString(property.GetValue(obj)?.ToString() ?? blankValue);
            w._writer.WriteEndElement();
        });
    }

    // Exposed for the dictionary-column callback so it can use base WriteCell type dispatch.
    internal void WriteCellViaBase(object value) => WriteCell(value);

    private sealed class ColumnWritingConfig<TObj>(string header, Action<ExcelObjectWriter<TObj>, TObj> writeCell)
    {
        public string Header { get; } = header;
        public Action<ExcelObjectWriter<TObj>, TObj> WriteCell { get; } = writeCell;
    }
}
