using Carfamsoft.Model2View.Annotations;
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

        [ImagePreview(Accept = ".jpg, .jpeg")]
        [DragDrop(Prompt = "Choose a photo or drop one here")]
        public string Photo { get; set; }
    }
}
