using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models
{
    public class TVShow
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string SortTitle { get; set; }
        public Int32 TVDbId { get; set; }
        public string TVDbResponse { get; set; }
        public string DisplayImage { get; set; }
        public string Url { get; set; }
        public string FirstAired { get; set; }
        public string Genres { get; set; }
        public string Network { get; set; }
        public string Overview { get; set; }
        public string Rating { get; set; }
        public string Runtime { get; set; }
        public string Status { get; set; }
        public string Stars { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public virtual List<TVShowToTVStar> TVStars { get; set; }
    }
}
