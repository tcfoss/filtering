using System.Diagnostics.CodeAnalysis;

namespace TcfOss.Filtering.Linq;

public record RangeFilter() : IFilter
{
    public required string Field { get; init; }
    public required object ValueFrom { get; init; }
    public required object ValueTo { get; init; }
    public bool Exclusive { get; init; }
    public bool Negated { get; init; }

    [SetsRequiredMembers]
    public RangeFilter(string field, object valueFrom, object valueTo, bool exclusive = false, bool negated = false) : this()
    {
        Field = field;
        ValueFrom = valueFrom;
        ValueTo = valueTo;
        Exclusive = exclusive;
        Negated = negated;
    }

    public string ToDynamicLinq(IManageValues valueManager)
    {
        string fromParam = $"@{valueManager.GetParameterIndex(ValueFrom)}";
        string toParam = $"@{valueManager.GetParameterIndex(ValueTo)}";

        string lower = Exclusive ? $"{Field} > {fromParam}" : $"{Field} >= {fromParam}";
        string upper = Exclusive ? $"{Field} < {toParam}" : $"{Field} <= {toParam}";
        string range = $"({lower} && {upper})";

        return Negated ? $"!{range}" : range;
    }
}
