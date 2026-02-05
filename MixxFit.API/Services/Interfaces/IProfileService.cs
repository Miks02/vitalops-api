using MixxFit.API.DTO.User;

namespace MixxFit.API.Services.Interfaces
{
    public interface IProfileService
    {
        Task<ProfilePageDto> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default);
    }
}
