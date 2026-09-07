namespace TcfOss.Filtering.Programmatic.DataTypes;

public record IntTypeRange : TypeRange<int>
{
    public override string ConvertValueToString(int value, MappingOptions options)
    {
        return value.ToString();
    }
}
