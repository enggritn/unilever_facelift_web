using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Facelift_App.Models.Api
{
    public class InspectionHeaderVM
    {
        public string TransactionId { get; set; }
        [Required(ErrorMessage = "Damage Classification is required.")]
        public string Classification { get; set; }
        [Required(ErrorMessage = "By WHOM is required.")]
        public string PIC { get; set; }
        [Required(ErrorMessage = "Remarks is required.")]
        public string Remarks { get; set; }
        //public List<InspectionItemVM> items { get; set; }
        public List<string> items { get; set; }

    }

    public class InspectionItemVM
    {
        public string TagId { get; set; }
    }

    public class InspectionApprovalVM
    {
        public string Status { get; set; }
        public string TransactionId { get; set; }
    }

    public class InspectionPICVM
    {
        public string PIC { get; set; }
        public string TransactionId { get; set; }
    }

    public class InspectionClassificationVM
    {
        public string Classification { get; set; }
        public string TransactionId { get; set; }
    }
}