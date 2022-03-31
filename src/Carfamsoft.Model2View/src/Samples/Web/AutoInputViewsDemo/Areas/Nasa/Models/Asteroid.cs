namespace AutoInputViewsDemo.Areas.Nasa.Models
{
    public class Asteroid
    {
        public LinkInfo Links { get; set; }
        public string Id { get; set; }
        public string NeoReferenceId { get; set; }
        public string Name { get; set; }
        public string Designation { get; set; }
        public string NasaJplUrl { get; set; }
        public double AbsoluteMagnitudeH { get; set; }
        public DiameterInfo EstimatedDiameter { get; set; }
        public bool IsPotentiallyHazardousAsteroid { get; set; }
        public System.Collections.Generic.IList<CloseApproachInfo> CloseApproachData { get; set; }
        public Orbit OrbitalData { get; set; }
        public bool IsSentryObject { get; set; }
    }
}