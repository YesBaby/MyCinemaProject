using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MyCinema.Models;
using MyCinema.Security;

namespace MyCinema.Controllers
{
    public class RoomsController : Controller
    {
        private CinemaDBConnection db = new CinemaDBConnection();

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var room = db.Room.Include(r => r.Cinema);
            return View(room.ToList());
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Room room = db.Room.Find(id);
            if (room == null)
            {
                return HttpNotFound();
            }

            return View(room);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.cinema_id = new SelectList(db.Cinema, "id", "name");
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,cinema_id")] Room room)
        {
            if (ModelState.IsValid)
            {
                db.Room.Add(room);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.cinema_id = new SelectList(db.Cinema, "id", "name", room.cinema_id);
            return View(room);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Room room = db.Room.Find(id);
            if (room == null)
            {
                return HttpNotFound();
            }
            ViewBag.cinema_id = new SelectList(db.Cinema, "id", "name", room.cinema_id);
            return View(room);
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,cinema_id")] Room room)
        {
            if (ModelState.IsValid)
            {
                db.Entry(room).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.cinema_id = new SelectList(db.Cinema, "id", "name", room.cinema_id);
            return View(room);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Room room = db.Room.Find(id);
            if (room == null)
            {
                return HttpNotFound();
            }
            return View(room);
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Room room = db.Room.Find(id);
            //Remove all the foreign keys
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
            //Remove the actual room
            db.Room.Remove(room);
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
