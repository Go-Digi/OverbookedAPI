namespace Overbookedapi.Models.DTO
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public int? HotelId { get; set; }
        public string Email { get; set; } = string.Empty;
    }
} 