using BlazorFormManager.ComponentModel.ViewAnnotations;
using System.ComponentModel.DataAnnotations;

namespace BlazorFormManager.Demo.Client.Models
{
    public class RegisterUserModel : UpdateUserModel
    {
        [Required]
        [DataType(DataType.Password)]
        [FormDisplay(GroupName = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
        [FormDisplay(GroupName = "Password")]
        public string ConfirmPassword { get; set; }
    }
}
