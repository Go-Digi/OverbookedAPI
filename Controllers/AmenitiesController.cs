using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Overbookedapi.Models;
using Overbookedapi.Models.DTO;
using Overbookedapi.Data;
using Overbookedapi.Utils;

namespace Overbookedapi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AmenitiesController : ControllerBase
{
    private readonly DataSet _context;
    private readonly IUserActivityLogger _userActivityLogger;

    public AmenitiesController(DataSet context, IUserActivityLogger userActivityLogger)
    {
        _context = context;
        _userActivityLogger = userActivityLogger;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Amenity>>> GetAmenities()
    {
        // TODO: Change to only get amenities by hotel after implementing JWT
        return await _context.Amenities.ToListAsync(); 
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Amenity>> GetAmenity(int id)
    {
        var amenity = await _context.Amenities.FindAsync(id);
        
        if (amenity == null)
        {
            return NotFound();
        }
        
        return amenity;
    }
    
    [HttpPost]
    public async Task<ActionResult<Amenity>> CreateAmenity(AmenityDTO amenity)
    {
        var newAmenity = new Amenity()
        {
            HotelId = amenity.HotelId,
            AmenityName = amenity.AmenityName,
            Description = amenity.Description,
            Price = amenity.Price,
        };
        _context.Amenities.Add(newAmenity);
        await _context.SaveChangesAsync();
            
        return CreatedAtAction(nameof(GetAmenity), new { id = amenity.AmenityId }, amenity);
    }
    
    public async Task<IActionResult> UpdateAmenity(int id, AmenityDTO amenity)
    {
        if (id != amenity.AmenityId)
        {
            return BadRequest();
        }
        
        var amnesityToUpdate = await _context.Amenities.FindAsync(id);
        if (amnesityToUpdate == null) return NotFound();
        amnesityToUpdate.AmenityName = amenity.AmenityName;
        amnesityToUpdate.Description = amenity.Description;
        amnesityToUpdate.Price = amenity.Price;
        _context.Amenities.Update(amnesityToUpdate);
        
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!AmenityExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        
        return NoContent();
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAmenity(int id)
    {
        var amenity = await _context.Amenities.FindAsync(id);
        if (amenity == null)
        {
            return NotFound();
        }
        
        _context.Amenities.Remove(amenity);
        
        var existingRoomAmenities = await _context.RoomAmenities.Where(x => x.AmenityId == id)
            .ToListAsync();
        _context.RoomAmenities.RemoveRange(existingRoomAmenities);
        
        await _context.SaveChangesAsync();
            
        return NoContent();
    }
    
    private bool AmenityExists(int id)
    {
        return _context.Amenities.Any(e => e.AmenityId == id);
    }

    
    
}