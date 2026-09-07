namespace TcfOss.Filtering.Contracts;

public class FilterMappingException(string message, Exception? innerException = null) : Exception(message, innerException)
{
    public const string UnknownFilterOperatorPattern = "Unknown operator: '{0}'";
    public const string UnknownLogicalOperatorPattern = "Unknown logical operator: '{0}'";
    public const string UnknownFilterTypePattern = "Unknown filter type: '{0}'";
    public const string UnknownQuantifiedOperatorPattern = "Unknown quantified operator: '{0}'";
    public const string InvalidValuePattern = "Invalid value for field '{0}': '{1}'";
    public const string NullValueNotAllowedPattern = "Value cannot be null for operator '{0}'";
    public const string UnknownFieldPattern = "Unknown field: '{0}'";
    public const string UnknownSortDirectionPattern = "Unknown sort direction: '{0}'";
    public const string UnsupportedCollectionNavigationPattern = "'{0}' is a collection; only '.Count' and '.Length' navigation is supported.";
    public const string UnsupportedFieldTypePattern = "Field '{0}' has an unsupported type '{1}' for filtering.";
    public const string EmptyValuesPattern = "Values cannot be empty for field '{0}'";
    public const string InvalidFieldNamePattern = "Field name '{0}' is invalid";
    public const string InvalidPagePattern = "Page must be greater than or equal to 1, but was {0}";
    public const string InvalidPageSizePattern = "PageSize must be greater than or equal to 1, but was {0}";
    public const string EmptyRequestedFieldsPattern = "RequestedFields cannot be empty";
}
