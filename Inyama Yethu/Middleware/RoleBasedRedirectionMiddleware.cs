using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Inyama_Yethu.Middleware
{
    public class RoleBasedRedirectionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RoleBasedRedirectionMiddleware> _logger;

        public RoleBasedRedirectionMiddleware(RequestDelegate next, ILogger<RoleBasedRedirectionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, 
            UserManager<IdentityUser> userManager, 
            SignInManager<IdentityUser> signInManager)
        {
            // Only handle GET requests to prevent redirection loops on POSTs
            if (context.Request.Method == "GET")
            {
                string requestPath = context.Request.Path.Value.ToLowerInvariant();
                _logger.LogInformation($"Request path: {requestPath}");

                // First, handle the direct case of /admin/livestock/create which should redirect to the proper area
                if (requestPath.Equals("/admin/livestock/create", StringComparison.OrdinalIgnoreCase))
                {
                    // If user is admin, redirect to admin's animals create
                    if (signInManager.IsSignedIn(context.User))
                    {
                        var user = await userManager.GetUserAsync(context.User);
                        if (user != null)
                        {
                            if (await userManager.IsInRoleAsync(user, "Administrator"))
                            {
                                _logger.LogInformation("Redirecting admin from /admin/livestock/create to /admin/animals/create");
                                context.Response.Redirect("/admin/animals/create");
                                return;
                            }
                            else if (await userManager.IsInRoleAsync(user, "Employee"))
                            {
                                _logger.LogInformation("Redirecting employee from /admin/livestock/create to /employee/livestock/create");
                                context.Response.Redirect("/employee/livestock/create");
                                return;
                            }
                        }
                    }
                }

                // Continue with signed-in user checks for other paths
                if (signInManager.IsSignedIn(context.User))
                {
                    var user = await userManager.GetUserAsync(context.User);
                    if (user != null)
                    {
                        // Handle admin users trying to access employee areas
                        if (await userManager.IsInRoleAsync(user, "Administrator") && 
                            requestPath.StartsWith("/admin/livestock"))
                        {
                            // Redirect from /admin/livestock to /admin/animals
                            string correctPath = requestPath.Replace("/admin/livestock", "/admin/animals");
                            _logger.LogInformation($"Redirecting admin from {requestPath} to {correctPath}");
                            context.Response.Redirect(correctPath);
                            return;
                        }
                        
                        // Handle employee users trying to access admin areas
                        if (await userManager.IsInRoleAsync(user, "Employee") && 
                            requestPath.StartsWith("/admin/"))
                        {
                            // Redirect admin controllers to their employee equivalents
                            string correctPath = requestPath.Replace("/admin/", "/employee/");
                            if (requestPath.StartsWith("/admin/animals"))
                            {
                                correctPath = correctPath.Replace("/animals", "/livestock");
                            }
                            _logger.LogInformation($"Redirecting employee from {requestPath} to {correctPath}");
                            context.Response.Redirect(correctPath);
                            return;
                        }
                    }
                }
            }

            // Continue the pipeline if no redirection is needed
            await _next(context);
        }
    }

    // Extension method to easily add the middleware to the pipeline
    public static class RoleBasedRedirectionMiddlewareExtensions
    {
        public static IApplicationBuilder UseRoleBasedRedirection(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RoleBasedRedirectionMiddleware>();
        }
    }
} 