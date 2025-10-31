namespace DemoProject.API.Data.Models
{
    public class Note
    {
        public int Id { get; set; }
        public string? Content { get; set; } 
        public string Url { get; set; }
        public bool IsSecure { get; set; }
        public string? PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; } 
        public DateTime? UpdatedAt { get; set; }
    }
}
