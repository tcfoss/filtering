using System.Diagnostics.CodeAnalysis;

namespace TcfOss.Filtering.Linq;

public record SetFilter() : IFilter
{
    public required string Field { get; init; }
    public required object[] Values { get; init; }
    public bool Negated { get; init; }

    /// <summary>
    /// The element type of the values, if known. Used to create correctly-typed arrays for Dynamic LINQ.
    /// </summary>
    public Type? ElementType { get; init; }

    [SetsRequiredMembers]
    public SetFilter(string field, object[] values, bool negated = false, Type? elementType = null) : this()
    {
        Field = field;
        Values = values;
        Negated = negated;
        ElementType = elementType;
    }

    public string ToDynamicLinq(IManageValues valueManager)
    {
        // Create a typed array using the element type if available.
        // This allows Dynamic LINQ to properly resolve the Contains method.
        Array typedArray;
        if (ElementType != null)
        {
            // Use the provided element type (which may be nullable)
            typedArray = Array.CreateInstance(ElementType, Values.Length);
            for (int i = 0; i < Values.Length; i++)
            {
                typedArray.SetValue(Values[i], i);
            }
        }
        else if (Values.Length > 0)
        {
            // Infer the element type from the first value if ElementType was not provided
            Type inferredType = Values[0].GetType();
            typedArray = Array.CreateInstance(inferredType, Values.Length);
            for (int i = 0; i < Values.Length; i++)
            {
                typedArray.SetValue(Values[i], i);
            }
        }
        else
        {
            // Fallback to object array if we have no type information
            typedArray = Array.CreateInstance(typeof(object), Values.Length);
        }

        int index = valueManager.GetParameterIndex(typedArray);
        string contains = $"@{index}.Contains({Field})";
        return Negated ? $"!({contains})" : contains;
    }
}
