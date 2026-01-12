using System;
using System.Collections.Generic;

namespace Models.ViewModels
{
    // View model for public post reading (keeps view lightweight and explicit)
    public class PostDetailsVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public int Likes { get; set; }
        public bool IsPublic { get; set; }

        // Profile info for avatar + link
        public int? ProfileId { get; set; }
        public string? ProfileImageUrl { get; set; }

        // New: full list of display names resolved from LikedBy user ids
        public List<string> LikedByDisplayNames { get; set; } = new List<string>();

        // New: friendly summary string for views (e.g. "alice, bob and 3 others")
        public string LikedByDisplay { get; set; } = string.Empty;
    }
}