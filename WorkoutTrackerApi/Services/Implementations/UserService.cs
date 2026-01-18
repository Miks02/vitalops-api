using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using WorkoutTrackerApi.DTO.User;
using WorkoutTrackerApi.Enums;
using WorkoutTrackerApi.Extensions;
using WorkoutTrackerApi.Models;
using WorkoutTrackerApi.Services.Interfaces;
using WorkoutTrackerApi.Services.Results;

namespace WorkoutTrackerApi.Services.Implementations;

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
                Weight = u.WeightKg,
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

    public async Task<ServiceResult> DeleteUserAsync(User user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.HandleIdentityResult(_logger);
    }

    public async Task<ServiceResult> DeleteUserAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Where(u => u.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if(user is null)
            return ServiceResult.Failure(Error.User.NotFound());

        return await DeleteUserAsync(user);
    }

    public async Task<ServiceResult<DateTime>> UpdateDateOfBirthAsync(UpdateDateOfBirthDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.DateOfBirth = dto.DateOfBirth;

        var updateResult = await _userManager.UpdateAsync(user);

        return updateResult.HandleIdentityResult(dto.DateOfBirth, _logger);
    }

    public async Task<ServiceResult<double>> UpdateWeightAsync(UpdateWeightDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.WeightKg = dto.Weight;

        var updateResult = await _userManager.UpdateAsync(user);

        return updateResult.HandleIdentityResult(dto.Weight, _logger);

    }

    public async Task<ServiceResult<double>> UpdateHeightAsync(UpdateHeightDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.HeightCm = dto.Height;

        var updateResult = await _userManager.UpdateAsync(user);

        
        return updateResult.HandleIdentityResult(dto.Height, _logger);

    }

    public async Task<ServiceResult<Gender>> UpdateGenderAsync(UpdateGenderDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.Gender = dto.Gender;

        var updateResult = await _userManager.UpdateAsync(user);

        return updateResult.HandleIdentityResult(dto.Gender, _logger);
    }

    public async Task<ServiceResult<UpdateFullNameDto>> UpdateFullNameAsync(UpdateFullNameDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;

        var updateResult = await _userManager.UpdateAsync(user);

        return updateResult.HandleIdentityResult(dto, _logger);
    }

    public async Task<ServiceResult<string>> UpdateEmailAsync(UpdateEmailDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.Email = dto.Email;

        var updateResult = await _userManager.UpdateAsync(user);

        return updateResult.HandleIdentityResult(dto.Email, _logger);
    }

    public async Task<ServiceResult<string>> UpdateUserNameAsync(UpdateUserNameDto dto, string userId, CancellationToken cancellationToken = default)
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