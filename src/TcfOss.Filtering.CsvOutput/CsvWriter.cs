using System.Text;

namespace TcfOss.Filtering.CsvOutput;

public class CsvWriter(Stream stream, SerializationOptions options) : IDisposable
{
    private readonly StreamWriter _writer = new(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), leaveOpen: true);
    private protected readonly SerializationOptions _options = options;
    private bool _disposed;

    public void WriteRow(IEnumerable<string> values)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _writer.WriteLine(string.Join(_options.Delimiter, values.Select(EscapeField)));
    }

    private string EscapeField(string value)
    {
        if (value.Contains(_options.Delimiter) || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_disposed)
        {
            return;
        }
        _writer.Flush();
        _writer.Dispose();
        _disposed = true;
    }
}
