namespace TcfOss.Filtering.Contracts;

public record DynamicDataRequest() : DataRequest
{
    public required string[] RequestedFields { get; init; }
}
