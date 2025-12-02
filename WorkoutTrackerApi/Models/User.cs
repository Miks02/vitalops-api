using Microsoft.AspNetCore.Identity;

namespace WorkoutTrackerApi.Models;

public class User : IdentityUser
{
    
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;
    
    public string? RefreshToken { get; set; } 
    
    public DateTime? TokenExpDate { get; set; }
}