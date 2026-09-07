using Microsoft.AspNetCore.Http;
using TcfOss.Filtering.Contracts;

namespace TcfOss.Filtering.AspNetCore;

public class FilterDeserializationExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (FilterDeserializationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
    }
}
