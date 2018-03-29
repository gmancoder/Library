using BookLibrary.Models.ServiceModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.API
{
    public class RetrieveRequest
    {
        [Required]
        [Display(Name = "Entity")]
        public string Entity { get; set; }

        [Display(Name = "Fields")]
        public List<string> Fields { get; set; }

        [Display(Name = "Conditions")]
        public List<RequestFilter> Conditions { get; set; }

        [Display(Name = "Order")]
        public List<string> Order { get; set; }
        [Display(Name = "WhereOperator")]
        public string WhereOperator { get; set; }
        [Display(Name = "OrderDirection")]
        public string OrderDirection { get; set; }
    }
}
