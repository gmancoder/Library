using BookLibrary.Models.PhotoGallery.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ServiceModels.PhotoGallery.Requests
{
    public class GetImagesResponse : PhotoGalleryResponse
    {
        public ImageResponse Results { get; set; }
    }
}
