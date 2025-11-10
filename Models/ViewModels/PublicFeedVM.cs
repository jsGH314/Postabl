using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace Models.ViewModels
{
    public class PublicFeedVM
    {
        public IEnumerable<PostCardVM> Posts { get; set; } = System.Linq.Enumerable.Empty<PostCardVM>();

        // Optional featured posts, promoted content, or pinned posts
        //public IEnumerable<BlogPost> Featured { get; set; } = System.Linq.Enumerable.Empty<BlogPost>();


        // UI metadata
        public int TotalPosts { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;

        // Optional: show whether current visitor is authenticated (useful to toggle CTA)
        public bool IsAuthenticated { get; set; }
    }
}
