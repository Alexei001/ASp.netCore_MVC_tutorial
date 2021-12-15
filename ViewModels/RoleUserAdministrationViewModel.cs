using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ASp.netCore_empty_tutorial.ViewModels
{
    public class RoleUserAdministrationViewModel
    {
        public string RoleId { get; set; }
        [Display(Name = "Role Name")]
        public string RoleName { get; set; }
        public bool IsSelected { get; set; }
    }
}
