using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.PhotoGallery.Requests
{
    public class PhotoGalleryResponse
    {
        public ObjectRequest request { get; set; }
        public string action { get; set; }
        public string method { get; set; }
        public bool Status { get; set; }
        public string Errors { get; set; }

    }
}
