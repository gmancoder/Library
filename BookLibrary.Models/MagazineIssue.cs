using System;

namespace BookLibrary.Models
{
    public class MagazineIssue
    {
        public Guid Id { get; set; }
        public Guid MagazineId { get; set; }
        public virtual Magazine Magazine { get; set; }
        public string PdfTitle { get; set; }
        public DateTime ReleaseDate { get; set; }
        public Int32 PdfId { get; set; }
        public string ImageFileName { get; set; }
        public string ReleaseDateText { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}