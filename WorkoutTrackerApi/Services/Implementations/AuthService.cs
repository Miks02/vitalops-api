using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WorkoutTrackerApi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WorkoutTrackerApi.DTO.Auth;
using WorkoutTrackerApi.DTO.User;
using WorkoutTrackerApi.Models;
using WorkoutTrackerApi.Services.Results;

namespace WorkoutTrackerApi.Services.Implementations;

public class AuthService : BaseService<AuthService>,IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    
    public AuthService(
        UserManager<User> userManager, 
        RoleManager<IdentityRole> roleManager, 
        IUserService userService,
        IHttpContextAccessor http,
        ILogger<AuthService> logger,
        IConfiguration configuration
        ) : base(http, logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _userService = userService;
        _configuration = configuration;
    }
    
    public async Task<ServiceResult<UserDto>> RegisterAsync(RegisterRequestDto request)
    {
        
        var user = new User()
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserName = request.UserName,
            Email = request.Email
        };

        var createUser = await _userManager.CreateAsync(user, request.Password);

        if (!createUser.Succeeded)
        {
            LogError("Error occurred while trying to register a new user");

            foreach (var error in createUser.Errors)
            {
                LogError("ERROR: " + error.Description);
                
                if(error.Code == "DuplicateUserName")
                    return ServiceResult<UserDto>.Failure(Error.User.UsernameAlreadyExists());
                if(error.Code == "DuplicateEmail")
                    return ServiceResult<UserDto>.Failure(Error.User.EmailAlreadyExists());
            }
            
            return ServiceResult<UserDto>.Failure(Error.Auth.RegistrationFailed());
        }

        var assignRoleResult = await AssignRoleAsync(user);

        if (!assignRoleResult.IsSucceeded)
        {
            var deleteUserResult = await _userService.DeleteUserAsync(user);

            if (!deleteUserResult.IsSucceeded)
                LogResultErrors("Error occurred while trying to delete user", true, deleteUserResult.Errors.ToArray());
            
            return ServiceResult<UserDto>.Failure(Error.Auth.RegistrationFailed());
        }

        var userDto = new UserDto()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserName = user.UserName,
            Email = user.Email
        };
        
        LogInformation($"User {userDto.UserName} registered successfully");
        return ServiceResult<UserDto>.Success(userDto);
    }

    public async Task<ServiceResult<TokenResponseDto>> LoginAsync(LoginRequestDto request)
    {
        
        var user = await _userService.GetUserByUserNameAsync(request.UserName);

        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return ServiceResult<TokenResponseDto>.Failure(Error.Auth.LoginFailed("Invalid email or password"));
        }    

        var tokenResult = await GenerateAuthTokens(user);

        if (!tokenResult.IsSucceeded)
        {
            LogCritical("Failed to generate access and/or refresh token for a user after successful login");
            return ServiceResult<TokenResponseDto>.Failure(tokenResult.Errors.ToArray());
        }
        
        LogInformation($"User {user.UserName} logged in successfully");
        return ServiceResult<TokenResponseDto>.Success(tokenResult.Payload!);

    }

    private string CreateAccessToken(User user, IList<string> roles)
    {
        
        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName!),
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("JwtConfig:Token")!));

        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _configuration["JwtConfig:Issuer"],
            audience: _configuration["JwtConfig:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    private string CreateRefreshToken()
    {
        var randomNumber = new Byte[32];

        using var rng = RandomNumberGenerator.Create();
        
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }

    private async Task<ServiceResult> AssignRefreshTokenAsync(User user)
    {
        user.RefreshToken = CreateRefreshToken();
        user.TokenExpDate = DateTime.UtcNow.AddDays(7);

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var identityErrors = result.Errors.Select(e => new Error(e.Code, e.Description));
            
            LogResultErrors("Unexpected error happened while assigning refresh token to the user", true, identityErrors.ToArray());
            return ServiceResult.Failure(Error.Auth.JwtError());
        }

        return ServiceResult.Success();
    }

    private async Task<ServiceResult<string>> RotateRefreshToken(User user)
    {
        var result = await AssignRefreshTokenAsync(user);

        if (!result.IsSucceeded)
        {
            return ServiceResult<string>.Failure(result.Errors.ToArray());
        }

        if (user.RefreshToken is null)
            return ServiceResult<string>.Failure(Error.General.UnknownError("Unexpected error occurred. Refresh token is null"));
        

        return ServiceResult<string>.Success(user.RefreshToken);
    }
    
    private async Task<ServiceResult<TokenResponseDto>> GenerateAuthTokens(User user)
    {
        var result = await RotateRefreshToken(user);

        if (!result.IsSucceeded)
            return ServiceResult<TokenResponseDto>.Failure(result.Errors.ToArray());

        var userRoles = await _userService.GetUserRolesAsync(user);
        
        var tokenResponse = new TokenResponseDto()
        {
            RefreshToken = result.Payload!,
            AccessToken = CreateAccessToken(user, userRoles)
        };

        return ServiceResult<TokenResponseDto>.Success(tokenResponse);
    }

    public async Task<ServiceResult<TokenResponseDto>> RotateAuthTokens(string refreshToken)
    {
        var user = await _userService.GetUserByRefreshTokenAsync(refreshToken);
        
        if(user is null)
            return ServiceResult<TokenResponseDto>.Failure(Error.User.NotFound());
        
        if(user.TokenExpDate < DateTime.UtcNow)
            return ServiceResult<TokenResponseDto>.Failure(Error.Auth.ExpiredToken());

        var result = await GenerateAuthTokens(user);

        if (!result.IsSucceeded)
            return ServiceResult<TokenResponseDto>.Failure(result.Errors.ToArray());
        
        return ServiceResult<TokenResponseDto>.Success(result.Payload!);
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