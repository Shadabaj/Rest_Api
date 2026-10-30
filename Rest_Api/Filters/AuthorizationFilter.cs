using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;

namespace Rest_Api.Filters
{
    /// <summary>
    /// Validates Bearer JWT on protected endpoints. Skips [AllowAnonymous].
    /// Example: [AuthorizationFilter(Roles = "Admin")] or [AuthorizationFilter(Roles = "Admin,HR")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AuthorizationFilter : Attribute, IAuthorizationFilter
    {
        /// <summary>Comma-separated roles, same style as [Authorize(Roles = "...")].</summary>
        public string? Roles { get; set; }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.Result != null)
                return;

            if (HasAllowAnonymous(context))
                return;

            var authorization = context.HttpContext.Request.Headers.Authorization.ToString();
            if (string.IsNullOrWhiteSpace(authorization) ||
                !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Authorization Bearer token is required." });
                return;
            }

            var token = authorization["Bearer ".Length..].Trim();
            if (string.IsNullOrWhiteSpace(token))
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Authorization Bearer token is required." });
                return;
            }

            var config = context.HttpContext.RequestServices.GetService<IConfiguration>();
            if (config == null)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                return;
            }

            var jwtKey = config["Jwt:Key"];
            var jwtIssuer = config["Jwt:Issuer"];
            var jwtAudience = config["Jwt:Audience"];
            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                return;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                handler.InboundClaimTypeMap.Clear();

                var principal = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    RoleClaimType = "role",
                    NameClaimType = "name",
                    ClockSkew = TimeSpan.FromMinutes(1)
                }, out _);

                context.HttpContext.User = principal;

                var requiredRoles = ParseRoles(Roles);
                if (requiredRoles.Length > 0 && !HasAnyRequiredRole(principal, requiredRoles))
                {
                    context.Result = new ObjectResult(new { message = "Access denied for this role." })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }
            }
            catch (SecurityTokenException)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Invalid or expired access token." });
            }
            catch (Exception)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Invalid or expired access token." });
            }
        }

        private static string[] ParseRoles(string? roles)
        {
            if (string.IsNullOrWhiteSpace(roles))
                return Array.Empty<string>();

            return roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        private static bool HasAllowAnonymous(AuthorizationFilterContext context)
        {
            if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())
                return true;

            return context.Filters.OfType<IAllowAnonymous>().Any();
        }

        private static bool HasAnyRequiredRole(ClaimsPrincipal principal, IEnumerable<string> roles)
        {
            var userRoles = principal.FindAll("role")
                .Select(c => c.Value)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return roles.Any(r => userRoles.Contains(r));
        }
    }
}
