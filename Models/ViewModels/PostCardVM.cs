using Models;

namespace Models.ViewModels
{
    public class PostCardVM
    {
        public BlogPost Post { get; set; } = new BlogPost();
        public int ProfileId { get; set; }
        public Profile Profile { get; set; } = new Profile();
        public string ProfileImageUrl { get; set; } = "/images/placeholder-profile.png";
        // New: full list of display names resolved from LikedBy user ids
        public List<string> LikedByDisplayNames { get; set; } = new List<string>();

        // New: friendly summary string for views (e.g. "alice, bob and 3 others")
        public string LikedByDisplay { get; set; } = string.Empty;
        public (List<string> Names, string Summary) LikedByDetails;
    }
}