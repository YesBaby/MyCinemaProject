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
    public class ProjectionsController : Controller
    {
        private CinemaDBConnection db = new CinemaDBConnection();

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var projection = db.Projection.Include(p => p.Movie).Include(p => p.Room);
            return View(projection.ToList());
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Projection projection = db.Projection.Find(id);
            if (projection == null)
            {
                return HttpNotFound();
            }
            return View(projection);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.movie_id = new SelectList(db.Movie, "id", "name");
            var rooms = db.Room
                .Select(r => new
                {
                    id = r.id,
                    roomInfo = r.Cinema.name + " - (Room: " + r.id + ")"
                });
            ViewBag.room_id = new SelectList(rooms, "id", "roomInfo");
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,start_date,movie_id,room_id,available_seats")] Projection projection)
        {
            List<Projection> projectionsList = db.Projection.ToList();

            if (ModelState.IsValid)
            {
                bool exists = false;
                foreach (var p in projectionsList)
                {
                    if (projection.room_id == p.room_id)
                    {
                        TimeSpan difference = p.start_date > projection.start_date ?
                        p.start_date - projection.start_date
                        : projection.start_date - p.start_date;

                        if ((int)difference.TotalMinutes < db.Movie.Find(projection.movie_id).duration + 15) // projection time + 15 mins to clean the room
                        {
                            exists = true;
                            break;
                        }
                    }
                }
                if (!exists)
                {
                    db.Projection.Add(projection);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            ViewBag.Error = "At this time the room is not available!";
            ViewBag.movie_id = new SelectList(db.Movie, "id", "name", projection.movie_id);
            ViewBag.room_id = new SelectList(db.Room, "id", "id", projection.room_id);
            return View(projection);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Projection projection = db.Projection.Find(id);
            if (projection == null)
            {
                return HttpNotFound();
            }
            ViewBag.movie_id = new SelectList(db.Movie, "id", "name", projection.movie_id);
            var rooms = db.Room
                .Select(r => new
                {
                    id = r.id,
                    roomInfo = r.Cinema.name + " - (Room: " + r.id + ")"
                });
            ViewBag.room_id = new SelectList(rooms, "id", "roomInfo", projection.room_id);
            return View(projection);
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,start_date,movie_id,room_id,available_seats")] Projection projection)
        {
            if (ModelState.IsValid)
            {
                db.Entry(projection).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.movie_id = new SelectList(db.Movie, "id", "name", projection.movie_id);
            var rooms = db.Room
                .Select(r => new
                {
                    id = r.id,
                    roomInfo = r.Cinema.name + " - (Room: " + r.id + ")"
                });
            ViewBag.room_id = new SelectList(rooms, "id", "roomInfo", projection.room_id);
            return View(projection);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Projection projection = db.Projection.Find(id);
            if (projection == null)
            {
                return HttpNotFound();
            }
            return View(projection);
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Projection projection = db.Projection.Find(id);
            //Remove all foreign keys:
            var projectionTickets = from t in db.Ticket
                                    join p in db.Projection on t.projection_id equals p.id
                                    where p.id == projection.id
                                    select t;
            foreach (var ticket in projectionTickets)
            {
                //Remove projection
                db.Ticket.Remove(ticket);
            }
            //delete projection
            db.Projection.Remove(projection);
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
