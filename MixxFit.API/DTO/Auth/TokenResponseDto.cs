namespace MixxFit.API.DTO.Auth;

public class TokenResponseDto
{
    public string AccessToken { get; set; } = null!;

    public string RefreshToken { get; set; } = null!;
}