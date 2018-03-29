using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ViewModels
{
    public class ArtistViewModel
    {
        public Artist Artist { get; set; }
        public List<LibraryObject> AssociatedArtists { get; set; }
        public Celebrity Celebrity { get; set; }
        public List<Album> Albums { get; set; }
        public List<Track> NonAlbumTracks { get; set; }
    }
}
