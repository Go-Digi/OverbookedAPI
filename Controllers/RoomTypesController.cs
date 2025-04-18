using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Overbookedapi.Models;
using Overbookedapi.Models.DTO;
using Overbookedapi.Data;
using Overbookedapi.Utils;
using Microsoft.AspNetCore.Authorization;

namespace Overbookedapi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RoomTypesController : BaseApiController
{
    private readonly DataSet _context;
    private readonly IUserActivityLogger _userActivityLogger;

    public RoomTypesController(DataSet context, IUserActivityLogger userActivityLogger)
    {
        _context = context;
        _userActivityLogger = userActivityLogger;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomType>>> GetRoomTypes()
    {
        return await _context.RoomTypes.Include(rt => rt.RoomRates).ToListAsync();
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<RoomType>> GetRoomType(Guid id)
    {
        var roomType = await _context.RoomTypes.Include(rt => rt.RoomRates)
            .Include(rt=>rt.RoomAmenities)
            .ThenInclude(ra=>ra.Amenity)
            .Include(rt=>rt.BlockedDates)
            .FirstOrDefaultAsync(rt => rt.RoomTypeId == id);

        if (roomType == null)
        {
            return NotFound();
        }

        return roomType;
    }
    
    
    // LET'S START WITH A SINGLE ROOM RATE FOR NOW
    [HttpPost]
    public async Task<ActionResult<RoomType>> CreateRoomType(RoomTypeDTO newRoomType)
    {

        newRoomType.RoomTypeId ??= Guid.NewGuid();
        var roomType = new RoomType()
        {
            RoomTypeId = (Guid)newRoomType.RoomTypeId,
            RoomTypeName = newRoomType.RoomTypeName,
            RoomCount = newRoomType.RoomCount,
            MaxCapacity = newRoomType.MaxCapacity,
            HotelId = newRoomType.HotelId,
        };
        _context.RoomTypes.Add(roomType);

        var roomRates = new RoomRate()
        {
            RoomTypeId = roomType.RoomTypeId,
            RoomRateName = "Default",
            Price = newRoomType.Price,
        };
        _context.RoomRates.Add(roomRates);

        if (newRoomType.AmenityIds != String.Empty)
        {
            var ids = newRoomType.AmenityIds.Split(",");
            var roomAmenities = new List<RoomAmenity>();
            foreach (var amenityId in ids)
            {
                var roomAmenity = new RoomAmenity()
                {
                    AmenityId = int.Parse(amenityId),
                    RoomTypeId = roomType.RoomTypeId,
                };
                roomAmenities.Add(roomAmenity);
            }
            _context.RoomAmenities.AddRange(roomAmenities);
        }
        
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRoomType), new { id = roomType.RoomTypeId }, roomType);
    }
    
