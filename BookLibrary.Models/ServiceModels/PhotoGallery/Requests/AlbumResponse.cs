﻿using BookLibrary.Models.PhotoGallery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ServiceModels.PhotoGallery.Requests
{
    public class AlbumResponse
    {
        public List<BookLibrary.Models.PhotoGallery.Album> albums { get; set; }
    }
}
