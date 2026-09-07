using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TcfOss.Filtering.Contracts;

public record SetFilter() : Filter
{
    [JsonPropertyName("filterType")]
    public override string FilterType => FilterTypes.Set;
    public required string Field { get; init; }
    public required string[] Values { get; init; }
    public bool Negated { get; init; }

    [SetsRequiredMembers]
    public SetFilter(string field, string[] values, bool negated = false) : this()
    {
        Field = field;
        Values = values;
        Negated = negated;
    }
}
