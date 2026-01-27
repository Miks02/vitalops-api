using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
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

        public async Task<WeightSummaryDto?> GetUserWeightSummaryAsync(
            string userId, 
            int? month = null, 
            int? year = null,
            CancellationToken cancellationToken = default)
        {
            var hasEntries = await _context.WeightEntries
                .AsNoTracking()
                .Where(w => w.UserId == userId)
                .AnyAsync(cancellationToken);

            if (!hasEntries)
                return null;

            var firstWeightEntry = await _context.WeightEntries
                .AsNoTracking()
                .Where(w => w.UserId == userId)
                .OrderBy(w => w.CreatedAt)
                .Select(w => new WeightRecordDto()
                {
                    Weight = w.Weight,
                    CreatedAt = w.CreatedAt               
                })
                .FirstOrDefaultAsync(cancellationToken);

            var lastWeightEntry = await _context.WeightEntries
                .AsNoTracking()
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CreatedAt)
                .Select(w => new WeightRecordDto()
                {
                    Weight = w.Weight,
                    CreatedAt = w.CreatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            var weightEntryYears = await GetUserWeightEntryYearsAsync(userId, cancellationToken);

            var weightListDetails = await GetUserWeightLogsAsync(userId, month, year, cancellationToken);

            var progress = lastWeightEntry!.Weight - firstWeightEntry!.Weight;

            return new WeightSummaryDto()
            {
                FirstEntry = firstWeightEntry,
                CurrentWeight = lastWeightEntry,
                Progress = progress,
                Years = weightEntryYears,
                WeightListDetails = weightListDetails
            };
        }

        public async Task<WeightListDetails> GetUserWeightLogsAsync(
            string userId,
            int? month = null,
            int? year = null,
            CancellationToken cancellationToken = default)
        {

            return new WeightListDetails()
            {
                WeightLogs = await BuildWeightEntriesQuery(userId, month, year).ToListAsync(cancellationToken),
                Months = await GetUserWeightEntryMonthsByYearAsync(userId, year ?? DateTime.UtcNow.Year,
                    cancellationToken)
            };
        }

        public async Task<WeightEntryDetailsDto?> GetUserWeightEntryByIdAsync(string userId, int id)
        {
            return await _context.WeightEntries
                .AsNoTracking()
                .Where(w => w.Id == id && w.UserId == userId)
                .Select(w => new WeightEntryDetailsDto()
                {
                    Id = w.Id,
                    Weight = w.Weight,
                    Notes = w.Notes,
                    Time = w.Time,
                    CreatedAt = w.CreatedAt
                })
                .FirstOrDefaultAsync();
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

        private IQueryable<WeightRecordDto> BuildWeightEntriesQuery(
            string userId, 
            int? month = null, 
            int? year = null)
        {
            var query = _context.WeightEntries
                .AsNoTracking()
                .OrderByDescending(w => w.CreatedAt)
                .Where(w => w.UserId == userId)
                .Select(w => new WeightRecordDto()
                {
                    Id = w.Id,
                    Weight = w.Weight,
                    TimeLogged = w.Time,
                    CreatedAt = w.CreatedAt
                });

            month ??= DateTime.UtcNow.Month;

            year ??= DateTime.UtcNow.Year;

            query = query.Where(w => w.CreatedAt.Year == year && w.CreatedAt.Month == month);
            
            return query;
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

        private async Task<IReadOnlyList<int>> GetUserWeightEntryYearsAsync(
            string userId,
            CancellationToken cancellationToken)
        {
            return await _context.WeightEntries
                .AsNoTracking()
                .Where(w => w.UserId == userId)
                .Select(w => w.CreatedAt.Year)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        private async Task<IReadOnlyList<int>> GetUserWeightEntryMonthsByYearAsync(
            string userId, 
            int year,
            CancellationToken cancellationToken)
        {
            return await _context.WeightEntries
                .Where(w => w.UserId == userId && w.CreatedAt.Year == year)
                .Select(w => w.CreatedAt.Month)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

    }
}