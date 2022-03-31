using Carfamsoft.Model2View.Annotations;
using Carfamsoft.Model2View.Testing.Properties;
using System.ComponentModel.DataAnnotations;

namespace Carfamsoft.Model2View.Testing
{
    [FormDisplayDefault(ShowGroupName = true, ResourceType = typeof(DisplayStrings), ColumnCssClass = "col-md-6")]
    public class ContactInfo
    {
        [Required]
        [StringLength(255)]
        [EmailAddress]
        [FormDisplay(Icon = "fas fa-envelope", UITypeHint = "email")]
        public string Email { get; set; }

        [StringLength(30)]
        [FormDisplay(Icon = "fas fa-phone", UITypeHint = "phone")]
        public string PhoneNumber { get; set; }

        [StringLength(30)]
        [FormDisplay(Icon = "fas fa-phone", UITypeHint = "phone")]
        public string HomePhone { get; set; }
        
        [StringLength(30)]
        [FormDisplay(Icon = "fas fa-phone", UITypeHint = "phone")]
        public string WorkPhone { get; set; }
        
        [FormDisplay(GroupName = "Address", GroupIcon = "fas fa-map-pin", ColumnCssClass = "col-12")]
        public LocationInfo Address { get; set; }
    }
}
