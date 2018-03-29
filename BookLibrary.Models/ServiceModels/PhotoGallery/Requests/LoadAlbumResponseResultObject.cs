using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.PhotoGallery.Requests
{
    public class LoadAlbumResponseResultObject
    {
        public string Source { get; set; }
        public string Dest { get; set; }
        public string Error { get; set; }
    }
}
