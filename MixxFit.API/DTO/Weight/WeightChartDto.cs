namespace MixxFit.API.DTO.Weight
{
    public class WeightChartDto
    {
        public IReadOnlyList<WeightRecordDto> Entries { get; set; } = [];
        public double? TargetWeight { get; set; }
    }
}
