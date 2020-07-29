using System.ComponentModel.DataAnnotations;

namespace BlazorFormManager.Demo.Client.Models
{
    public class UpdateUserModel
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(30)]
        public string PhoneNumber { get; set; }
    }
}
