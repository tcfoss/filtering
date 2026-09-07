using System.Diagnostics.CodeAnalysis;

namespace TcfOss.Filtering.Linq;

public record CompositeFilter() : IFilter
{
    public required LogicalOperator LogicalOperator { get; init; }
    public required IFilter[] Filters { get; init; }

    [SetsRequiredMembers]
    public CompositeFilter(IFilter[] filters, LogicalOperator logicalOperator) : this()
    {
        Filters = filters;
        LogicalOperator = logicalOperator;
    }

    public string ToDynamicLinq(IManageValues valueManager)
    {
        string operatorString = LogicalOperator == LogicalOperator.And ? " and " : " or ";
        return $"({string.Join(operatorString, Filters.Select(f => f.ToDynamicLinq(valueManager)))})";
    }
}
