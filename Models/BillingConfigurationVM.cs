using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Facelift_App.Models
{
    public class BillingConfigurationVM
    {
        [Required(ErrorMessage = "Depreciation Year required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Depreciation Year cannot below 1.")]
        [Remote("IsUniqueYear", "BillingValidation", AdditionalFields = "x")]
        public string BillingYear { get; set; }
        [Required(ErrorMessage = "Billing Price required.")]
        public string BillingPrice { get; set; }
        public bool IsActive { get; set; }
    }
}