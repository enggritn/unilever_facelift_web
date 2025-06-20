using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models
{
    public class BillingRentDTO
    {
        public string TransactionId { get; set; }
        public string TransactionCode { get; set; }
        public string AgingType { get; set; }
        public string WarehouseId { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public string CurrentYear { get; set; }
        public string CurrentMonth { get; set; }
        public string TotalBilling { get; set; }
        public string Tax { get; set; }
        public string Payment { get; set; }
        public string GrandTotal { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedAt { get; set; }
        public string StartPeriod { get; set; }
        public string LastPeriod { get; set; }
        public string Remarks { get; set; }
        public IEnumerable<BillingRentItemDTO> Items { get; set; }
    }


    public class InvoiceRentDTO
    {
        public string TransactionId { get; set; }
        public string TransactionCode { get; set; }
        public string CurrentMonth { get; set; }
        public string CurrentYear { get; set; }
        public string MGV { get; set; }
        public string Tax { get; set; }
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedAt { get; set; }
        public string TotalBilling { get; set; }
        public string GrandTotal { get; set; }

        public IEnumerable<InvoiceRentDetailDTO> list { get; set; }

        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
    }

    public class InvoiceRentDetailDTO
    {
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public string WarehouseCategory { get; set; }
        public string CurrentMonth { get; set; }
        public string CurrentYear { get; set; }
        public string TotalBillingRent { get; set; }
    }

    public class InvoiceEmailDTO
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Warehouses { get; set; }
        public string[] WarehouseIds { get; set; }
    }

    public class InvoiceEmailVM
    {
        public string UserEmail { get; set; }
        public string[] WarehouseIds { get; set; }
    }
}