using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.CustomTypeProviders;
using Microsoft.EntityFrameworkCore;

namespace TcfOss.Filtering.EntityFrameworkCore;

public class CustomTypeProvider(ParsingConfig config)
    : DefaultDynamicLinqCustomTypeProvider(config, [typeof(EF), typeof(DbFunctionsExtensions)])
{
}
