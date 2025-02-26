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
    
    

    
    
}