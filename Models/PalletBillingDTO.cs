using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models
{
    public class PalletBillingDTO
    {
        public string AgingId { get; set; }
        public string PalletId { get; set; }
        public string ReceivedAt { get; set; }
        public string CurrentMonth { get; set; }
        public string CurrentYear { get; set; }
        public string PalletName { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public string PalletOwner { get; set; }
        public string PalletProducer { get; set; }
        public string WarehouseId { get; set; }
        public string PalletStatus { get; set; }
        public string TotalMinutes { get; set; }
        public string TotalHours { get; set; }
        public string TotalDays { get; set; }
        public string PalletAgeMonth { get; set; }
        public string PalletAgeYear { get; set; }
        public string BillingPrice { get; set; }
        public string TotalBilling { get; set; }
        public string PalletCondition { get; set; }
        public string LastTransactionDate { get; set; }
        public string CutOffAt { get; set; }
    }
}