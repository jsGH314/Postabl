/*using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Follower
    {
        //Linkning profiles for followers and following//

        [Required]
        [ForeignKey("Profile")]
        //Profile who is doing the following
        public int Id { get; set; } //Follower Id
        [Required]
        public Profile FollowerProfile { get; set; }

        //The profile that is following   
        [Required]
        [ForeignKey("Profile")]
        public int ProfileId { get; set; } //Profile being followed
        [Required]
        public Profile FollowedProfile { get; set; }


    }
}*/
