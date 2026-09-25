using System.Globalization;
using System.Text;

namespace TcfOss.Filtering.Contracts.Errors;

public class FilterException(string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    protected static string Apply(CompositeFormat format, params object[] args)
    {
        return string.Format(CultureInfo.CurrentCulture, format, args);
    }
}

public class UnknownFilterOperatorException(string op, string fieldName, Exception? innerException = null)
    : FilterException(Apply(s_format, op, fieldName), innerException)
{
    public string Operator => op;
    public string FieldName => fieldName;

    private static readonly CompositeFormat s_format = CompositeFormat.Parse("Unknown filter operator: '{0}' for field '{1}'.");
}

public class UnknownLogicalOperatorException(string op, Exception? innerException = null)
    : FilterException(Apply(s_format, op), innerException)
{
    public string Operator => op;

    private static readonly CompositeFormat s_format = CompositeFormat.Parse("Unknown logical operator: '{0}'.");
}

public class UnknownFilterTypeException(string type, Exception? innerException = null)
    : FilterException(Apply(s_format, type), innerException)
{
    public string Type => type;

    private static readonly CompositeFormat s_format = CompositeFormat.Parse("Unknown filter type: '{0}'.");
}

public class UnknownQuantifiedOperatorException(string op, string fieldName, Exception? innerException = null)
    : FilterException(Apply(s_format, op, fieldName), innerException)
{
    public string Operator => op;
    public string FieldName => fieldName;

    private static readonly CompositeFormat s_format = CompositeFormat.Parse("Unknown quantified operator: '{0}' for field '{1}'.");
}

public class InvalidValueException(object value, string fieldName, Exception? innerException = null)
    : FilterException(Apply(s_format, fieldName, value), innerException)
{
    public string FieldName => fieldName;
    public object Value => value;

    private static readonly CompositeFormat s_format = CompositeFormat.Parse("Invalid value '{1}' for field '{0}'.");
}

public class NullNotAllowedException(string op, string fieldName, Exception? innerException = null)
    : FilterException(Apply(s_format, op, fieldName), innerException)
{
    public string Operator => op;
    public string FieldName => fieldName;

    private static readonly CompositeFormat s_format = CompositeFormat.Parse("Value cannot be null for operator '{0}' on field '{1}'.");
}

public class UnknownFieldException(string fieldName, Exception? innerException = null)
    : FilterException(Apply(s_format, fieldName), innerException)
{
    public string FieldName => fieldName;

    private static readonly CompositeFormat s_format = CompositeFormat.Parse("Unknown field: '{0}'.");
}

public class UnknownSortDirectionException(string direction, string fieldName, Exception? innerException = null)
    : FilterException(Apply(s_format, direction, fieldName), innerException)
{
    public string Direction => direction;
    public string FieldName => fieldName;

    private static readonly CompositeFormat s_format = CompositeFormat.Parse("Unknown sort direction: '{0}' for field '{1}'.");
}

public class UnsupportedCollectionNavigationException(string fieldName, string givenProperty, Exception? innerException = null)
    : FilterException(Apply(s_format, fieldName, givenProperty), innerException)
{
    public string FieldName => fieldName;
    public string GivenProperty => givenProperty;

    private static readonly CompositeFormat s_format = CompositeFormat.Parse("Unsupported navigation property '{1}'. Field '{0}' is a collection; only .Count and .Length are supported.");
}

public class UnsupportedFieldTypeException(string fieldName, string fieldType, Exception? innerException = null)
    : FilterException(Apply(s_format, fieldName, fieldType), innerException)
{
    public string FieldName => fieldName;
    public string FieldType => fieldType;

    private static readonly CompositeFormat s_format = CompositeFormat.Parse("Field '{0}' has unsupported type '{1}'.");
}

public class InvalidEmptyValueSetException(string fieldName, Exception? innerException = null)
    : FilterException(Apply(s_format, fieldName), innerException)
{
    public string FieldName => fieldName;

    private static readonly CompositeFormat s_format = CompositeFormat.Parse("Empty value set is not allowed for field '{0}'.");
}

public class InvalidFieldNameException(string fieldName, Exception? innerException = null)
    : FilterException(Apply(s_format, fieldName), innerException)
{
    public string FieldName => fieldName;

    private static readonly CompositeFormat s_format = CompositeFormat.Parse("Invalid field name: '{0}'.");
}

public class InvalidPageException(int pageNumber, Exception? innerException = null)
    : FilterException(Apply(s_format, pageNumber), innerException)
{
    public int PageNumber => pageNumber;

    private static readonly CompositeFormat s_format = CompositeFormat.Parse("Invalid Page: '{0}'. Page must be greater than or equal to 1.");
}

public class InvalidPageSizeException(int pageSize, Exception? innerException = null)
    : FilterException(Apply(s_format, pageSize), innerException)
{
    public int PageSize => pageSize;

    private static readonly CompositeFormat s_format = CompositeFormat.Parse("Invalid PageSize: '{0}'. Page size must be greater than or equal to 1.");
}

public class EmptyRequestedFieldsException(Exception? innerException = null)
    : FilterException(Apply(s_format), innerException)
{
    private static readonly CompositeFormat s_format = CompositeFormat.Parse("RequestedFields cannot be empty.");
}
