using Carfamsoft.Model2View.Annotations;
using System;
using System.ComponentModel.DataAnnotations;

namespace Carfamsoft.Model2View.Testing
{
    public class AutoUpdateUserModel : UpdateUserModel
    {
        [DisplayIgnore]
        public string Id { get; set; }

        [Range(typeof(DayOfWeek), "Monday", "Friday")]
        [FormDisplay(GroupName = "PleaseSelect", Tag = "select", Name = "", Order = 1, Prompt = nameof(FavouriteWorkingDay), Icon = "fas fa-calendar")]
        public string FavouriteWorkingDay { get; set; }

        [FormDisplay(GroupName = "PleaseSelect", UIHint = "select", Name = "", Order = 2)]
        public int AgeRange { get; set; }

        //[Range(typeof(ConsoleColor), "Black", "White")]
        [FormDisplay(Type = "radio", Order = 3, Options = "Black|Blue|White")]
        public string FavouriteColor { get; set; }

        [FormDisplay(Order = 4, Description = "LoginWithEmailAndSms")]
        public bool TwoFactorEnabled { get; set; }
    }
}
