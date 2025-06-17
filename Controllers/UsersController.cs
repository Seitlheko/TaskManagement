using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    public UsersController(AppDbContext context) => _context = context;

    // 🔓 Allow login/signup without auth
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(User user)
    {
        if (string.IsNullOrEmpty(user.Email))
            return BadRequest("Email is required");

        if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            return BadRequest("User already exists");

        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok(user);
    }

    // 🔐 Requires JWT
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll() =>
        Ok(await _context.Users.ToListAsync());

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> Get(int id)
    {
        var user = await _context.Users.FindAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null) return NotFound();
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
