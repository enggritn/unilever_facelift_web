using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models
{
    public class RecallDTO
    {
        public string TransactionId { get; set; }
        public string TransactionCode { get; set; }
        public string Remarks { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public string TransactionStatus { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedAt { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedAt { get; set; }
    }

    public class RecallItemDTO
    {
        public string TransactionItemId { get; set; }
        public string TagId { get; set; }
        public string PreviousWarehouseName { get; set; }
        public string ScannedBy { get; set; }
        public string ScannedAt { get; set; }
        public string PreviousPalletCondition { get; set; }
        public string NewPalletCondition { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedAt { get; set; }
    }
}