using VitalOps.API.DTO.Weight;
using VitalOps.API.Models;
using VitalOps.API.Services.Results;

namespace VitalOps.API.Services.Interfaces
{
    public interface IWeightEntryService
    {
        Task<WeightSummaryDto?> GetUserWeightSummaryAsync(string userId, CancellationToken cancellationToken);
        Task<Result<WeightEntryDetailsDto>> AddWeightEntryAsync(WeightCreateRequestDto request, string userId, CancellationToken cancellationToken);
        Task<Result> DeleteEntryAsync(int id, string userId, CancellationToken cancellationToken);

    }
}
