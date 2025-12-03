namespace WorkoutTrackerApi.DTO.Auth;

public class LoginRequestDto
{
    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;
    
    public bool RememberMe { get; set; }
}