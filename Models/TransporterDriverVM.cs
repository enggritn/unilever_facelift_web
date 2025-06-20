using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Facelift_App.Models
{
    public class TransporterDriverVM
    {
        public string DriverId { get; set; }

        [Required(ErrorMessage = "Driver Name is required.")]
        [MinLength(2, ErrorMessage = "Driver Name can not less than 2 characters.")]
        [MaxLength(50, ErrorMessage = "Driver Name can not more than 50 characters.")]
        public string DriverName { get; set; }


        [Required(ErrorMessage = "Phone is required.")]
        [MaxLength(20, ErrorMessage = "Phone can not more than 20 characters.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Phone is not valid.")]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "License Number is required.")]
        [MaxLength(30, ErrorMessage = "License Number can not more than 30 characters.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "License Number is not valid.")]
        [Remote("IsUniqueLicense", "TransporterValidation", AdditionalFields = "DriverId")]
        public string LicenseNumber { get; set; }

        public bool IsActive { get; set; }
    }
}