using Microsoft.EntityFrameworkCore;
using VitalOps.API.Data;
using VitalOps.API.DTO.Weight;
using VitalOps.API.Models;
using VitalOps.API.Services.Interfaces;
using VitalOps.API.Services.Results;

namespace VitalOps.API.Services.Implementations
{
    public class WeightEntryService : IWeightEntryService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<WeightEntryService> _logger;

        public WeightEntryService(
            AppDbContext context, 
            ILogger<WeightEntryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<WeightSummaryDto?> GetUserWeightSummaryAsync(string userId, CancellationToken cancellationToken)
        {
            var entries = await _context.WeightEntries
                .AsNoTracking()
                .OrderByDescending(w => w.CreatedAt)
                .Where(w => w.UserId == userId)
                .Select(w => new WeightEntryDetailsDto()
                {
                    Id = w.Id,
                    Weight = w.Weight,
                    Time = w.Time,
                    Notes = w.Notes
                })
                .ToListAsync(cancellationToken);

            if (entries.Count == 0)
                return null;

            var firstWeightEntry = await _context.WeightEntries
                .AsNoTracking()
                .OrderBy(w => w.CreatedAt)
                .Where(w => w.UserId == userId)
                .Select(w => w.Weight)
                .FirstOrDefaultAsync(cancellationToken);

            var lastWeightEntry = await _context.WeightEntries
                .AsNoTracking()
                .OrderByDescending(w => w.CreatedAt)
                .Where(w => w.UserId == userId)
                .Select(w => w.Weight)
                .FirstOrDefaultAsync(cancellationToken);

            var progress = lastWeightEntry - firstWeightEntry;

            return new WeightSummaryDto()
            {
                CurrentWeight = lastWeightEntry,
                Progress = progress,
                WeightEntries = entries
            };
        }

        public async Task<Result<WeightEntryDetailsDto>> AddWeightEntryAsync(
            WeightCreateRequestDto request,
            string userId,
            CancellationToken cancellationToken)
        {
            var weightEntriesToday = await _context.WeightEntries
                .AsNoTracking()
                .Where(w => w.UserId == userId && w.CreatedAt.Date == DateTime.UtcNow.Date)
                .Select(w => w.Id)
                .CountAsync(cancellationToken);

            if (weightEntriesToday == 1)
                return Result<WeightEntryDetailsDto>.Failure(Error.General.LimitReached());

            var newEntry = new WeightEntry()
            {
                Weight = request.Weight,
                Time = request.Time,
                UserId = userId,
                Notes = request.Notes
            };

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await _context.WeightEntries.AddAsync(newEntry, cancellationToken);

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == newEntry.UserId, cancellationToken);

                if (user is null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<WeightEntryDetailsDto>.Failure(Error.User.NotFound(userId));
                }

                user.CurrentWeight = newEntry.Weight;

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction failed for User {UserId}. Rolling back.", newEntry.UserId);
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

            _logger.LogInformation("Weight logged successfully");

            var createdWeightEntry = new WeightEntryDetailsDto()
            {
                Id = newEntry.Id,
                Weight = newEntry.Weight,
                Time = newEntry.Time,
                Notes = newEntry.Notes,
                CreatedAt = newEntry.CreatedAt
            };

            return Result<WeightEntryDetailsDto>.Success(createdWeightEntry);
        }

        public async Task<Result> DeleteEntryAsync(int id, string userId, CancellationToken cancellationToken)
        {
            var entry = await _context.WeightEntries
                .Where(w => w.Id == id && w.UserId == userId)
                .FirstOrDefaultAsync(cancellationToken);

            if (entry is null)
                return Result.Failure(Error.Resource.NotFound("Weight entry"));


            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                _context.WeightEntries.Remove(entry);
                await _context.SaveChangesAsync(cancellationToken);

                var user = await _context.Users
                    .Where(u => u.Id == userId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (user is null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result.Failure(Error.Resource.NotFound(userId));
                }

                user.CurrentWeight = await GetLastWeightFromUser(user.Id, cancellationToken);

                _logger.LogInformation("Current weight: {weight}", user.CurrentWeight);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("Transaction failed for user {id} with exception: {ex}. Rolling back changes...", userId, ex);
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

            return Result.Success();
        }

        private async Task<double?> GetLastWeightFromUser(string userId, CancellationToken cancellationToken)
        {
            var lastWeight = await _context.WeightEntries
                .OrderByDescending(w => w.CreatedAt)
                .Where(w => w.UserId == userId)
                .Select(w => w.Weight)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastWeight is 0)
                return null;

            return lastWeight;
        }

    }
}