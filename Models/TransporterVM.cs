using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Facelift_App.Models
{
    public class TransporterVM
    {
        [Required(ErrorMessage = "Transporter Name is required.")]
        [MinLength(2, ErrorMessage = "Transporter Name can not less than 2 characters.")]
        [MaxLength(50, ErrorMessage = "Transporter Name can not more than 50 characters.")]
        [Remote("IsUniqueName", "TransporterValidation", AdditionalFields = "x")]
        public string TransporterName { get; set; }

        public string Address { get; set; }

        [Required(ErrorMessage = "Phone is required.")]
        [MaxLength(20, ErrorMessage = "Phone can not more than 20 characters.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Phone is not valid.")]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email is not valid.")]
        [MaxLength(50, ErrorMessage = "Email can not more than 50 characters.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "PIC is required.")]
        [MaxLength(50, ErrorMessage = "PIC can not more than 50 characters.")]
        public string PIC { get; set; }

        public bool IsActive { get; set; }

        public TransporterDriverVM transporterDriverVM { get; set; }
        public TransporterTruckVM transporterTruckVM { get; set; }
        public string[] WarehouseIds { get; set; }
    }
}