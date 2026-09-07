using TcfOss.Filtering.Contracts;

namespace TcfOss.Filtering.Programmatic.DataTypes;

public interface ITypeRange
{
    Filter? ToContractFilter(string field, MappingOptions options);
}
