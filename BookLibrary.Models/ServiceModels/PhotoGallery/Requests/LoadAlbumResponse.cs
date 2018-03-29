using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.PhotoGallery.Requests
{
    public class LoadAlbumResponse : PhotoGalleryResponse
    {
        public LoadAlbumResponseResult Results { get; set; }
    }
}
