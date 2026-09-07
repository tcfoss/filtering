namespace TcfOss.Filtering.Programmatic.DataTypes;

public record DoubleTypeRange : TypeRange<double>
{
    public override string ConvertValueToString(double value, MappingOptions options)
    {
        return value.ToString();
    }
}
