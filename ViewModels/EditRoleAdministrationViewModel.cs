using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ASp.netCore_empty_tutorial.ViewModels
{
    public class EditRoleAdministrationViewModel
    {
        public EditRoleAdministrationViewModel()
        {
            Users = new List<string>();
        }

        public string Id { get; set; }
        [Required(ErrorMessage ="The Name field must be required")]
        public string Name { get; set; }
        public List<string> Users { get; set; } 
    }
}
