using System.Linq.Dynamic.Core;

namespace TcfOss.Filtering.EntityFrameworkCore;

public class CustomParsingConfig
{
    public static readonly ParsingConfig ParsingConfig;

    static CustomParsingConfig()
    {
        ParsingConfig = new ParsingConfig
        {
            ResolveTypesBySimpleName = true
        };

        ParsingConfig.CustomTypeProvider = new CustomTypeProvider(ParsingConfig);
    }
}
