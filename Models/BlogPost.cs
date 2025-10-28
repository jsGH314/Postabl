using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class BlogPost
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; } = DateTime.Now;
        public string Author { get; set; } = string.Empty;
        //Visibility status- if it is set to private, only followers can see it
        //public bool IsPublic { get; set; } = true;
        public int Likes { get; set; } = 0;
        //TODO: Enable commenting on posts
        //public List<Comment> Comments { get; set; } = new List<Comment>();

        //Will link to Profile entity
        [ForeignKey("Profile")]
        public int ProfileId { get; set; }


    }
}
