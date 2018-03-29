using BookLibrary.Models.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.PhotoGallery.Requests
{
    public class ObjectRequest
    {
        public List<string> fields { get; set; }
        public List<RequestFilter> conditions { get; set; }
    }
}
