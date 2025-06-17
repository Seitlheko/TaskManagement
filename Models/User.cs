namespace TaskManagement.Models
{
    public class User
    {   
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; } // e.g., Admin, User, Guest
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        // Additional properties can be added as needed
    }
}