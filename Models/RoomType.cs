using System.Text.Json.Serialization;

namespace Overbookedapi.Models;

public class RoomType
{
    public Guid RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = String.Empty;
    public int RoomCount { get; set; }
    public int MaxCapacity { get; set; }
    public int HotelId { get; set; }
    
    [JsonIgnore]
    public Hotel Hotel { get; set; }
    public List<RoomRate> RoomRates { get; set; } = new();
    public List<BlockedDate> BlockedDates { get; set; } = new();
}