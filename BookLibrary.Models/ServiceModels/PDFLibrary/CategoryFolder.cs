using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ServiceModels.PDFLibrary
{
    public class CategoryFolder : Category
    {
        public int Category_Id { get; set; }
        public int? Category_Folder_Id { get; set; }
    }
}
