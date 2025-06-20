using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models
{
    public class PalletDTO
    {
        public string TagId { get; set; }
        public string PalletCode { get; set; }
        public string PalletTypeId { get; set; }
        public string PalletName { get; set; }
        public string PalletCondition { get; set; }
        public string WarehouseId { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public string PalletOwner { get; set; }
        public string PalletProducer { get; set; }
        public string ProducedDate { get; set; }
        public string Description { get; set; }
        public string SapId { get; set; }
        public string RegisteredBy { get; set; }
        public string RegisteredAt { get; set; }
        public string ReceivedBy { get; set; }
        public string ReceivedAt { get; set; }
        public string PalletMovementStatus { get; set; }
        public string LastTransactionName { get; set; }
        public string LastTransactionCode { get; set; }
        public string LastTransactionDate { get; set; }
    }
}