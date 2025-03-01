using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Overbookedapi.Models;
using Overbookedapi.Models.DTO;
using Overbookedapi.Data;
using Overbookedapi.Utils;

namespace Overbookedapi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoomTypesController : ControllerBase
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
        var roomType = await _context.RoomTypes.Include(rt => rt.RoomRates).FirstOrDefaultAsync(rt => rt.RoomTypeId == id);

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
        
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("{roomTypeId}/availability")]
    public async Task<ActionResult<List<Availability>>> GetAvailability(Guid roomTypeId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        if (startDate == null || endDate == null)
        {
            return BadRequest("Please provide valid dates");
        }

        if (startDate > endDate)
        {
            return BadRequest("End date cannot be earlier than start date");
        }
        
        var blockedDates = await _context.BlockedDates.Where(x => x.Date >= startDate && 
                                                                                x.Date <= endDate &&
                                                                                x.RoomTypeId == roomTypeId)
                                                                        .ToListAsync();
         var roomType = await _context.RoomTypes.FirstOrDefaultAsync(rt => rt.RoomTypeId == roomTypeId);
         if (roomType == null) { return NotFound("Room type not found"); }
         // TODO: query bookings

         var availabilities = new List<Availability>();
         var dateDiff = endDate - startDate;
         var days = dateDiff?.Days ?? 0;
         for (int i = 0; i <= days; i++)
         {
             var currDate = startDate?.AddDays(i);
             var blockedDate = blockedDates.FirstOrDefault(x => x.Date == currDate);
             var availability = GetAvailability(roomType, (DateTime)currDate, blockedDate);
             availabilities.Add(availability);
         }

         return Ok(availabilities);
    }
    private static Availability GetAvailability(RoomType roomType, DateTime queryDate, BlockedDate? blockedDate)
    {
        // TODO: query bookings
        
        var blockedDateCount = blockedDate?.RoomCount ?? 0;
        var availability = new Availability()
        {
            AvailabilityDate = queryDate,
            Available = roomType.RoomCount - blockedDateCount, // Correct formula: available = totalRooms - blockedRoomCount - number of bookings
            RoomTypeId = roomType.RoomTypeId,
        };

        return availability;
    }

    [HttpPost("{roomTypeId}/availability")]
    public async Task<ActionResult> BlockRooms(Guid roomTypeId, [FromBody] Availability newAvailability)
    {
        if (newAvailability.AvailabilityDate == null) return BadRequest("Date cannot be empty");
        var blockedDate = await _context.BlockedDates.FirstOrDefaultAsync(x => x.Date == newAvailability.AvailabilityDate);
        
        var roomType = await _context.RoomTypes.FirstOrDefaultAsync(rt => rt.RoomTypeId == roomTypeId);
        if (roomType == null) { return NotFound("Room type not found"); }
        
        var roomAvailability = GetAvailability(roomType, (DateTime)newAvailability.AvailabilityDate, blockedDate);
        var availabilityDiff = roomAvailability?.Available - newAvailability.Available;
        if(availabilityDiff < 0) return BadRequest("Invalid number of available rooms");
        
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

    
    
}