using System.Diagnostics.CodeAnalysis;

namespace TcfOss.Filtering.Contracts;

public record SortComponent()
{
    public required string Field { get; init; }
    public required string Direction { get; init; }

    [SetsRequiredMembers]
    public SortComponent(string field, string direction)
        : this()
    {
        Field = field;
        Direction = direction;
    }
}
