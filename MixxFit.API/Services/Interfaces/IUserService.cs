using MixxFit.API.DTO.User;
using MixxFit.API.Enums;
using MixxFit.API.Models;
using MixxFit.API.Services.Results;

namespace MixxFit.API.Services.Interfaces;

public interface IUserService
{
    Task<UserDetailsDto> GetUserDetailsAsync(string id, CancellationToken cancellationToken = default);

    Task<Result> DeleteUserAsync(User user);
    Task<Result> DeleteUserAsync(string id, CancellationToken cancellation = default);
    Task<Result> DeleteProfilePictureAsync(string userId, CancellationToken cancellationToken = default);

    Task<Result<DateTime>> UpdateDateOfBirthAsync(UpdateDateOfBirthDto dto, string userId, CancellationToken cancellationToken = default);
    Task<Result<double>> UpdateHeightAsync(UpdateHeightDto dto, string userId, CancellationToken cancellationToken = default);
    Task<Result<Gender>> UpdateGenderAsync(UpdateGenderDto dto, string userId, CancellationToken cancellationToken = default);
    Task<Result<UpdateFullNameDto>> UpdateFullNameAsync(UpdateFullNameDto dto, string userId, CancellationToken cancellationToken = default);
    Task<Result<string>> UpdateEmailAsync(UpdateEmailDto dto, string userId, CancellationToken cancellationToken = default);
    Task<Result<string>> UpdateUserNameAsync(UpdateUserNameDto dto, string userId, CancellationToken cancellationToken = default);
    Task<Result<double>> UpdateTargetWeightAsync(UpdateTargetWeightDto dto, string userId, CancellationToken cancellationToken = default);
    Task<Result<string>> UpdateProfilePictureAsync(IFormFile imageFile, string userId, CancellationToken cancellationToken);

}