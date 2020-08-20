using BlazorFormManager.ComponentModel.ViewAnnotations;
using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorFormManager.Demo.Client.Models
{
    public class AutoUpdateUserModel : UpdateUserModel
    {
        [DisplayIgnore]
        public string Id { get; set; }

        [FormDisplay(GroupName = "Please select", UIHint = "select", Name = "", Order = 2)]
        public int AgeRange { get; set; }

        [Range(typeof(DayOfWeek), "Monday", "Friday")]
        [FormDisplay(GroupName = "Please select", UIHint = "select", Name = "", Order = 1, Prompt = "[Favourite Working Day]", Icon = "fas fa-calendar")]
        public string FavouriteWorkingDay { get; set; }

        [FormDisplay(UITypeHint = "radio", Order = 3, Name = "What's your favourite color?")]
        public string FavouriteColor { get; set; }

        [FormDisplay(Order = 4, Name = "Enable two-factor authentication", Description = "Log in with your email and an SMS confirmation")]
        public bool TwoFactorEnabled { get; set; }
    }
}
