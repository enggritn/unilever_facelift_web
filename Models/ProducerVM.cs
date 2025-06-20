using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Facelift_App.Models
{
    public class ProducerVM
    {
        [Required(ErrorMessage = "Producer Name is required.")]
        [MinLength(2, ErrorMessage = "Producer Name can not less than 2 characters.")]
        [MaxLength(50, ErrorMessage = "Producer Name can not more than 50 characters.")]
        [Remote("IsUniqueName", "ProducerValidation", AdditionalFields = "x")]
        public string ProducerName { get; set; }

        public bool IsActive { get; set; }
    }
}