using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TcfOss.Filtering.Contracts.Newtonsoft;

[ExcludeFromCodeCoverage]
public class FilterConverter : JsonConverter<Filter>
{
    public override bool CanWrite => false;

    public override Filter? ReadJson(JsonReader reader, Type objectType, Filter? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        string filterType = jsonObject["filterType"]?.Value<string>() ?? throw new FilterDeserializationException("Missing 'filterType' discriminator");

        // The `serializer.Populate` call will fill in the actual values, temporarily set to null!
        Filter filterDto = filterType.ToLowerInvariant() switch
        {
            FilterTypes.Simple => new SimpleFilter() { Field = null!, Operator = null! },
            FilterTypes.Composite => new CompositeFilter() { LogicalOperator = null!, Filters = null! },
            FilterTypes.Set => new SetFilter() { Field = null!, Values = null! },
            FilterTypes.Range => new RangeFilter() { Field = null!, ValueFrom = null!, ValueTo = null! },
            FilterTypes.Quantified => new QuantifiedFilter() { Field = null!, SubFilter = null!, Operator = null!, IsNegated = false, IsNullable = false },
            _ => throw new FilterDeserializationException($"Unknown filter type: {filterType}")
        };

        serializer.Populate(jsonObject.CreateReader(), filterDto);

        if (filterDto is SimpleFilter { Field: null })
        {
            throw new FilterDeserializationException("Missing required property 'field'");
        }
        if (filterDto is SimpleFilter { Operator: null })
        {
            throw new FilterDeserializationException("Missing required property 'operator'");
        }
        if (filterDto is CompositeFilter { LogicalOperator: null })
        {
            throw new FilterDeserializationException("Missing required property 'logicalOperator'");
        }
        if (filterDto is CompositeFilter { Filters: null })
        {
            throw new FilterDeserializationException("Missing required property 'filters'");
        }
        if (filterDto is SetFilter { Field: null })
        {
            throw new FilterDeserializationException("Missing required property 'field'");
        }
        if (filterDto is SetFilter { Values: null })
        {
            throw new FilterDeserializationException("Missing required property 'values'");
        }
        if (filterDto is RangeFilter { Field: null })
        {
            throw new FilterDeserializationException("Missing required property 'field'");
        }
        if (filterDto is RangeFilter { ValueFrom: null })
        {
            throw new FilterDeserializationException("Missing required property 'valueFrom'");
        }
        if (filterDto is RangeFilter { ValueTo: null })
        {
            throw new FilterDeserializationException("Missing required property 'valueTo'");
        }
        if (filterDto is QuantifiedFilter { Field: null })
        {
            throw new FilterDeserializationException("Missing required property 'field'");
        }
        if (filterDto is QuantifiedFilter { Operator: null })
        {
            throw new FilterDeserializationException("Missing required property 'operator'");
        }
        if (filterDto is QuantifiedFilter { SubFilter: null })
        {
            throw new FilterDeserializationException("Missing required property 'subFilter'");
        }

        return filterDto;
    }

    public override void WriteJson(JsonWriter writer, Filter? value, JsonSerializer serializer) => throw new UnreachableException();
}
