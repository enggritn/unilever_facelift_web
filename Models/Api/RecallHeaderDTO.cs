using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models.Api
{
    public class RecallHeaderDTO
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

        public IEnumerable<RecallItemDTO> items { get; set; }
    }
}