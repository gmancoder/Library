using BookLibrary.Functions.Core;
using BookLibrary.Models;
using BookLibrary.Models.ServiceModels;
using BookLibrary.Models.ServiceModels.CelebrityCentral;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Services
{
    public class CelebrityCentralService
    {
        private const string URL = "http://celebrities-api.gmancoder.com/";
        private const string musician_id = "18977988-C138-471E-9A20-98D25376F490";

        private string token = "";
        public CelebrityCentralService(string username, string password)
        {
            GetToken(username, password, out token);
        }

        private bool GetToken(string user, string pass, out string token)
        {
            string url = URL + "token";

            string data = String.Format("grant_type=password&username={0}&password={1}", user, pass);

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

        public List<Celebrity> GetCelebrities(string letter = "")
        {
            string url = URL + "api/retrieve";
            CelebrityRetrieveRequest request = new CelebrityRetrieveRequest
            {
                Entity = "celebrity",
                Fields = new List<string> { "id", "name" }
            };
            request.Conditions = new List<RequestFilter>();
            if (letter != "")
            {
                request.Conditions.Add(new RequestFilter
                {
                    field = "name",
                    @operator = "starts with",
                    values = new List<object> { { letter } }
                });
                
            }

            CelebrityRequestResponse<Celebrity> response = JsonConvert.DeserializeObject<CelebrityRequestResponse<Celebrity>>(Core.PostRemoteData(url, JsonConvert.SerializeObject(request), token));

            return response.Results;
        }

        public List<Celebrity> GetCelebrity(Guid CelebrityId)
        {
            string url = URL + "api/retrieve";
            CelebrityRetrieveRequest request = new CelebrityRetrieveRequest
            {
                Entity = "celebrity",
                Fields = new List<string> ()
            };
            request.Conditions = new List<RequestFilter>();
            request.Conditions.Add(new RequestFilter
            {
                field = "id",
                @operator = "=",
                values = new List<object> { { CelebrityId } }
            });

            CelebrityRequestResponse<Celebrity> response = JsonConvert.DeserializeObject<CelebrityRequestResponse<Celebrity>>(Core.PostRemoteData(url, JsonConvert.SerializeObject(request), token));

            return response.Results;
        }

        public List<Celebrity> FindCelebrity(string name)
        {
            string url = URL + "api/retrieve";
            CelebrityRetrieveRequest request = new CelebrityRetrieveRequest
            {
                Entity = "celebrity",
                Fields = new List<string>()
            };
            request.Conditions = new List<RequestFilter>();
            request.Conditions.Add(new RequestFilter
            {
                field = "name",
                @operator = "=",
                values = new List<object> { { name } }
            });

            CelebrityRequestResponse<Celebrity> response = JsonConvert.DeserializeObject<CelebrityRequestResponse<Celebrity>>(Core.PostRemoteData(url, JsonConvert.SerializeObject(request), token));

            return response.Results;
        }
    }
}
