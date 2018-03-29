using BookLibrary.Models;
using BookLibrary.Models.ServiceModels.PDFLibrary;
using BookLibrary.Models.ViewModels;
using BookLibrary.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Functions
{
    public class MagazineIssues
    {
        private static PDFLibraryService pdfService = new PDFLibraryService("grbrewer@gmail.com", "!Pass248word");
        private static ApplicationDbContext db = new ApplicationDbContext();
        public static List<string> Fields
        {
            get
            {
                return new List<string>
                {
                    "MagazineId", "ReleaseDate", "PdfId", "CreatedDate", "ModifiedDate", "ImageFileName", "PdfTitle", "ReleaseDateText"
                };
            }
        }

        public static List<string> RequiredFields
        {
            get
            {
                return new List<string>
                {
                    "MagazineId", "ReleaseDate", "PdfId"
                };
            }
        }

        public static bool MagazineIssueExists(int pdfId)
        {
            return db.MagazineIssues.Where(mi => mi.PdfId == pdfId).Count() > 0;
        }

        public static List<LibraryObject> GetAsObject(int take = 5000)
        {
            List<LibraryObject> objects = new List<LibraryObject>();
            List<MagazineIssue> magazinesIssues = db.MagazineIssues.Include(mi => mi.Magazine).OrderByDescending(o => o.CreatedDate).Take(take).ToList();
            foreach (MagazineIssue magazineIssue in magazinesIssues)
            {
                objects.Add(new LibraryObject
                {
                    Type = "Magazine",
                    Id = magazineIssue.Magazine.Id,
                    Name = magazineIssue.PdfTitle,
                    Image = magazineIssue.ImageFileName,
                    CreatedDate = magazineIssue.ReleaseDate
                });
            }
            return objects;
        }

        public static int Count()
        {
            return db.MagazineIssues.Count();
        }

        public static List<MagazineIssue> GetMagazineIssues(Guid? magazine_issue_id, Guid? magazine_id)
        {
            db.Configuration.LazyLoadingEnabled = false;
            IQueryable<MagazineIssue> issues = db.MagazineIssues;
            if (magazine_issue_id.HasValue)
            {
                issues = issues.Where(i => i.Id == magazine_issue_id.Value);
            }
            if (magazine_id.HasValue)
            {
                issues = issues.Where(i => i.MagazineId == magazine_id);
            }

            return issues.OrderBy(i => i.ReleaseDate).ToList();
        }

        public static bool SyncIssues(Magazine magazine, out Exception ex)
        {
            if (pdfService.LoggedIn())
            {
                List<CategoryFolder> folders = pdfService.GetCategoryFolderById(magazine.PdfCategoryFolderId);
                if (folders != null && folders.Count() > 0)
                {
                    CategoryFolder folder = folders[0];
                    List<Pdf> pdfs = pdfService.GetPDFsByPath(folder.Path);
                    foreach (Pdf pdf in pdfs)
                    {
                        if (!MagazineIssueExists(pdf.Id))
                        {
                            MagazineIssue newIssue = new MagazineIssue
                            {
                                Id = Guid.NewGuid(),
                                MagazineId = magazine.Id,
                                ReleaseDate = pdf.Created_At,
                                ReleaseDateText = pdf.Created_At.ToShortDateString(),
                                PdfTitle = pdf.Name,
                                PdfId = pdf.Id,
                                ImageFileName = pdfService.ExternalUrl + pdf.Cover_Path.Remove(0, 1),
                                CreatedDate = DateTime.Now,
                                ModifiedDate = DateTime.Now
                            };
                            db.MagazineIssues.Add(newIssue);
                            db.SaveChanges();
                        }
                    }
                    if (magazine.MagazineIssues != null)
                    {
                        foreach (MagazineIssue issue in magazine.MagazineIssues)
                        {
                            if (!pdfService.PdfExists(issue.PdfId))
                            {
                                db.Database.ExecuteSqlCommand("delete from magazineissues where id = '" + issue.Id + "'");
                            }
                        }
                    }
                    ex = null;
                    return true;
                }

                ex = new Exception("Category Folder not found");
                return false;
            }
            ex = new Exception("PDF Library login failed");
            return false;
        }

        public static IssueByDateViewModel GetMagazineIssuesByDate(int skip, int perPage)
        {
            IssueByDateViewModel issues = new IssueByDateViewModel();
            issues.Issues = new Dictionary<int, Dictionary<int, IssueByMonthView>>();
            List<MagazineIssue> allIssues = db.MagazineIssues.Include(m => m.Magazine).OrderByDescending(m => m.ReleaseDate).ToList();
            
            foreach (MagazineIssue issue in allIssues)
            {
                DateTime releaseDate = issue.ReleaseDate;
                int month = releaseDate.Month;
                int year = releaseDate.Year;
                if (!issues.Issues.ContainsKey(year))
                {
                    issues.Issues.Add(year, new Dictionary<int, IssueByMonthView>());
                }
                if (!issues.Issues[year].ContainsKey(month))
                {
                    issues.Issues[year].Add(month, new IssueByMonthView
                    {
                        MonthName = Core.Core.MonthName(month),
                        Issues = new List<MagazineIssue>()
                    });
                }
                issues.Issues[year][month].Issues.Add(issue);
            }

            return issues;
        }

        public static bool SaveIssue(Guid id, DateTime release_date, string release_date_text, out MagazineIssue issue)
        {
            issue = db.MagazineIssues.Find(id);
            if (issue == null)
            {
                return false;
            }
            issue.ReleaseDate = release_date;
            if(!String.IsNullOrEmpty(release_date_text))
            {
                issue.ReleaseDateText = release_date_text;
            }
            
            issue.ModifiedDate = DateTime.Now;
            db.Entry(issue).State = EntityState.Modified;
            db.SaveChanges();
            return true;
        }

    }
}
