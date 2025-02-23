namespace Overbookedapi.Models;

public class UserActivityLog
{
    public int UserActivityLogId { get; set; }
    public int UserId { get; set; }
    public string Log { get; set; }
    public DateTime LoggedAt { get; set; }
    public User User { get; set; }
}