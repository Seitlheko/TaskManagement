using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TaskManagement.Data;
using TaskManagement.Models;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    public UsersController(AppDbContext context) => _context = context;

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest("Email and password are required");

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("User already exists");

        using var sha = SHA256.Create();
        string hashed = Convert.ToBase64String(
            sha.ComputeHash(Encoding.UTF8.GetBytes(request.Password))
        );

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = hashed,
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            user.Id,
            user.Name,
            user.Email,
            user.Role,
            user.CreatedAt,
            user.UpdatedAt
        });
    }


    // 🔐 Authenticated routes
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var users = await _context.Users
            .Select(u => new { u.Id, u.Name, u.Email, u.Role })
            .ToListAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> Get(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        return Ok(new { user.Id, user.Name, user.Email, user.Role });
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
