using BookLibrary.Functions.Core;
using BookLibrary.Models;
using BookLibrary.Models.ServiceModels.Amazon;
using BookLibrary.Models.ViewModels;
using BookLibrary.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;

namespace BookLibrary.Functions
{
    public static class Albums
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        
        public static List<string> Fields
        {
            get
            {
                return new List<string>
                {
                    "ArtistId","NumberOfDiscs","ASIN","EAN","UPC","Title","ReleaseDate","Binding","ImageFileName","CreatedDate","ModifiedDate","EntryType","Url","AmazonResponse"
                };
            }
        }

        public static void ApplySortTitle()
        {
            List<Album> albums = db.Albums.ToList();
            foreach(Album album in albums)
            {
                album.SortTitle = Core.Core.ApplySortTitle(album.Title);
                db.Entry(album).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public static List<string> RequiredFields
        {
            get
            {
                return new List<string>
                {
                    "ArtistId","Title","EntryType"
                };
            }
        }
        public static bool AmazonAlbumExists(string ASIN)
        {
            return db.Albums.Where(a => a.ASIN == ASIN).Count() > 0;
        }
        public static bool ManualAlbumExists(string Title, Guid ArtistId)
        {
            return db.Albums.Where(a => a.Title == Title && a.ArtistId == ArtistId).Count() > 0;
        }

        public static List<LibraryObject> GetAsObject(int take = 5000)
        {
            List<Album> albums = db.Albums.Include(a => a.Artist).OrderByDescending(o => o.CreatedDate).Take(take).ToList();
            return AlbumsToObjects(albums);
        }

        public static List<LibraryObject> GetAsObject(string q)
        {
            List<Album> albums = db.Albums.Where(a => a.Title.ToLower().Contains(q.ToLower())).Include(a => a.Artist).OrderBy(a => a.Title).ToList();
            return AlbumsToObjects(albums);
        }

        private static List<LibraryObject> AlbumsToObjects(List<Album> albums)
        {
            List<LibraryObject> objects = new List<LibraryObject>();
            foreach (Album album in albums)
            {
                LibraryObjectBy by = new LibraryObjectBy
                {
                    ByText = "By",
                    ByValue = Artists.GetArtistName(album.ArtistId),
                    ByType = "Artist",
                    ById = album.ArtistId
                };
                LibraryObject albumObject = AsObject(album);
                albumObject.ByObjects = new List<LibraryObjectBy> { by };
                objects.Add(AsObject(album));
            }
            return objects;
        }

        public static LibraryObject AsObject(Album album)
        {
            
            return new LibraryObject
            {
                Type = "Album",
                Id = album.Id,
                Name = album.Title,
                Image = "/Content/Images/albums/" + album.ImageFileName,
                CreatedDate = album.CreatedDate
            };
        }

        public static int Count()
        {
            return db.Albums.Count();
        }

        public static int DetermineDiscCountFromTracks(List<Track> tracks)
        {
            int discCount = 0;
            foreach(Track track in tracks)
            {
                if(track.DiscNumber > discCount)
                {
                    discCount = track.DiscNumber;
                }
            }
            return discCount;
        }

        public static bool GetAmazonAlbum(string ASIN, out ItemLookupResponse response, out string amazonXml, out Exception ex)
        {
            ex = null;
            response = null;
            amazonXml = "";
            AmazonService amzService = new AmazonService();
            int tries = 0;
            int retries = 5;
            while (true)
            {
                try
                {
                    amazonXml = amzService.SearchAmazon(ASIN, "ASIN").ToString();
                    XmlSerializer serializer = new XmlSerializer(typeof(ItemLookupResponse));
                    response = (ItemLookupResponse)serializer.Deserialize(Core.Core.GenerateStreamFromString(amazonXml));
                    return true;
                }
                catch (Exception e)
                {
                    ex = e;
                    tries += 1;
                    if (tries > retries)
                    {
                        return false;
                    }
                    Thread.Sleep(15);
                }
            }
        }
        public static bool AmazonAlbum(ref Album album, out bool noDiscs, out Exception ex, bool edit = false)
        {
            ex = null;
            noDiscs = false;
            ItemLookupResponse amzResponse;
            string amazonResponse;
            if(!GetAmazonAlbum(album.ASIN, out amzResponse, out amazonResponse, out ex))
            {
                return false;
            }
            
            try
            {
                album.AmazonResponse = amazonResponse;
                Item amzItem = amzResponse.Items.Item;
                if (amzItem == null)
                {
                    ex = new Exception("No Amazon Item returned");
                    return false;
                }
                album.Binding = amzItem.ItemAttributes.Binding;
                album.EAN = amzItem.ItemAttributes.EAN;
                if (amzItem.Discs == null || amzItem.Discs.Count() == 0)
                {
                    noDiscs = true;
                    album.NumberOfDiscs = 1;
                }
                else
                {
                    album.NumberOfDiscs = amzItem.Discs.Count();
                }
                album.ReleaseDate = amzItem.ItemAttributes.ReleaseDate.ToShortDateString();
                album.Title = amzItem.ItemAttributes.Title;
                album.UPC = amzItem.ItemAttributes.UPC;
                album.Url = amzItem.DetailPageURL;

                //Image
                string filename = ConfigurationManager.AppSettings["AlbumImagePath"] + "\\unknown_album.png";
                if (amzItem.LargeImage != null)
                {
                    string[] image_pieces = amzItem.LargeImage.URL.Split('/');
                    filename = image_pieces[image_pieces.Length - 1];
                    filename = album.Id.ToString().Replace("-", "") + "." + Path.GetExtension(filename);
                    string new_path = ConfigurationManager.AppSettings["AlbumImagePath"] + "\\" + filename;
                    if (!Core.Core.DownloadFileFromUrl(amzItem.LargeImage.URL, new_path, out ex))
                    {
                        return false;
                    }
                }
                else if (amzItem.MediumImage != null)
                {
                    string[] image_pieces = amzItem.MediumImage.URL.Split('/');
                    filename = image_pieces[image_pieces.Length - 1];
                    filename = album.Id.ToString().Replace("-", "") + "." + Path.GetExtension(filename);
                    string new_path = ConfigurationManager.AppSettings["AlbumImagePath"] + "\\" + filename;
                    if (!Core.Core.DownloadFileFromUrl(amzItem.MediumImage.URL, new_path, out ex))
                    {
                        return false;
                    }
                }
                else if (amzItem.SmallImage != null)
                {
                    string[] image_pieces = amzItem.SmallImage.URL.Split('/');
                    filename = image_pieces[image_pieces.Length - 1];
                    filename = album.Id.ToString().Replace("-", "") + "." + Path.GetExtension(filename);
                    string new_path = ConfigurationManager.AppSettings["AlbumImagePath"] + "\\" + filename;
                    if (!Core.Core.DownloadFileFromUrl(amzItem.SmallImage.URL, new_path, out ex))
                    {
                        return false;
                    }
                }
                else
                {
                    string new_filename = ConfigurationManager.AppSettings["AlbumImagePath"] + "\\" + album.Id.ToString().Replace("-", "") + "." + Path.GetExtension(filename);
                    File.Copy(filename, new_filename);
                    filename = new_filename;
                }
                album.ImageFileName = filename;

                if (!edit)
                {
                    album.SortTitle = Core.Core.ApplySortTitle(album.Title);
                    db.Albums.Add(album);
                    db.SaveChanges();
                    
                    
             
                
                    if (!noDiscs)
                    {
                        List<Track> trackList = new List<Track>();
                        int disc = 0;
                        foreach (Disc amzDisc in amzItem.Discs)
                        {
                            disc += 1;
                            foreach (AmazonTrack amzTrack in amzDisc.TrackList)
                            {
                                if (!Tracks.CreateTrack(amzTrack.Value, album, amzTrack.Number, amzDisc.Number, out ex))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
                else
                {
                    Guid albumId = album.Id;
                    List<Track> trackList = db.Tracks.Where(t => t.AlbumId == albumId).ToList();
                    foreach(Track track in trackList)
                    {
                        if (track.ArtistId != album.ArtistId && album.ArtistId != Artists.VariousArtists.Id)
                        {
                            track.ArtistId = album.ArtistId;
                            track.ModifiedDate = DateTime.Now;
                            db.Entry(track).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }

                Categories.Cleanup("Album", album.Id);
                Categories.PopulateCategories<Album>(amzItem.BrowseNodes.ToList(), album);

                ex = null;
                return true;
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }
        }

        public static List<LibraryObject> GetAlbumsForPerson(Guid personId)
        {
            List<LibraryObject> albums = new List<LibraryObject>();
            Artist artist = db.Artists.Include(ar => ar.AlbumList).Where(a => a.PersonId == personId).FirstOrDefault();
            if(artist != null)
            {
                foreach(Album album in artist.AlbumList)
                {
                    albums.Add(AsObject(album));
                }
            }
            return albums;
        }
    }
}
