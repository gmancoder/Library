using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.PhotoGallery.Requests
{
    public class LoadAlbumRequest
    {
        public int id { get; set; }
        public string path { get; set; }
        public int page { get; set; }
    }
}
