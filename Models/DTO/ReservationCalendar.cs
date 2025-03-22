namespace Overbookedapi.Models.DTO;

// This DTO is used for the calendar
public class ReservationCalendar
{
    public Guid ReservationId { get; set; }
    public DateTimeOffset CheckInDate { get; set; }
    public DateTimeOffset CheckOutDate { get; set; }
}