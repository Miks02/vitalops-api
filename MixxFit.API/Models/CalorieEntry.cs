namespace MixxFit.API.Models;

public class CalorieEntry
{
    public int Id { get; set; }

    public string Description { get; set; } = null!;
    public int Calories { get; set; }

    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}