using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Facelift_App.Models
{
    public class PalletTypeVM
    {
        [Required(ErrorMessage = "Pallet Name is required.")]
        [MinLength(2, ErrorMessage = "Pallet Name can not less than 2 characters.")]
        [MaxLength(50, ErrorMessage = "Pallet Name can not more than 50 characters.")]
        [Remote("IsUniqueName", "PalletTypeValidation", AdditionalFields = "x")]
        public string PalletName { get; set; }

        public string PalletDescription { get; set; }

        public bool IsActive { get; set; }
    }
}