using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ViewModels
{
    public class PageLink
    {
        public string Text { get; set; }
        public string Url { get; set; }
        public string Target { get; set; }
        public string AdditionalArgs { get; set; }

        public PageLink()
        {

        }

        public PageLink(string text, string url, string target="", string args="")
        {
            Text = text;
            Url = url;
            Target = target;
            AdditionalArgs = args;
        }
    }
}
