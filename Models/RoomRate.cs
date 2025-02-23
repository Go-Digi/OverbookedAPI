namespace Overbookedapi.Models;

public class RoomRate
{
    public int RoomRateId { get; set; }
    public Guid RoomTypeId { get; set; }
    public string RoomRateName { get; set; } = String.Empty;
    public double Price { get; set; }
    
    public RoomType RoomType { get; set; }
}
    