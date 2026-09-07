using System.Linq.Expressions;

namespace TcfOss.Filtering.Linq.Tests;

public class ValueManagerTests
{
    [Fact]
    public void DifferentValuesDifferentIndexes()
    {
        var valueManager = new ValueManager();

        int index1 = valueManager.GetParameterIndex("Hello");
        int index2 = valueManager.GetParameterIndex(42);
        int index3 = valueManager.GetParameterIndex(3.14);

        Assert.Equal(0, index1);
        Assert.Equal(1, index2);
        Assert.Equal(2, index3);
    }

    [Fact]
    public void SameValueSameIndex()
    {
        var valueManager = new ValueManager();

        int index1 = valueManager.GetParameterIndex("Hello");
        int index2 = valueManager.GetParameterIndex(42);
        int index3 = valueManager.GetParameterIndex("Hello");

        Assert.Equal(0, index1);
        Assert.Equal(1, index2);
        Assert.Equal(0, index3);
    }

    [Theory]
    [InlineData("Hello", typeof(string))]
    [InlineData(42, typeof(int))]
    [InlineData(42u, typeof(uint))]
    [InlineData((short)42, typeof(short))]
    [InlineData((ushort)42, typeof(ushort))]
    [InlineData(42L, typeof(long))]
    [InlineData(42UL, typeof(ulong))]
    [InlineData(3.14f, typeof(float))]
    [InlineData(3.14, typeof(double))]
    [InlineData(true, typeof(bool))]
    [InlineData(false, typeof(bool))]
    [InlineData((byte)42, typeof(byte))]
    [InlineData((sbyte)42, typeof(sbyte))]
    public void CorrectValueTypes_Primitive(object value, Type expectedType)
    {
        var valueManager = new ValueManager();
        _ = valueManager.GetParameterIndex(value);

        ConstantExpression[] values = valueManager.GetValueExpressions();
        Assert.Single(values);
        ConstantExpression actualValue = values[0];
        Assert.Equal(expectedType, actualValue.Type);
        Assert.Equal(value, actualValue.Value);
    }

    [Theory]
    [ClassData(typeof(NonPrimitiveTestData))]
    public void CorrectValueTypes_NonPrimitive(object value, Type expectedType)
    {
        var valueManager = new ValueManager();
        _ = valueManager.GetParameterIndex(value);

        ConstantExpression[] values = valueManager.GetValueExpressions();
        Assert.Single(values);
        ConstantExpression actualValue = values[0];
        Assert.Equal(expectedType, actualValue.Type);
        Assert.Equal(value, actualValue.Value);
    }

    [Fact]
    public void UnsupportedType_ThrowsException()
    {
        var valueManager = new ValueManager();
        object unsupportedValue = new();

        valueManager.GetParameterIndex(unsupportedValue);
        Assert.Throws<NotSupportedException>(() => valueManager.GetValueExpressions());
    }

    class NonPrimitiveTestData : IEnumerable<TheoryDataRow<object, Type>>
    {
        public IEnumerator<TheoryDataRow<object, Type>> GetEnumerator()
        {
            yield return new TheoryDataRow<object, Type>(42m, typeof(decimal)) { Label = "Decimal" };
            yield return new TheoryDataRow<object, Type>(new DateTime(2024, 1, 1), typeof(DateTime)) { Label = "DateTime" };
            yield return new TheoryDataRow<object, Type>(new DateOnly(2024, 1, 1), typeof(DateOnly)) { Label = "DateOnly" };
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
