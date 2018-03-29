using BookLibrary.Functions.Core;
using BookLibrary.Models;
using BookLibrary.Models.ServiceModels.Amazon;
using BookLibrary.Models.ViewModels;
using BookLibrary.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BookLibrary.Functions
{
    public class Books
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        public static List<string> Fields
        {
            get
            {
                return new List<string> { "Authors", "ISBN", "Title", "Manufacturer", "AmazonProductGroup", "DetailPageUrl", "ImageFileName", "Reading", "AmazonResponse", "CreatedDate", "ModifiedDate", "PdfId", "EntryType", "PublicationDate", "Publisher", "ReleaseDate", "ASIN" };
            }
        }
        public static List<string> RequiredFields
        {
            get
            {
                return new List<string>
                {
                    "Title","EntryType"
                };
            }
        }

        public static bool AmazonBookExists(string ISBN)
        {
            return db.Books.Where(b => b.ISBN == ISBN).Count() > 0;
        }
        public static bool ManualBookExists(string Title, string Authors)
        {
            return db.Books.Where(b => b.Title == Title && b.Authors == Authors).Count() > 0;
        }

        public static List<LibraryObject> GetAsObject(int take = 5000)
        {
            List<Book> books = db.Books.Include(b => b.BookAuthors).OrderByDescending(o => o.CreatedDate).Take(take).ToList();
            return BooksToObjects(books);
        }

        public static List<LibraryObject> GetAsObject(string q)
        {
            List<Book> books = db.Books.Where(b => b.Title.ToLower().Contains(q.ToLower())).Include(b => b.BookAuthors).OrderBy(b => b.Title).ToList();
            return BooksToObjects(books);
        }

        private static List<LibraryObject> BooksToObjects(List<Book> books)
        {
            List<LibraryObject> objects = new List<LibraryObject>();
            foreach (Book book in books)
            {
                List<LibraryObjectBy> byObjects = new List<LibraryObjectBy>();
                foreach(BookAuthor author in book.BookAuthors)
                {
                    byObjects.Add(new LibraryObjectBy
                    {
                        ByText = "By",
                        ByValue = author.AuthorName,
                        ByType = "Author",
                        ById = author.AuthorId
                    });
                }
                LibraryObject bookObject = BookToObject(book);
                bookObject.ByObjects = byObjects;
                objects.Add(bookObject);
            }
            return objects;
        }

        public static LibraryObject BookToObject(Book book)
        {
            return new LibraryObject
            {
                Type = "Book",
                Id = book.Id,
                Name = book.Title,
                Image = "/Content/Images/books/" + book.ImageFileName,
                CreatedDate = book.CreatedDate
            };
        }
        public static int Count()
        {
            return db.Books.Count();
        }
        public static bool GetAmazonBook(string ISBN, out ItemLookupResponse response, out string amazonXml, out Exception ex)
        {
            ex = null;
            response = null;
            amazonXml = "";
            AmazonService amzService = new AmazonService();
            int tries = 0;
            int retries = 5;
            while (true)
            {
                try
                {
                    amazonXml = amzService.SearchAmazon(ISBN, "ISBN").ToString();
                    XmlSerializer serializer = new XmlSerializer(typeof(ItemLookupResponse));
                    response = (ItemLookupResponse)serializer.Deserialize(Core.Core.GenerateStreamFromString(amazonXml));
                    return true;
                }
                catch (Exception e)
                {
                    ex = e;
                    tries += 1;
                    if (tries > retries)
                    {
                        return false;
                    }
                    Thread.Sleep(15);
                }
            }
        }

        public static bool AmazonBook(ref Book book, out Exception ex, bool edit = false)
        {
            ItemLookupResponse amzResponse;
            string amazonResponse;
            if (!GetAmazonBook(book.ISBN, out amzResponse, out amazonResponse, out ex))
            {
                return false;
            }

            try
            {
                book.AmazonResponse = amazonResponse;
                Item amzItem = amzResponse.Items.Item;
                if (amzItem == null)
                {
                    ex = new Exception("No Amazon Item returned");
                    return false;
                }
                book.AmazonProductGroup = amzItem.ItemAttributes.ProductGroup;
                book.DetailPageUrl = amzItem.DetailPageURL;
                book.Manufacturer = amzItem.ItemAttributes.Manufacturer;
                book.Title = amzItem.ItemAttributes.Title;
                book.ASIN = amzItem.ASIN;
                book.PublicationDate = amzItem.ItemAttributes.PublicationDate;
                if (book.PublicationDate.Value == DateTime.MinValue)
                {
                    book.PublicationDate = null;
                }
                book.Publisher = amzItem.ItemAttributes.Publisher;
                book.ReleaseDate = amzItem.ItemAttributes.ReleaseDate;
                if(book.ReleaseDate.Value == DateTime.MinValue)
                {
                    book.ReleaseDate = null;
                }
                string filename = ConfigurationManager.AppSettings["BookImagePath"] + "\\unknown_book.png";
                if (amzItem.LargeImage != null)
                {
                    string[] image_pieces = amzItem.LargeImage.URL.Split('/');
                    filename = image_pieces[image_pieces.Length - 1];
                    filename = book.Id.ToString().Replace("-", "") + "." + Path.GetExtension(filename);
                    string new_path = ConfigurationManager.AppSettings["BookImagePath"] + "\\" + filename;
                    if (!Core.Core.DownloadFileFromUrl(amzItem.LargeImage.URL, new_path, out ex))
                    {
                        return false;
                    }
                }
                else if(amzItem.MediumImage != null)
                {
                    string[] image_pieces = amzItem.MediumImage.URL.Split('/');
                    filename = image_pieces[image_pieces.Length - 1];
                    filename = book.Id.ToString().Replace("-", "") + "." + Path.GetExtension(filename);
                    string new_path = ConfigurationManager.AppSettings["BookImagePath"] + "\\" + filename;
                    if (!Core.Core.DownloadFileFromUrl(amzItem.MediumImage.URL, new_path, out ex))
                    {
                        return false;
                    }
                }
                else if (amzItem.SmallImage != null)
                {
                    string[] image_pieces = amzItem.SmallImage.URL.Split('/');
                    filename = image_pieces[image_pieces.Length - 1];
                    filename = book.Id.ToString().Replace("-", "") + "." + Path.GetExtension(filename);
                    string new_path = ConfigurationManager.AppSettings["BookImagePath"] + "\\" + filename;
                    if (!Core.Core.DownloadFileFromUrl(amzItem.SmallImage.URL, new_path, out ex))
                    {
                        return false;
                    }
                }
                else
                {
                    string new_filename = ConfigurationManager.AppSettings["BookImagePath"] + "\\" + book.Id.ToString().Replace("-", "") + "." + Path.GetExtension(filename);
                    File.Copy(filename, new_filename);
                    filename = new_filename;
                }
                book.ImageFileName = filename;
                book.Authors = String.Join(",", amzItem.ItemAttributes.Authors);

                //Author a = db.Authors.Find(book.AuthorId);
                //if(a == null)
                //{
                //    ex = new Exception("Author not found with ID " + book.AuthorId);
                //    return false;
                //}

                if (!edit)
                {
                    try
                    {
                        book.SortTitle = Core.Core.ApplySortTitle(book.Title);
                        db.Books.Add(book);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        ex = new Exception(JsonConvert.SerializeObject(book), e);
                        return false;
                    }
                }

                db.Database.ExecuteSqlCommand("delete from BookAuthors where BookId = '" + book.Id + "'");
                if (amzItem.ItemAttributes.Authors.Count() > 0)
                {
                    foreach (string amzAuthor in amzItem.ItemAttributes.Authors)
                    {
                        Author author;
                        if (!Authors.FindCreateAuthorByName(amzAuthor, out author, out ex))
                        {
                            return false;
                        }
                        BookAuthor bookAuthor;
                        if (!BookAuthors.CreateBookAuthor(book, author, out bookAuthor, out ex))
                        {
                            return false;
                        }
                    }

                }

                db.Database.ExecuteSqlCommand("delete from ObjectToCategories where ObjectId = '" + book.Id + "' and ObjectType = 'Book'");
                Categories.PopulateCategories<Book>(amzItem.BrowseNodes.ToList(), book);

                ex = null;
                return true;
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }
        }

        public static bool PopulateBookAuthors(Book book, out Exception ex)
        {
            ex = null;
            if (!string.IsNullOrWhiteSpace(book.Authors))
            {
                string[] authors = book.Authors.Split(',');
                foreach (string author in authors)
                {
                    Author newAuthor;
                    if (!Authors.FindCreateAuthorByName(author.Trim(), out newAuthor, out ex))
                    {
                        return false;
                    }
                    BookAuthor bookAuthor;
                    if (!BookAuthors.CreateBookAuthor(book, newAuthor, out bookAuthor, out ex))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static LibraryObject GetReading()
        {
            List<Book> books =  db.Books.Include(b => b.BookAuthors).Where(b => b.Reading == true).ToList();
            if (books.Count() > 0)
            {
                return BooksToObjects(books)[0];
            }
            return null;
        }

        public static bool Reading(Guid bookId)
        {
            Book book = db.Books.Find(bookId);
            if (book == null)
            {
                return false;
            }
            book.Reading = true;
            db.Entry(book).State = EntityState.Modified;
            db.SaveChanges();
            return true;
        }

        public static void UnSetReading()
        {
            List<Book> readBooks = db.Books.Where(b => b.Reading == true).ToList();
            foreach(Book book in readBooks)
            {
                book.Reading = false;
                db.Entry(book).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public static void ApplySortTitle()
        {
            List<Book> books = db.Books.ToList();
            foreach (Book book in books)
            {
                book.SortTitle = Core.Core.ApplySortTitle(book.Title);
                db.Entry(book).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public static List<LibraryObject> GetBooksForPerson(Guid personId)
        {
            List<LibraryObject> books = new List<LibraryObject>();
            Author author = db.Authors.Where(a => a.PersonId == personId).FirstOrDefault();
            if(author != null)
            {
                List<BookAuthor> allBooks = db.BookAuthors.Include(ba => ba.Book).Where(ba => ba.AuthorId == author.Id).ToList();
                foreach(BookAuthor book in allBooks)
                {
                    books.Add(BookToObject(book.Book));
                }
            }
            return books;
        }

    }
}
