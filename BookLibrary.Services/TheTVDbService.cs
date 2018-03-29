using BookLibrary.Functions;
using BookLibrary.Functions.Core;
using BookLibrary.Models.ServiceModels.TheTVDb;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BookLibrary.Services
{
    public class TheTVDbService
    {
        private const string APIKEY = "CD75B3258A02D1AC";
        private const string APIROOT = "https://api.thetvdb.com";
        private string token = "";
        public TheTVDbService(string userName, string userKey)
        {
            TVDbTokenRequest request = new TVDbTokenRequest
            {
                apikey = APIKEY,
                username = userName,
                userkey = userKey
            };

            string url = APIROOT + "/login";
            try
            {
                TVDbTokenResponse response = JsonConvert.DeserializeObject<TVDbTokenResponse>(Core.PostRemoteData(url, JsonConvert.SerializeObject(request)));
                if (response != null)
                {
                    token = response.token;
                }
            }
            catch { }
                 
        }

        public bool LoggedIn()
        {
            return token != "";
        }

        public SearchResponse SearchForShow(string showName)
        {
            string url = APIROOT + "/search/series?name=" + HttpUtility.UrlEncode(showName);
            try
            {
                return JsonConvert.DeserializeObject<SearchResponse>(Core.PostRemoteData(url, "", token, "GET"));
            }
            catch
            {
                return null;
            }
        }

        public SeriesResponse GetShowData(int id)
        {
            string url = APIROOT + "/series/" + id;
            try
            {
                return JsonConvert.DeserializeObject<SeriesResponse>(Core.PostRemoteData(url, "", token, "GET"));
            }
            catch
            {
                return null;
            }
        }

        public SeriesActorResponse GetShowActors(int id)
        {
            string url = APIROOT + "/series/" + id + "/actors";
            try
            {
                return JsonConvert.DeserializeObject<SeriesActorResponse>(Core.PostRemoteData(url, "", token, "GET"));
            }
            catch
            {
                return null;
            }
        }

        public SeriesEpisodeSummaryResponse GetEpisodeSummary(int id)
        {
            string url = APIROOT + "/series/" + id + "/episodes/summary";
            try
            {
                return JsonConvert.DeserializeObject<SeriesEpisodeSummaryResponse>(Core.PostRemoteData(url, "", token, "GET"));
            }
            catch
            {
                return null;
            }
        }

        public SeriesImageResponse GetSeriesImages (int id, string keyType = "")
        {
            string url = APIROOT + "/series/" + id + "/images/query";
            if(keyType != "")
            {
                url += "?keyType=" + keyType;
            }
            try
            {
                return JsonConvert.DeserializeObject<SeriesImageResponse>(Core.PostRemoteData(url, "", token, "GET"));
            }
            catch
            {
                return null;
            }
        }
    }
}
