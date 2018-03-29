using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.PhotoGallery
{
    public class AlbumView
    {
        public Album Album { get; set; }
        public List<Uri> Images { get; set; }
    }
}
