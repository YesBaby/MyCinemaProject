using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyCinema.ViewModels
{
    public class ProjectionViewModel
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public int RoomID { get; set; }
        public int FreeSeats { get; set; }
    }
}