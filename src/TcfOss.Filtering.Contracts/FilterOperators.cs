namespace TcfOss.Filtering.Contracts;

public static class FilterOperators
{
    /// <summary>
    /// Equal to.
    /// </summary>
    public const string EqualTo = "eq";
    /// <summary>
    /// Not equal to.
    /// </summary>
    public const string NotEqualTo = "neq";
    /// <summary>
    /// Is null.
    /// </summary>
    public const string IsNull = "nu";
    /// <summary>
    /// Is not null.
    /// </summary>
    public const string IsNotNull = "nnu";
    /// <summary>
    /// Greater than.
    /// </summary>
    public const string GreaterThan = "gt";
    /// <summary>
    /// Greater than or equal to.
    /// </summary>
    public const string GreaterThanOrEqualTo = "gte";
    /// <summary>
    /// Less than.
    /// </summary>
    public const string LessThan = "lt";
    /// <summary>
    /// Less than or equal to.
    /// </summary>
    public const string LessThanOrEqualTo = "lte";
    /// <summary>
    /// Starts with.
    /// </summary>
    public const string StartsWith = "sw";
    /// <summary>
    /// Does not start with.
    /// </summary>
    public const string DoesNotStartWith = "nsw";
    /// <summary>
    /// Ends with.
    /// </summary>
    public const string EndsWith = "ew";
    /// <summary>
    /// Does not end with.
    /// </summary>
    public const string DoesNotEndWith = "new";
    /// <summary>
    /// Contains.
    /// </summary>
    public const string Contains = "cn";
    /// <summary>
    /// Does not contain.
    /// </summary>
    public const string DoesNotContain = "ncn";
    /// <summary>
    /// Like. Warning: Requires the TcfOss.Filtering.EntityFrameworkCore package.
    /// </summary>
    public const string Like = "lk";
    /// <summary>
    /// Not like. Warning: Requires the TcfOss.Filtering.EntityFrameworkCore package.
    /// </summary>
    public const string NotLike = "nlk";
}
