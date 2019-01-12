using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyCinema.ViewModels
{
    public class UserProfileViewModel
    {
        [Required]
        [StringLength(50, ErrorMessage = "Password must be between 3-50 characters!", MinimumLength = 3)]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Password must be between 3-50 characters!", MinimumLength = 3)]
        public string NewPassword { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Password must be between 3-50 characters!", MinimumLength = 3)]
        [Compare("NewPassword", ErrorMessage = "Confirm password doesn't match!")]
        public string ConfirmPassword { get; set; }
    }
}