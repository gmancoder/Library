using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models
{
    public class BookAuthor
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public string BookTitle { get; set; }
        public virtual Book Book { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; }
        public virtual Author Author { get; set; }
    }
}
