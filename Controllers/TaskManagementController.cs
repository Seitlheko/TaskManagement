using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagement.Data;
using TaskManagement.Models;

[ApiController]
[Route("tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _context;
    public TasksController(AppDbContext context) => _context = context;

    private int GetUserId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    private string GetUserRole() =>
        User.FindFirstValue(ClaimTypes.Role) ?? "User";

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        var isAdmin = GetUserRole() == "Admin";

        var query = _context.Tasks
            .Include(t => t.AssignedTo)
            .AsQueryable();

        if (!isAdmin)
            query = query.Where(t => t.AssignedToId == userId);

        var tasks = await query.Select(t => new
        {
            t.Id,
            t.Title,
            t.Description,
            t.Status,
            t.CreatedAt,
            t.UpdatedAt,
            t.StartDate,
            t.EndDate,
            AssignedTo = new
            {
                t.AssignedToId,
                t.AssignedTo.Name,
                t.AssignedTo.Email,
                t.AssignedTo.Role
            }
        }).ToListAsync();

        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var userId = GetUserId();
        var isAdmin = GetUserRole() == "Admin";

        var task = await _context.Tasks
            .Include(t => t.AssignedTo)
            .Where(t => t.Id == id && (isAdmin || t.AssignedToId == userId))
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                t.Status,
                t.CreatedAt,
                t.UpdatedAt,
                t.StartDate,
                t.EndDate,
                AssignedTo = new
                {
                    t.AssignedToId,
                    t.AssignedTo.Name,
                    t.AssignedTo.Email,
                    t.AssignedTo.Role
                }
            }).FirstOrDefaultAsync();

        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
    {
        var assignedUser = await _context.Users.FindAsync(request.AssignedToId);
        if (assignedUser == null)
            return BadRequest("Assigned user not found");

        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            AssignedToId = request.AssignedToId,
            AssignedTo = assignedUser,
            Status = request.Status,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = task.Id }, new
        {
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.CreatedAt,
            task.UpdatedAt,
            task.StartDate,
            task.EndDate,
            AssignedTo = new
            {
                task.AssignedToId,
                assignedUser.Name,
                assignedUser.Email
            }
        });

    }


    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] TaskItem updated)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task is null) return NotFound();

        var assignedUser = await _context.Users.FindAsync(updated.AssignedToId);
        if (assignedUser == null) return BadRequest("Assigned user not found");

        task.Title = updated.Title;
        task.Description = updated.Description;
        task.AssignedTo = assignedUser;
        task.Status = updated.Status;
        task.StartDate = updated.StartDate;
        task.EndDate = updated.EndDate;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task is null) return NotFound();

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
