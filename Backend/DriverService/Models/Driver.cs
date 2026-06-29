namespace DriverService.Models;

public class Driver
{
    public int Id { get; set; }
    public int AuthUserId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Country { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
