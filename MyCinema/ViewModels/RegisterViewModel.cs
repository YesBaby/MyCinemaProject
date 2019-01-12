using MyCinema.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyCinema.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(50, ErrorMessage = "Name must be between 3-50 characters!", MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Password must be between 3-50 characters!", MinimumLength = 3)]
        public string Password { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Password must be between 3-50 characters!", MinimumLength = 3)]
        [Compare("Password", ErrorMessage ="Confirm password doesn't match!")]
        public string ConfirmPassword { get; set; }

        public User ViewModelToDBModel()
        {
            return new User()
            {
                name = this.Username,
                password = this.Password
            };
        }

        public User ViewModelToDBModelWithAdmin(bool adminValue)
        {
            return new User()
            {
                name = this.Username,
                password = this.Password,
                is_admin = adminValue
            };
        }
    }
}