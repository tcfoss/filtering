using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TcfOss.Filtering.Contracts;

public record SimpleFilter() : Filter
{
    [JsonPropertyName("filterType")]
    public override string FilterType => FilterTypes.Simple;
    public required string Field { get; init; }
    public required string Operator { get; init; }
    public string? Value { get; init; }

    [SetsRequiredMembers]
    public SimpleFilter(string field, string op, string? value = null)
        : this()
    {
        Field = field;
        Operator = op;
        Value = value;
    }
}
