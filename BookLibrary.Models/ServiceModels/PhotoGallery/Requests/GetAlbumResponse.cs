using BookLibrary.Models.ServiceModels.PhotoGallery.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.PhotoGallery.Requests
{
    public class GetAlbumResponse : PhotoGalleryResponse
    {
        public AlbumResponse Results { get; set; }
    }
}
