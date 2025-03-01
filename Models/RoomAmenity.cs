using System.Text.Json.Serialization;

namespace Overbookedapi.Models;

public class RoomAmenity
{
    public int RoomAmenityId { get; set; }
    public Guid RoomTypeId { get; set; }
    public int AmenityId { get; set; }
    
    [JsonIgnore] public RoomType RoomType { get; set; }
    [JsonIgnore] public Amenity Amenity { get; set; }
}