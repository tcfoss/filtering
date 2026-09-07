namespace TcfOss.Filtering.Programmatic.DataTypes;

public record DecimalTypeRange : TypeRange<decimal>
{
    public override string ConvertValueToString(decimal value, MappingOptions options)
    {
        return value.ToString();
    }
}
