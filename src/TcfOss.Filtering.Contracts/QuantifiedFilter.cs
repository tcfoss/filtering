using System.Text.Json.Serialization;

namespace TcfOss.Filtering.Contracts;

public record QuantifiedFilter() : Filter
{
    [JsonPropertyName("filterType")]
    public override string FilterType => FilterTypes.Quantified;
    public required string Field { get; init; }
    public required string Operator { get; init; }
    public required Filter SubFilter { get; init; }
    public bool IsNegated { get; init; }
    public bool IsNullable { get; init; }
}
