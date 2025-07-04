namespace TaskManagement.Models
{
    public class TaskItem

    {

        public int Id { get; set; }
        public DateTime Date { get; set; }

        public string? Title { get; set; }

        public string? Description{ get; set; }

        public int? AssignedToId { get; set; } // Foreign key reference
        public User? AssignedTo { get; set; }  // Navigation property

        public Status Status { get; set; } = Status.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime StartDate { get; set; } 

        public DateTime EndDate { get; set; }

    }
}
