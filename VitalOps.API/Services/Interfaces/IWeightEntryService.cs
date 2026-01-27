using VitalOps.API.DTO.Weight;
using VitalOps.API.Models;
using VitalOps.API.Services.Results;

namespace VitalOps.API.Services.Interfaces
{
    public interface IWeightEntryService
    {
        Task<WeightSummaryDto?> GetUserWeightSummaryAsync(string userId, int? month, int? year, CancellationToken cancellationToken);
        Task<WeightListDetails> GetUserWeightLogsAsync(string userId, int? month, int? year,
            CancellationToken cancellationToken);
        Task<WeightEntryDetailsDto?> GetUserWeightEntryByIdAsync(string userId, int id);
        Task<Result<WeightEntryDetailsDto>> AddWeightEntryAsync(WeightCreateRequestDto request, string userId, CancellationToken cancellationToken);
        Task<Result> DeleteEntryAsync(int id, string userId, CancellationToken cancellationToken);

    }
}
