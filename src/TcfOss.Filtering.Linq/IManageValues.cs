using System.Linq.Expressions;

namespace TcfOss.Filtering.Linq;

public interface IManageValues
{
    public int GetParameterIndex(object value);
    public ConstantExpression[] GetValueExpressions();

    public static ConstantExpression GetMemberExpression(object value)
    {
        return value switch
        {
            string s => GetMemberExpression(s),
            int i => GetMemberExpression(i),
            uint ui => GetMemberExpression(ui),
            short s => GetMemberExpression(s),
            ushort us => GetMemberExpression(us),
            long l => GetMemberExpression(l),
            ulong ul => GetMemberExpression(ul),
            float f => GetMemberExpression(f),
            double d => GetMemberExpression(d),
            decimal m => GetMemberExpression(m),
            bool b => GetMemberExpression(b),
            byte by => GetMemberExpression(by),
            sbyte sb => GetMemberExpression(sb),
            DateTime dt => GetMemberExpression(dt),
            DateOnly d => GetMemberExpression(d),
            Array arr => Expression.Constant(arr, arr.GetType()),
            _ => throw new NotSupportedException($"Type {value.GetType()} is not supported.")
        };
    }

    public static ConstantExpression GetMemberExpression<T>(T value)
    {
        // var parameter = Expression.Property(Expression.Constant(value), )
        // var parameter = Expression.Parameter(typeof(T), "x");
        // var member = Expression.Property(parameter, "Value");
        ConstantExpression constant = Expression.Constant(value, typeof(T));
        return constant;
    }
}
