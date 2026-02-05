namespace MixxFit.API.DTO.Workout
{
    public class WorkoutsPerMonthDto
    {
        public IReadOnlyList<int> Years { get; set; } = new List<int>();

        public int JanuaryWorkouts { get; set; }
        public int FebruaryWorkouts { get; set; }
        public int MarchWorkouts { get; set; }
        public int AprilWorkouts { get; set; }
        public int MayWorkouts { get; set; }
        public int JuneWorkouts { get; set; }
        public int JulyWorkouts { get; set; }
        public int AugustWorkouts { get; set; }
        public int SeptemberWorkouts { get; set; }
        public int OctoberWorkouts { get; set; }
        public int NovemberWorkouts { get; set; }
        public int DecemberWorkouts { get; set; }
    }
}
