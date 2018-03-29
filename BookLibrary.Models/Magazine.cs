using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models
{
    public class Magazine
    {
        public Guid Id { get; set; }
        public Int32 PdfCategoryFolderId { get; set; }
        public string Title { get; set; }
        public string SortTitle { get; set; }
        public virtual List<MagazineIssue> MagazineIssues { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
