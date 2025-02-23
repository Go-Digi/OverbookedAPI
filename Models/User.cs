namespace Overbookedapi.Models;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int RoleId { get; set; }
    public int HotelId { get; set; }
    public Hotel Hotel { get; set; }
    public List<UserActivityLog> UserActivityLogs { get; set; } = new();
}