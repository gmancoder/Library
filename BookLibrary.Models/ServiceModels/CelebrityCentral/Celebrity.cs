using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BookLibrary.Models
{
    public class Celebrity
    {
        private CelebrityType _type;
        public Guid Id { get; set; }
        public Guid celebrityTypeId { get; set; }
        public CelebrityType Type
        {
            get; set;
        }
        public Guid? CrmId { get; set; }
        public int AlbumId { get; set; }
        public string Name { get; set; }
        public DateTime? Birthday { get; set; }
        public String Website { get; set; }
        [AllowHtml]
        public string Details { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public bool IsSecretCelebrity { get; set; }
    }
}
