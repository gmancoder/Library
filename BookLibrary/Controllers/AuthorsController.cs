using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BookLibrary.Models;
using BookLibrary.Models.ViewModels;
using BookLibrary.Functions;
using BookLibrary.Functions.Core;
using System.IO;
using Newtonsoft.Json;

namespace BookLibrary.Web.Controllers
{
    public class AuthorsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private Logger log = new Logger(typeof(AuthorsController));
        // GET: Authors
        public ActionResult Index()
        {
            Authors.MigrateToPeople();
            int page = 1;
            if (!String.IsNullOrEmpty(Request.QueryString["p"]))
            {
                Int32.TryParse(Request.QueryString["p"], out page);
            }
            int perPage = Config.Get<Int32>("ItemsPerPage");
            int skip = (page - 1) * perPage;
            List<Author> authors = db.Authors.Include(a => a.Person).OrderBy(a => a.Person.SortName).Skip(skip).Take(perPage).ToList();
            ViewBag.SubTitle = authors.Count + " total authors";
            if (User.Identity.IsAuthenticated)
            {
                List<PageLink> links = new List<PageLink>();
                links.Add(new PageLink("Add Author", "/Authors/Create"));
                ViewBag.LinkList = links;
            }
            int authorCount = Authors.Count();
            double pageCount = Math.Ceiling((double)authorCount / (double)perPage);
            int start = page - 4;
            if (start < 1)
            {
                start = 1;
            }
            int end = page + 4;
            if (end > pageCount)
            {
                end = (int)pageCount;
            }

            List<string> pages = new List<string>();
            for (int idx = start; idx <= end; idx++)
            {
                if (idx != page)
                {
                    pages.Add("<a href='/Authors?p=" + idx + "'>" + idx + "</a>&nbsp;");
                }
                else
                {
                    pages.Add("<strong>" + idx + "</strong>&nbsp;");
                }
            }

            ViewBag.CurrentPage = page;
            ViewBag.Pages = pages;
            ViewBag.TotalItems = authorCount;
            return View(authors);
        }

        // GET: Authors/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            List<PageLink> links = new List<PageLink>();
            PersonViewModel viewModel;
            Exception ex;
            if (!People.GeneratePersonView(id.Value, "Author", out viewModel, out ex))
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }

            People.DrawLinkListForView(viewModel, "Author", ref links);

            if (User.Identity.IsAuthenticated)
            {
                links.Add(new PageLink("Add Book", "/Books/Create?author=" + id.Value));
                links.Add(new PageLink("Edit", "/Authors/Edit/" + id.Value));
                links.Add(new PageLink("Delete", "/Authors/Delete/" + id.Value, args: @"onClick=""return confirm('Are you sure?');"""));
            }

            ViewBag.LinkList = links;
            return View("PeopleDetails", viewModel);
        }

        [Authorize]
        // GET: Authors/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Authors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Author author)
        {
            log.Info("Create");
            log.Info(JsonConvert.SerializeObject(author));
            Exception ex;
            if (ModelState.IsValid)
            {
                author.Id = Guid.NewGuid();
                author.CreatedDate = DateTime.Now;
                
                if (!Authors.AuthorExists(Request.Form["name"]))
                {
                    if (UpdateAuthor(ref author, out ex))
                    {
                        db.Authors.Add(author);
                        db.SaveChanges();
                        return RedirectToAction("Details", new { id = author.Id });
                    }
                    else
                    {
                        ModelState.AddModelError("ERR", ex.Message);
                        log.Error("Error", ex);
                    }
                }
                else
                {
                    ModelState.AddModelError("ERR", "Author already exists");
                    log.Error("Author '" + Request.Form["name"] + "' already exists");
                }
            }

            return View(author);
        }

        private bool UpdateAuthor(ref Author author, out Exception ex)
        {
            ex = null;
            author.ModifiedDate = DateTime.Now;
            HttpPostedFileBase display_image = Request.Files["AuthorDisplayImage"];
            string file_path = null;
            if (display_image.ContentLength > 0 && HttpPostedFileBaseExtensions.IsImage(display_image))
            {
                var fileName = Path.GetFileName(display_image.FileName);
                var ext = Path.GetExtension(display_image.FileName);
                string new_file_name = author.Id.ToString().Replace("-", "") + ext;
                var path = Path.Combine(Server.MapPath("~/Content/images/authors"), new_file_name);
                display_image.SaveAs(path);
                file_path = "/Content/images/authors/" + new_file_name;
            }

            Person person;
            if(People.FindSavePerson(Request.Form["Name"], false, out person, out ex, file_path))
            {
                author.PersonId = person.Id;
                return true;
            }
            
            return false;
        }
        [Authorize]
        // GET: Authors/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Author author = db.Authors.Include(a =>a.Person).Where(a => a.Id == id).FirstOrDefault();
            if (author == null)
            {
                return HttpNotFound();
            }
            return View(author);
        }

        // POST: Authors/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Author author)
        {
            log.Info("Edit");
            log.Info(JsonConvert.SerializeObject(author));
            Exception ex;
            if (ModelState.IsValid)
            {
                if(!UpdateAuthor(ref author, out ex))
                {
                    ModelState.AddModelError("ERR", ex.Message);
                    log.Error("Error", ex);
                    return View(author);
                }
                db.Entry(author).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", new { id = author.Id });
            }
            return View(author);
        }
        [Authorize]
        // GET: Authors/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Author author = db.Authors.Find(id);
            if (author == null)
            {
                return HttpNotFound();
            }
            db.Authors.Remove(author);
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
