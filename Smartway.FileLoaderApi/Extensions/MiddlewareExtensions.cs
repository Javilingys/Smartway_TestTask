using Smartway.FileLoaderApi.Middleware;

namespace Smartway.FileLoaderApi.Extensions;

public static class MiddlewareExtensions
{
    public static void UseExceptionMiddleware(this WebApplication app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
    }
}
