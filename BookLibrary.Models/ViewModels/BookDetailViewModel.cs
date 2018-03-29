using BookLibrary.Models.ServiceModels.Amazon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ViewModels
{
    public class BookDetailViewModel : AmazonObjectDetailViewModel<Book>
    {
        public List<CelebrityDetail<Author>> AuthorDetail { get; set; }
    }
}
