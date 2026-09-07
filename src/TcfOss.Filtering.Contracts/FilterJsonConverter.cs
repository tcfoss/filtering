using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace TcfOss.Filtering.Contracts;

[ExcludeFromCodeCoverage]
public partial class FilterJsonConverter : JsonConverter<Filter>
{
    private static readonly Regex s_dataTypeRegex = GetDataTypeRegex();

    public override Filter? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Utf8JsonReader readerCopy = reader;
        using var doc = JsonDocument.ParseValue(ref readerCopy);

        if (!doc.RootElement.TryGetProperty("filterType", out JsonElement typeElement))
        {
            throw new FilterDeserializationException("Missing 'filterType' discriminator");
        }

        string? filterType = typeElement.GetString();

        try
        {
            return filterType?.ToLowerInvariant() switch
            {
                FilterTypes.Simple => JsonSerializer.Deserialize<SimpleFilter>(ref reader, options),
                FilterTypes.Composite => JsonSerializer.Deserialize<CompositeFilter>(ref reader, options),
                FilterTypes.Set => JsonSerializer.Deserialize<SetFilter>(ref reader, options),
                FilterTypes.Range => JsonSerializer.Deserialize<RangeFilter>(ref reader, options),
                FilterTypes.Quantified => JsonSerializer.Deserialize<QuantifiedFilter>(ref reader, options),
                _ => throw new FilterDeserializationException($"Unknown filter type: {filterType}")
            };
        }
        catch (JsonException ex) when (s_dataTypeRegex.IsMatch(ex.Message))
        {
            throw new FilterDeserializationException(s_dataTypeRegex.Replace(ex.Message, m => m.Groups[1].Value), ex);
        }
    }

    public override void Write(Utf8JsonWriter writer, Filter value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value, value.GetType(), options);

    [GeneratedRegex(@"TcfOss\.Filtering\.\w+\.(\w+)")]
    private static partial Regex GetDataTypeRegex();
}
