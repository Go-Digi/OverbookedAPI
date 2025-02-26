namespace Overbookedapi.Models.DTO;

public class RoomTypeDTO
{
    public Guid? RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = String.Empty;
    public int RoomCount { get; set; }
    public int MaxCapacity { get; set; }
    public int HotelId { get; set; }
    public decimal Price { get; set; }
}