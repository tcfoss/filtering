using TcfOss.Filtering.Contracts;

namespace TcfOss.Filtering.Linq;

public abstract class QueryOperator
{
    public static readonly EqualToOperator EqualTo = EqualToOperator.Instance;
    public static readonly NotEqualToOperator NotEqualTo = NotEqualToOperator.Instance;
    public static readonly IsNullOperator IsNull = IsNullOperator.Instance;
    public static readonly IsNotNullOperator IsNotNull = IsNotNullOperator.Instance;
    public static readonly GreaterThanOperator GreaterThan = GreaterThanOperator.Instance;
    public static readonly GreaterThanOrEqualToOperator GreaterThanOrEqualTo = GreaterThanOrEqualToOperator.Instance;
    public static readonly LessThanOperator LessThan = LessThanOperator.Instance;
    public static readonly LessThanOrEqualToOperator LessThanOrEqualTo = LessThanOrEqualToOperator.Instance;
    public static readonly StartsWithOperator StartsWith = StartsWithOperator.Instance;
    public static readonly DoesNotStartWithOperator DoesNotStartWith = DoesNotStartWithOperator.Instance;
    public static readonly EndsWithOperator EndsWith = EndsWithOperator.Instance;
    public static readonly DoesNotEndWithOperator DoesNotEndWith = DoesNotEndWithOperator.Instance;
    public static readonly ContainsOperator Contains = ContainsOperator.Instance;
    public static readonly DoesNotContainOperator DoesNotContain = DoesNotContainOperator.Instance;
    public static readonly LikeOperator Like = LikeOperator.Instance;
    public static readonly NotLikeOperator NotLike = NotLikeOperator.Instance;

    public static QueryOperator Parse(string operatorName)
    {
        return operatorName switch
        {
            "EqualTo" => EqualTo,
            "NotEqualTo" => NotEqualTo,
            "IsNull" => IsNull,
            "IsNotNull" => IsNotNull,
            "GreaterThan" => GreaterThan,
            "GreaterThanOrEqualTo" => GreaterThanOrEqualTo,
            "LessThan" => LessThan,
            "LessThanOrEqualTo" => LessThanOrEqualTo,
            "StartsWith" => StartsWith,
            "DoesNotStartWith" => DoesNotStartWith,
            "EndsWith" => EndsWith,
            "DoesNotEndWith" => DoesNotEndWith,
            "Contains" => Contains,
            "DoesNotContain" => DoesNotContain,
            "Like" => Like,
            "NotLike" => NotLike,
            _ => throw new ArgumentException($"Invalid operator name: {operatorName}")
        };
    }

    /// <summary>
    /// Parse a short contract key (as defined in <see cref="TcfOss.Filtering.Contracts.FilterOperators"/>)
    /// into a <see cref="QueryOperator"/>.
    /// </summary>
    public static QueryOperator ParseContractKey(string key)
    {
        return key switch
        {
            FilterOperators.EqualTo => EqualTo,
            FilterOperators.NotEqualTo => NotEqualTo,
            FilterOperators.IsNull => IsNull,
            FilterOperators.IsNotNull => IsNotNull,
            FilterOperators.GreaterThan => GreaterThan,
            FilterOperators.GreaterThanOrEqualTo => GreaterThanOrEqualTo,
            FilterOperators.LessThan => LessThan,
            FilterOperators.LessThanOrEqualTo => LessThanOrEqualTo,
            FilterOperators.StartsWith => StartsWith,
            FilterOperators.DoesNotStartWith => DoesNotStartWith,
            FilterOperators.EndsWith => EndsWith,
            FilterOperators.DoesNotEndWith => DoesNotEndWith,
            FilterOperators.Contains => Contains,
            FilterOperators.DoesNotContain => DoesNotContain,
            FilterOperators.Like => Like,
            FilterOperators.NotLike => NotLike,
            _ => throw new FilterMappingException(string.Format(FilterMappingException.UnknownFilterOperatorPattern, key))
        };
    }

    public abstract string ToDynamicLinq(string fieldName, object value, IManageValues valueManager);


    private static string GetFieldDynamicLinq(string fieldName)
    {
        return fieldName;
    }

    private static string GetParameterDynamicLinq(object value, IManageValues valueManager)
    {
        return $"@{valueManager.GetParameterIndex(value)}";
    }

    public class EqualToOperator : QueryOperator
    {
        public static readonly EqualToOperator Instance = new();
        private EqualToOperator() { }

        public override string ToDynamicLinq(string fieldName, object value, IManageValues valueManager)
        {
            return $"{GetFieldDynamicLinq(fieldName)} == {GetParameterDynamicLinq(value, valueManager)}";
        }
    }

    public class NotEqualToOperator : QueryOperator
    {
        public static readonly NotEqualToOperator Instance = new();
        private NotEqualToOperator() { }

        public override string ToDynamicLinq(string fieldName, object value, IManageValues valueManager)
        {
            return $"{GetFieldDynamicLinq(fieldName)} != {GetParameterDynamicLinq(value, valueManager)}";
        }
    }

    public class IsNullOperator : QueryOperator
    {
        public static readonly IsNullOperator Instance = new();
        private IsNullOperator() { }

