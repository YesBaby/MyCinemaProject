using MyCinema.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyCinema.ViewModels
{
    public class SeatModel
    {
        public Seat Seat { get; set; }

        public bool Available { get; set; }
    }
}