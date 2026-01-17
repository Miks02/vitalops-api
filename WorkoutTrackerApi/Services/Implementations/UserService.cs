using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using WorkoutTrackerApi.DTO.User;
using WorkoutTrackerApi.Enums;
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

        if (!result.Succeeded)
        {
            var identityErrors = result.Errors.Select(e => new Error(e.Code, e.Description));
            return ServiceResult.Failure(identityErrors.ToArray());
        }
        
        return ServiceResult.Success();
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

        if (updateResult.Succeeded)
            return ServiceResult<DateTime>.Success(dto.DateOfBirth);

        foreach (var error in updateResult.Errors)
            _logger.LogWarning("Code: {code} Description: {description}", error.Code, error.Description);
        
        return ServiceResult<DateTime>.Failure(updateResult.Errors.ToArray());

    }

    public async Task<ServiceResult<double>> UpdateWeightAsync(UpdateWeightDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.WeightKg = dto.Weight;

        var updateResult = await _userManager.UpdateAsync(user);

        if (updateResult.Succeeded)
            return ServiceResult<double>.Success(dto.Weight);

        foreach (var error in updateResult.Errors)
            _logger.LogWarning("Code: {code} Description: {description}", error.Code, error.Description);
        
        return ServiceResult<double>.Failure(updateResult.Errors.ToArray());

    }

    public async Task<ServiceResult<double>> UpdateHeightAsync(UpdateHeightDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.HeightCm = dto.Height;

        var updateResult = await _userManager.UpdateAsync(user);

        if (updateResult.Succeeded)
            return ServiceResult<double>.Success(dto.Height);

        foreach (var error in updateResult.Errors)
            _logger.LogWarning("Code: {code} Description: {description}", error.Code, error.Description);
        
        return ServiceResult<double>.Failure(updateResult.Errors.ToArray());

    }

    public async Task<ServiceResult<Gender>> UpdateGenderAsync(UpdateGenderDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.Gender = dto.Gender;

        var updateResult = await _userManager.UpdateAsync(user);

        if (updateResult.Succeeded)
            return ServiceResult<Gender>.Success(dto.Gender);

        foreach (var error in updateResult.Errors)
            _logger.LogWarning("Code: {code} Description: {description}", error.Code, error.Description);
        
        return ServiceResult<Gender>.Failure(updateResult.Errors.ToArray());
    }

    public async Task<ServiceResult<UpdateFullNameDto>> UpdateFullNameAsync(UpdateFullNameDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;

        var updateResult = await _userManager.UpdateAsync(user);

        if (updateResult.Succeeded)
            return ServiceResult<UpdateFullNameDto>.Success(dto);

        foreach (var error in updateResult.Errors)
            _logger.LogWarning("Code: {code} Description: {description}", error.Code, error.Description);
        
        return ServiceResult<UpdateFullNameDto>.Failure(updateResult.Errors.ToArray());

    }

    public async Task<ServiceResult<string>> UpdateEmailAsync(UpdateEmailDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.Email = dto.Email;

        var updateResult = await _userManager.UpdateAsync(user);

        if (updateResult.Succeeded)
            return ServiceResult<string>.Success(dto.Email);

        var identityErrors = updateResult.Errors.ToArray();

        foreach(var error in identityErrors)
        {
            switch (error.Code)
            {
                case "DuplicateEmail":
                    _logger.LogWarning("VALIDATION ERROR: Email is already taken");
                    return ServiceResult<string>.Failure(Error.User.EmailAlreadyExists());
                case "InvalidEmail":
                    _logger.LogWarning("VALIDATION ERROR: Email is invalid");
                    return ServiceResult<string>.Failure(Error.Validation.InvalidInput());
                default:
                    _logger.LogWarning("Error occurred: \n Code: {code} {description}", error.Code, error.Description);
                    break;
            }
        }
        return ServiceResult<string>.Failure(identityErrors);

    }

    public async Task<ServiceResult<string>> UpdateUserNameAsync(UpdateUserNameDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.UserName = dto.UserName;

        var updateResult = await _userManager.UpdateAsync(user);

        if (updateResult.Succeeded)
            return ServiceResult<string>.Success(dto.UserName);

        var identityErrors = updateResult.Errors.ToArray();

        foreach (var error in identityErrors)
        {
            switch (error.Code)
            {
                case "DuplicateUsername":
                    _logger.LogWarning("VALIDATION ERROR: Username is already taken");
                    return ServiceResult<string>.Failure(Error.User.UsernameAlreadyExists());
                case "InvalidUsername":
                    _logger.LogWarning("VALIDATION ERROR: Username is invalid");
                    return ServiceResult<string>.Failure(Error.Validation.InvalidInput());
                default:
                    _logger.LogWarning("Error occurred: \n Code: {code} {description}", error.Code, error.Description);
                    break;
            }
        }
        return ServiceResult<string>.Failure(identityErrors);

    }

    private async Task<User> GetUserForUpdateAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId), "CRITICAL ERROR: UserID is null or empty");

        var user = await _userManager.FindByIdAsync(userId);

        return user ?? throw new InvalidOperationException("CRITICAL ERROR: User is null");
    }
}