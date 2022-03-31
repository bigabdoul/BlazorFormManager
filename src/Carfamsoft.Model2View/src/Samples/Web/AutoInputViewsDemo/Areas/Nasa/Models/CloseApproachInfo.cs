namespace AutoInputViewsDemo.Areas.Nasa.Models
{
    public class CloseApproachInfo
    {
        public string CloseApproachDate { get; set; }
        public string CloseApproachDateFull { get; set; }
        public long EpochDateCloseApproach { get; set; }
        public Velocity RelativeVelocity { get; set; }
        public Distance MissDistance { get; set; }
        public string OrbitingBody { get; set; }
    }
}