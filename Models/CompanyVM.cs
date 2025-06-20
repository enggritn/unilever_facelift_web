using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Facelift_App.Models
{
    public class CompanyVM
    {
        [Required(ErrorMessage = "Company Name is required.")]
        [MinLength(2, ErrorMessage = "Company Name can not less than 2 characters.")]
        [MaxLength(50, ErrorMessage = "Company Name can not more than 50 characters.")]
        [Remote("IsUniqueName", "CompanyValidation", AdditionalFields = "x")]
        public string CompanyName { get; set; }


        [Required(ErrorMessage = "Company Abbreviation is required.")]
        [MinLength(3, ErrorMessage = "Company Abbreviation can not less than 3 characters.")]
        [MaxLength(3, ErrorMessage = "Company Abbreviation can not more than 3 characters.")]
        [Remote("IsUniqueAbb", "CompanyValidation", AdditionalFields = "x")]
        public string CompanyAbb { get; set; }

        public bool IsActive { get; set; }
    }
}