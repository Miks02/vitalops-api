namespace VitalOps.API.Helpers
{
    public static class Utility
    {
        public static TimeSpan? ValidateMinutesAndSeconds(int? minutes, int? seconds)
        {
            if (minutes is null || seconds is null)
                return null;

            TimeSpan fromMinutes = TimeSpan.FromMinutes((double)minutes);
            TimeSpan fromSeconds = TimeSpan.FromSeconds((double)seconds);

            return fromMinutes + fromSeconds;
        }
    }
}
