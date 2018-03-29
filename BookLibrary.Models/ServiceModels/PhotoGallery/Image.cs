using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.PhotoGallery
{
    public class Image
    {
        public int ID { get; set; }
        public string NAME { get; set; }
        public int ALBUM_ID { get; set; }
        public string ALIAS { get; set; }
        public string PATH { get; set; }
        public string THUMB_PATH { get; set; }
        public int ALBUM_DEFAULT { get; set; }
        public DateTime CREATED_AT { get; set; }
        public DateTime UPDATED_AT { get; set; }
        public int DISPLAY_RANK { get; set; }
        public string EXTENSION { get; set; }
        public long FILE_SIZE { get; set; }
        public int VIEW_COUNT { get; set; }
        public DateTime? LAST_VIEWED { get; set; }
        public string MIME_TYPE { get; set; }
    }
}
