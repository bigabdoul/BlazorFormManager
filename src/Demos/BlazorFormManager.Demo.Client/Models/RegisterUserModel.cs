using BlazorFormManager.ComponentModel.ViewAnnotations;
using System.ComponentModel.DataAnnotations;

namespace BlazorFormManager.Demo.Client.Models
{
    [FormDisplayDefault(ShowGroupName = false)]
    public class RegisterUserModel : UpdateUserModel
    {
        [Required]
        [FormDisplay(GroupName = "Password", UITypeHint = "password", Order = 1)]
        public string Password { get; set; }

        [Required]
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
        [FormDisplay(GroupName = "Password", UITypeHint = "password", Order = 2)]
        public string ConfirmPassword { get; set; }

        [FormDisplay(UITypeHint = "file", Name = "", InputCssClass = "", Order = 3)]
        public string Photo { get; set; }
    }
}
