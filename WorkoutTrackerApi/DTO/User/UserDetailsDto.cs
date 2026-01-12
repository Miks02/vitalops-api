using WorkoutTrackerApi.Enums;

namespace WorkoutTrackerApi.DTO.User
{
    public class UserDetailsDto
    {
        public string FullName { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? ImagePath { get; set; }

        public Gender? Gender { get; set; }
        public double? Weight { get; set; }
        public double? Height { get; set; }

        public AccountStatus AccountStatus { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public DateTime RegisteredAt { get; set; }

        public int? Age => DateOfBirth.HasValue
            ? DateTime.UtcNow.Year - DateOfBirth.Value.Year - (DateTime.UtcNow.DayOfYear < DateOfBirth.Value.DayOfYear ? 1 : 0)
            : null;

    }
}
