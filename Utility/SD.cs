using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public static class SD
    {
        //Has full control of the site. Can manage users and content.
        public const string Role_Admin = "Admin";
        //Can manage content but has limited user management capabilities.
        //Mainly focuses on moderating user-generated content in groups, forums, etc. (coming soon)
        public const string Role_Moderator = "Moderator";
        //Regular user with standard access to site features.
        public const string Role_User = "User";
    }
}
