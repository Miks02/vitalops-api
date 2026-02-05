using MixxFit.API.DTO.Global;

namespace MixxFit.API.DTO.Weight
{
    public class WeightSummaryDto
    {
        public WeightRecordDto FirstEntry { get; set; } = null!;
        public CurrentWeightDto CurrentWeight { get; set; } = null!;
        public double Progress { get; set; }
        public WeightListDetails WeightListDetails { get; set; } = null!;
        public WeightChartDto WeightChart { get; set; } = null!;
        public IReadOnlyList<int> Years { get; set; } = [];

    }
}
