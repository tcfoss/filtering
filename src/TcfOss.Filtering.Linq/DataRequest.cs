namespace TcfOss.Filtering.Linq;

public record DataRequest()
{
    public IFilter? Filter { get; init; }
    public SortComponent[] Sorts { get; init; } = [];
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 25;
}
