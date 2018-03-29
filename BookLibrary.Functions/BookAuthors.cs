using BookLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Functions
{
    public class BookAuthors
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        public static bool CreateBookAuthor(Book book, Author authorIn, out BookAuthor authorOut, out Exception ex)
        {
            ex = null;
            authorOut = db.BookAuthors.Where(ba => ba.BookId == book.Id && ba.AuthorId == authorIn.Id).FirstOrDefault();
            if (authorOut == null)
            {
                authorOut = new BookAuthor
                {
                    Id = Guid.NewGuid(),
                    BookId = book.Id,
                    BookTitle = book.Title,
                    AuthorId = authorIn.Id,
                    AuthorName = authorIn.Person.Name
                };

                try
                {
                    db.BookAuthors.Add(authorOut);
                    db.SaveChanges();
                    return true;
                }
                catch (Exception exp)
                {
                    ex = exp;
                    return false;
                }
            }
            return true;
            
        }
    }
}
