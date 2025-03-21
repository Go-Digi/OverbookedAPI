using System.ComponentModel.DataAnnotations;

namespace Overbookedapi.Models;

public class Reservation
{
    [Key]
    public Guid ReservationId { get; set; }
    public string? GuestName { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int HotelId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public int? ExtraPersonCount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    
    public List<ReservationRoom> ReservationRooms { get; set; } = new();
}