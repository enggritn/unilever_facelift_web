using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models
{
    
    public class InvoiceDeffectDTO
    {
        public string TransactionId { get; set; }
        public string TransactionCode { get; set; }
        public string CurrentMonth { get; set; }
        public string CurrentYear { get; set; }
        public string Allowance { get; set; }
        public string TotalPallet { get; set; }
        public string CurrentDeffect { get; set; }
        public string PreviousDeffect { get; set; }
        public string TotalDeffect { get; set; }
        public string AllowanceQty { get; set; }
        public string TotalExceed { get; set; }
        public string LastExceed { get; set; }
        public string ExceedQty { get; set; }
        public string PricePerPallet { get; set; }
        public string Tax { get; set; }
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedAt { get; set; }
        public string TotalBilling { get; set; }
        public string GrandTotal { get; set; }
        public string TotalDamage { get; set; }
        public string TotalLoss { get; set; }

        //public IEnumerable<InvoiceRentDetailDTO> list { get; set; }

        //public string WarehouseCode { get; set; }
        //public string WarehouseName { get; set; }
        public bool pdfButton { get; set; }
        public bool excelButton { get; set; }
    }

    public class InvoiceDeffectItemDTO
    {
        public string TransactionItemId { get; set; }
        public string TransactionId { get; set; }
        public string PalletId { get; set; }
        public string DeffectType { get; set; }
    }

    //public class InvoiceRentDetailDTO
    //{
    //    public string WarehouseCode { get; set; }
    //    public string WarehouseName { get; set; }
    //    public string WarehouseCategory { get; set; }
    //    public string CurrentMonth { get; set; }
    //    public string CurrentYear { get; set; }
    //    public string TotalBillingRent { get; set; }
    //}

    //public class InvoiceEmailDTO
    //{
    //    public string Id { get; set; }
    //    public string Email { get; set; }
    //    public string Warehouses { get; set; }
    //    public string[] WarehouseIds { get; set; }
    //}

    //public class InvoiceEmailVM
    //{
    //    public string UserEmail { get; set; }
    //    public string[] WarehouseIds { get; set; }
    //}
}