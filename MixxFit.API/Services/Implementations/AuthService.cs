using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MixxFit.API.DTO.Auth;
using MixxFit.API.Models;
using MixxFit.API.Services.Interfaces;
using MixxFit.API.Services.Results;
using MixxFit.API.Extensions;

namespace MixxFit.API.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly IUserService _userService;

    public AuthService
            (
            ILogger<AuthService> logger,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IUserService userService
            )
    {
        _logger = logger;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _userService = userService;
    }


    public async Task<Result<AuthResponseDto>> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var user = new User()
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserName = request.UserName,
            Email = request.Email,
        };

        var createResult = (await _userManager.CreateAsync(user, request.Password)).HandleIdentityResult(_logger);

        if (!createResult.IsSucceeded)
            return Result<AuthResponseDto>.Failure(createResult.Errors.ToArray());
        
        var assignResult = await AssignRoleAsync(user);

        if (!assignResult.IsSucceeded)
            return Result<AuthResponseDto>.Failure(assignResult.Errors.ToArray());

        var generateTokens = (await GenerateAuthTokens(user)).HandleResult(_logger);

        if (!generateTokens.IsSucceeded)
            return Result<AuthResponseDto>.Failure(generateTokens.Errors.ToArray());
        

        var userDetails = await _userService.GetUserDetailsAsync(user.Id, cancellationToken);

        var responseDto = new AuthResponseDto()
        {
            AccessToken = generateTokens.Payload!.AccessToken,
            RefreshToken = generateTokens.Payload!.RefreshToken,
            User = userDetails
        };

        return Result<AuthResponseDto>.Success(responseDto);


    }

    public async Task<Result<AuthResponseDto>> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            _logger.LogError("Failed sign in for user with email: {email}. User not found", request.Email);
            return Result<AuthResponseDto>.Failure(Error.Auth.LoginFailed("Incorrect email or password"));
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            _logger.LogError("Failed sign in for user with email: {email}. Incorrect password", request.Email);
            return Result<AuthResponseDto>.Failure(Error.Auth.LoginFailed("Incorrect email or password"));
        }

        var generateTokensResult = await GenerateAuthTokens(user);

        if(!generateTokensResult.IsSucceeded)
            return Result<AuthResponseDto>.Failure(generateTokensResult.Errors.ToArray());

        var userDetails = await _userService.GetUserDetailsAsync(user.Id, cancellationToken);

        var responseDto = new AuthResponseDto()
        {
            AccessToken = generateTokensResult.Payload!.AccessToken,
            RefreshToken = generateTokensResult.Payload!.RefreshToken,
            User = userDetails
        };
        

        return Result<AuthResponseDto>.Success(responseDto);
    }

    public async Task<Result> UpdatePasswordAsync(
        string userId, 
        UpdatePasswordDto request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure(Error.User.NotFound(userId));

        var changePasswordResult = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!changePasswordResult.Succeeded)
        {
            _logger.LogWarning("Error occurred while trying to change user's password {id}", userId);
            return Result.Failure(changePasswordResult.Errors.ToArray());
        }

        var logoutResult = await LogoutAsync(user.RefreshToken!, cancellationToken);

        return logoutResult.HandleResult(_logger);
    }

    public async Task<Result<AuthResponseDto>> RotateAuthTokens(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Where(u => u.RefreshToken == refreshToken)
            .FirstOrDefaultAsync(cancellationToken);

        _logger.LogInformation("Refresh token value: {refreshToken}", refreshToken);

        if (user is null)
        {
            _logger.LogError("Failed to regenerate access and refresh tokens. User is null");
            return Result<AuthResponseDto>.Failure(Error.Auth.JwtError("Failed to regenerate auth tokens"));
        }

        if (user.RefreshToken is null)
        {
            _logger.LogError("Failed to regenerate access and refresh tokens. Refresh token is null");
            return Result<AuthResponseDto>.Failure(Error.Auth.JwtError("Failed to regenerate auth tokens"));
        }

        if (user.TokenExpDate < DateTime.UtcNow)
        {
            var error = Error.Auth.ExpiredToken();
            _logger.LogError("Failed to regenerate auth tokens. {error}", error.Description);
            return Result<AuthResponseDto>.Failure(error);
        }

        var createAccessTokenResult = await CreateAccessToken(user);
        var newRefreshToken = (await AssignRefreshToken(user)).HandleResult(_logger);

        if (!newRefreshToken.IsSucceeded)
            return Result<AuthResponseDto>.Failure(newRefreshToken.Errors.ToArray());
        

        var authResponse = new AuthResponseDto()
        {
            RefreshToken = newRefreshToken.Payload!,
            AccessToken = createAccessTokenResult
        };

        return Result<AuthResponseDto>.Success(authResponse);
    }

    public async Task<Result> LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(refreshToken))
            return Result.Failure(Error.Auth.JwtError("Refresh token is missing"));
        
        
        var user = await _userManager.Users
            .Where(u => u.RefreshToken == refreshToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            _logger.LogError("User with refresh token {refreshToken} has not been found", refreshToken);
            return Result.Failure(Error.User.NotFound());
        }

        user.RefreshToken = null;
        user.TokenExpDate = null;

        var removeTokenResult = (await _userManager.UpdateAsync(user)).HandleIdentityResult(_logger);

        if (!removeTokenResult.IsSucceeded)
            return Result.Failure(removeTokenResult.Errors.ToArray());
        

        return Result.Success();
    }

    private async Task<string> CreateAccessToken(User user)
    {
        var rolesList = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>()
        {
            new (JwtRegisteredClaimNames.Sub, user.Id),
            new (JwtRegisteredClaimNames.Email, user.Email!),
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (ClaimTypes.NameIdentifier, user.Id),
        };

        claims.AddRange(rolesList.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtConfig:Token"]!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken
        (
            issuer: _configuration["JwtConfig:Issuer"],
            audience: _configuration["JwtConfig:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_configuration.GetValue<double>("JwtConfig:ExpirationInMinutes")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string CreateRefreshToken()
    {
        var randomBytes = new Byte[32];

        using var rng = RandomNumberGenerator.Create();

        rng.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes);
    }

    private async Task<Result<string>> AssignRefreshToken(User user)
    {
        user.RefreshToken = CreateRefreshToken();
        user.TokenExpDate = DateTime.UtcNow.AddDays(7);

        return (await _userManager.UpdateAsync(user)).HandleIdentityResult(user.RefreshToken, _logger);
    }

    private async Task<Result<TokenResponseDto>> GenerateAuthTokens(User user)
    {

        var assignRefreshToken = await AssignRefreshToken(user);

        if (!assignRefreshToken.IsSucceeded)
            return Result<TokenResponseDto>.Failure(assignRefreshToken.Errors.ToArray());


        var tokenResponse = new TokenResponseDto()
        {
            AccessToken = await CreateAccessToken(user),
            RefreshToken = assignRefreshToken.Payload!
        };

        _logger.LogInformation("Auth tokens generated successfully");
        return Result<TokenResponseDto>.Success(tokenResponse);

    }

    private async Task<Result> AssignRoleAsync(User user)
    {

        if (!await _roleManager.RoleExistsAsync("User"))
        {
            var createRoleResult = (await _roleManager.CreateAsync(new IdentityRole("User"))).HandleIdentityResult(_logger);

            if (!createRoleResult.IsSucceeded)
                return createRoleResult;
        }

        var addToRoleResult = (await _userManager.AddToRoleAsync(user, "User")).HandleIdentityResult(_logger);
        if (!addToRoleResult.IsSucceeded)
            return addToRoleResult;

        return Result.Success();

    }
}
