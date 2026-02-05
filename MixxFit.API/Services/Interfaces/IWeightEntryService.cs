using MixxFit.API.DTO.Weight;
using MixxFit.API.Services.Results;
using MixxFit.API.Models;

namespace MixxFit.API.Services.Interfaces
{
    public interface IWeightEntryService
    {
        Task<WeightSummaryDto?> GetUserWeightSummaryAsync(string userId, int? month, int? year, double? targetWeight = null, CancellationToken cancellationToken = default);
        Task<WeightListDetails> GetUserWeightLogsAsync(string userId, int? month, int? year, CancellationToken cancellationToken);
        Task<WeightEntryDetailsDto?> GetUserWeightEntryByIdAsync(string userId, int id);
        Task<WeightChartDto> GetUserWeightChartAsync(string userId, double? targetWeight, CancellationToken cancellationToken = default);
        Task<Result<WeightEntryDetailsDto>> AddWeightEntryAsync(WeightCreateRequestDto request, string userId, CancellationToken cancellationToken);
        Task<Result> DeleteEntryAsync(int id, string userId, CancellationToken cancellationToken);

    }
}
