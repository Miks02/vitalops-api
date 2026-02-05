using MixxFit.API.DTO.User;

namespace MixxFit.API.Services.Interfaces
{
    public interface IProfileService
    {
        Task<ProfilePageDto> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
    }
}
