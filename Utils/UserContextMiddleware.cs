using System.Security.Claims;

namespace Overbookedapi.Utils
{
    public class UserContext
    {
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public int? HotelId { get; set; }
    }

    public class UserContextMiddleware
    {
        private readonly RequestDelegate _next;

        public UserContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var userContext = new UserContext();
            
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                var roleClaim = context.User.FindFirst(ClaimTypes.Role);
                var hotelIdClaim = context.User.FindFirst("HotelId");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    userContext.UserId = userId;
                }

                if (roleClaim != null)
                {
                    userContext.Role = roleClaim.Value;
                }

                if (hotelIdClaim != null && int.TryParse(hotelIdClaim.Value, out int hotelId))
                {
                    userContext.HotelId = hotelId;
                }
            }

            context.Items["UserContext"] = userContext;
            await _next(context);
        }
    }

    public static class UserContextMiddlewareExtensions
    {
        public static IApplicationBuilder UseUserContext(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserContextMiddleware>();
        }
    }
} 