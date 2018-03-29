using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ViewModels
{
    public class UpdateRolesViewModel
    {
        public string userId { get; set; }
        public string emailAddress { get; set; }
        public List<IdentityUserRole> userRoles { get; set; }
        public List<IdentityRole> allRoles { get; set; }
    }
}
