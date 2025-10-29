using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Models.ViewModels
{
    public class RoleManagementVM
    {
        public ApplicationUser User { get; set; }
        public IEnumerable<SelectListItem> RoleList { get; set; }
    }
}
