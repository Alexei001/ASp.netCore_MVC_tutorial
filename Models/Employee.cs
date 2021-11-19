using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ASp.netCore_empty_tutorial.Models
{
    public class Employee
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50,MinimumLength =3,ErrorMessage ="Between 3 and 50 char")]
        public string Name { get; set; }
        [Required]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}",ErrorMessage ="Invalid Email Adress")]
        [Display(Name ="Office Email")]
        public string Email { get; set; }
        [Required]
        public DeptEnum? Department { get; set; }
        [Display(Name ="Image")]
        public string ImagePath { get; set; }

    }
}
