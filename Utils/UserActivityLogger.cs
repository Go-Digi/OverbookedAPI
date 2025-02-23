
using Overbookedapi.Data;
using Overbookedapi.Models;

namespace Overbookedapi.Utils;

public interface IUserActivityLogger
{
    Task LogActivityAsync(int userId, string log);
}
public class UserActivityLogger : IUserActivityLogger
{
    private readonly DataSet _context;
    private readonly ILogger<UserActivityLogger> _logger;

    public UserActivityLogger(DataSet context, ILogger<UserActivityLogger> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task LogActivityAsync(int userId, string log)
    {
        try
        {
            var activityLog = new UserActivityLog
            {
                UserId = userId,
                Log = log,
                LoggedAt = DateTime.UtcNow
            };

            await _context.UserActivityLogs.AddAsync(activityLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging user activity for UserId: {UserId}", userId);
        }
    }
    
}