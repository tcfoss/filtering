using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TcfOss.Filtering.Contracts;

public record CompositeFilter() : Filter
{
    [JsonPropertyName("filterType")]
    public override string FilterType => FilterTypes.Composite;
    public required string LogicalOperator { get; init; }
    public required Filter[] Filters { get; init; }

    [SetsRequiredMembers]
    public CompositeFilter(string logicalOperator, Filter[] filters)
        : this()
    {
        LogicalOperator = logicalOperator;
        Filters = filters;
    }
}
