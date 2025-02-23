namespace Overbookedapi.Models;

public class RoomType
{
    public Guid RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = String.Empty;
    public int RoomCount { get; set; }
    public int MaxCapacity { get; set; }
    public int HotelId { get; set; }
    
    public Hotel Hotel { get; set; }
    public List<RoomRate> RoomRates { get; set; } = new();
}