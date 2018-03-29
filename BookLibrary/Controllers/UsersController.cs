using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BookLibrary.Models;
using System.Web.Helpers;
using BookLibrary.Models.ViewModels;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using BookLibrary.Functions;
using Newtonsoft.Json;

namespace BookLibrary.Web.Controllers
{
    public class UsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private Logger log = new Logger(typeof(UsersController));
        // GET: Users
        [Authorize]
        public ActionResult Index()
        {
            List<PageLink> links = new List<PageLink>();
            if (User.Identity.IsAuthenticated)
            {
                links.Add(new PageLink("Add User", "/Users/Create"));
                links.Add(new PageLink("Roles", "/Roles"));
            }
            ViewBag.LinkList = links;
            return View(db.Users.ToList());
        }

        // GET: Users/Details/5
        [Authorize]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = db.Users.Find(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            return RedirectToAction("Edit", new { id = id });
        }

        // GET: Users/Create
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ApplicationUser applicationUser)
        {
            log.Info("Create");
            log.Info(JsonConvert.SerializeObject(applicationUser));
            if (ModelState.IsValid)
            {
                bool error = false;
                if(Request.Form["password"] != Request.Form["password_confirm"])
                {
                    ModelState.AddModelError("ERROR_1", "Password fields do not match");
                    log.Error("Password fields do not match");
                    error = true;
                }
                if (db.Users.Where(ux => ux.Email == applicationUser.Email || ux.UserName == applicationUser.UserName).ToList().Count() > 0)
                {
                    ModelState.AddModelError("ERROR_2", "User already exists.");
                    log.Error("User already exists.");
                    error = true;
                }

                if (!error)
                {
                    applicationUser.Id = Guid.NewGuid().ToString().Replace("-", "");
                    applicationUser.SecurityStamp = Crypto.GenerateSalt();
                    applicationUser.PasswordHash = Crypto.HashPassword(Request.Form["password"]);


                    db.Users.Add(applicationUser);
                    db.SaveChanges();
                    return RedirectToAction("Edit", new { id = applicationUser.Id });
                }
            }

            return View(applicationUser);
        }

        // GET: Users/Edit/5
        [Authorize]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = db.Users.Find(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            return View(applicationUser);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ApplicationUser applicationUser)
        {
            log.Info("Edit");
            log.Info(JsonConvert.SerializeObject(applicationUser));
            if (ModelState.IsValid)
            {
                bool error = false;
                if (Request.Form["password"] != "" && (Request.Form["password"] != Request.Form["password_confirm"]))
                {
                    ModelState.AddModelError("ERROR_1", "Password fields do not match");
                    log.Error("Password fields do not match");
                    error = true;
                }
                else if(Request.Form["password"] != "")
                {
                    applicationUser.PasswordHash = Crypto.HashPassword(Request.Form["password"]); 
                }
                if (db.Users.Where(ux => (ux.Email == applicationUser.Email || ux.UserName == applicationUser.UserName) && ux.Id != applicationUser.Id).ToList().Count() > 0)
                {
                    ModelState.AddModelError("ERROR_2", "User already exists.");
                    log.Error("User already exists.");
                    error = true;
                }

                if (!error)
                {
                    db.Entry(applicationUser).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View(applicationUser);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(string id)
        {
            return HttpNotFound();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [Authorize]
        public ActionResult UpdateRoles(string id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = db.Users.Find(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }

            UpdateRolesViewModel viewModel = new UpdateRolesViewModel
            {
                allRoles = db.Roles.ToList(),
                userRoles = applicationUser.Roles.ToList(),
                userId = applicationUser.Id,
                emailAddress = applicationUser.Email
            };

            List<PageLink> links = new List<PageLink>();
            if (User.Identity.IsAuthenticated)
            {
                links.Add(new PageLink("Add Role", "/Roles/Create"));
            }
            ViewBag.LinkList = links;
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateRoles()
        {
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            String[] selectedRoles = Request.Form.GetValues("user_role");
            string id = Request.Form["userId"];
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = db.Users.Find(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }

            foreach(IdentityRole role in db.Roles.ToList())
            {
                if (selectedRoles.Contains(role.Id) && !UserManager.IsInRole(id, role.Name))
                {
                    UserManager.AddToRole(id, role.Name);
                } 
                else if(!selectedRoles.Contains(role.Id) && UserManager.IsInRole(id, role.Name))
                {
                    UserManager.RemoveFromRole(id, role.Name);
                }
            }

            return RedirectToAction("Index");
        }
    }
}
