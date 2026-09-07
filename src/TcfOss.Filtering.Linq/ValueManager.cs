using System.Linq.Expressions;

namespace TcfOss.Filtering.Linq;

public class ValueManager : IManageValues
{
    private readonly Dictionary<object, int> _valueToIndex = [];
    private readonly List<object> _values = [];

    public int GetParameterIndex(object value)
    {
        if (_valueToIndex.TryGetValue(value, out int index))
        {
            return index;
        }

        index = _values.Count;
        _values.Add(value);
        _valueToIndex[value] = index;
        return index;
    }

    public ConstantExpression[] GetValueExpressions()
    {
        return [.. _values.Select(IManageValues.GetMemberExpression)];
    }
}
