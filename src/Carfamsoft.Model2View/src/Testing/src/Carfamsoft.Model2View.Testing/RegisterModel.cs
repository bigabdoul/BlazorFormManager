using Carfamsoft.Model2View.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Carfamsoft.Model2View.Testing
{
    public class RegisterModel
    {
        /*********** Authentication ***********/

        public string UserName { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        /*********** Personal Info ***********/
        [Display(Order = 1)]
        public string FirstName { get; set; }

        [Display(Order = 2)]
        public string LastName { get; set; }

        [Display(Order = 3)]
        public string Gender { get; set; }

        /*********** Contact ***********/

        public string Email { get; set; }

        public string MobilePhone { get; set; }

        public string HomePhone { get; set; }

        /*********** Address ***********/

        public int? CountryId { get; set; }

        public string City { get; set; }

        public string Address { get; set; }

        [DisplayIgnore]
        public string Token { get; set; }
    }
}
