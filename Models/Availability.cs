namespace Overbookedapi.Models;

public class Availability
{
    public Guid RoomTypeId { get; set; }
    public int Available { get; set; }
    public DateTime? AvailabilityDate { get; set; }
}