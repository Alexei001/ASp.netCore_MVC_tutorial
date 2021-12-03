using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ASp.netCore_empty_tutorial.Utilities
{
    public class ValidationEmailDomainAttribute : ValidationAttribute

    {
        private readonly string validationDomain;

        public ValidationEmailDomainAttribute(string validationDomain)
        {
            this.validationDomain = validationDomain;
        }
        public override bool IsValid(object? value)
        {
            string[] arrStrings = value.ToString().Split('@');
            return arrStrings[1].ToUpper() == validationDomain.ToUpper();
        }
    }
}
