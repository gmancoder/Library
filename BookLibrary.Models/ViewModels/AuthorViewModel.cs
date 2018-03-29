using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ViewModels
{
    public class AuthorViewModel
    {
        public Author Author { get; set; }
        public List<BookAuthor> Books { get; set; }
        public Celebrity Celebrity { get; set; }
    }
}
