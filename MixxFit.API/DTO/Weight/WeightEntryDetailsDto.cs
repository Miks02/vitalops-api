namespace MixxFit.API.DTO.Weight
{
    public class WeightEntryDetailsDto
    {
        public int Id { get; set; }
        public double Weight { get; set; }
        public TimeSpan Time { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Notes { get; set; }
    }
}
