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
    
    

    
    
}