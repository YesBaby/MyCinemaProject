using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MyCinema.Models;
using MyCinema.Security;
using MyCinema.ViewModels;

namespace MyCinema.Controllers
{
    public class TicketsController : Controller
    {
        private CinemaDBConnection db = new CinemaDBConnection();

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var ticket = db.Ticket.Include(t => t.Projection).Include(t => t.Seat).Include(t => t.User);
            return View(ticket.ToList());
        }

        [CustomAuthorize(Roles = "Admin,User")]
        public ActionResult BookTicket(int projection_id)
        {
            Projection currentProjection = db.Projection.Find(projection_id);
            ViewBag.projection = currentProjection;

            List<SeatModel> seats = new List<SeatModel>();
            foreach (var seat in currentProjection.Room.Seat)
            {
                bool found = false;
                foreach (var ticket in db.Ticket)
                {
                    if (ticket.projection_id == currentProjection.id)
                    {
                        if (ticket.seat_id == seat.id)
                        {
                            found = true;
                            break;
                        }
                    }
                }
                if (found)
                {
                    seats.Add(new SeatModel { Seat = seat, Available = false });
                }
                else
                {
                    seats.Add(new SeatModel { Seat = seat, Available = true });
                }
            }
            List<SeatModel> ordered = seats.OrderBy(seat => seat.Seat.row).ThenBy(seat => seat.Seat.col).ToList();

            ViewBag.seats = ordered;

            return View();
        }

        [CustomAuthorize(Roles="Admin,User")]
        public ActionResult BookingCompleted(int pro_id, string seats)
        {
            var seatIDs = seats.Split(',');
            foreach (var sid in seatIDs)
            {
                db.Ticket.Add(new Ticket
                {
                    user_id = db.User.Where(u => u.name.Equals(SessionPersister.Username)).First().id,
                    projection_id = pro_id,
                    seat_id = Convert.ToInt32(sid),
                    price = 10
                });
            }
            db.SaveChanges();
            return RedirectToAction("BookTicket", new { projection_id = pro_id});
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Ticket.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Create()
        {
            var projections = db.Projection
                .Select(x => new
                {
                    id = x.id,
                    movieInfo = x.Movie.name + " (" + x.start_date + ")" + " - " + x.Room.Cinema.name
                });
            ViewBag.projection_id = new SelectList(projections, "id", "movieInfo");
            var seats = db.Seat
                .Select(x => new
                {
                    id = x.id,
                    seatInfo = "Room: " + x.room_id + " (Row: " + x.row + ", Seat: " + x.col + ")"
                });
            ViewBag.seat_id = new SelectList(seats, "id", "seatInfo");
            ViewBag.user_id = new SelectList(db.User, "id", "name");
            return View();
        }

        public JsonResult GetSeats(int projectionID)
        {
            db.Configuration.ProxyCreationEnabled = false;

            Projection currentProjection = db.Projection.Find(projectionID);
            List<Ticket> listOfTickets = db.Ticket.Where(t => t.projection_id == projectionID).ToList();
            List<Seat> roomSeats = db.Seat.Where(s => s.room_id == currentProjection.room_id).ToList();
            List<Seat> ordered = new List<Seat>();
            foreach (var seat in roomSeats)
            {
                bool found = false;
                foreach (var ticket in listOfTickets)
                {
                    if (ticket.seat_id == seat.id)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    ordered.Add(seat);
                }
            }
            //ordered roomSeats
            ordered = ordered.OrderBy(seat => seat.row).ThenBy(seat => seat.col).ToList();
            return Json(ordered, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,user_id,projection_id,seat_id,price")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                db.Ticket.Add(ticket);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.projection_id = new SelectList(db.Projection, "id", "id", ticket.projection_id);
            ViewBag.seat_id = new SelectList(db.Seat, "id", "id", ticket.seat_id);
            ViewBag.user_id = new SelectList(db.User, "id", "name", ticket.user_id);
            return View(ticket);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Ticket.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            // Get seats method RIPOFF
            Projection currentProjection = db.Projection.Find(ticket.projection_id);
            List<Ticket> listOfTickets = db.Ticket.Where(t => t.projection_id == ticket.projection_id).ToList();
            List<Seat> roomSeats = db.Seat.Where(s => s.room_id == currentProjection.room_id).ToList();
            List<Seat> ordered = new List<Seat>();
            foreach (var seat in roomSeats)
            {
                bool found = false;
                foreach (var t in listOfTickets)
                {
                    if (t.seat_id == seat.id)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    ordered.Add(seat);
                }
            }
            ordered.Add(db.Seat.Find(ticket.seat_id));
            //ordered roomSeats
            ordered = ordered.OrderBy(seat => seat.row).ThenBy(seat => seat.col).ToList();
            // End Get seats method RIPOFF
            var projections = db.Projection
               .Select(x => new
               {
                   id = x.id,
                   movieInfo = x.Movie.name + " (" + x.start_date + ")" + " - " + x.Room.Cinema.name
               });
            ViewBag.projection_id = new SelectList(projections, "id", "movieInfo", ticket.projection_id);
            var seats = ordered
                .Select(s => new
                {
                    id = s.id,
                    seatInfo = "Room: " + s.room_id + " (Row: " + s.row + ", Seat: " + s.col + ")"
                });
            ViewBag.seat_id = new SelectList(seats, "id", "seatInfo", ticket.seat_id);
            ViewBag.user_id = new SelectList(db.User, "id", "name", ticket.user_id);
            return View(ticket);
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,user_id,projection_id,seat_id,price")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            // Get seats method RIPOFF
            Projection currentProjection = db.Projection.Find(ticket.projection_id);
            List<Ticket> listOfTickets = db.Ticket.Where(t => t.projection_id == ticket.projection_id).ToList();
            List<Seat> roomSeats = db.Seat.Where(s => s.room_id == currentProjection.room_id).ToList();
            List<Seat> ordered = new List<Seat>();
            foreach (var seat in roomSeats)
            {
                bool found = false;
                foreach (var t in listOfTickets)
                {
                    if (t.seat_id == seat.id)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    ordered.Add(seat);
                }
            }
            ordered.Add(db.Seat.Find(ticket.seat_id));
            //ordered roomSeats
            if (ordered.Any() && ordered.Count != 1)
            {
                ordered = ordered.OrderBy(seat => seat.row).ThenBy(seat => seat.col).ToList();
            }
            // End Get seats method RIPOFF
            var projections = db.Projection
               .Select(x => new
               {
                   id = x.id,
                   movieInfo = x.Movie.name + " (" + x.start_date + ")" + " - " + x.Room.Cinema.name
               });
            ViewBag.projection_id = new SelectList(projections, "id", "movieInfo", ticket.projection_id);
            var seats = ordered
                .Select(s => new
                {
                    id = s.id,
                    seatInfo = "Room: " + s.room_id + " (Row: " + s.row + ", Seat: " + s.col + ")"
                });
            ViewBag.seat_id = new SelectList(seats, "id", "seatInfo", ticket.seat_id);
            ViewBag.user_id = new SelectList(db.User, "id", "name", ticket.user_id);
            return View(ticket);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Ticket.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket ticket = db.Ticket.Find(id);
            db.Ticket.Remove(ticket);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [CustomAuthorize(Roles = "Admin,User")]
        public ActionResult DeleteTicketForUser(int id)
        {
            Ticket ticket = db.Ticket.Find(id);
            db.Ticket.Remove(ticket);
            db.SaveChanges();
            return RedirectToAction("UserPanel", "Users");
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
