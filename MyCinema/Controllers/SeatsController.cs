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
    public class SeatsController : Controller
    {
        private CinemaDBConnection db = new CinemaDBConnection();

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var seat = db.Seat.Include(s => s.Room);
            return View(seat.ToList());
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Seat seat = db.Seat.Find(id);
            if (seat == null)
            {
                return HttpNotFound();
            }
            return View(seat);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Create()
        {
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
        public ActionResult Create([Bind(Include = "id,row,col,room_id")] Seat seat)
        {
            if (ModelState.IsValid)
            {
                bool exists = false;
                foreach (var s in db.Seat)
                {
                    if(s.room_id == seat.room_id)
                    {
                        if((s.row == seat.row) && (s.col == seat.col))
                        {
                            exists = true;
                            break;
                        }
                    }
                }
                if (!exists)
                {
                    db.Seat.Add(seat);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Error = "Such seat already exists!";
                    var roomz = db.Room
                                    .Select(r => new
                                    {
                                        id = r.id,
                                        roomInfo = r.Cinema.name + " - (Room: " + r.id + ")"
                                    });
                    ViewBag.room_id = new SelectList(roomz, "id", "roomInfo");
                }
            }
            var rooms = db.Room
                .Select(r => new
                {
                    id = r.id,
                    roomInfo = r.Cinema.name + " - (Room: " + r.id + ")"
                });
            ViewBag.room_id = new SelectList(rooms, "id", "roomInfo");
            return View(seat);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Seat seat = db.Seat.Find(id);
            if (seat == null)
            {
                return HttpNotFound();
            }
            ViewBag.room_id = new SelectList(db.Room, "id", "id", seat.room_id);
            return View(seat);
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,row,col,room_id")] Seat seat)
        {
            if (ModelState.IsValid)
            {
                db.Entry(seat).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.room_id = new SelectList(db.Room, "id", "id", seat.room_id);
            return View(seat);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Seat seat = db.Seat.Find(id);
            if (seat == null)
            {
                return HttpNotFound();
            }
            return View(seat);
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Seat seat = db.Seat.Find(id);
            db.Seat.Remove(seat);
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
