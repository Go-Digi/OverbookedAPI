using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Overbookedapi.Models.DTO;

public class ReservationDTO
{
    public string GuestName { get; set; }
    public DateTimeOffset CheckInDate { get; set; }
    public DateTimeOffset CheckOutDate { get; set; }
    public RoomTypeWithRate[] RoomTypes { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DownpaymentAmount { get; set; }
    public decimal TotalPaymentAmount { get; set; }
    
}

public class RoomTypeWithRate
{
    public Guid RoomTypeId { get; set; }
    public int RoomRateId { get; set; }
    public int RoomCount { get; set; }
}