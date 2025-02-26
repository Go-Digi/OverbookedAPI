using System.Text.Json.Serialization;

namespace Overbookedapi.Models;

public class RoomRate
{
    public int RoomRateId { get; set; }
    public Guid RoomTypeId { get; set; }
    public string RoomRateName { get; set; } = String.Empty;
    public decimal Price { get; set; }
    
    [JsonIgnore]
    public RoomType RoomType { get; set; }
}
    