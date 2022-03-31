using Carfamsoft.Model2View.Annotations;
using System.ComponentModel.DataAnnotations;

namespace BlazorFormManager.Demo.Client.Models
{
    [FormDisplayDefault(ShowGroupName = true)]
    public class UpdateUserModel
    {
        [Required]
        [StringLength(100)]
        [FormDisplay(GroupName = "Personal info", Icon = "fas fa-user")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        [FormDisplay(GroupName = "Personal info", Name = "Last / Family Name", Prompt = "Last Name", Icon = "fas fa-user")]
        public string LastName { get; set; }

        [Required]
        [StringLength(255)]
        [EmailAddress]
        [FormDisplay(GroupName = "Contact details", Icon = "fas fa-envelope", UITypeHint = "email")]
        public string Email { get; set; }

        [StringLength(30)]
        [FormDisplay(GroupName = "Contact details", Icon = "fas fa-phone", UITypeHint = "phone")]
        public string PhoneNumber { get; set; }
    }
}
