using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyCinema.ViewModels
{
    public class MovieViewModel
    {
        public int Id { get; set; }
        public string MovieName {get; set;}
        public int Duration { get; set; }
    }
}