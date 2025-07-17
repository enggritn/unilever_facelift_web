using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models
{
    public class AccidentDTO
    {
        public string TransactionId { get; set; }
        public string TransactionCode { get; set; }
        public string RefNumber { get; set; }
        public string AccidentType { get; set; }
        public string Remarks { get; set; }
        public string WarehouseName { get; set; }
        public string TransactionStatus { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedAt { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedAt { get; set; }
        public string Qty { get; set; }
        public string Aging { get; set; }
        public string Day { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string ReasonType { get; set; }
        public string ReasonName { get; set; }
        public IEnumerable<AccidentItemDTO> Items { get; set; }
    }
}