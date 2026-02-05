
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MixxFit.API.DTO.User;
using MixxFit.API.Enums;
using MixxFit.API.Models;
using MixxFit.API.Services.Interfaces;
using MixxFit.API.Services.Results;
using MixxFit.API.Extensions;

namespace MixxFit.API.Services.Implementations;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly IFileService _fileService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<User> userManager, 
        IFileService fileService, 
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _fileService = fileService;
        _logger = logger;
    }

    public async Task<UserDetailsDto> GetUserDetailsAsync(string id, CancellationToken cancellationToken)
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
                TargetWeight = u.TargetWeight,
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

    public async Task<Result> DeleteUserAsync(
        string id, 
        CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .Where(u => u.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if(user is null)
            return Result.Failure(Error.User.NotFound());

        if (string.IsNullOrWhiteSpace(user.ImagePath)) 
            return await DeleteUserAsync(user);
        
        var fileRemovalResult = _fileService.DeleteFile(user.ImagePath);

        if (!fileRemovalResult.IsSucceeded)
            return Result.Failure(fileRemovalResult.Errors.ToArray());

        return await DeleteUserAsync(user);
    }

    public async Task<Result> DeleteProfilePictureAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserForUpdateAsync(userId, cancellationToken);

        if (string.IsNullOrEmpty(user.ImagePath))
            return Result.Failure(Error.Resource.NotFound("Profile image"));

        var fileRemovalResult = _fileService.DeleteFile(user.ImagePath);

        if (!fileRemovalResult.IsSucceeded)
            return Result.Failure(fileRemovalResult.Errors.ToArray());

        user.ImagePath = null;

        var updateResult = await _userManager.UpdateAsync(user);

        return updateResult.HandleIdentityResult(_logger);
    }


    public async Task<Result<DateTime>> UpdateDateOfBirthAsync(
        UpdateDateOfBirthDto dto,
        string userId, 
        CancellationToken cancellationToken)
    {
        var user = await GetUserForUpdateAsync(userId, cancellationToken);

        user.DateOfBirth = dto.DateOfBirth.ToUniversalTime();

        var updateResult = await _userManager.UpdateAsync(user);

        return updateResult.HandleIdentityResult(dto.DateOfBirth, _logger);
    }

    public async Task<Result<double>> UpdateHeightAsync(
        UpdateHeightDto dto, 
        string userId, 
        CancellationToken cancellationToken)
    {
        var user = await GetUserForUpdateAsync(userId, cancellationToken);

        user.HeightCm = dto.Height;

        var updateResult = await _userManager.UpdateAsync(user);

        
        return updateResult.HandleIdentityResult(dto.Height, _logger);

    }

    public async Task<Result<Gender>> UpdateGenderAsync(
        UpdateGenderDto dto, 
        string userId, 
        CancellationToken cancellationToken)
    {
        var user = await GetUserForUpdateAsync(userId, cancellationToken);

        user.Gender = dto.Gender;

        var updateResult = await _userManager.UpdateAsync(user);

        return updateResult.HandleIdentityResult(dto.Gender, _logger);
    }

    public async Task<Result<UpdateFullNameDto>> UpdateFullNameAsync(
        UpdateFullNameDto dto, 
        string userId, 
        CancellationToken cancellationToken)
    {
        var user = await GetUserForUpdateAsync(userId, cancellationToken);

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;

        var updateResult = await _userManager.UpdateAsync(user);

        return updateResult.HandleIdentityResult(dto, _logger);
    }

    public async Task<Result<string>> UpdateEmailAsync(
        UpdateEmailDto dto, 
        string userId, 
        CancellationToken cancellationToken)
    {
        var user = await GetUserForUpdateAsync(userId, cancellationToken);

        user.Email = dto.Email;

        var updateResult = await _userManager.UpdateAsync(user);

        return updateResult.HandleIdentityResult(dto.Email, _logger);
    }

    public async Task<Result<string>> UpdateUserNameAsync(
        UpdateUserNameDto dto, 
        string userId, 
        CancellationToken cancellationToken)
    {
        var user = await GetUserForUpdateAsync(userId, cancellationToken);

        user.UserName = dto.UserName;

        var updateResult = await _userManager.UpdateAsync(user);

        return updateResult.HandleIdentityResult(dto.UserName, _logger);

    }

    public async Task<Result<double>> UpdateTargetWeightAsync(
        UpdateTargetWeightDto dto,
        string userId,
        CancellationToken cancellationToken)
    {
        var user = await GetUserForUpdateAsync(userId, cancellationToken);

        user.TargetWeight = dto.TargetWeight;

        var updateResult = await _userManager.UpdateAsync(user);

        return updateResult.HandleIdentityResult(dto.TargetWeight, _logger);
    }

    public async Task<Result<string>> UpdateProfilePictureAsync(
        IFormFile imageFile,
        string userId,
        CancellationToken cancellationToken)
    {
        var user = await GetUserForUpdateAsync(userId, cancellationToken); 

        var fileUploadResult = await _fileService.UploadFile(imageFile, user.ImagePath, "user_avatars");

        if (!fileUploadResult.IsSucceeded)
        {
            _logger.LogInformation("Uploading the file has failed");
            return Result<string>.Failure(fileUploadResult.Errors.ToArray());
        }

        user.ImagePath = fileUploadResult.Payload;
        var updateResult = await _userManager.UpdateAsync(user);

        return updateResult.HandleIdentityResult(fileUploadResult.Payload!, _logger);
    }

    private async Task<User> GetUserForUpdateAsync(string userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId), "CRITICAL ERROR: UserID is null or empty");

        var user = await _userManager.Users
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync(cancellationToken);

        return user ?? throw new InvalidOperationException("CRITICAL ERROR: User is null");
    }

    
}