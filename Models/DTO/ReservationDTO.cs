using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Overbookedapi.Models.DTO;

public class ReservationDTO
{
    public string GuestName { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public RoomTypeWithRate[] RoomTypes { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DownpaymentAmount { get; set; }
    public decimal TotalPaymentAmount { get; set; }
    
}

public class RoomTypeWithRate
{
    public Guid RoomTypeId { get; set; }
    public int RoomRateId { get; set; }
}