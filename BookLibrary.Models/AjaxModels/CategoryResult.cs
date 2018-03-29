using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.AjaxModels
{
    public class CategoryResult
    {
        public string ResultType { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
    }
}
