using TcfOss.Filtering.Contracts;

namespace TcfOss.Filtering.Programmatic;

public record MultiSearchFilter<T>(string Field)
    : SearchFilter(Field)
{
    public List<T> Values { get; set; } = [];
    public bool Negated { get; set; } = false;

    public MultiSearchFilter(string field, List<T> values) : this(field)
    {
        Values = values;
    }

    public override Filter? ToContractFilter(MappingOptions options)
    {
        List<T> filteredValues = Values;
        if (options.MultiSearchOmitNulls)
        {
            filteredValues = [.. filteredValues.Where(v => v != null)];
        }
        List<string> stringValues = [.. filteredValues.Select(v => v?.ToString() ?? string.Empty)];

        if (options.MultiSearchOmitEmptyStrings)
        {
            stringValues = [.. stringValues.Where(v => !string.IsNullOrEmpty(v))];
        }

        if (stringValues.Count == 0)
        {
            return null;
        }

        return new SetFilter
        {
            Field = Field,
            Values = [.. stringValues],
            Negated = Negated
        };
    }
}
