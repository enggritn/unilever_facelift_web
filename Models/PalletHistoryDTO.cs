using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models
{
    public class PalletHistoryDTO
    {
        public string TagId { get; set; }
        public string PalletName { get; set; }
        public string PalletOwner { get; set; }
        public string PalletProducer { get; set; }
        public string ProducedDate { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public string TransactionCode { get; set; }
        public string TransactionType { get; set; }
        public string TransactionStatus { get; set; }
        public string TransactionDate { get; set; }
        public string ScannedDate { get; set; }
        public string ScannedBy { get; set; }
    }

    public class PalletDeffectHistoryDTO
    {
        public string TagId { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public string ScannedAt { get; set; }
        public string ScannedBy { get; set; }
        public string TransactionCode { get; set; }
        public string PalletCondition { get; set; }
    }
}