    [HttpPut]
    public async Task<ActionResult<RoomType>> UpdateRoomType(RoomTypeDTO newRoomType)
    {
        if (newRoomType.RoomTypeId == null || newRoomType.RoomTypeId == Guid.Empty)
        {
            return BadRequest("Room type id cannot be empty");
        }
        var roomType = await _context.RoomTypes.FirstOrDefaultAsync(rt => rt.RoomTypeId == newRoomType.RoomTypeId);
        if (roomType == null)
        {
            return NotFound("Room type not found");
        }
        roomType.RoomTypeName = newRoomType.RoomTypeName;
        roomType.RoomCount = newRoomType.RoomCount;
        roomType.MaxCapacity = newRoomType.MaxCapacity;
        roomType.HotelId = newRoomType.HotelId;
        _context.RoomTypes.Update(roomType);

        // since we only use one room rate for now, we don't need to list all rates
        var roomRate = await _context.RoomRates.FirstOrDefaultAsync(rt => rt.RoomTypeId == newRoomType.RoomTypeId);
        if (roomRate == null)
        {
            return NotFound("Room rate not found");
        }
        roomRate.Price = newRoomType.Price;
        _context.RoomRates.Update(roomRate);
        
        var updatedAmenityIds = newRoomType.AmenityIds.Split(",")
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(int.Parse)
            .ToList();
        var existingRoomAmenities = await _context.RoomAmenities.Where(x => x.RoomTypeId == newRoomType.RoomTypeId)
            .ToListAsync();

        var amenityIdsToAdd = updatedAmenityIds
            .Except(existingRoomAmenities.Select(x => x.AmenityId))
            .ToList();
        
        var amenityIdsToRemove = existingRoomAmenities
            .Select(x => x.AmenityId)
            .Except(updatedAmenityIds)
            .ToList();

        var roomAmenities = new List<RoomAmenity>();
        foreach (var id in amenityIdsToAdd)
        {
            var roomAmenity = new RoomAmenity()
            {
                AmenityId = id,
                RoomTypeId = roomType.RoomTypeId,
            };
            roomAmenities.Add(roomAmenity);
        }
        _context.RoomAmenities.AddRange(roomAmenities);
        
        roomAmenities = new List<RoomAmenity>();
        foreach (var id in amenityIdsToRemove)
        {
            var roomAmenity = existingRoomAmenities.FirstOrDefault(x => x.AmenityId == id);
            roomAmenities.Add(roomAmenity);
        }
        _context.RoomAmenities.RemoveRange(roomAmenities);
        
        
        await _context.SaveChangesAsync();

        return Ok();
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult<RoomType>> DeleteRoomType(Guid id)
    {
        var roomType = await _context.RoomTypes.Include(rt => rt.RoomRates).FirstOrDefaultAsync(rt => rt.RoomTypeId == id);
        if (roomType == null)
        {
            return NotFound();
        }
        _context.RoomTypes.Remove(roomType);
        
        var roomRate = await _context.RoomRates.FirstOrDefaultAsync(rt => rt.RoomTypeId == id);
        if (roomRate == null)
        {
            return NotFound("Room rate not found");
        }
        _context.RoomRates.Remove(roomRate);
        
        var existingRoomAmenities = await _context.RoomAmenities.Where(x => x.RoomTypeId == id)
            .ToListAsync();
        _context.RoomAmenities.RemoveRange(existingRoomAmenities);
        
        await _context.SaveChangesAsync();

        return Ok();
    }

    // Retrieves the availability of a specific room type for a specified date range, considering reservations and blocked dates.
    [HttpGet("{roomTypeId}/availability")]
    public async Task<ActionResult<List<Availability>>> GetAvailability(Guid roomTypeId, [FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate)
    {
        if (startDate == null || endDate == null)
        {
            return BadRequest("Please provide valid dates");
        }

        if (startDate > endDate)
        {
            return BadRequest("End date cannot be earlier than start date");
        }
        
        var blockedDates = await _context.BlockedDates.Where(x => x.Date >= startDate.Value.UtcDateTime && 
                                                                                x.Date <= endDate.Value.UtcDateTime &&
                                                                                x.RoomTypeId == roomTypeId)
                                                                        .ToListAsync();
         var roomType = await _context.RoomTypes.FirstOrDefaultAsync(rt => rt.RoomTypeId == roomTypeId);
         if (roomType == null) { return NotFound("Room type not found"); }
         
        var reservations = await _context.Reservations
            .Include(r => r.ReservationRooms)
            .Where(r => r.ReservationRooms.Any(rr => rr.RoomTypeId == roomTypeId) &&
                        r.CheckInDate >= startDate && r.CheckOutDate <= endDate)
            .ToListAsync();

         var availabilities = new List<Availability>();
         var dateDiff = endDate - startDate;
         var days = dateDiff?.Days ?? 0;
         for (int i = 0; i <= days; i++)
         {
             var currDate = startDate.Value.UtcDateTime.AddDays(i);
             var blockedDate = blockedDates.FirstOrDefault(x => x.Date == currDate);
             var reservedRoomCount = reservations.Where(r => r.CheckInDate <= currDate && r.CheckOutDate > currDate)
                 .Select(r => r.ReservationRooms.Find(rr => rr.RoomTypeId == roomTypeId)?.RoomCount ?? 0)
                 .Sum();
             var availability = GetAvailability(roomType, currDate, blockedDate, reservedRoomCount);
             availabilities.Add(availability);
         }

         return Ok(availabilities);
    }

    // Blocks or updates the number of rooms available for a specific room type on a given date by considering existing reservations and room availability.
    [HttpPost("{roomTypeId}/availability")]
    public async Task<ActionResult> BlockRooms(Guid roomTypeId, [FromBody] Availability newAvailability)
    {
        if (newAvailability.AvailabilityDate == null) return BadRequest("Date cannot be empty");
        var blockedDate = await _context.BlockedDates.FirstOrDefaultAsync(x => x.Date == newAvailability.AvailabilityDate);
        
        var roomType = await _context.RoomTypes.FirstOrDefaultAsync(rt => rt.RoomTypeId == roomTypeId);
        if (roomType == null) { return NotFound("Room type not found"); }

        var date = (DateTime)newAvailability.AvailabilityDate;
        
        var reservedRoomCount = await _context.Reservations.Where(r => r.CheckInDate <= date && r.CheckOutDate > date)
            .Select(r => r.ReservationRooms.Find(rr => rr.RoomTypeId == roomTypeId).RoomCount)
            .SumAsync();
        
        var roomAvailability = GetAvailability(roomType, (DateTime)newAvailability.AvailabilityDate, blockedDate, reservedRoomCount);
        var availabilityDiff = roomAvailability?.Available - newAvailability.Available;
        if (availabilityDiff < 0) return BadRequest("Invalid number of available rooms");
        
        if (blockedDate == null)
        {
            blockedDate = new BlockedDate()
            {
                Date = newAvailability.AvailabilityDate ?? DateTime.Now,
                RoomTypeId = roomTypeId,
                Note = "User blocked rooms",
                RoomCount = 0
            };
            _context.BlockedDates.Add(blockedDate);
        }
        else
        {
            blockedDate.Note = "User modified the number of rooms blocked";
        }
        blockedDate.RoomCount += availabilityDiff ?? 0;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("available-room-types")]
    public async Task<ActionResult<List<Guid>>> GetAvailableRoomTypes([FromQuery] DateTimeOffset startDate, [FromQuery] DateTimeOffset endDate)
    {
        if (startDate == default || endDate == default || startDate > endDate)
        {
            return BadRequest("Please provide a valid date range.");
        }

        // Get all room types
        var roomTypes = await _context.RoomTypes.ToListAsync();

        var availableRoomTypeIds = new List<Guid>();

        foreach (var roomType in roomTypes)
        {
            // Get the blocked dates for the given room type within the date range
            var blockedDates = await _context.BlockedDates
                .Where(x => x.RoomTypeId == roomType.RoomTypeId &&
                            x.Date >= startDate.UtcDateTime &&
                            x.Date <= endDate.UtcDateTime)
                .ToListAsync();

            // Get reservations for the room type in the date range
            var reservations = await _context.Reservations
                .Include(r => r.ReservationRooms)
                .Where(r => r.ReservationRooms.Any(rr => rr.RoomTypeId == roomType.RoomTypeId) &&
                            r.CheckInDate < endDate &&
                            r.CheckOutDate > startDate)
                .ToListAsync();

            // Calculate availability for each date in the range
            bool isAvailable = true;

            var totalDays = (endDate - startDate).Days + 1;

            for (int i = 0; i < totalDays; i++)
            {
                var currentDate = startDate.UtcDateTime.AddDays(i);

                // Count reserved rooms for that day
                var reservedRoomCount = reservations
                    .Where(r => r.CheckInDate.Date <= currentDate.Date && r.CheckOutDate.Date > currentDate.Date)
                    .SelectMany(r => r.ReservationRooms)
                    .Where(rr => rr.RoomTypeId == roomType.RoomTypeId)
                    .Sum(rr => rr.RoomCount);

                // Count blocked rooms for that day
                var blockedRoomCount = blockedDates
                    .FirstOrDefault(x => x.Date.Date == currentDate.Date)?.RoomCount ?? 0;

                var availableRooms = roomType.RoomCount - blockedRoomCount - reservedRoomCount;

                if (availableRooms <= 0)
                {
                    isAvailable = false;
                    break;
                }
            }

            if (isAvailable)
            {
                availableRoomTypeIds.Add(roomType.RoomTypeId);
            }
        }

        return Ok(availableRoomTypeIds);
    }
    [HttpPut("{roomTypeId}")]
    public async Task<ActionResult> ToggleRoomTypeStatus(Guid roomTypeId, [FromQuery] DateTime dateQuery, [FromQuery] bool isOpen)
    {
        var roomType = await _context.RoomTypes.FirstOrDefaultAsync(rt => rt.RoomTypeId == roomTypeId);
        if (roomType == null) { return NotFound("Room type not found"); }
        
        var blockedDate = await _context.BlockedDates.FirstOrDefaultAsync(x => x.Date.Date == dateQuery.Date &&
                                                                               x.RoomTypeId == roomTypeId);
        if (isOpen)
        {
            if (blockedDate != null)  _context.BlockedDates.Remove(blockedDate);
            await _context.SaveChangesAsync();
        }
        else
        {
            var newAvailability = new Availability()
            {
                AvailabilityDate = dateQuery,
                RoomTypeId = roomTypeId,
                Available = 0 // close all rooms of that room type
            };
            await BlockRooms(roomTypeId, newAvailability);
        }
        
        return Ok();
    }
    
    private static Availability GetAvailability(RoomType roomType, DateTime queryDate, BlockedDate? blockedDate, int reservedRooms)
    {
        var blockedDateCount = blockedDate?.RoomCount ?? 0;
        var availability = new Availability()
        {
            AvailabilityDate = queryDate,
            Available = roomType.RoomCount - blockedDateCount - reservedRooms,
            RoomTypeId = roomType.RoomTypeId,
        };

        return availability;
    }

    
    
}
