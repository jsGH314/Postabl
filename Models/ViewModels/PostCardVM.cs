using Models;

namespace Models.ViewModels
{
    public class PostCardVM
    {
        public BlogPost Post { get; set; } = new BlogPost();
        public int ProfileId { get; set; }
        public Profile Profile { get; set; } = new Profile();
        public string ProfileImageUrl { get; set; } = "/images/placeholder-profile.png";
    }
}