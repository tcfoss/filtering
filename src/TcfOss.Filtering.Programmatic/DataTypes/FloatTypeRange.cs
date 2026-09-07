namespace TcfOss.Filtering.Programmatic.DataTypes;

public record FloatTypeRange : TypeRange<float>
{
    public override string ConvertValueToString(float value, MappingOptions options)
    {
        return value.ToString();
    }
}
