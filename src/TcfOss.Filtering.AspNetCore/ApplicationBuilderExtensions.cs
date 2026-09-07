using Microsoft.AspNetCore.Builder;

namespace TcfOss.Filtering.AspNetCore;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseFilterDeserializationExceptionHandler(this IApplicationBuilder app)
        => app.UseMiddleware<FilterDeserializationExceptionMiddleware>();
}
