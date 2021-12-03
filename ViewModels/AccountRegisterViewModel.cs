using ASp.netCore_empty_tutorial.Utilities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ASp.netCore_empty_tutorial.ViewModels
{
    public class AccountRegisterViewModel
    {
        [Required]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "Invalid Email Adress")]
        [Remote(action:"IsEmailInUse",controller:"Account")]
        [ValidationEmailDomain(validationDomain:"gmail.com",ErrorMessage ="The email must be Gmail.com")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password and Confirmation Password don't match.")]
        public string ConfirmPassword { get; set; }
        public string City { get; set; }
    }
}
