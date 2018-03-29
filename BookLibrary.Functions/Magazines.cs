using BookLibrary.Models;
using BookLibrary.Models.ServiceModels.Amazon;
using BookLibrary.Models.ServiceModels.PDFLibrary;
using BookLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using BookLibrary.Models.ViewModels;

namespace BookLibrary.Functions
{
    
    public class Magazines
    {
        private static PDFLibraryService pdfService = new PDFLibraryService("grbrewer@gmail.com", "!Pass248word");
        private static ApplicationDbContext db = new ApplicationDbContext();

        public static List<string> Fields
        {
            get
            {
                return new List<string>
                {
                    "PdfCategoryFolderId", "Title", "CreatedDate", "ModifiedDate"
                };
            }
        }

        public static void ApplySortTitle()
        {
            List<Magazine> allMagazines = db.Magazines.ToList();
            foreach(Magazine magazine in allMagazines)
            {
                magazine.SortTitle = Core.Core.ApplySortTitle(magazine.Title);
                db.Entry(magazine).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public static List<string> RequiredFields
        {
            get
            {
                return new List<string>
                {
                    "Title"
                };
            }
        }

        public static int Count()
        {
            return db.Magazines.Count();
        }

        public static List<LibraryObject> GetAsObject(int take = 5000)
        {
            List<Magazine> magazines = db.Magazines.OrderByDescending(o => o.CreatedDate).Take(take).ToList();
            return MagazinesToObjects(magazines);
        }

        public static List<LibraryObject> GetAsObject(string q)
        {
            List<Magazine> magazines = db.Magazines.Where(m => m.Title.ToLower().Contains(q.ToLower())).OrderBy(m => m.Title).ToList();
            return MagazinesToObjects(magazines);
        }

        private static List<LibraryObject> MagazinesToObjects(List<Magazine> magazines)
        {
            List<LibraryObject> objects = new List<LibraryObject>();
            
            foreach (Magazine magazine in magazines)
            {
                objects.Add(new LibraryObject
                {
                    Type = "Magazine",
                    Id = magazine.Id,
                    Name = magazine.Title,
                    Image = GetMagazineThumb(magazine.Id),
                    CreatedDate = magazine.CreatedDate
                });
            }
            return objects;
        }

        public static bool GetCategoryFolder(ref Magazine magazine, out Exception ex)
        {
            ex = null;
            if (pdfService.LoggedIn())
            {
                Models.ServiceModels.PDFLibrary.Category magazineCategory = pdfService.GetCategoryByName("Magazines");

                if (magazineCategory != null)
                {

                    CategoryFolder folder = pdfService.GetCategoryFolderByName(magazineCategory.Id, magazine.Title);
                    if (folder != null)
                    {
                        ex = null;
                        magazine.PdfCategoryFolderId = folder.Id;
                        return true;
                    }
                    ex = new Exception("Magazine with Title " + magazine.Title + " not found in PDF Library");
                    return false;
                }
                ex = new Exception("Magazines Category not found in PDF Library");
                return false;
            }
            ex = new Exception("Login to PDF Library API Failed");
            return false;
        }

        public static bool FindCreateCategory(Magazine magazine, out Exception ex)
        {
            ex = null;
            if(pdfService.LoggedIn())
            {
                List<string> folder_path = new List<string>();
                List<CategoryFolder> folders = pdfService.GetCategoryFolderById(magazine.PdfCategoryFolderId);
                if(folders != null && folders.Count() > 0)
                {
                    CategoryFolder folder = folders[0];
                    if (folder.Category_Folder_Id.HasValue)
                    {
                        if (!RecurseCategoryPath(folder.Category_Folder_Id.Value, ref folder_path, out ex))
                        {
                            return false;
                        }
                    }
                }
                folder_path.Reverse();
                folder_path.Add("Magazines");
                Categories.PopulateCategories<Magazine>(new List<BrowseNode> { Categories.CategoryPathToBrowseNodeTree(folder_path) }, magazine);
                return true;
            }
            ex = new Exception("Login to PDF Library API Failed");
            return false;
        }

        

        private static bool RecurseCategoryPath(int Id, ref List<string> path, out Exception ex)
        {
            List<CategoryFolder> folders = pdfService.GetCategoryFolderById(Id);
            if (folders != null && folders.Count() > 0)
            {
                CategoryFolder folder = folders[0];
                path.Add(folder.Name);
                if (folder.Category_Folder_Id.HasValue)
                {
                    if (!RecurseCategoryPath(folder.Category_Folder_Id.Value, ref path, out ex))
                    {
                        return false;
                    }
                }
            }
            ex = null;
            return true;
        }

        

        public static string GetMagazineThumb(Guid id)
        {
            Magazine magazine = db.Magazines.Include(m => m.MagazineIssues).Where(m => m.Id == id).FirstOrDefault();
            if(magazine != null)
            {
                if (magazine.MagazineIssues.Count() > 0)
                {
                    return magazine.MagazineIssues.OrderByDescending(i => i.ReleaseDate).First().ImageFileName;
                }
                return "/Content/images/magazine_generic.png";
            }
            return "";
        }

        public static MagazineListItemViewModel MagazineToView(Magazine magazine)
        {
            return new MagazineListItemViewModel
            {
                Magazine = magazine,
                ImageFileName = Magazines.GetMagazineThumb(magazine.Id),
                CategoryStreams = Categories.DrawBreadcrumbsForObject(magazine.Id)
            };
        }

        public static bool SyncIssues(Guid? id, out Magazine magazine, out Exception ex)
        {
            db.Configuration.LazyLoadingEnabled = false;
            magazine = db.Magazines.Find(id);
            if(magazine == null)
            {
                ex = new KeyNotFoundException("Magazine not found");
                return false;
            }

            return MagazineIssues.SyncIssues(magazine, out ex);
        }
    }
}
