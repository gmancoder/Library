using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ServiceModels.TheTVDb
{
    public class SeriesActor
    {
        public Int32 id { get; set; }
        public string image { get; set; }
        public string imageAdded { get; set; }
        public Int32? imageAuthor { get; set; }
        public string lastUpdated { get; set; }
        public string name { get; set; }
        public string role { get; set; }
        public Int32? seriesId { get; set; }
        public Int32? sortOrder { get; set; }
    }
}
