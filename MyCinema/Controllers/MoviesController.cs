using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MyCinema.Models;
using System.Data.Entity.Infrastructure;
using MyCinema.Security;

namespace MyCinema.Controllers
{
    public class MoviesController : Controller
    {
        private CinemaDBConnection db = new CinemaDBConnection();

        [CustomAuthorize(Roles = "Admin,User")]
        public ActionResult Index()
        {
            return View(db.Movie.ToList());
        }

        [AllowAnonymous]
        public ActionResult MovieProjections(int movie_id)
        {
            Movie currentMovie = db.Movie.Find(movie_id);
            ViewBag.currentMovie = currentMovie;

            List<Projection> movieProjections = new List<Projection>();
            foreach (var projection in db.Projection)
            {
                if(projection.movie_id == movie_id)
                {
                    movieProjections.Add(projection);
                }
            }
            ViewBag.movieProjections = movieProjections;
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movie = db.Movie.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,name,duration")] Movie movie, HttpPostedFileBase upload)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (upload != null && upload.ContentLength > 0)
                    {
                        using (var reader = new System.IO.BinaryReader(upload.InputStream))
                        {
                            movie.picture = reader.ReadBytes(upload.ContentLength);
                        }
                    }

                    db.Movie.Add(movie);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(movie);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movie = db.Movie.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, HttpPostedFileBase upload)
        {
            if(id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var movieToUpdate = db.Movie.Find(id);
            if (TryUpdateModel(movieToUpdate, "", new string[] { "id", "name", "duration" }))
            {
                if (ModelState.IsValid)
                {
                    if (upload != null && upload.ContentLength > 0)
                    {
                        using (var reader = new System.IO.BinaryReader(upload.InputStream))
                        {
                            movieToUpdate.picture = reader.ReadBytes(upload.ContentLength);
                        }
                    }

                    db.Entry(movieToUpdate).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View(movieToUpdate);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movie = db.Movie.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Movie movie = db.Movie.Find(id);
            //delete the foreign keys:
            var movieProjections = from p in db.Projection
                                   join m in db.Movie on p.movie_id equals m.id
                                   where m.id == movie.id
                                   select p;
            foreach (var projection in movieProjections)
            {
                var projectionTickets = from t in db.Ticket
                                        join p in db.Projection on t.projection_id equals p.id
                                        where p.id == projection.id
                                        select t;
                foreach (var ticket in projectionTickets)
                {
                    //Remove tickets
                    db.Ticket.Remove(ticket);
                }
                //Remove projections
                db.Projection.Remove(projection);
            }
            //delete the actual movie:
            db.Movie.Remove(movie);
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
