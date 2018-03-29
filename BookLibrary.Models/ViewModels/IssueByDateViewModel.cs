using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ViewModels
{
    public class IssueByDateViewModel
    {
        public Dictionary<int, Dictionary<int, IssueByMonthView>> Issues { get; set; }
    }

    public class IssueByMonthView
    {
        public string MonthName { get; set; }
        public List<MagazineIssue> Issues {get;set;}
    }
}
