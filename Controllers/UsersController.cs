using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Overbookedapi.Models;
using Overbookedapi.Data;
using Overbookedapi.Models.DTO;

namespace Overbookedapi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly DataSet _dbContext;

    public UsersController(DataSet dbContext)
    {
        _dbContext = dbContext;
    }

    // GET: api/Users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return Ok(await _dbContext.Users.ToListAsync());
    }

    // GET: api/Users/{email}
    [HttpGet("email/{email}")]
    public async Task<ActionResult<UserRecoil>> GetUser(string email)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);

        if (user == null)
        {
            return NotFound();
        }

        var recoilData = new UserRecoil()
        {
            Email = user.Email,
            HotelId = user.HotelId,
            Username = user.Username,
            RoleId = user.RoleId
        };

        return Ok(recoilData);
    }

    // POST: api/Users
    [HttpPost]
    public async Task<ActionResult> CreateUser(UserDTO userDTO)
    {
        // Validate hotel credentials
        var hotel = await _dbContext.Hotels
            .FirstOrDefaultAsync(h => h.HotelId == userDTO.HotelId && h.Password == userDTO.HotelPassword);

        if (hotel == null)
        {
            return BadRequest("Invalid Hotel ID or Hotel Password.");
        }
        var newUser = new User
        {
            Email = userDTO.Email,
            HotelId = userDTO.HotelId,
            Username = userDTO.Email.Split("@")[0], // default
            RoleId = 1 // Assuming a default RoleId, like '1' for a standard role
        };

        _dbContext.Users.Add(newUser);
        await _dbContext.SaveChangesAsync();

        return Ok($"User with Id {newUser.UserId} has been created.");
    }

    // PUT: api/Users/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, User user)
    {
        if (id != user.UserId)
        {
            return BadRequest();
        }

        _dbContext.Entry(user).State = EntityState.Modified;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserExists(id))
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

    // DELETE: api/Users/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    private bool UserExists(int id)
    {
        return _dbContext.Users.Any(e => e.UserId == id);
    }
}