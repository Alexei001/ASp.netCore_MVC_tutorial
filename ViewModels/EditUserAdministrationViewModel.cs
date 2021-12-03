using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ASp.netCore_empty_tutorial.ViewModels
{
    public class EditUserAdministrationViewModel
    {
        public EditUserAdministrationViewModel()
        {
            Roles = new List<string>();
            Claims= new List<string>();
        }
        public string Id { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string City { get; set; }

        public List<string> Roles { get; set; }
        public IList<string> Claims { get; set; }
    }
}
