using System.Globalization;

namespace TcfOss.Filtering.Programmatic.DataTypes;

public record DateTimeTypeRange : TypeRange<DateTime>
{
    public override string ConvertValueToString(DateTime value, MappingOptions options)
    {
        return value.ToString(options.DateTimeFormat, CultureInfo.InvariantCulture);
    }
}
