using MyCinema.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyCinema.ViewModels
{
    public class CinemaViewModel
    {
        [Required]
        [StringLength(50, ErrorMessage = "Name must be between 3-50 characters!", MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Address must be between 10-50 characters!", MinimumLength = 10)]
        public string Address { get; set; }

        public Cinema ViewModelToDBModel()
        {
            return new Cinema()
            {
                name = this.Name,
                address = this.Address
            };
        }
    }
}