using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.PhotoGallery
{
    public class Album
    {
        public int ID { get; set; }
        public int CATEGORY_ID { get; set; }
        public string NAME { get; set; }
        public string ALIAS { get; set; }
        public int ANONYMOUS_ACCESS { get; set; }
        public DateTime CREATED_AT { get; set; }
        public DateTime UPDATED_AT { get; set; }
        public string PATH { get; set; }
        public int SYNC { get; set; }
        public DateTime LAST_SYNC_DATE { get; set; }
    }

}
