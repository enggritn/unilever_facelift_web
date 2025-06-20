using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models
{
    public class InspectionHeaderDTO
    {
        public string TransactionId { get; set; }
        public string TransactionCode { get; set; }
        public string RefNumber { get; set; }
        public string Type { get; set; }
        public string PIC { get; set; }
        public string Classification { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public string Remarks { get; set; }
        public string TransactionStatus { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedAt { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedAt { get; set; }
        public string ApprovalStatus { get; set; }
    }

    public class InspectionItemDTO
    {
        public string TransactionItemId { get; set; }
        public string TransactionId { get; set; }
        public string TransactionCode { get; set; }
        public string OriginId { get; set; }
        public string OriginCode { get; set; }
        public string OriginName { get; set; }
        public string TagId { get; set; }
        public string Classification { get; set; }
        public string PIC { get; set; }
        public string ScannedBy { get; set; }
        public string ScannedAt { get; set; }
        public string WarehouseId { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public string VerifiedBy { get; set; }
        public string VerifiedAt { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedAt { get; set; }
    }
}