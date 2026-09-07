namespace TcfOss.Filtering.Linq;

public record DynamicDataRequest() : DataRequest
{
    public required string[] RequestedFields { get; init; }
}
