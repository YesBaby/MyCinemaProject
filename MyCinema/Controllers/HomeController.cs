using MyCinema.Models;
using MyCinema.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyCinema.Controllers
{
    public class HomeController : Controller
    {
        private CinemaDBConnection db = new CinemaDBConnection();

        [AllowAnonymous]
        public ActionResult Index()
        {
            var cinemas = db.Cinema
                .Select(c => new
                {
                    id = c.id,
                    cinemaInfo = c.name
                });
            ViewBag.cinemas = new SelectList(cinemas, "id", "cinemaInfo");
            ViewBag.movies = db.Movie.ToList();
            return View();
        }

        public JsonResult GetMovies(int cinemaID)
        {
            db.Configuration.ProxyCreationEnabled = false;

            Cinema currentCinema = db.Cinema.Find(cinemaID);
            List<Room> cinemaRooms = db.Room.Where(r => r.cinema_id == currentCinema.id).ToList();
            List<Projection> cinemaProjections = new List<Projection>();
            foreach (var projection in db.Projection)
            {
                foreach (var room in cinemaRooms)
                {
                    if (projection.room_id == room.id)
                    {
                        cinemaProjections.Add(projection);
                    }
                }
            }
            List<Movie> cinemaMovies = new List<Movie>();
            List<MovieViewModel> cinemaMoviesForView = new List<MovieViewModel>();
            foreach (var p in cinemaProjections)
            {
                foreach (var m in db.Movie)
                {
                    if (p.movie_id == m.id && !cinemaMovies.Contains(m))
                    {
                        MovieViewModel movie = new MovieViewModel
                        {
                            Id = m.id,
                            MovieName = m.name,
                            Duration = m.duration
                        };
                        cinemaMovies.Add(m);
                        cinemaMoviesForView.Add(movie);
                    }
                }

            }
            return Json(cinemaMoviesForView, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProjections(int cinemaID, int movieID)
        {
            db.Configuration.ProxyCreationEnabled = false;

            Cinema currentCinema = db.Cinema.Find(cinemaID);
            List<Room> currentCinemaRooms = db.Room.Where(r => r.cinema_id == cinemaID).ToList();
            Movie currentMovie = db.Movie.Find(movieID);
            List<Projection> movieProjections = new List<Projection>();
            List<ProjectionViewModel> movieProjectionsForView = new List<ProjectionViewModel>();
            foreach (var projection in db.Projection)
            {
                Room currentRoom = db.Room.Find(projection.room_id);
                int currentRoomSeatCount = 0;
                foreach (var seat in db.Seat)
                {
                    if(seat.room_id == currentRoom.id)
                    {
                        currentRoomSeatCount++;
                    }
                }
                // if the projection has the same Movie
                if (projection.movie_id == movieID)
                {
                    bool sameCinema = false;
                    foreach (var room in currentCinemaRooms)
                    {
                        //if the projection is in the same CINEMA
                        if(room.id == projection.room_id)
                        {
                            sameCinema = true;
                            break;
                        }
                    }
                    if(sameCinema)
                    {
                        movieProjections.Add(projection);
                        int takenSeats = 0;
                        foreach (var ticket in db.Ticket)
                        {
                            if (ticket.projection_id == projection.id)
                            {
                                takenSeats++;
                            }
                        }

                        ProjectionViewModel pvm = new ProjectionViewModel
                        {
                            Id = projection.id,
                            StartDate = projection.start_date,
                            RoomID = projection.room_id,
                            FreeSeats = currentRoomSeatCount - takenSeats
                        };
                        movieProjectionsForView.Add(pvm);
                    }
                }
            }
            return Json(movieProjectionsForView, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [AllowAnonymous]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}