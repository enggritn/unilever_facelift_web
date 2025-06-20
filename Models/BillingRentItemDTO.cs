using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models
{
    public class BillingRentItemDTO
    {
        public BillingRentDTO header { get; set; }
        public string TransactionItemId { get; set; }
        public string PalletId { get; set; }
        public string PalletTypeName { get; set; }
        public string PalletOwner { get; set; }
        public string PalletProducer { get; set; }
        public string CurrentYear { get; set; }
        public string CurrentMonth { get; set; }
        public string TotalMinutes { get; set; }
        public string TotalHours { get; set; }
        public string TotalDays { get; set; }
        public string BillingYear { get; set; }
        public string BillingPrice { get; set; }
        public string TotalBilling { get; set; }
    }
}