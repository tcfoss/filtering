using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TcfOss.Filtering.Contracts;

public record RangeFilter() : Filter
{
    [JsonPropertyName("filterType")]
    public override string FilterType => FilterTypes.Range;
    public required string Field { get; init; }
    public required string ValueFrom { get; init; }
    public required string ValueTo { get; init; }
    public bool Exclusive { get; init; }
    public bool Negated { get; init; }

    [SetsRequiredMembers]
    public RangeFilter(string field, string valueFrom, string valueTo, bool exclusive = false, bool negated = false)
        : this()
    {
        Field = field;
        ValueFrom = valueFrom;
        ValueTo = valueTo;
        Exclusive = exclusive;
        Negated = negated;
    }
}