        public override string ToDynamicLinq(string fieldName, object value, IManageValues valueManager)
        {
            return $"{GetFieldDynamicLinq(fieldName)} == null";
        }
    }

    public class IsNotNullOperator : QueryOperator
    {
        public static readonly IsNotNullOperator Instance = new();
        private IsNotNullOperator() { }

        public override string ToDynamicLinq(string fieldName, object value, IManageValues valueManager)
        {
            return $"{GetFieldDynamicLinq(fieldName)} != null";
        }
    }

    public class GreaterThanOperator : QueryOperator
    {
        public static readonly GreaterThanOperator Instance = new();
        private GreaterThanOperator() { }

        public override string ToDynamicLinq(string fieldName, object value, IManageValues valueManager)
        {
            return $"{GetFieldDynamicLinq(fieldName)} > {GetParameterDynamicLinq(value, valueManager)}";
        }
    }

    public class GreaterThanOrEqualToOperator : QueryOperator
    {
        public static readonly GreaterThanOrEqualToOperator Instance = new();
        private GreaterThanOrEqualToOperator() { }

        public override string ToDynamicLinq(string fieldName, object value, IManageValues valueManager)
        {
            return $"{GetFieldDynamicLinq(fieldName)} >= {GetParameterDynamicLinq(value, valueManager)}";
        }
    }

    public class LessThanOperator : QueryOperator
    {
        public static readonly LessThanOperator Instance = new();
        private LessThanOperator() { }

        public override string ToDynamicLinq(string fieldName, object value, IManageValues valueManager)
        {
            return $"{GetFieldDynamicLinq(fieldName)} < {GetParameterDynamicLinq(value, valueManager)}";
        }
    }

    public class LessThanOrEqualToOperator : QueryOperator
    {
        public static readonly LessThanOrEqualToOperator Instance = new();
        private LessThanOrEqualToOperator() { }

        public override string ToDynamicLinq(string fieldName, object value, IManageValues valueManager)
        {
            return $"{GetFieldDynamicLinq(fieldName)} <= {GetParameterDynamicLinq(value, valueManager)}";
        }
    }

    public class StartsWithOperator : QueryOperator
    {
        public static readonly StartsWithOperator Instance = new();
        private StartsWithOperator() { }

        public override string ToDynamicLinq(string fieldName, object value, IManageValues valueManager)
        {
            return $"{GetFieldDynamicLinq(fieldName)}.StartsWith({GetParameterDynamicLinq(value, valueManager)})";
        }
    }

    public class DoesNotStartWithOperator : QueryOperator
    {
        public static readonly DoesNotStartWithOperator Instance = new();
        private DoesNotStartWithOperator() { }

        public override string ToDynamicLinq(string fieldName, object value, IManageValues valueManager)
        {
            return $"!{GetFieldDynamicLinq(fieldName)}.StartsWith({GetParameterDynamicLinq(value, valueManager)})";
        }
    }

    public class EndsWithOperator : QueryOperator
    {
        public static readonly EndsWithOperator Instance = new();
        private EndsWithOperator() { }

        public override string ToDynamicLinq(string fieldName, object value, IManageValues valueManager)
        {
            return $"{GetFieldDynamicLinq(fieldName)}.EndsWith({GetParameterDynamicLinq(value, valueManager)})";
        }
    }

    public class DoesNotEndWithOperator : QueryOperator
    {
        public static readonly DoesNotEndWithOperator Instance = new();
        private DoesNotEndWithOperator() { }

        public override string ToDynamicLinq(string fieldName, object value, IManageValues valueManager)
        {
            return $"!{GetFieldDynamicLinq(fieldName)}.EndsWith({GetParameterDynamicLinq(value, valueManager)})";
        }
    }

    public class ContainsOperator : QueryOperator
    {
        public static readonly ContainsOperator Instance = new();
        private ContainsOperator() { }

        public override string ToDynamicLinq(string fieldName, object value, IManageValues valueManager)
        {
            return $"{GetFieldDynamicLinq(fieldName)}.Contains({GetParameterDynamicLinq(value, valueManager)})";
        }
    }

    public class DoesNotContainOperator : QueryOperator
    {
        public static readonly DoesNotContainOperator Instance = new();
        private DoesNotContainOperator() { }

        public override string ToDynamicLinq(string fieldName, object value, IManageValues valueManager)
        {
            return $"!{GetFieldDynamicLinq(fieldName)}.Contains({GetParameterDynamicLinq(value, valueManager)})";
        }
    }

    public class LikeOperator : QueryOperator
    {
        public static readonly LikeOperator Instance = new();
        private LikeOperator() { }

        public override string ToDynamicLinq(string fieldName, object value, IManageValues valueManager)
        {
            return $"DbFunctionsExtensions.Like(EF.Functions, {GetFieldDynamicLinq(fieldName)}, {GetParameterDynamicLinq(value, valueManager)})";
        }
    }

    public class NotLikeOperator : QueryOperator
    {
        public static readonly NotLikeOperator Instance = new();
        private NotLikeOperator() { }

        public override string ToDynamicLinq(string fieldName, object value, IManageValues valueManager)
        {
            return $"!DbFunctionsExtensions.Like(EF.Functions, {GetFieldDynamicLinq(fieldName)}, {GetParameterDynamicLinq(value, valueManager)})";
        }
    }
}
