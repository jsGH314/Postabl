using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Profile
    {
        //Profile Id
        [Key]
        public int Id { get; set; }
        public string Bio { get; set; } = string.Empty;
        //Display name for the profile
        public string DisplayName { get; set; } = string.Empty;
        //Profiles can have many blog posts
        public List<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
        //TODO: Lists of followers and following
        //public List<Follower> Followers { get; set; } = new List<Follower>();
        //public List<Follower> Following { get; set; } = new List<Follower>();

        //Will link to User entity
        //Users can only have one profile
        [ForeignKey("User")]
        public int UserId { get; set; }

    }
}
