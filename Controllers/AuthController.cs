using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Overbookedapi.Data;
using Overbookedapi.Models.DTO;
using Overbookedapi.Utils;

namespace Overbookedapi.Controllers
{
    public class AuthController : BaseApiController
    {
        private readonly DataSet _context;
        private readonly IJwtService _jwtService;

        public AuthController(DataSet context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (user == null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            var token = _jwtService.GenerateToken(
                userId: user.UserId,
                role: user.RoleId.ToString(),
                hotelId: user.HotelId
            );

            return Ok(new LoginResponse
            {
                Token = token,
                UserId = user.UserId,
                Role = user.RoleId.ToString(),
                HotelId = user.HotelId,
                Email = user.Email
            });
        }
    }
} 