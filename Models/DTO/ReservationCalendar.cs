namespace Overbookedapi.Models.DTO;

// This DTO is used for the calendar
public class ReservationCalendar
{
    public Guid ReservationId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
}