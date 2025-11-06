using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class UserProfileVM
    {
        public ApplicationUser User { get; set; }
        public Profile Profile { get; set; }
        public IEnumerable<BlogPost> BlogPostList { get; set; }
    }
}
