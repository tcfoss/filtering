using System.Text.Json.Serialization;

namespace TcfOss.Filtering.Contracts;

[JsonConverter(typeof(FilterJsonConverter))]
public abstract record Filter()
{
    [JsonPropertyName("filterType")]
    public abstract string FilterType { get; }
}
