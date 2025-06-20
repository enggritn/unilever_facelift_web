using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Facelift_App.Models.Api
{
    public class Manual
    {
        [Required(ErrorMessage = "Tag Id is required.")]
        public string TagId { get; set; }

        [Required(ErrorMessage = "Warehouse Name is required.")]
        public string WarehouseName { get; set; }

        [Required(ErrorMessage = "Pallet Condition is required.")]
        public string PalletCondition { get; set; }

    }
}