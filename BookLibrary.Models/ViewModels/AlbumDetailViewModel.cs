using BookLibrary.Models.ServiceModels.Amazon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ViewModels
{
    public class AlbumDetailViewModel : AmazonObjectDetailViewModel<Album>
    {
		public int NumberOfDiscs { get; set; }
		public Celebrity ArtistDetail { get; set; }
    }
}
