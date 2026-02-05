namespace MixxFit.API.Extensions
{
    public static class TimeSpanExtensions
    {
        public static int? ToIntegerFromNullableSeconds(this TimeSpan? timeSpan)
        {
            if (timeSpan is null)
                return null;

            return timeSpan.Value.Seconds;

        }

        public static int? ToIntegerFromNullableMinutes(this TimeSpan? timeSpan)
        {
            if (timeSpan is null)
                return null;
            return (int)timeSpan.Value.TotalMinutes;
        }
        
    }
}
