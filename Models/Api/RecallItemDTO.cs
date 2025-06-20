using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models.Api
{
    public class RecallItemDTO
    {
        public string TransactionItemId { get; set; }
        public string TransactionId { get; set; }
        public string TagId { get; set; }
        public string PreviousWarehouseId { get; set; }
        public string PreviousWarehouseCode { get; set; }
        public string PreviousWarehouseName { get; set; }
        public string ScannedBy { get; set; }
        public string ScannedAt { get; set; }
        public string PreviousPalletCondition { get; set; }
        public string NewPalletCondition { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedAt { get; set; }
    }
}