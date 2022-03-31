using Carfamsoft.Model2View.Annotations;
using Carfamsoft.Model2View.Testing.Properties;
using System.ComponentModel.DataAnnotations;

namespace Carfamsoft.Model2View.Testing
{
    [FormDisplayDefault(ShowGroupName = true, ResourceType = typeof(DisplayStrings))]
    public class UpdateUserModel
    {
        [Required]
        [StringLength(100)]
        [FormDisplay(GroupName = "PersonalInfo", GroupIcon = "fas fa-user", Icon = "fas fa-user", ColumnCssClass = "col-md-6")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        [FormDisplay(GroupName = "PersonalInfo", Icon = "fas fa-user", ColumnCssClass = "col-md-6")]
        public string LastName { get; set; }

        [FormDisplay(GroupName = "ContactDetails", GroupIcon = "fas fa-phone", ColumnCssClass = "col-12")]
        public ContactInfo Contact { get; set; }
    }
}