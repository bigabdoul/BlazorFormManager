namespace AutoInputViewsDemo.Areas.Nasa.Models
{
    public class Orbit
    {
        public string OrbitId { get; set; }
        public string OrbitDeterminationDate { get; set; }
        public string FirstObservationDate { get; set; }
        public string LastObservationDate { get; set; }
        public int DataArcInDays { get; set; }
        public int ObservationsUsed { get; set; }
        public string OrbitUncertainty { get; set; }
        public string MinimumOrbitIntersection { get; set; }
        public string JupiterTisserandInvariant { get; set; }
        public string EpochOsculation { get; set; }
        public string Eccentricity { get; set; }
        public string SemiMajorAxis { get; set; }
        public string Inclination { get; set; }
        public string AscendingNodeLongitude { get; set; }
        public string OrbitalPeriod { get; set; }
        public string PerihelionDistance { get; set; }
        public string PerihelionArgument { get; set; }
        public string AphelionDistance { get; set; }
        public string PerihelionTime { get; set; }
        public string MeanAnomaly { get; set; }
        public string MeanMotion { get; set; }
        public string Equinox { get; set; }
        public OrbitClass OrbitClass { get; set; }
    }
}