using System.Diagnostics.CodeAnalysis;

namespace TcfOss.Filtering.Contracts;

public record DataResult<T>()
{
    public required T[] Data { get; init; }
    public required int TotalCount { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }

    [SetsRequiredMembers]
    public DataResult(T[] data, int totalCount, int page, int pageSize)
        : this()
    {
        Data = data;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }

    [SetsRequiredMembers]
    public DataResult(IEnumerable<T> data, int totalCount, int page, int pageSize)
        : this([.. data], totalCount, page, pageSize)
    {
    }
}
