using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Facelift_App.Models
{
    public class TransporterTruckVM
    {
        public string TruckId { get; set; }

        [Required(ErrorMessage = "Plate Number is required.")]
        [MaxLength(15, ErrorMessage = "Plate Number can not more than 15 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Plate Number is not valid.")]
        [Remote("IsUniquePlate", "TransporterValidation", AdditionalFields = "TruckId")]
        public string PlateNumber { get; set; }

        public bool IsActive { get; set; }

    }
}