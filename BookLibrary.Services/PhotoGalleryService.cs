using BookLibrary.Functions.Core;
using BookLibrary.Models.PhotoGallery;
using BookLibrary.Models.PhotoGallery.Requests;
using BookLibrary.Models.ServiceModels;
using BookLibrary.Models.ServiceModels.PhotoGallery.Requests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BookLibrary.Services
{
    public class PhotoGalleryService
    {
        private const string URL = "http://gallery.gmancoder.com/";
        private const string ROOT = "/var/www/html/gmancoder.com/subdomains/gallery/public";
        private const int FLOOD_CNT = 100;
        private const int FLOOD_WAIT = 16;

        private int flood_idx = 0;
        private string token = "";
        public PhotoGalleryService(string username, string password)
        {
            FloodControl();
            GetToken(username, password, out token);
        }

        private bool GetToken(string user, string pass, out string token)
        {
            string url = URL + "oauth/token";

            string client_id = ConfigurationManager.AppSettings["PhotoGalleryClientId"];
            string client_secret = ConfigurationManager.AppSettings["PhotoGalleryClientSecret"];

            string data = String.Format("grant_type=password&client_id={0}&client_secret={1}&username={2}&password={3}", client_id, client_secret, user, pass);

            try
            {
                string tokenResponse = Core.PostRemoteData(url, data);
                TokenResponse response = JsonConvert.DeserializeObject<TokenResponse>(tokenResponse);
                token = response.access_token;
                return true;
            }
            catch (Exception ex)
            {
                token = "";
                return false;
            }

        }

        public bool LoggedIn()
        {
            return token != "";
        }

        private string RetrieveRequest(string action, List<string> fields, List<RequestFilter> conditions)
        {
            string url = URL + "api/" + action + "/get";
            ObjectRequest request = new ObjectRequest
            {
                fields = fields,
                conditions = conditions
            };
            FloodControl();
            return Core.PostRemoteData(url, JsonConvert.SerializeObject(request), token);
        }

        public BookLibrary.Models.PhotoGallery.Album GetAlbum(int albumId)
        {
            RequestFilter filter = new RequestFilter
            {
                field = "id",
                @operator = "=",
                values = new List<object>() { albumId }
            };
            string response = RetrieveRequest("album", new List<string>(), new List<RequestFilter>() { filter });

            GetAlbumResponse resp = JsonConvert.DeserializeObject<GetAlbumResponse>(response);
            if (resp.Results.albums.Count == 0)
            {
                return resp.Results.albums[0];
            }
            return new Album();
        }

        public int FindAlbum(string name)
        {
            RequestFilter filter = new RequestFilter
            {
                field = "name",
                @operator = "=",
                values = new List<object>() { name }
            };
            string response = RetrieveRequest("album", new List<string>(), new List<RequestFilter>() { filter });
            GetAlbumResponse resp = JsonConvert.DeserializeObject<GetAlbumResponse>(response);
            if (resp.Results.albums.Count() == 0)
            {
                return 0;
            }
            else if (resp.Results.albums.Count() > 1)
            {
                return 0 - resp.Results.albums.Count();
            }
            return resp.Results.albums[0].ID;
        }

        private void FloodControl()
        {
            flood_idx += 1;
            if (flood_idx >= FLOOD_CNT)
            {
                Thread.Sleep(FLOOD_WAIT);
                flood_idx = 0;
            }
        }

        public List<Uri> LoadAlbum(int albumId, int page = 1)
        {
            string url = URL + "api/album/load";
            LoadAlbumRequest request = new LoadAlbumRequest
            {
                id = albumId,
                page = page,
                path = "/" + albumId
            };
            FloodControl();
            string resp = Core.PostRemoteData(url, JsonConvert.SerializeObject(request), token);
            LoadAlbumResponse response = JsonConvert.DeserializeObject<LoadAlbumResponse>(resp);
            List<Uri> images = new List<Uri>();
            if (response.Status)
            {
                foreach (LoadAlbumResponseResultObject copied in response.Results.Copied)
                {
                    images.Add(new Uri(copied.Dest.Replace(ROOT, URL)));
                }
            }

            return images;
        }

        public List<Uri> GetImages(int albumId)
        {
            List<Uri> images = new List<Uri>();
            RequestFilter filter = new RequestFilter
            {
                field = "album_id",
                @operator = "=",
                values = new List<object>() { albumId }
            };
            string resp = RetrieveRequest("image", new List<string>(), new List<RequestFilter>() { filter });
            GetImagesResponse response = JsonConvert.DeserializeObject<GetImagesResponse>(resp);
            if (response != null)
            {
                foreach (Image image in response.Results.images.OrderBy(i => i.DISPLAY_RANK))
                {
                    images.Add(new Uri(image.THUMB_PATH.Replace(ROOT, URL)));
                }
            }
            return images;
        }

        public Uri GetPrimaryImage(int albumId)
        {
            RequestFilter albumFilter = new RequestFilter
            {
                field = "album_id",
                @operator = "=",
                values = new List<object>() { albumId }
            };
            RequestFilter imageFilter = new RequestFilter
            {
                field = "display_rank",
                @operator = "=",
                values = new List<object>() { 1 }
            };
            string resp = RetrieveRequest("image", new List<string>(), new List<RequestFilter>() { albumFilter, imageFilter });
            GetImagesResponse response = JsonConvert.DeserializeObject<GetImagesResponse>(resp);

            if(response != null)
            {
                return new Uri(response.Results.images[0].THUMB_PATH.Replace(ROOT, URL));
            }
            return null;
        }
    }
}
