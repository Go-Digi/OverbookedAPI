using Microsoft.AspNetCore.Mvc;
using Overbookedapi.Models; // Assuming your models are under this namespace
using Overbookedapi.Data; // Assuming your DbContext is here
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Overbookedapi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HotelsController : ControllerBase
{
    private readonly DataSet _context; // Replace ApplicationDbContext with your DbContext

    public HotelsController(DataSet context)
    {
        _context = context;
    }

    // GET: api/Hotels
    // Retrieves only HotelId and HotelName
    [HttpGet]
    public ActionResult<IEnumerable<object>> GetHotels()
    {
        var hotels = _context.Hotels
            .Select(h => new
            {
                h.HotelId,
                h.HotelName
            })
            .ToList();

        return Ok(hotels);
    }

    // GET: api/Hotels/{id}
    [HttpGet("{id}")]
    public ActionResult<Hotel> GetHotel(int id)
    {
        var hotel = _context.Hotels.Find(id);

        if (hotel == null)
        {
            return NotFound();
        }

        return Ok(hotel);
    }

    // POST: api/Hotels
    [HttpPost]
    public ActionResult<Hotel> CreateHotel(Hotel hotel)
    {
        _context.Hotels.Add(hotel);
        _context.SaveChanges();

        return CreatedAtAction(nameof(GetHotel), new { id = hotel.HotelId }, hotel);
    }

    // PUT: api/Hotels/{id}
    [HttpPut("{id}")]
    public IActionResult UpdateHotel(int id, Hotel hotel)
    {
        if (id != hotel.HotelId)
        {
            return BadRequest();
        }

        _context.Entry(hotel).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

        try
        {
            _context.SaveChanges();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Hotels.Any(h => h.HotelId == id))
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

    // DELETE: api/Hotels/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteHotel(int id)
    {
        var hotel = _context.Hotels.Find(id);

        if (hotel == null)
        {
            return NotFound();
        }

        _context.Hotels.Remove(hotel);
        _context.SaveChanges();

        return NoContent();
    }
}