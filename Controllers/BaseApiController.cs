using Microsoft.AspNetCore.Mvc;
using Overbookedapi.Utils;

namespace Overbookedapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        protected UserContext CurrentUser
        {
            get
            {
                return HttpContext.Items["UserContext"] as UserContext ?? new UserContext();
            }
        }

        protected bool IsHotelStaff => CurrentUser.Role == "HotelStaff" && CurrentUser.HotelId.HasValue;
        protected bool IsAdmin => CurrentUser.Role == "Admin";
    }
} 