using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ServiceModels.PDFLibrary
{
    public class Pdf
    {
        public int Id { get; set; }
        public int Category_Id { get; set; }
        public int Category_Folder_Id { get; set; }
        public string Name { get; set; }
        public string Cover_Path { get; set; }
        public int View_Count { get; set; }
        public string FileName { get; set; }
        public bool Reading { get; set; }
        public DateTime? Last_Request_Date { get; set; }
        public string Reader_IP { get; set; }
        public DateTime? Last_View_Date { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }

    }
}
