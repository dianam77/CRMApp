using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace CRM_Project.Middleware
{
    public class RoleMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var roles = context.User.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                // ذخیره نقش‌های کاربر در context برای استفاده در مراحل بعدی
                context.Items["UserRoles"] = roles;
            }

            await _next(context);
        }
    }
}
