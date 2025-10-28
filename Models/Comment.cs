/*using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        // Foreign key to BlogPost
        public int BlogPostId { get; set; }
        public BlogPost? BlogPost { get; set; }
        // Foreign key to Profile (author of the comment)
        public int ProfileId { get; set; }
        //public Profile Profile { get; set; }

        /*
        // Optional: For nested comments (replies)
        public int? ParentCommentId { get; set; } // Nullable foreign key for parent comment

        [ForeignKey("ParentCommentId")]
        public Comment ParentComment { get; set; }

        public ICollection<Comment> ChildComments { get; set; } // Collection of child comments
        
    }
}
*/
