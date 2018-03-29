using BookLibrary.Models;
using System;
using System.Data;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using BookLibrary.Models.ViewModels;

namespace BookLibrary.Functions
{
    public static class Tracks
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        public static List<string> Fields
        {
            get
            {
                return new List<String> { "ArtistId", "AlbumId", "Name", "DiscNumber", "TrackNumber", "Lyrics", "CreatedDate", "ModifiedDate" };
            }
        }

        public static List<string> RequiredFields
        {
            get
            {
                return new List<String> { "ArtistId", "AlbumId", "Name", "DiscNumber", "TrackNumber" };
            }
        }
        public static bool TrackExists(string Name, Guid AlbumId, int TrackNumber, int DiscNumber)
        {
            return db.Tracks.Where(t => t.Name == Name && t.AlbumId == AlbumId && t.TrackNumber == TrackNumber && t.DiscNumber == DiscNumber).Count() > 0;
        }
        public static bool CreateTrack(string name, Album album, int trackNumber, int discNumber, out Exception ex)
        {
            Guid artistId;
            try
            {
                Match reArtist = Regex.Match(name, @" - (.*)[^\(\)\[\]]$");
                if (reArtist.Length > 0 && album.ArtistId == Artists.VariousArtists.Id)
                {
                    Artist artist;
                    string artistName = reArtist.Value.Replace(" - ", "");
                    if (!Artists.FindCreateArtistByName(artistName, out artist, out ex))
                    {
                        return false;
                    }
                    artistId = artist.Id;
                    name = name.Replace(reArtist.Groups[0].Value, "");
                }
                else
                {
                    artistId = album.ArtistId;
                }
            }
            catch
            {
                artistId = album.ArtistId;
            }

            return CreateTrack(name, artistId, album, discNumber, trackNumber, out ex);
        }

        public static bool CreateTrack(string name, Guid artistId, Album album, int discNumber, int trackNumber, out Exception ex)
        {
            ex = null;
            try
            {
                Track track = new Track();
                track.Id = Guid.NewGuid();
                track.Name = name;
                track.CreatedDate = DateTime.Now;
                track.ModifiedDate = DateTime.Now;
                track.ArtistId = artistId;
                track.AlbumId = album.Id;
                track.DiscNumber = discNumber;
                track.TrackNumber = trackNumber;
                track.Lyrics = GetLyrics(artistId, name);
                if(!String.IsNullOrEmpty(track.Lyrics))
                {
                    track.Lyrics = track.Lyrics.Replace("\n", "<br />");
                }
                db.Tracks.Add(track);
                db.SaveChanges();
                return true;
            }
            catch(Exception e)
            {
                ex = e;
                return false;
            }
        }

        public static string GetLyrics(Guid artistId, string trackName)
        {
            Artist artist = db.Artists.Find(artistId);
            string lyrics = null;
            if(artist != null)
            {
                lyrics = GetLyricsFromWikia(artist.Name, trackName);
                if (String.IsNullOrEmpty(lyrics))
                {
                    lyrics = GetLyricsFromMusixMatch(artist.Name, trackName);
                }
            }
            return lyrics;
        }

        public static string GetLyricsFromMusixMatch(string artistName, string trackName)
        {
            string lyrics = null;
            try
            {
                string url = "https://www.musixmatch.com/lyrics/" + artistName.Replace(" ", "-") + "/" + trackName.Replace(" ", "-");
                lyrics = GetLyrics(url, "p", "mxm-lyrics__content");
            }
            catch { }

            return lyrics;
        }

        public static string GetLyricsFromWikia(string artistName, string trackName)
        {
            string lyrics = null;
            try
            {
                string url = "http://lyrics.wikia.com/wiki/" + artistName.Replace(" ", "_") + ":" + trackName.Replace(" ", "_");
                lyrics = GetLyrics(url, "div", "lyricbox");
            }
            catch { }

            return lyrics;
        }

        private static string GetLyrics(string url, string elementName, string className)
        {
            string lyrics = null;
            try
            {
                var web = new HtmlWeb();
                var doc = web.Load(url);

                var nodes = doc.DocumentNode.Descendants(elementName)
                    .Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value == className)
                    .ToList();
                foreach (HtmlNode node in nodes)
                {
                    try
                    {
                        lyrics += node.InnerHtml;
                    }
                    catch { }
                }
            }
            catch (Exception ex) { }
            return lyrics;
        }

        public static TrackOfTheDay GetTrackOfTheDay()
        {
            DateTime today = DateTime.Now;
            TrackOfTheDay trackOfTheDay = db.TracksOfTheDay.Where(t => t.Date.Month == today.Month && t.Date.Year == today.Year && t.Date.Day == today.Day).FirstOrDefault();
            if (trackOfTheDay == null)
            {
                List<Track> trackList = db.Tracks.Where(t => t.Lyrics != null && t.Lyrics != String.Empty).ToList();
                if (trackList.Count() > 0)
                {
                    Random rnd = new Random();
                    int tId = rnd.Next(0, trackList.Count());
                    trackOfTheDay = new TrackOfTheDay
                    {
                        Id = Guid.NewGuid(),
                        TrackId = trackList[tId].Id,
                        Date = DateTime.Now
                    };
                    db.TracksOfTheDay.Add(trackOfTheDay);
                    db.SaveChanges();
                }
            }
            return trackOfTheDay;
        }

        public static List<LibraryObject> GetAsObject(string q)
        {
            try
            {
                List<Track> tracks = db.Tracks.Where(t => t.Name.ToLower().Contains(q.ToLower())).Include(t => t.Artist).Include(t => t.Album).OrderBy(t => t.Name).ToList();
                return GetAsObject(tracks);
            }
            catch
            {
                return new List<LibraryObject>();
            }
        }

        private static List<LibraryObject> GetAsObject(List<Track> tracks)
        {
            List<LibraryObject> objects = new List<LibraryObject>();
            try
            {
                foreach (Track track in tracks)
                {
                    LibraryObjectBy by = new LibraryObjectBy
                    {
                        ById = track.ArtistId,
                        ByText = "By",
                        ByType = "Artist",
                        ByValue = track.Artist.Name
                    };
                    objects.Add(new LibraryObject
                    {
                        Id = track.Id,
                        Name = track.Name,
                        Image = "/Content/Images/albums/" + track.Album.ImageFileName,
                        ByObjects = new List<LibraryObjectBy> { by },
                        Type = "Track"
                    });
                }
            }
            catch { }
            return objects;
        }

        public static List<Track> GetNonAlbumTracksForArtist(Guid artistId)
        {
            List<Track> NonAlbumTracks = new List<Track>();
            List<Track> artistTracks = db.Tracks.Where(t => t.ArtistId == artistId).Include(t => t.Album).ToList();
            List<Album> artistAlbums = db.Albums.Where(a => a.ArtistId == artistId).ToList();
            foreach (Track artistTrack in artistTracks)
            {
                bool found = false;
                foreach (Album artistAlbum in artistAlbums)
                {
                    if (artistTrack.AlbumId == artistAlbum.Id)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    NonAlbumTracks.Add(artistTrack);
                }
            }

            return NonAlbumTracks;
        }

        public static List<LibraryObject> GetNonAlbumTracksForPerson(Guid personId)
        {
            List<LibraryObject> NonAlbumTracks = new List<LibraryObject>();
            Artist artist = db.Artists.Where(a => a.PersonId == personId).FirstOrDefault();
            if(artist != null)
            {
                NonAlbumTracks = GetAsObject(GetNonAlbumTracksForArtist(artist.Id));
            }
            return NonAlbumTracks;
        }
    }
}
