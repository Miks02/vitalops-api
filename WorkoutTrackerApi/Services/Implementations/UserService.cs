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
    
    public async Task<User?> GetUserByIdAsync(string id)
    {
        return await _userManager.FindByIdAsync(id);
    }

    public async Task<User?> GetUserByUserNameAsync(string username)
    {
        return await _userManager.FindByNameAsync(username);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
    {
        return await _userManager.Users.Where(u => u.RefreshToken == refreshToken).FirstOrDefaultAsync();
    }

    public async Task<IList<string>> GetUserRolesAsync(User user)
    {
        return await _userManager.GetRolesAsync(user);
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

    public async Task<ServiceResult> DeleteUserAsync(string id)
    {
        var user = await GetUserByIdAsync(id);
        
        if(user is null)
            return ServiceResult.Failure(Error.User.NotFound());

        return await DeleteUserAsync(user);
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
            }).FirstOrDefaultAsync(cancellationToken);

        if(user is null)
            throw new InvalidOperationException("User not found");

        return user;
    }

    public async Task<ServiceResult<DateTime>> UpdateDateOfBirthAsync(UpdateDateOfBirthDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.DateOfBirth = dto.DateOfBirth;

        var updateResult = await _userManager.UpdateAsync(user);

        if(!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                _logger.LogWarning("Code: {code} Description: {description}", error.Code, error.Description);
            }
            return ServiceResult<DateTime>.Failure(updateResult.Errors.ToArray());
        }

        return ServiceResult<DateTime>.Success(dto.DateOfBirth);
    }

    public async Task<ServiceResult<double>> UpdateWeightAsync(UpdateWeightDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.WeightKg = dto.Weight;

        var updateResult = await _userManager.UpdateAsync(user);

        if(!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                _logger.LogWarning("Code: {code} Description: {description}", error.Code, error.Description);
            }
            return ServiceResult<double>.Failure(updateResult.Errors.ToArray());
        }

        return ServiceResult<double>.Success(dto.Weight);
    }

    public async Task<ServiceResult<double>> UpdateHeightAsync(UpdateHeightDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.HeightCm = dto.Height;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                _logger.LogWarning("Code: {code} Description: {description}", error.Code, error.Description);
            }
            return ServiceResult<double>.Failure(updateResult.Errors.ToArray());
        }

        return ServiceResult<double>.Success(dto.Height);
    }

    public async Task<ServiceResult<Gender>> UpdateGenderAsync(UpdateGenderDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.Gender = dto.Gender;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                _logger.LogWarning("Code: {code} Description: {description}", error.Code, error.Description);
            }
            return ServiceResult<Gender>.Failure(updateResult.Errors.ToArray());
        }

           return ServiceResult<Gender>.Success(dto.Gender);
    }

    public async Task<ServiceResult<UpdateFullNameDto>> UpdateFullNameAsync(UpdateFullNameDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                _logger.LogWarning("Code: {code} Description: {description}", error.Code, error.Description);
            }
            return ServiceResult<UpdateFullNameDto>.Failure(updateResult.Errors.ToArray());
        }

        return ServiceResult<UpdateFullNameDto>.Success(dto);
    }

    public async Task<ServiceResult<string>> UpdateEmailAsync(UpdateEmailDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.Email = dto.Email;

        var updateResult = await _userManager.UpdateAsync(user);

        if(!updateResult.Succeeded)
        {
            var identityErrors = updateResult.Errors.ToArray();

            foreach(var error in identityErrors)
            {
                if(error.Code == "DuplicateEmail")
                {
                    _logger.LogWarning("VALIDATION ERROR: Email is already taken");
                    return ServiceResult<string>.Failure(Error.User.EmailAlreadyExists());
                }
                if(error.Code == "InvalidEmail")
                {
                    _logger.LogWarning("VALIDATION ERROR: Email is invalid");
                    return ServiceResult<string>.Failure(Error.Validation.InvalidInput());
                }

                _logger.LogWarning($"Error occurred: \n Code: {error.Code} {error.Description}");
            }
            return ServiceResult<string>.Failure(identityErrors);
        }

        return ServiceResult<string>.Success(dto.Email);
    }

    public async Task<ServiceResult<string>> UpdateUserNameAsync(UpdateUserNameDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId);

        user.UserName = dto.UserName;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            var identityErrors = updateResult.Errors.ToArray();

            foreach (var error in identityErrors)
            {
                if (error.Code == "DuplicateUsername")
                {
                    _logger.LogWarning("VALIDATION ERROR: Username is already taken");
                    return ServiceResult<string>.Failure(Error.User.UsernameAlreadyExists());
                }
                if (error.Code == "InvalidUsername")
                {
                    _logger.LogWarning("VALIDATION ERROR: Username is invalid");
                    return ServiceResult<string>.Failure(Error.Validation.InvalidInput());
                }

                _logger.LogWarning($"Error occurred: \n Code: {error.Code} {error.Description}");
            }
            return ServiceResult<string>.Failure(identityErrors);
        }

        return ServiceResult<string>.Success(dto.UserName);
    }

    private async Task<User> GetUserForUpdateAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId), "CRITICAL ERROR: UserID is null or empty");

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            throw new InvalidOperationException("CRITICAL ERROR: User is null");

        return user;
    }
}