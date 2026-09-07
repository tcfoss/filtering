namespace TcfOss.Filtering.XlsxOutput;

public class SerializationOptions
{
    public string BlankValue { get; init; } = "";
    public bool IncludeHeaders { get; init; } = true;
    public IEnumerable<string>? Headers { get; init; }
}
