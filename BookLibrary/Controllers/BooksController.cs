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
using BookLibrary.Models.ServiceModels.Amazon;
using Humanizer;
using Newtonsoft.Json;

namespace BookLibrary.Web.Controllers
{
    public class BooksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private Logger log = new Logger(typeof(BooksController));
        // GET: Books
        public ActionResult Index()
        {
            //Books.ApplySortTitle();
            int page = 1;
            if (!String.IsNullOrEmpty(Request.QueryString["p"]))
            {
                Int32.TryParse(Request.QueryString["p"], out page);
            }
            int perPage = Config.Get<Int32>("ItemsPerPage");
            int skip = (page - 1) * perPage;
            var books = db.Books.Include(b => b.BookAuthors).OrderBy(b => b.SortTitle).Skip(skip).Take(perPage);
            List<PageLink> links = new List<PageLink>();
            
            if (User.Identity.IsAuthenticated)
            {
                links.Add(new PageLink("Add Book", "/Books/Create"));
                ViewBag.LinkList = links;
            }
            links.Add(new PageLink("Recently Added", "/Books/Recent"));
            int bookCount = Books.Count();
            double pageCount = Math.Ceiling((double)bookCount / (double)perPage);
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
                    pages.Add("<a href='/Books?p=" + idx + "'>" + idx + "</a>&nbsp;");
                }
                else
                {
                    pages.Add("<strong>" + idx + "</strong>&nbsp;");
                }
            }

            ViewBag.CurrentPage = page;
            ViewBag.Pages = pages;
            ViewBag.TotalItems = bookCount;
            return View(books.ToList());
        }

        // GET: Books/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Include(b => b.BookAuthors).Where(b => b.Id == id).FirstOrDefault();
            if (book == null)
            {
                return HttpNotFound();
            }
            BookDetailViewModel bookDetailView = new BookDetailViewModel
            {
                Object = book,
                Offers = new Offers(),
                Reviews = new List<EditorialReview>(),
                SimilarProducts = new List<Book>(),
                CategoryStreams = new List<List<Category>>(),
                AuthorDetail = new List<CelebrityDetail<Author>>()
            };
            ItemLookupResponse amzResponse;
            Exception ex;
            if (Core.ParseAmazonXml(book.AmazonResponse, out amzResponse, out ex))
            {
                bookDetailView.Offers = amzResponse.Items.Item.Offers;
                if (amzResponse.Items.Item.EditorialReviews != null)
                {
                    bookDetailView.Reviews = amzResponse.Items.Item.EditorialReviews.ToList();
                }
                if (amzResponse.Items.Item.SimilarProducts != null)
                {
                    foreach (SimilarProduct product in amzResponse.Items.Item.SimilarProducts)
                    {
                        Book similarAlbum = db.Books.Where(a => a.ASIN == product.ASIN).FirstOrDefault();
                        if (similarAlbum != null)
                        {
                            bookDetailView.SimilarProducts.Add(similarAlbum);
                        }
                    }
                }
            }
            string ByString = "";
            int aidx = 0;
            foreach (BookAuthor bookAuthor in book.BookAuthors)
            {
                Author author = Authors.AuthorById(bookAuthor.AuthorId);
                if(aidx > 0)
                {
                    if (book.BookAuthors.Count > 2)
                    {
                        ByString += ",";
                    }
                    if(aidx == book.BookAuthors.Count - 1)
                    {
                        ByString += " and ";
                    }
                }
                ByString += "<a href='/Authors/Details/" + author.Id + "'>" + author.Person.Name + "</a>";
                aidx += 1;
                if (author.Person.CelebrityId.HasValue)
                {
                    Celebrity celebrity;
                    if (!Authors.GetCelebrity(author.Person.CelebrityId.Value, out celebrity, out ex))
                    {
                        return RedirectToAction("Index");
                    }
                    bookDetailView.AuthorDetail.Add(new CelebrityDetail<Author>
                    {
                        Item = author,
                        Celebrity = celebrity
                    });
                    string url_add = "";
                    if (Request.UserHostAddress.StartsWith("192.168.1."))
                    {
                        url_add = ":8081";
                    }
                    //links.Add(new PageLink("Artist Details", "http://celebritycentral.gmancoder.com" + url_add + "/Celebrities/Details/" + celebrity.Id, "_blank"));
                }
            }
            string details = "";
            if(!String.IsNullOrEmpty(book.ISBN))
            {
                details += "<br /><strong>ISBN: </strong>" + book.ISBN;
            }
            if(!String.IsNullOrEmpty(book.Publisher))
            {
                details += "<br /><strong>Publisher: </strong>" + book.Publisher;
            }
            if(book.PublicationDate.HasValue && book.PublicationDate != DateTime.MinValue)
            {
                details += "<br /><strong>Published On: </strong>" + book.PublicationDate.Value.ToShortDateString();
            }
            if(book.ReleaseDate.HasValue && book.ReleaseDate != DateTime.MinValue)
            {
                details += "<br /><strong>Released On: </strong>" + book.ReleaseDate.Value.ToShortDateString();
            }
            if(!String.IsNullOrEmpty(book.Manufacturer))
            {
                details += "<br /><strong>Produced By: </strong>" + book.Manufacturer;
            }
            ViewBag.SubTitle = ByString + details;
            bookDetailView.CategoryStreams = Categories.DrawBreadcrumbsForObject(book.Id);

            List<PageLink> links = new List<PageLink>();
            links.Add(new PageLink("Book Information", book.DetailPageUrl, "_blank"));
            if (bookDetailView.AuthorDetail != null && bookDetailView.AuthorDetail.Count() > 0)
            {
                links.Add(new PageLink("About the " + "Author".ToQuantity(bookDetailView.AuthorDetail.Count(), ShowQuantityAs.None), "#author"));
            }
            if (bookDetailView.SimilarProducts.Count() > 0)
            {
                links.Add(new PageLink("Similar Items", "#similar"));
            }
            if (bookDetailView.Reviews.Count() > 0)
            {
                links.Add(new PageLink("Reviews", "#reviews"));
            }
            if (bookDetailView.Offers != null && bookDetailView.Offers.TotalOffers != null && bookDetailView.Offers.TotalOffers > 0)
            {
                links.Add(new PageLink("Shopping Offers", "#shopping"));
            }
            if(book.PdfId.HasValue)
            {
                links.Add(new PageLink("View PDF", "http://pdflib.gmancoder.com/pdfs/" + book.PdfId.Value));
            }
            if (User.Identity.IsAuthenticated)
            {
                if (!book.Reading)
                {
                    links.Add(new PageLink("Mark As Reading", "/Books/Reading/" + book.Id));
                }
                links.Add(new PageLink("Edit", "/Books/Edit/" + book.Id));
                links.Add(new PageLink("Delete", "/Books/Delete/" + book.Id, args: @"onClick=""return confirm('Are you sure?');"""));
            }
            ViewBag.LinkList = links;
            return View(bookDetailView);
        }
        [Authorize]
        public ActionResult Reading(Guid id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            Books.UnSetReading();
            Books.Reading(id);
            return RedirectToAction("Details", new { id = id });
        }
        [Authorize]
        // GET: Books/Create
        public ActionResult Create()
        {
            Guid authorId = Guid.Empty;
            ViewBag.Author = "";
            if (Request.QueryString["author"] != null)
            {
                Guid.TryParse(Request.QueryString["author"], out authorId);
                Author author = Authors.AuthorById(authorId);
                if(author != null)
                {
                    ViewBag.Author = author.Person.Name;
                }
            }
            ViewBag.CategoryHtml = Categories.GetCategoryHtml(null);
            //ViewBag.AuthorId = new SelectList(db.Authors.OrderBy(a => a.Name), "Id", "Name", authorId);
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Book book)
        {
            log.Info("Create");
            log.Info(JsonConvert.SerializeObject(book));
            Exception ex;
            List<Guid> selectedCategories = new List<Guid>();
            if (ModelState.IsValid)
            {
                bool amazon = false;
                book.Id = Guid.NewGuid();
                book.CreatedDate = book.ModifiedDate = DateTime.Now;
                if(book.EntryType == "Amazon")
                {
                    if(Books.AmazonBookExists(book.ISBN))
                    {
                        ModelState.AddModelError("ERR", "Book with ISBN '" + book.ISBN + "' already exists");
                        ViewBag.Error = true;
                        ViewBag.CategoryHtml = Categories.GetCategoryHtml(null);
                        log.Error("Book with ISBN '" + book.ISBN + "' already exists");
                        //ViewBag.AuthorId = new SelectList(db.Authors.OrderBy(a => a.Name), "Id", "Name", book.AuthorId);
                        return View(book);
                    }
                    amazon = true;
                    if(!Books.AmazonBook(ref book, out ex))
                    {
                        ModelState.AddModelError("ERR", Core.ParseException(ex));
                        ViewBag.Error = true;
                        ViewBag.CategoryHtml = Categories.GetCategoryHtml(null);
                        log.Error("Error", ex);
                        //ViewBag.AuthorId = new SelectList(db.Authors.OrderBy(a => a.Name), "Id", "Name", book.AuthorId);
                        return View(book);
                    }

                    return RedirectToAction("Details", new { id = book.Id });
                }
                else
                {
                    if(Books.ManualBookExists(book.Title, book.Authors))
                    {
                        ModelState.AddModelError("ERR", "Book " + book.Title + " already exists for this author");
                        log.Error("Book " + book.Title + " already exists for this author");
                        ViewBag.Error = true;
                        ViewBag.CategoryHtml = Categories.GetCategoryHtml(null);
                        //ViewBag.AuthorId = new SelectList(db.Authors.OrderBy(a => a.Name), "Id", "Name", book.AuthorId);
                        return View(book);
                    }

                    

                    HttpPostedFileBase display_image = Request.Files["ImageFileName"];
                    if (display_image.ContentLength == 0)
                    {
                        ModelState.AddModelError("ERR", "Image Upload required for manual entry");
                        log.Error("Image Upload required for manual entry");
                        //ViewBag.AuthorId = new SelectList(db.Authors.OrderBy(a => a.Name), "Id", "Name", book.AuthorId);
                        ViewBag.CategoryHtml = Categories.GetCategoryHtml(null);
                        ViewBag.Error = true;
                        return View(book);
                    }
                    else if (HttpPostedFileBaseExtensions.IsImage(display_image))
                    {
                        var fileName = Path.GetFileName(display_image.FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/images/books"), fileName);
                        display_image.SaveAs(path);
                        book.ImageFileName = fileName;
                    }
                    else
                    {
                        ModelState.AddModelError("ERR", "Book requires an image");
                        log.Error("Book requires an image");
                        //ViewBag.AuthorId = new SelectList(db.Authors.OrderBy(a => a.Name), "Id", "Name", book.AuthorId);
                        ViewBag.CategoryHtml = Categories.GetCategoryHtml(null);
                        ViewBag.Error = true;
                        return View(book);
                    }
                    book.SortTitle = Core.ApplySortTitle(book.Title);
                    db.Books.Add(book);
                    db.SaveChanges();

                    if(!Books.PopulateBookAuthors(book, out ex))
                    {
                        ModelState.AddModelError("ERR", Core.ParseException(ex));
                        log.Error("Error", ex);
                        //ViewBag.AuthorId = new SelectList(db.Authors.OrderBy(a => a.Name), "Id", "Name", book.AuthorId);
                        ViewBag.CategoryHtml = Categories.GetCategoryHtml(null);
                        ViewBag.Error = true;
                        return View(book);
                    }

                    PopulateCategories(book);
                    return RedirectToAction("Details", new { id = book.Id });
                }
                
            }
            ViewBag.Error = true;
            ViewBag.CategoryHtml = Categories.GetCategoryHtml(null);
            //ViewBag.AuthorId = new SelectList(db.Authors, "Id", "Name", book.AuthorId);
            return View(book);
        }

        [Authorize]
        // GET: Books/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            List<ObjectToCategory> bookCategories = db.ObjectToCategories.Where(bc => bc.ObjectId == id && bc.ObjectType == "Book").ToList();
            List<Guid> selectedCategories = new List<Guid>();
            foreach (ObjectToCategory bookCategory in bookCategories)
            {
                selectedCategories.Add(bookCategory.CategoryId);
            }
            ViewBag.CategoryHtml = Categories.GetCategoryHtml(selectedCategories);
            //ViewBag.AuthorId = new SelectList(db.Authors, "Id", "Name", book.AuthorId);
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Book book)
        {
            log.Info("Edit");
            log.Info(JsonConvert.SerializeObject(book));
            Exception ex;
            List<Guid> selectedCategories = new List<Guid>();
            if (ModelState.IsValid)
            {
                bool amazon = false;
                if (book.EntryType == "Amazon")
                {
                    amazon = true;
                    if (!Books.AmazonBook(ref book, out ex, true))
                    {
                        ModelState.AddModelError("ERR", Core.ParseException(ex));
                        log.Error("Error", ex);
                        ViewBag.Error = true;
                        ViewBag.CategoryHtml = Categories.GetCategoryHtml(null);
                        //ViewBag.AuthorId = new SelectList(db.Authors.OrderBy(a => a.Name), "Id", "Name", book.AuthorId);
                        return View(book);
                    }
                }
                else
                {
                    HttpPostedFileBase display_image = Request.Files["ImageFileName"];
                    if (display_image.ContentLength == 0)
                    {
                        ModelState.AddModelError("ERR", "Image Upload required for manual entry");
                        log.Error("Image Upload required for manual entry");
                        ViewBag.CategoryHtml = Categories.GetCategoryHtml(null);
                        //ViewBag.AuthorId = new SelectList(db.Authors.OrderBy(a => a.Name), "Id", "Name", book.AuthorId);
                        ViewBag.Error = true;
                        return View(book);
                    }
                    else if (HttpPostedFileBaseExtensions.IsImage(display_image))
                    {
                        var fileName = Path.GetFileName(display_image.FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/images/books"), fileName);
                        display_image.SaveAs(path);
                        book.ImageFileName = fileName;
                    }
                    else
                    {
                        ModelState.AddModelError("ERR", "Book requires an image");
                        log.Error("Book requires an image");
                        ViewBag.CategoryHtml = Categories.GetCategoryHtml(null);
                        //ViewBag.AuthorId = new SelectList(db.Authors.OrderBy(a => a.Name), "Id", "Name", book.AuthorId);
                        ViewBag.Error = true;
                        return View(book);
                    }
                    db.Database.ExecuteSqlCommand("delete from BookAuthors where BookId = '" + book.Id + "'");
                    if(!Books.PopulateBookAuthors(book, out ex))
                    {
                        ModelState.AddModelError("ERR", Core.ParseException(ex));
                        log.Error("Error", ex);
                        ViewBag.CategoryHtml = Categories.GetCategoryHtml(null);
                        //ViewBag.AuthorId = new SelectList(db.Authors.OrderBy(a => a.Name), "Id", "Name", book.AuthorId);
                        ViewBag.Error = true;
                        return View(book);
                    }
                    db.Database.ExecuteSqlCommand("delete from ObjectToCategories where BookId = '" + book.Id + "'");
                    PopulateCategories(book);
                }
                book.ModifiedDate = DateTime.Now;
                book.SortTitle = Core.ApplySortTitle(book.Title);
                db.Entry(book).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", new { @Id = book.Id });
            }
            //ViewBag.AuthorId = new SelectList(db.Authors, "Id", "Name", book.AuthorId);
            ViewBag.CategoryHtml = Categories.GetCategoryHtml(null);
            return View(book);
        }
        [Authorize]
        // GET: Books/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            db.Books.Remove(book);
            db.SaveChanges();
            return View(book);
        }

        private void PopulateCategories(Book book)
        {
            List<Guid> selectedCategories = new List<Guid>();
            if (!String.IsNullOrWhiteSpace(Request.Form["categoryTree"]))
            {
                string categoryTree = Request.Form["categoryTree"];
                string[] categoryIds = categoryTree.Split(',');
                foreach (string categoryId in categoryIds)
                {
                    Guid CategoryId;
                    if (Guid.TryParse(categoryId, out CategoryId))
                    {
                        ObjectToCategory bookCategory = new ObjectToCategory
                        {
                            ObjectId = book.Id,
                            ObjectType = "Book",
                            CategoryId = CategoryId,
                            //Id = Guid.NewGuid()
                        };
                        selectedCategories.Add(CategoryId);
                        db.ObjectToCategories.Add(bookCategory);
                        db.SaveChanges();
                    }
                }
            }
        }

        

        public ActionResult Recent()
        {
            int page = 1;
            if (!String.IsNullOrEmpty(Request.QueryString["p"]))
            {
                Int32.TryParse(Request.QueryString["p"], out page);
            }
            int perPage = Config.Get<Int32>("ItemsPerPage");
            int skip = (page - 1) * perPage;
            RecentBookViewModel recentView = new RecentBookViewModel
            {
                Today = new List<Book>(),
                Yesterday = new List<Book>(),
                ThisWeek = new List<Book>(),
                LastWeek = new List<Book>(),
                Older = new Dictionary<int, List<Book>>(),
                Count = 0
            };
            List<Book> bookList = db.Books.Include(b=> b.BookAuthors).OrderByDescending(o => o.CreatedDate).Skip(skip).Take(perPage).ToList();
            string today = DateTime.Now.ToString("yyyyMMdd");
            string yesterday = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");

            DateTime todayDate = DateTime.Now;
            while (todayDate.DayOfWeek != DayOfWeek.Sunday)
            {
                todayDate = todayDate.AddDays(-1);
            }
            string thisWeekStart = todayDate.ToString("yyyyMMdd");
            todayDate = todayDate.AddDays(-1);
            string lastWeekEnd = todayDate.ToString("yyyyMMdd");
            while (todayDate.DayOfWeek != DayOfWeek.Sunday)
            {
                todayDate = todayDate.AddDays(-1);
            }
            string lastWeekStart = todayDate.ToString("yyyyMMdd");


            foreach (Book book in bookList)
            {
                string bookDate = book.CreatedDate.ToString("yyyyMMdd");
                if (bookDate == today)
                {
                    recentView.Today.Add(book);
                }
                else if (bookDate == yesterday)
                {
                    recentView.Yesterday.Add(book);
                }
                else if (String.Compare(bookDate, thisWeekStart) >= 0)
                {
                    recentView.ThisWeek.Add(book);
                }
                else if (String.Compare(bookDate, lastWeekStart) >= 0)
                {
                    recentView.LastWeek.Add(book);
                }
                else
                {
                    int year = book.CreatedDate.Year;
                    if (!recentView.Older.ContainsKey(year))
                    {
                        recentView.Older.Add(year, new List<Book>());
                    }
                    recentView.Older[year].Add(book);
                }
                recentView.Count += 1;
            }
            List<PageLink> links = new List<PageLink>();
            links.Add(new PageLink("Books By Name", "/Books"));
            if (User.Identity.IsAuthenticated)
            {
                links.Add(new PageLink("Add Book", "/Books/Create"));
            }
            ViewBag.LinkList = links;
            int bookCount = Books.Count();
            double pageCount = Math.Ceiling((double)bookCount / (double)perPage);
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
                    pages.Add("<a href='/Books/Recent?p=" + idx + "'>" + idx + "</a>&nbsp;");
                }
                else
                {
                    pages.Add("<strong>" + idx + "</strong>&nbsp;");
                }
            }

            ViewBag.CurrentPage = page;
            ViewBag.Pages = pages;
            ViewBag.TotalItems = bookCount;
            return View(recentView);
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
