using System.Diagnostics.CodeAnalysis;

namespace TcfOss.Filtering.Linq;

public record SortComponent()
{
    public required string Field { get; init; }
    public required SortDirection Direction { get; init; }

    [SetsRequiredMembers]
    public SortComponent(string field, SortDirection direction) : this()
    {
        Field = field;
        Direction = direction;
    }

    public string ToDynamicLinq()
    {
        return $"{Field} {(Direction == SortDirection.Ascending ? "asc" : "desc")}";
    }
}
