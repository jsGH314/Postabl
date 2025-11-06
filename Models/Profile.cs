using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
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
        public int Id { get; set; }
        public string? Bio { get; set; } = string.Empty;
        //Display name for the profile
        public string? DisplayName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; } = DateTime.Now;
        public string? ProfileImageUrl { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; } = string.Empty;
        //Profiles can have many blog posts
        public List<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
        //TODO: Lists of followers and following
        //public List<Follower> Followers { get; set; } = new List<Follower>();
        //public List<Follower> Following { get; set; } = new List<Follower>();

        //Navigation property to ApplicationUser
        [ValidateNever]
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }

        //Will link to Profile entity
        public string ApplicationUserId { get; set; } = string.Empty;

        public Profile()
        {
            // Do not create a default ApplicationUser here - leave navigation property for EF and set ApplicationUserId explicitly in controllers
            
        }

    }
}
