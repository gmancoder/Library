using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.AjaxModels
{
    public class AjaxResponse
    {
        public AjaxResponse(bool s, string type, AjaxRequest req, Dictionary<string, object> resp, Dictionary<string, string> err)
        {
            Status = s;
            StatusType = type;
            Request = req;
            Results = resp;
            Errors = new List<Dictionary<string, string>>();
            Errors.Add(err);
        }

        public AjaxResponse() { }
        public bool Status { get; set; }
        public string StatusType { get; set; }
        public AjaxRequest Request { get; set; }
        public Dictionary<string, object> Results { get; set; }
        public List<Dictionary<string, string>> Errors { get; set; }
    }
}
