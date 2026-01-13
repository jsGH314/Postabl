using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace Models.ViewModels
{
    public class UserProfileVM
    {
        public ApplicationUser User { get; set; }
        public Profile Profile { get; set; }
        public IEnumerable<BlogPost> BlogPostList { get; set; }
        
        // Dictionary mapping post ID to display names of users who liked it
        public Dictionary<int, string> LikedByDisplayMap { get; set; } = new Dictionary<int, string>();
    }
}
