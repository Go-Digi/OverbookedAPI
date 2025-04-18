using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Overbookedapi.Models;
using Overbookedapi.Data;
using Overbookedapi.Models.DTO;
using Overbookedapi.Utils;
using Microsoft.AspNetCore.Authorization;

namespace Overbookedapi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReservationsController : BaseApiController
{
    private readonly DataSet _context;
    private readonly IUserActivityLogger _userActivityLogger;

    public ReservationsController(DataSet context, IUserActivityLogger userActivityLogger)
    {
        _context = context;
        _userActivityLogger = userActivityLogger;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservationCalendar>>> GetReservations([FromQuery] DateTimeOffset? checkIn, [FromQuery] DateTimeOffset? checkOut)
    {
        if (checkIn == null|| checkOut == null)
        {
            return BadRequest("Check In Date and Check Out Date should be provided");
        }
        
        // should also query only with hotelId
        var reservations = await _context.Reservations
            .Where(x => x.CheckInDate >= checkIn.Value.UtcDateTime && 
                        x.CheckOutDate <= checkOut.Value.UtcDateTime)
            .Select(x => new ReservationCalendar()
            {
                ReservationId = x.ReservationId,
                CheckInDate = x.CheckInDate,
                CheckOutDate = x.CheckOutDate,
            })
            .OrderBy(x => x.CheckInDate)
            .ToListAsync();
        
        return reservations;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Reservation>> GetReservation(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        
        // should check if reservation is from hotel for extra security
        
        return reservation;
    }
    
    
    [HttpPost]
    public async Task<ActionResult<Reservation>> CreateReservation([FromBody] ReservationDTO reservationDTO)
    {
        if (reservationDTO.RoomTypes.Length == 0)
        {
            return BadRequest("Invalid reservation data. RoomTypeIds must not be null or empty.");
        }
        
        var reservation = new Reservation
        {
            ReservationId = Guid.NewGuid(),
            GuestName = reservationDTO.GuestName,
            CheckInDate = reservationDTO.CheckInDate.UtcDateTime,
            CheckOutDate = reservationDTO.CheckOutDate.UtcDateTime,
            TotalAmount = reservationDTO.TotalPaymentAmount,
            PaidAmount = reservationDTO.DownpaymentAmount,
            DiscountAmount = reservationDTO.DiscountAmount,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            HotelId = 0, // TODO: Set this to the appropriate hotel ID for validation
            ReservationRooms = new List<ReservationRoom>()
        };

        foreach (var r in reservationDTO.RoomTypes)
        {
            reservation.ReservationRooms.Add(new ReservationRoom
            {
                RoomTypeId = r.RoomTypeId,
                RoomRateId = r.RoomRateId,
                ReservationId = reservation.ReservationId,
                RoomCount = r.RoomCount
            });
        }

        try
        {
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            // await _userActivityLogger.LogActivityAsync($"Reservation created for guest {reservationDTO.GuestName}.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while creating the reservation. " + ex.Message);
        }
        return CreatedAtAction(nameof(GetReservation), new { id = reservation.ReservationId }, reservation);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReservation(Guid id, [FromBody] ReservationDTO reservationDTO)
    {
        if (reservationDTO.RoomTypes.Length == 0)
        {
            return BadRequest("Invalid reservation data. RoomTypeIds must not be null or empty.");
        }

        var reservation = await _context.Reservations
            .Include(r => r.ReservationRooms) // Include related rooms for update
            .FirstOrDefaultAsync(r => r.ReservationId == id);

        if (reservation == null)
        {
            return NotFound($"Reservation with ID {id} not found.");
        }

        reservation.GuestName = reservationDTO.GuestName;
        reservation.CheckInDate = reservationDTO.CheckInDate.UtcDateTime;
        reservation.CheckOutDate = reservationDTO.CheckOutDate.UtcDateTime;
        reservation.TotalAmount = reservationDTO.TotalPaymentAmount;
        reservation.PaidAmount = reservationDTO.DownpaymentAmount;
        reservation.DiscountAmount = reservationDTO.DiscountAmount;
        reservation.LastUpdatedAt = DateTime.UtcNow;

        _context.ReservationRooms.RemoveRange(reservation.ReservationRooms);

        reservation.ReservationRooms = reservationDTO.RoomTypes
            .Select(r => new ReservationRoom
            {
                RoomTypeId = r.RoomTypeId,
                RoomRateId = r.RoomRateId,
                ReservationId = reservation.ReservationId
            })
            .ToList();

        try
        {
            await _context.SaveChangesAsync();

            // Optionally log the activity
            // await _userActivityLogger.LogActivityAsync($"Reservation updated for guest {reservation.GuestName}.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while updating the reservation. " + ex.Message);
        }

        return NoContent(); 
    }
    
    // TODO: Make a soft delete feature
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReservation(Guid id)
    {
        var reservation = await _context.Reservations
            .Include(r => r.ReservationRooms) // Include related rooms to ensure cascading deletion
            .FirstOrDefaultAsync(r => r.ReservationId == id);

        if (reservation == null)
        {
            return NotFound($"Reservation with ID {id} not found.");
        }

        try
        {
            _context.ReservationRooms.RemoveRange(reservation.ReservationRooms); // Remove related rooms
            _context.Reservations.Remove(reservation); // Remove the reservation itself
            await _context.SaveChangesAsync();

            // Optionally log the activity
            // await _userActivityLogger.LogActivityAsync($"Reservation with ID {id} deleted.");

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while deleting the reservation. " + ex.Message);
        }
    }
}
