using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.PhotoGallery.Requests
{
    public class LoadAlbumResponseResult
    {
        public string Path { get; set; }
        public int Page { get; set; }
        public List<LoadAlbumResponseResultObject> Copied { get; set; }
        public List<LoadAlbumResponseResultObject> Errors { get; set; }

    }
}
