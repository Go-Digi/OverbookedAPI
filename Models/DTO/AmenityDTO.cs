using System.Text.Json.Serialization;

namespace Overbookedapi.Models.DTO;

public class AmenityDTO
{
    public int AmenityId { get; set; }
    public string AmenityName { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int HotelId { get; set; }
}