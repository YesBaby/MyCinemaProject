using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MyCinema.Models;
using MyCinema.ViewModels;
using MyCinema.Security;

namespace MyCinema.Controllers
{
    public class CinemasController : Controller
    {
        private CinemaDBConnection db = new CinemaDBConnection();

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View(db.Cinema.ToList());
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cinema cinema = db.Cinema.Find(id);
            if (cinema == null)
            {
                return HttpNotFound();
            }
            return View(cinema);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CinemaViewModel cinema)
        {
            if (ModelState.IsValid)
            {
                db.Cinema.Add(cinema.ViewModelToDBModel());
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(cinema);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cinema cinema = db.Cinema.Find(id);
            if (cinema == null)
            {
                return HttpNotFound();
            }
            return View(cinema);
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,name,address")] Cinema cinema)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cinema).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(cinema);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cinema cinema = db.Cinema.Find(id);
            if (cinema == null)
            {
                return HttpNotFound();
            }
            return View(cinema);
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Cinema cinema = db.Cinema.Find(id);

            //delete all foreign keys first:
            var cinemaRooms = from r in db.Room
                              join c in db.Cinema on r.cinema_id equals c.id
                              where c.id == cinema.id
                              select r;
            foreach (var room in cinemaRooms)
            {
                var roomSeats = from s in db.Seat
                                join r in db.Room on s.room_id equals r.id
                                where r.id == room.id
                                select s;
                foreach (var seat in roomSeats)
                {
                    var seatTickets = from t in db.Ticket
                                      join s in db.Seat on t.seat_id equals s.id
                                      where s.id == seat.id
                                      select t;
                    foreach (var ticket in seatTickets)
                    {
                        //Remove ticket
                        db.Ticket.Remove(ticket);
                    }
                    //Remove seat
                    db.Seat.Remove(seat);
                }

                var roomProjections = from p in db.Projection
                                      join r in db.Room on p.room_id equals r.id
                                      where r.id == room.id
                                      select p;
                foreach (var projection in roomProjections)
                {
                    //Remove projection
                    db.Projection.Remove(projection);
                }
                //Remove room
                db.Room.Remove(room);
            }

            //delete the cinema itself:
            db.Cinema.Remove(cinema);
            db.SaveChanges();
            return RedirectToAction("Index");
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
