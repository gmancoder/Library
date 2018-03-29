using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models
{
    public class Book
    {
        public Guid Id { get; set; }
        //public Guid AuthorId { get; set; }
        //public virtual Author Author { get; set; }
        public string ISBN { get; set; }
        public string EntryType { get; set; }
        public string Title { get; set; }
        public string SortTitle { get; set; }
        public string Manufacturer { get; set; }
        public string AmazonProductGroup { get; set; }
        public string DetailPageUrl { get; set; }
        public string ImageFileName { get; set; }
        public bool Reading { get; set; }
        public string AmazonResponse { get; set; }
        public int? PdfId { get; set; }
        public DateTime? PublicationDate { get; set; }
        public string Publisher { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string ASIN { get; set; }
        public string Authors { get; set; }
        public virtual List<BookAuthor> BookAuthors { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
