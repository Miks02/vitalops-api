namespace MixxFit.API.DTO.Weight
{
    public class WeightListDetails
    {
        public IReadOnlyList<WeightRecordDto> WeightLogs { get; set; } = [];
        public IReadOnlyList<int> Months { get; set; } = [];
    }
}
