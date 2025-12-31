using Newtonsoft.Json;
using WorkoutTrackerApi.DTO.User;

namespace WorkoutTrackerApi.DTO.Auth;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = null!;
    
    [JsonIgnore]
    public string RefreshToken { get; set; } = null!;
    
    public UserDto? User { get; set; }
}