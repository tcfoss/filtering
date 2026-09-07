using System.Diagnostics.CodeAnalysis;

namespace TcfOss.Filtering.Linq;

public record SimpleFilter() : IFilter
{
    public required string Field { get; init; }
    public required QueryOperator Operator { get; init; }
    public required object Value { get; init; }

    [SetsRequiredMembers]
    public SimpleFilter(string field, QueryOperator op, object value) : this()
    {
        Field = field;
        Operator = op;
        Value = value;
    }

    public string ToDynamicLinq(IManageValues valueManager)
    {
        return Operator.ToDynamicLinq(Field, Value, valueManager);
    }
}
