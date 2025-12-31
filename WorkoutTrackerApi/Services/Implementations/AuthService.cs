using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WorkoutTrackerApi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WorkoutTrackerApi.Data;
using WorkoutTrackerApi.DTO.Auth;
using WorkoutTrackerApi.DTO.User;
using WorkoutTrackerApi.Models;
using WorkoutTrackerApi.Services.Results;

namespace WorkoutTrackerApi.Services.Implementations;

public class AuthService : BaseService<AuthService>, IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthService
            (   
            ILogger<AuthService> logger, 
            UserManager<User> userManager, 
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration
            ) : base(logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }
    
    
    public async Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        
        try
        {
            var user = new User()
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.UserName,
                Email = request.Email,
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);

            if (!createResult.Succeeded)
            {
                LogError("Error happened while trying to create a user");

                foreach (var error in createResult.Errors)
                {
                    LogError("ERROR: " + error.Description);
                    if (error.Code == "DuplicateEmail")
                        return ServiceResult<AuthResponseDto>.Failure(Error.User.EmailAlreadyExists());
                    if(error.Code == "DuplicateUserName")
                        return ServiceResult<AuthResponseDto>.Failure(Error.User.UsernameAlreadyExists());
                }
            }

            var assignResult = await AssignRoleAsync(user);

            if (!assignResult.IsSucceeded)
                return ServiceResult<AuthResponseDto>.Failure(assignResult.Errors.ToArray());

            var generateTokens = await GenerateAuthTokens(user);

            if (!generateTokens.IsSucceeded)
            {
                LogError("Failed to generate tokens for a newly registered user " + user.Id);
                return ServiceResult<AuthResponseDto>.Failure(generateTokens.Errors.ToArray());
            }

            var responseDto = new AuthResponseDto()
            {
                AccessToken = generateTokens.Payload!.AccessToken,
                User = new UserDto()
                {
                    Email = user.Email,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                }
            };
            
            return ServiceResult<AuthResponseDto>.Success(responseDto);
        }
        catch (Exception ex)
        {
            LogError("Error happened while trying to create new user ", ex);
        }
        
        return ServiceResult<AuthResponseDto>.Failure(Error.Database.OperationFailed());
    }

    public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            LogError($"Failed sign in for user with email: {request.Email}. User not found");
            return ServiceResult<AuthResponseDto>.Failure(Error.Auth.LoginFailed("Incorrect email or password"));
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            LogError($"Failed sign in for user with email: {request.Email}. Incorrect password");
            return ServiceResult<AuthResponseDto>.Failure(Error.Auth.LoginFailed("Incorrect email or password"));
        }

        var newAccessToken = await GenerateAuthTokens(user);
        
        if(!newAccessToken.IsSucceeded)
            return ServiceResult<AuthResponseDto>.Failure(newAccessToken.Errors.ToArray());
        
        LogInformation($"Sign in successful for user {user.Email}");
        
        var responseDto = new AuthResponseDto()
        {
            AccessToken = newAccessToken.Payload!.AccessToken,
            User = new UserDto()
            {
                Email = user.Email!,
                UserName = user.UserName!,
                FirstName = user.FirstName,
                LastName = user.LastName
            }
        };
        
        return ServiceResult<AuthResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult> LogoutAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            LogError($"User with id {userId} had not been found");
            return ServiceResult.Failure(Error.User.NotFound(userId));
        }

        user.RefreshToken = null;
        user.TokenExpDate = null;
        
        var removeTokenResult = await _userManager.UpdateAsync(user);

        if (!removeTokenResult.Succeeded)
        {
            var errors = removeTokenResult.Errors.Select(e => new Error(e.Code, e.Description));
            LogResultErrors($"Failed to sign out | UserID: {userId} " + errors);
            return ServiceResult.Failure(errors.ToArray());
        }
        
        LogInformation($"{user.UserName} signed out successfully!");
        return ServiceResult.Success();

    }

    private async Task<string> CreateAccessToken(User user)
    {
        var rolesList = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>()
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
        };
        
        foreach (var role in rolesList)
            claims.Add(new Claim(ClaimTypes.Role, role));

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

    private string CreateRefreshToken()
    {
        var randomBytes = new Byte[32];

        using var rng = RandomNumberGenerator.Create();

        rng.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes);
    }

    private async Task<ServiceResult> AssignRefreshToken(User user)
    {
        user.RefreshToken = CreateRefreshToken();
        user.TokenExpDate = DateTime.UtcNow.AddDays(7);

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            LogError("Error happened while assigning refresh token to the user");
            return ServiceResult.Failure(Error.Auth.JwtError());
        }

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<TokenResponseDto>> RotateAuthTokens(string refreshToken)
    {
        var user = await _userManager.Users
            .Where(u => u.RefreshToken == refreshToken)
            .FirstOrDefaultAsync();

        if (user is null || user.RefreshToken is null)
        {
            LogError($"Failed to regenerate access and refresh tokens. User or refresh token is null");
            return ServiceResult<TokenResponseDto>.Failure(Error.Auth.JwtError("Failed to regenerate auth tokens"));
        }

        if (user.TokenExpDate < DateTime.UtcNow)
        {
            var expiredError = Error.Auth.JwtError("Refresh token has expired. Login required");
            LogError("Failed to regenerate auth tokens. " + expiredError);
            return ServiceResult<TokenResponseDto>.Failure(expiredError);
        }
        
        var newAccessToken = await CreateAccessToken(user);
        var newRefreshToken = await AssignRefreshToken(user);

        if (!newRefreshToken.IsSucceeded)
        {
            LogResultErrors(newRefreshToken.Errors.ToArray());
            return ServiceResult<TokenResponseDto>.Failure(newRefreshToken.Errors.ToArray());
        }
        
        
        return ServiceResult<TokenResponseDto>.Success(new TokenResponseDto() {AccessToken = newAccessToken});

    }

    public async Task<ServiceResult<TokenResponseDto>> GenerateAuthTokens(User user)
    {

        var assignRefreshToken = await AssignRefreshToken(user);

        if (!assignRefreshToken.IsSucceeded)
            return ServiceResult<TokenResponseDto>.Failure(assignRefreshToken.Errors.ToArray());

        var newAccessToken = await CreateAccessToken(user);
        
        LogInformation("Auth tokens generated successfully");
        return ServiceResult<TokenResponseDto>.Success(new TokenResponseDto() {AccessToken = newAccessToken});

    }
    
    private async Task<ServiceResult> AssignRoleAsync(User user)
    {
        
        if (!await _roleManager.RoleExistsAsync("User"))
        {
            IdentityResult createRoleResult = await _roleManager.CreateAsync(new IdentityRole("User"));

            if (!createRoleResult.Succeeded)
            {
                var identityErrors = createRoleResult.Errors.Select(e => new Error(e.Code, e.Description));
                
                LogResultErrors("Unexpected error happened while creating a new role for the user", identityErrors.ToArray());
                
                return ServiceResult.Failure(Error.General.UnknownError("Unexpected error happened while creating a new role"));
            }
        }

        IdentityResult addToRoleResult = await _userManager.AddToRoleAsync(user, "User");
        if (!addToRoleResult.Succeeded)
        {
            var identityErrors = addToRoleResult.Errors.Select(e => new Error(e.Code, e.Description));

            LogResultErrors("Unexpected error happened while assigning role to the user", identityErrors.ToArray());
            
            return ServiceResult.Failure(Error.General.UnknownError("Unexpected error happened while assigning role to the user"));
        }
        LogInformation("User assigned to role successfully");
        return ServiceResult.Success();

    }
}