using Carfamsoft.Model2View.Testing.Properties;
using Carfamsoft.Model2View.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Carfamsoft.Model2View.Testing
{
    [FormDisplayDefault(ResourceType = typeof(DisplayStrings))]
    public class RegisterBusinessModel
    {
        /*********** Authentication ***********/

        [Required]
        [Display(Name = "UserName", GroupName = "Authentication", Order = 1)]
        [FormDisplay(Icon = "fas fa fa-user")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6, ErrorMessageResourceName = "PasswordLengthError", ErrorMessageResourceType = typeof(DisplayStrings))]
        [DataType(DataType.Password)]
        [Display(Name = "Password", GroupName = "Authentication", Order = 2)]
        [FormDisplay(Icon = "fas fa fa-lock")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword", GroupName = "Authentication", Order = 3)]
        [FormDisplay(Icon = "fas fa fa-lock")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.", ErrorMessageResourceName = "ConfirmPasswordError", ErrorMessageResourceType = typeof(DisplayStrings))]
        public string ConfirmPassword { get; set; }

        /*********** Personal Info ***********/

        [Required]
        [Display(Name = "FirstName", GroupName = "PersonalInfo", Order = 4)]
        [FormDisplay(Icon = "fas fa fa-user")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "LastName", GroupName = "PersonalInfo", Order = 5)]
        [FormDisplay(Icon = "fas fa fa-user")]
        public string LastName { get; set; }

        [Display(Name = "Gender", GroupName = "PersonalInfo", Order = 6)]
        [FormDisplay(UIHint = "select", Options = "=Select|F=Female|M=Male")]
        public string Gender { get; set; }

        /*********** Contact ***********/

        [Required, StringLength(56)]
        [Display(Name = "Email", GroupName = "Contact", Order = 7)]
        [FormDisplay(Icon = "fas fa fa-envelope")]
        public string Email { get; set; }

        [StringLength(15)]
        [Display(Name = "MobilePhone", GroupName = "Contact", Order = 8)]
        [FormDisplay(Icon = "fas fa fa-mobile")]
        public string MobilePhone { get; set; }

        [StringLength(15)]
        [Display(Name = "HomePhone", GroupName = "Contact", Order = 9)]
        [FormDisplay(Icon = "fas fa fa-phone")]
        public string HomePhone { get; set; }

        /*********** Address ***********/

        [Display(Name = "CountryId", Order = 10)]
        [FormDisplay(Icon = "fas fa fa-globe-africa")]
        public int? CountryId { get; set; }

        [StringLength(50)]
        [Display(Name = "CityName", Order = 11)]
        [FormDisplay(Icon = "fas fa fa-city")]
        public string City { get; set; }

        [StringLength(100)]
        [Display(Name = "Address", Order = 12)]
        [FormDisplay(UIHint = "textarea", Icon = "fas fa fa-home")]
        public string Address { get; set; }

        [DisplayIgnore]
        public string Token { get; set; }
    }
}
