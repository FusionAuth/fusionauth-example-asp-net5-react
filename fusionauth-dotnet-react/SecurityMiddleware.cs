using fusionauth_dotnet_react.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fusionauth_dotnet_react
{
    public class SecurityMiddleware
    {
        private readonly RequestDelegate _next;
        public SecurityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var accessibleUrls = new[]
            {
                "/api/security/login",
                "/api/security/oauth-callback"
            };
            var token = httpContext.Session.GetString(SessionKeys.Token);

            // Redirect to login if user is not authenticated. This instruction is neccessary for JS async calls, otherwise everycall will return unauthorized without explaining why
            if (string.IsNullOrEmpty(token) && !accessibleUrls.Contains(httpContext.Request.Path.Value, StringComparer.OrdinalIgnoreCase))
            {
                // The login controller will start the PKCE process
                httpContext.Response.Redirect("/api/security/login");

                //httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            }
            else
            {
                // Move forward into the pipeline
                await _next(httpContext);

                if (httpContext.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    httpContext.Session.Clear();
                }
            }
        }
    }
    public static class SecurityMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityMiddleware>();
        }
    }
}
