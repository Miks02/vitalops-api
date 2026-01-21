using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using VitalOps.API.DTO.User;
using VitalOps.API.Enums;
using VitalOps.API.Models;
using VitalOps.API.Services.Interfaces;
using VitalOps.API.Services.Results;
using VitalOps.API.Extensions;

namespace VitalOps.API.Services.Implementations;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<UserService> _logger;

    public UserService(UserManager<User> userManager, ILogger<UserService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<UserDetailsDto> GetUserDetailsAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserDetailsDto
            {
                FullName = u.FirstName + ' ' + u.LastName,
                UserName = u.UserName!,
                Email = u.Email!,
                ImagePath = u.ImagePath,
                CurrentWeight = u.CurrentWeight,
                Height = u.HeightCm,
                DateOfBirth = u.DateOfBirth,
                RegisteredAt = u.CreatedAt,
                AccountStatus = u.AccountStatus,
                Gender = u.Gender
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            throw new InvalidOperationException("User not found");

        return user;
    }

    public async Task<Result> DeleteUserAsync(User user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.HandleIdentityResult(_logger);
    }

    public async Task<Result> DeleteUserAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Where(u => u.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if(user is null)
            return Result.Failure(Error.User.NotFound());

        return await DeleteUserAsync(user);
    }

    public async Task<Result<DateTime>> UpdateDateOfBirthAsync(UpdateDateOfBirthDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.DateOfBirth = dto.DateOfBirth;

        var updateResult = await _userManager.UpdateAsync(user);

        return updateResult.HandleIdentityResult(dto.DateOfBirth, _logger);
    }

    public async Task<Result<double>> UpdateHeightAsync(UpdateHeightDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.HeightCm = dto.Height;

        var updateResult = await _userManager.UpdateAsync(user);

        
        return updateResult.HandleIdentityResult(dto.Height, _logger);

    }

    public async Task<Result<Gender>> UpdateGenderAsync(UpdateGenderDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.Gender = dto.Gender;

        var updateResult = await _userManager.UpdateAsync(user);

        return updateResult.HandleIdentityResult(dto.Gender, _logger);
    }

    public async Task<Result<UpdateFullNameDto>> UpdateFullNameAsync(UpdateFullNameDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;

        var updateResult = await _userManager.UpdateAsync(user);

        return updateResult.HandleIdentityResult(dto, _logger);
    }

    public async Task<Result<string>> UpdateEmailAsync(UpdateEmailDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.Email = dto.Email;

        var updateResult = await _userManager.UpdateAsync(user);

        return updateResult.HandleIdentityResult(dto.Email, _logger);
    }

    public async Task<Result<string>> UpdateUserNameAsync(UpdateUserNameDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.UserName = dto.UserName;

        var updateResult = await _userManager.UpdateAsync(user);

        return updateResult.HandleIdentityResult(dto.UserName, _logger);

    }

    private async Task<User> GetUserForUpdateAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId), "CRITICAL ERROR: UserID is null or empty");

        var user = await _userManager.FindByIdAsync(userId);

        return user ?? throw new InvalidOperationException("CRITICAL ERROR: User is null");
    }
}