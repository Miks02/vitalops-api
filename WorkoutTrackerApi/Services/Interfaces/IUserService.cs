using WorkoutTrackerApi.DTO.User;
using WorkoutTrackerApi.Enums;
using WorkoutTrackerApi.Models;
using WorkoutTrackerApi.Services.Results;

namespace WorkoutTrackerApi.Services.Interfaces;

public interface IUserService
{
    Task<UserDetailsDto> GetUserDetailsAsync(string id, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteUserAsync(User user);
    Task<ServiceResult> DeleteUserAsync(string id, CancellationToken cancellation = default);

    Task<ServiceResult<DateTime>> UpdateDateOfBirthAsync(UpdateDateOfBirthDto dto, string userId, CancellationToken cancellationToken = default);
    Task<ServiceResult<double>> UpdateWeightAsync(UpdateWeightDto dto, string userId, CancellationToken cancellationToken = default);
    Task<ServiceResult<double>> UpdateHeightAsync(UpdateHeightDto dto, string userId, CancellationToken cancellationToken = default);
    Task<ServiceResult<Gender>> UpdateGenderAsync(UpdateGenderDto dto, string userId, CancellationToken cancellationToken = default);
    Task<ServiceResult<UpdateFullNameDto>> UpdateFullNameAsync(UpdateFullNameDto dto, string userId, CancellationToken cancellationToken = default);
    Task<ServiceResult<string>> UpdateEmailAsync(UpdateEmailDto dto, string userId, CancellationToken cancellationToken = default);
    Task<ServiceResult<string>> UpdateUserNameAsync(UpdateUserNameDto dto, string userId, CancellationToken cancellationToken = default);
}