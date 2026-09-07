namespace TcfOss.Filtering.CsvOutput;

public class SerializationOptions
{
    public string BlankValue { get; init; } = "";
    public string Delimiter { get; init; } = ",";
    public bool IncludeHeaders { get; init; } = true;
    public IEnumerable<string>? Headers { get; init; }
}
