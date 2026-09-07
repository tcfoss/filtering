using System.Globalization;

namespace TcfOss.Filtering.Programmatic.DataTypes;

public record DateTypeRange : TypeRange<DateOnly>
{
    public override string ConvertValueToString(DateOnly value, MappingOptions options)
    {
        return value.ToString(options.DateFormat, CultureInfo.InvariantCulture);
    }
}
