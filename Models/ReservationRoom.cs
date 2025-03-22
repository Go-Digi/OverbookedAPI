using System.ComponentModel.DataAnnotations;

namespace Overbookedapi.Models;

public class ReservationRoom
{
    [Key]
    public int ReservationRoomId { get; set; }
    public Guid RoomTypeId { get; set; }
    public int RoomRateId { get; set; }
    public Guid ReservationId { get; set; }
    public int RoomCount { get; set; }

    // Navigation properties (assuming relationships exist)
    public virtual RoomType RoomType { get; set; }
    public virtual RoomRate RoomRate { get; set; }
    public virtual Reservation Reservation { get; set; }
}