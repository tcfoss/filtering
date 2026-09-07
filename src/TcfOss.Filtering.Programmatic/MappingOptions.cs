namespace TcfOss.Filtering.Programmatic;

public class MappingOptions
{
    public string DateFormat { get; init; } = "yyyy-MM-dd";
    public string DateTimeFormat { get; init; } = "O";
    public bool NullValueToIsNullFilter { get; init; }
    public bool HandleStringWildcards { get; init; } = true;
    public bool SupportLikeOperator { get; init; } = true;
    public int MaxPageSize { get; init; } = 500;
    public int DefaultPageSize { get; init; } = 20;
    public int DefaultPage { get; init; } = 1;
    public bool MultiSearchOmitNulls { get; init; } = true;
    public bool MultiSearchOmitEmptyStrings { get; init; } = true;
}
