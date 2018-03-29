using BookLibrary.Models.PhotoGallery.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ServiceModels.PDFLibrary.Requests
{
    public class PDFLibraryResponse
    {
        public ObjectRequest request { get; set; }
        public string action { get; set; }
        public string method { get; set; }
        public bool Status { get; set; }
        public string Errors { get; set; }
    }
}
