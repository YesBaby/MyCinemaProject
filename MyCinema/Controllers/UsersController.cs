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
    public class UsersController : Controller
    {
        private CinemaDBConnection db = new CinemaDBConnection();

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AdminPanel()
        {
            return View();
        }

        [CustomAuthorize(Roles = "Admin,User")]
        public ActionResult UserPanel()
        {
            var userTickets = from t in db.Ticket
                              join u in db.User on t.user_id equals u.id
                              where u.name == SessionPersister.Username
                              select t;
            if (userTickets == null)
            {
                ViewBag.Error = "No tickets!";
                return View("UserPanel");
            }
            if(TempData["success"] != null)
            {
                ViewBag.Success = "Successfully changed!";
                TempData.Remove("success");
            }
            ViewBag.userTicketsList = userTickets.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Admin,User")]
        public ActionResult UserPanel(UserProfileViewModel model)
        {
            User user = db.User.Where(u => u.name.Equals(SessionPersister.Username)).FirstOrDefault();
            if (ModelState.IsValid)
            {
                if (model.CurrentPassword != user.password)
                {
                    ViewBag.PasswordError = "Password is not correct";
                    return View(model);
                }
                user.password = model.NewPassword;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                TempData["success"] = "true";
                return RedirectToAction("UserPanel");
            }

            var userTickets = from t in db.Ticket
                              join u in db.User on t.user_id equals u.id
                              where u.name == SessionPersister.Username
                              select t;

            ViewBag.userTicketsList = userTickets.ToList();
            ViewBag.SaveError = "Bad Save - model not valid";
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(UserViewModel uvm)
        {
            UserModel um = new UserModel();
            if (string.IsNullOrEmpty(uvm.User.name) || string.IsNullOrEmpty(uvm.User.password)
                || um.LoginUser(uvm.User.name, uvm.User.password) == null)
            {
                ViewBag.Error = "Account is Invalid";
                return View("Login");
            }
            SessionPersister.Username = uvm.User.name;
            User testUser = um.FindUser(uvm.User.name);
            SessionPersister.Admin = testUser.is_admin.ToString();
            if (testUser.is_admin == true)
            {
                return View("AdminPanel");
            }
            return View("UserPanel");
        }

        [CustomAuthorize(Roles = "Admin,User")]
        public ActionResult Logout()
        {
            SessionPersister.Username = string.Empty;
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (db.User.Where(usr => usr.name.Equals(model.Username)).FirstOrDefault() != null)
                {
                    ViewBag.Error = "Account already exists!";
                    return View("Register");
                }
                db.User.Add(model.ViewModelToDBModel());
                db.SaveChanges();
                return RedirectToAction("Login");
            }
            return View(model);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View(db.User.ToList());
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RegisterViewModel model, bool is_admin = false)
        {
            if (ModelState.IsValid)
            {
                if (db.User.Where(usr => usr.name.Equals(model.Username)).FirstOrDefault() != null)
                {
                    ViewBag.Error = "Account already exists!";
                    return View("Create");
                }
                db.User.Add(model.ViewModelToDBModelWithAdmin(is_admin));
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.User.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,name,password,is_admin")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.User.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.User.Find(id);

            //delete all foreign keys first:
            var userTickets = from t in db.Ticket
                              join u in db.User on t.user_id equals u.id
                              where u.name == user.name
                              select t;
            foreach (var ticket in userTickets)
            {
                db.Ticket.Remove(ticket);
            }
            //delete user himself:
            db.User.Remove(user);
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
