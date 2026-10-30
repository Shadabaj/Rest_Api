using System.Diagnostics;
using DAL.Entity;
using Microsoft.AspNetCore.Mvc.Filters;
using Rest_Api.IpAddress;
using Services.Interface;

namespace Rest_Api.Filters
{
    /// <summary>
    /// Writes one AuditTrial row after each action for authenticated users.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ActivityLogFilter : Attribute, IActionFilter
    {
        private const string StartKey = "__ActivityLogStartUtc";

        public void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Items[StartKey] = Stopwatch.GetTimestamp();
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            try
            {
                var user = context.HttpContext.User;
                if (user?.Identity?.IsAuthenticated != true)
                    return;

                var userIdRaw = user.FindFirst("userId")?.Value;
                if (!int.TryParse(userIdRaw, out var userId) || userId <= 0)
                    return;

                var userName = user.FindFirst("sub")?.Value
                    ?? user.Identity?.Name
                    ?? user.FindFirst("name")?.Value
                    ?? "unknown";

                var started = context.HttpContext.Items[StartKey] is long startTimestamp
                    ? Stopwatch.GetElapsedTime(startTimestamp)
                    : TimeSpan.Zero;

                var controller = context.RouteData.Values["controller"]?.ToString() ?? string.Empty;
                var action = context.RouteData.Values["action"]?.ToString() ?? string.Empty;
                var statusCode = context.HttpContext.Response.StatusCode;
                if (context.Exception != null && !context.ExceptionHandled)
                    statusCode = StatusCodes.Status500InternalServerError;
                else if (context.Result is Microsoft.AspNetCore.Mvc.ObjectResult objectResult && objectResult.StatusCode.HasValue)
                    statusCode = objectResult.StatusCode.Value;
                else if (context.Result is Microsoft.AspNetCore.Mvc.StatusCodeResult statusResult)
                    statusCode = statusResult.StatusCode;

                var ipService = context.HttpContext.RequestServices.GetService<IipAddress>();
                var ip = ipService?.GetIpAddress();
                    

                var message = context.Exception != null && !context.ExceptionHandled
                    ? Truncate($"Failed: {context.Exception.Message}", 400)
                    : Truncate($"HTTP {statusCode} {context.HttpContext.Request.Method}", 400);

                var audit = new AuditTrial
                {
                    UserId = userId,
                    UserName = Truncate(userName, 128),
                    ControllerName = Truncate(controller, 128),
                    ActionName = Truncate(action, 128),
                    CreatedByIp = Truncate(ip, 45),
                    ExecutionTimeMs = (int)Math.Min(int.MaxValue, started.TotalMilliseconds),
                    Timestamp = DateTime.UtcNow,
                    Message = message
                };

                var activityLog = context.HttpContext.RequestServices.GetService<IActivityLogService>();
                activityLog?.LogActivity(audit);
            }
            catch
            {
                // Never fail the request because activity logging failed.
            }
        }

        private static string Truncate(string value, int maxLen)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            return value.Length <= maxLen ? value : value[..maxLen];
        }
    }
}
