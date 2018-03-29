using BookLibrary.Functions.Core;
using BookLibrary.Models.PhotoGallery.Requests;
using BookLibrary.Models.ServiceModels;
using BookLibrary.Models.ServiceModels.PDFLibrary;
using BookLibrary.Models.ServiceModels.PDFLibrary.Requests;
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
    public class PDFLibraryService
    {
        private const string URL = "http://pdflib.gmancoder.com/";
        private const string ROOT = "/var/www/html/gmancoder.com/subdomains/pdflib/public";
        //private const string URL = "http://192.168.1.131:8000/";
        //private const string ROOT = "/projects/P01942/PDF_Library_2_0_Development/pdflib/public";
        private const int FLOOD_CNT = 100;
        private const int FLOOD_WAIT = 16;

        private int flood_idx = 1;
        private string token = "";
        public PDFLibraryService(string username, string password)
        {
            FloodControl();
            GetToken(username, password, out token);
        }

        public string ExternalUrl
        {
            get { return URL; }
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

        private bool GetToken(string user, string pass, out string token)
        {
            string url = URL + "oauth/token";

            string client_id = ConfigurationManager.AppSettings["PDFLibraryClientId"];
            string client_secret = ConfigurationManager.AppSettings["PDFLibraryClientSecret"];

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

        public List<Pdf> GetPDFsByCategory(string letter, int category_id)
        {
            List<Pdf> books = new List<Pdf>();
            List<RequestFilter> conditions = new List<RequestFilter>();
            RequestFilter condition = new RequestFilter
            {
                field = "category_id",
                @operator = "=",
                values = new List<object> { category_id }
            };
            conditions.Add(condition);
            condition = new RequestFilter
            {
                field = "name",
                @operator = "like",
                values = new List<object> { letter + "%" }
            };
            conditions.Add(condition);
            RetrieveResponse<PDFResponse> response = JsonConvert.DeserializeObject<RetrieveResponse<PDFResponse>>(RetrieveRequest("pdfs", new List<string>(), conditions));
            return response.Results.pdfs;
        }

        public List<Pdf> GetPDFById(Int32 id)
        {
            RequestFilter condition = new RequestFilter
            {
                field = "id",
                @operator = "=",
                values = new List<object> { id }
            };
            RetrieveResponse<PDFResponse> response = JsonConvert.DeserializeObject<RetrieveResponse<PDFResponse>>(RetrieveRequest("pdfs", new List<string>(), new List<RequestFilter> { condition }));

            return response.Results.pdfs;
        }

        public bool PdfExists(Int32 pdfId)
        {
            return GetPDFById(pdfId).Count() > 0;
        }

        public List<Pdf> GetPDFsByPath(string path)
        {
            RequestFilter condition = new RequestFilter
            {
                field = "filename",
                @operator = "like",
                values = new List<object> { path + "/%" }
            };
            RetrieveResponse<PDFResponse> response = JsonConvert.DeserializeObject<RetrieveResponse<PDFResponse>>(RetrieveRequest("pdfs", new List<string>(), new List<RequestFilter> { condition }));

            return response.Results.pdfs;
        }

        public List<Category> GetCategoryById(Int32 id)
        {
            RequestFilter condition = new RequestFilter
            {
                field = "id",
                @operator = "=",
                values = new List<object> { id }
            };
            RetrieveResponse<CategoryResponse> response = JsonConvert.DeserializeObject<RetrieveResponse<CategoryResponse>>(RetrieveRequest("categories", new List<string>(), new List<RequestFilter> { condition }));

            return response.Results.categories;
        }

        public List<CategoryFolder> GetCategoryFolderById(Int32 id)
        {
            RequestFilter condition = new RequestFilter
            {
                field = "id",
                @operator = "=",
                values = new List<object> { id }
            };
            RetrieveResponse<CategoryFolderResponse> response = JsonConvert.DeserializeObject<RetrieveResponse<CategoryFolderResponse>>(RetrieveRequest("category_folders", new List<string>(), new List<RequestFilter> { condition }));

            return response.Results.category_folders;
        }

        public Category GetCategoryByName(string category_name)
        {
            RequestFilter condition = new RequestFilter
            {
                field = "name",
                @operator = "=",
                values = new List<object> { category_name }
            };

            RetrieveResponse<CategoryResponse> response = JsonConvert.DeserializeObject<RetrieveResponse<CategoryResponse>>(RetrieveRequest("categories", new List<string>(), new List<RequestFilter> { condition }));
            if(response.Status && response.Results.categories.Count() > 0)
            {
                return response.Results.categories[0];
            }
            return null;
        }

        public CategoryFolder GetCategoryFolderByName(int category_id, string folder_name)
        {
            List<RequestFilter> conditions = new List<RequestFilter>();
            RequestFilter condition = new RequestFilter
            {
                field = "category_id",
                @operator = "=",
                values = new List<object> { category_id }
            };
            conditions.Add(condition);
            condition = new RequestFilter
            {
                field = "name",
                @operator = "=",
                values = new List<object> { folder_name }
            };
            conditions.Add(condition);
            RetrieveResponse<CategoryFolderResponse> response = JsonConvert.DeserializeObject<RetrieveResponse<CategoryFolderResponse>>(RetrieveRequest("category_folders", new List<string>(), conditions));
            if (response.Status && response.Results.category_folders.Count() > 0)
            {
                return response.Results.category_folders[0];
            }
            return null;
        }
    }
}
