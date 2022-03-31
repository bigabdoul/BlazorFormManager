using Carfamsoft.Model2View.Annotations;
using Carfamsoft.Model2View.Testing.Properties;

namespace Carfamsoft.Model2View.Testing
{
    [FormDisplayDefault(ShowGroupName = true, ResourceType = typeof(DisplayStrings), ColumnCssClass = "col-md-6")]
    public class LocationInfo
    {
        [FormDisplay(Icon = "fas fa-home")]
        public string FullAddress { get; set; }

        [FormDisplay(Icon = "fas fa-home")]
        public string CustomAddress { get; set; }

        [FormDisplay(Icon = "fas fa-map-pin")]
        public double Latitude { get; set; }

        [FormDisplay(Icon = "fas fa-map-pin")]
        public double Longitude { get; set; }
    }
}
