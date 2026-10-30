using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Rest_Api.IpAddress;
using System.Threading.Tasks;

namespace Rest_Api.Custom_Middlewar
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class Middleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<Middleware> _logger;
        private readonly IipAddress _ipAddress;

        public Middleware(RequestDelegate next, ILogger<Middleware> logger, IipAddress ipAddress)
        {
            _next = next;
            _logger = logger;
            _ipAddress = ipAddress;
        }

        public Task Invoke(HttpContext httpContext)
        {
            Console.WriteLine($"Request: {httpContext.Request.Method} {httpContext.Request.Path}");
            _logger.LogInformation($"Request Shadab is best: {httpContext.Request.Method} {httpContext.Request.Path}");

            _ipAddress.GetIpAddress();

            return _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<Middleware>();
        }
    }
}
