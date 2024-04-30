using BlazorFormManager.Demo.Server.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlazorFormManager.Demo.Server.Models
{
    public class ApplicationUser : IdentityUser
    {
        //public ApplicationUser()
        //{
        //    Orders = new HashSet<Order>();
        //}

        [StringLength(100)]
        public string FirstName { get; set; }

        [StringLength(100)]
        public string LastName { get; set; }

        public byte[] Photo { get; set; }
        public int AgeRange { get; set; }

        [StringLength(30)]
        public string FavouriteColor { get; set; }

        [StringLength(20)]
        public string FavouriteWorkingDay { get; set; }

        public DateTime? BirthDay { get; set; }
        public char Gender { get; set; }
        //public ICollection<Order> Orders { get; set; }
    }
}
