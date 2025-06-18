using TaskManagement.Models;

public class CreateTaskRequest
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int AssignedToId { get; set; }
    public Status Status { get; set; } = Status.Pending;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
