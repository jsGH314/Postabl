using System;

namespace Models.ViewModels
{
    // View model for public post reading (keeps view lightweight and explicit)
    public class PostDetailsVM
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Author { get; set; }
        public DateTime PublishedDate { get; set; }
        public int Likes { get; set; }
        public bool IsPublic { get; set; }

        // Profile info for avatar + link
        public int? ProfileId { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}