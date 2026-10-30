using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Rest_Api.Filters
{
    /// <summary>
    /// Catches unhandled action exceptions, writes them through Serilog (ILogger),
    /// and returns a consistent JSON 500 response.
    /// </summary>
    public class ExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ILogger<ExceptionFilter> _logger;

        public ExceptionFilter(ILogger<ExceptionFilter> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            if (context.ExceptionHandled)
                return;

            var http = context.HttpContext;
            var correlationId = http.Response.Headers["X-Correlation-Id"].FirstOrDefault()
                ?? http.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                ?? http.TraceIdentifier;

            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();
            var userName = http.User?.FindFirst("sub")?.Value
                ?? http.User?.Identity?.Name;

            _logger.LogError(
                context.Exception,
                "Unhandled exception in {Filter} {@Context}",
                nameof(ExceptionFilter),
                new
                {
                    Controller = controller,
                    Action = action,
                    Method = http.Request.Method,
                    Path = http.Request.Path.Value,
                    UserName = userName,
                    CorrelationId = correlationId
                });

            context.Result = new ObjectResult(new
            {
                message = "An unexpected error occurred.",
                status = StatusCodes.Status500InternalServerError,
                correlationId
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };

            context.ExceptionHandled = true;
        }
    }
}
