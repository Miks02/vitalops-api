namespace MixxFit.API.DTO.Weight
{
    public class WeightCreateRequestDto
    {
        public double Weight { get; set; }
        public TimeSpan Time { get; set; }
        public string? Notes { get; set; }
    }
}
