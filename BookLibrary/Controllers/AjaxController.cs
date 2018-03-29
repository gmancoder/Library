//using BookLibrary.Functions.Celebrities;
using BookLibrary.Functions;
using BookLibrary.Models;
using BookLibrary.Models.AjaxModels;
using BookLibrary.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BookLibrary.Web.Controllers
{
    public class AjaxController : Controller
    {
        private Logger log = new Logger(typeof(AjaxController));

        [HttpPost]       
        public string HandleRequest(string ajax_action, string ajax_method)
        {
           
            AjaxRequest request = CreateRequestObject(ajax_action, ajax_method);
            log.Info(JsonConvert.SerializeObject(request));
            AjaxResponse response = ProcessRequest(ajax_action, ajax_method, request);
            log.Info(JsonConvert.SerializeObject(response, new JsonSerializerSettings()
        {
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented
        }));
            return JsonConvert.SerializeObject(response, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        private AjaxRequest CreateRequestObject(string action, string method)
        {
            AjaxRequest request = new AjaxRequest();
            request.action = action;
            request.method = method;
            request.data = new Dictionary<string, object>();
            Request.Form.CopyTo(request.data, false);
            return request;
        }

        private AjaxResponse ProcessRequest(string action, string method, AjaxRequest request)
        {
            AjaxResponse response = new AjaxResponse();
            RequestMethodStatus method_status = new RequestMethodStatus();
            CelebrityCentralService celebrityService = new CelebrityCentralService("grbrewer@gmail.com", "!Pass248word");
            PDFLibraryService pdfService = new PDFLibraryService("grbrewer@gmail.com", "!Pass248word");
            if (celebrityService.LoggedIn() && pdfService.LoggedIn())
            {
                switch (action)
                {
                    case "magazine_issue":
                    case "magazine_issues":
                        switch (method)
                        {
                            case "get":
                                Guid? magazine_issue_id = null;
                                Guid? magazine_id = null;
                                if (request.data.ContainsKey("magazine_issue_id"))
                                {
                                    Guid mi_id;
                                    if(!Guid.TryParse(request.data["magazine_issue_id"].ToString(), out mi_id))
                                    {
                                        
                                        response = new AjaxResponse(false, "Error", request, null, new Dictionary<string, string> { { "Message", "magazine_issue_id not a GUID" } });
                                        break;
                                    }
                                    magazine_issue_id = mi_id;
                                    response = new AjaxResponse(true, "OK", request, new Dictionary<string, object> { { "Issues", MagazineIssues.GetMagazineIssues(magazine_issue_id, null) } }, null);
                                    break;
                                }
                                if (request.data.ContainsKey("magazine_id"))
                                {
                                    Guid m_id;
                                    if (!Guid.TryParse(request.data["magazine_id"].ToString(), out m_id))
                                    {
                                        
                                        response = new AjaxResponse(false, "Error", request, null, new Dictionary<string, string> { { "Message", "magazine_id not a GUID" } });
                                        break;
                                    }
                                    magazine_id = m_id;
                                    response = new AjaxResponse(true, "OK", request, new Dictionary<string, object> { { "Issues", MagazineIssues.GetMagazineIssues(null, magazine_id) } }, null);
                                    break;
                                }
                                response = new AjaxResponse(false, "Error", request, null, new Dictionary<string, string> { { "Message", "magazine_id or magazine_issue_id must be specified" } });
                                
                                break;
                            case "save":
                                Guid id;
                                DateTime release_date;
                                if(!request.data.ContainsKey("id"))
                                {
                                    response = new AjaxResponse(false, "Error", request, null, new Dictionary<string, string> { { "Message", "id not specified" } });
                                    break;
                                }
                                else if(!Guid.TryParse(request.data["id"].ToString(), out id))
                                {
                                    response = new AjaxResponse(false, "Error", request, null, new Dictionary<string, string> { { "Message", "id not valid" } });
                                    break;
                                }
                                id = new Guid(request.data["id"].ToString());
                                if (!request.data.ContainsKey("release_date"))
                                {
                                    response = new AjaxResponse(false, "Error", request, null, new Dictionary<string, string> { { "Message", "release_date not specified" } });
                                    break;
                                }
                                else if (!DateTime.TryParse(request.data["release_date"].ToString(), out release_date))
                                {
                                    response = new AjaxResponse(false, "Error", request, null, new Dictionary<string, string> { { "Message", "release_date not valid" } });
                                    break;
                                }
                                release_date = Convert.ToDateTime(request.data["release_date"]);
                                if(!request.data.ContainsKey("release_date_text"))
                                {
                                    response = new AjaxResponse(false, "Error", request, null, new Dictionary<string, string> { { "Message", "release_date_text not specified" } });
                                    break;
                                }
                                string release_date_text = request.data["release_date_text"].ToString();
                                MagazineIssue issue;
                                if (MagazineIssues.SaveIssue(id, release_date, release_date_text, out issue))
                                {
                                    return new AjaxResponse(true, "OK", request, new Dictionary<string, object> { { "Issue", issue } }, null);
                                }
                                else
                                {
                                    return new AjaxResponse(false, "Error", request, null, new Dictionary<string, string> { { "Message", "Save failed" } });
                                }
                            default:
                                return new AjaxResponse(false, "Error", request, null, new Dictionary<string, string>() { { "Message", String.Format("Method {0} for Action {1} not defined", method, action) } });
                        }
                        break;
                    case "pdf":
                    case "pdfs":
                        switch(method)
                        {
                            case "get":
                                string letter = "";
                                Int32 Id;
                                if (request.data.ContainsKey("letter"))
                                {
                                    letter = request.data["letter"].ToString();
                                    response = new AjaxResponse(true, "OK", request, new Dictionary<string, object> { { "PDFs", pdfService.GetPDFsByCategory(letter, 1) } }, null);
                                    break;
                                }
                                if(request.data.ContainsKey("Id"))
                                {
                                    if(Int32.TryParse(request.data["Id"].ToString(), out Id))
                                    {
                                        Id = Convert.ToInt32(request.data["Id"].ToString());
                                        response = new AjaxResponse(true, "OK", request, new Dictionary<string, object> { { "PDFs", pdfService.GetPDFById(Id) } }, null);
                                        break;
                                    }
                                }
                                return new AjaxResponse(false, "Error", request, null, new Dictionary<string, string> { { "Message", "Id or letter must be specified" } });
                            default:
                                return new AjaxResponse(false, "Error", request, null, new Dictionary<string, string>() { { "Message", String.Format("Method {0} for Action {1} not defined", method, action) } });
                        }
                        break;
                    case "author":
                    case "authors":
                        switch(method)
                        {
                            case "get":
                                string letter = "";
                                if (request.data.ContainsKey("letter"))
                                {
                                    letter = request.data["letter"].ToString();
                                }

                                response = new AjaxResponse(true, "OK", request, new Dictionary<string, object> { { "Authors", Authors.GetAuthors(letter) } }, null);

                                break;
                            default:
                                return new AjaxResponse(false, "Error", request, null, new Dictionary<string, string>() { { "Message", String.Format("Method {0} for Action {1} not defined", method, action) } });
                        }
                        break;
                    case "celebrity":
                    case "celebrities":
                        switch (method)
                        {
                            case "get":
                                string letter = "";
                                if (request.data.ContainsKey("letter"))
                                {
                                    letter = request.data["letter"].ToString();
                                }

                                response = new AjaxResponse(true, "OK", request, new Dictionary<string, object> { { "Celebrities", celebrityService.GetCelebrities(letter) } }, null);

                                break;
                            default:
                                return new AjaxResponse(false, "Error", request, null, new Dictionary<string, string>() { { "Message", String.Format("Method {0} for Action {1} not defined", method, action) } });
                        }
                        break;
                    case "category":
                        switch (method)
                        {
                            case "load":
                                Guid? categoryId = null;
                                if (request.data.ContainsKey("category_id"))
                                {
                                    Guid category_id;
                                    if (!Guid.TryParse(request.data["category_id"].ToString(), out category_id))
                                    {
                                        return new AjaxResponse(false, "Error", request, null, new Dictionary<string, string>() { { "Message", "Category Id not a GUID" } });
                                    }
                                    categoryId = category_id;
                                }

                                response = new AjaxResponse(true, "OK", request, new Dictionary<string, object> { { "Items", Categories.GetResults(categoryId) } }, null);

                                break;
                            default:
                                return new AjaxResponse(false, "Error", request, null, new Dictionary<string, string>() { { "Message", String.Format("Method {0} for Action {1} not defined", method, action) } });
                        }
                        break;
                    case "artist":
                        switch (method)
                        {
                            case "get":
                                string letter = "";
                                if (request.data.ContainsKey("letter"))
                                {
                                    letter = request.data["letter"].ToString();
                                }

                                response = new AjaxResponse(true, "OK", request, new Dictionary<string, object> { { "Artists", Artists.GetArtists(letter) } }, null);

                                break;
                            default:
                                return new AjaxResponse(false, "Error", request, null, new Dictionary<string, string>() { { "Message", String.Format("Method {0} for Action {1} not defined", method, action) } });
                        }
                        break;
                    default:
                        response = new AjaxResponse(false, "Error", request, null, new Dictionary<string, string>() { { "Message", String.Format("Action {0} not defined", action) } });
                        break;
                }

                return response;
            }
            return new AjaxResponse(false, "Error", request, null, new Dictionary<string, string>() { { "Message", "Login Failed" } });
        }
    }
}