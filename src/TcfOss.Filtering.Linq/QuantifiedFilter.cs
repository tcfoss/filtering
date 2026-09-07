using System.Diagnostics.CodeAnalysis;

namespace TcfOss.Filtering.Linq;

public record QuantifiedFilter() : IFilter
{
    public required QuantifiedOperator QuantifiedOperator { get; init; }
    public required string FieldName { get; init; }
    public required IFilter SubFilter { get; init; }
    public bool IsNullable { get; init; }
    public bool IsNegated { get; init; }

    [SetsRequiredMembers]
    public QuantifiedFilter(string fieldName, IFilter subFilter, QuantifiedOperator quantifiedOperator)
        : this()
    {
        FieldName = fieldName;
        SubFilter = subFilter;
        QuantifiedOperator = quantifiedOperator;
    }

    public string ToDynamicLinq(IManageValues valueManager)
    {
        string negatedString = IsNegated ? "!" : "";
        string opString = QuantifiedOperator == QuantifiedOperator.Any ? "Any" : "All";
        string subFilterString = SubFilter.ToDynamicLinq(valueManager);
        if (IsNullable)
        {
            return $"{negatedString}({FieldName} != null and {FieldName}.{opString}({subFilterString}))";
        }
        return $"{negatedString}{FieldName}.{opString}({subFilterString})";
    }
}
