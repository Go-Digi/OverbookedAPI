using System.Text.Json.Serialization;

namespace Overbookedapi.Models;

public class Amenity
{
    public int AmenityId { get; set; }
    public string AmenityName { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int HotelId { get; set; }
    
    [JsonIgnore] public Hotel Hotel { get; set; }
    [JsonIgnore] public List<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
}