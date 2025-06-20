using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Facelift_App.Models
{
    public class BillingRentVM
    {
        public string TransactionCode { get; set; }
        [Required(ErrorMessage = "Year is required.")]
        public int CurrentYear { get; set; }
        [Required(ErrorMessage = "Month is required.")]
        public int CurrentMonth { get; set; }
        

        [Required(ErrorMessage = "Warehouse is required.")]
        [Remote("IsWarehouseExist", "WarehouseValidation")]
        public string WarehouseId { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Tax cannot below 0.")]
        public int Tax { get; set; }

        public string CreatedBy { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:ii:ss}")]
        public DateTime CreatedAt { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
        public DateTime StartPeriod { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
        public DateTime LastPeriod { get; set; }

        public string TotalPallet { get; set; }
        public string TotalPrice { get; set; }
        public string SubTotal { get; set; }
        public string GrandTotal { get; set; }
        public int Payment { get; set; }
        [Required(ErrorMessage = "Aging Type is required.")]
        public string AgingType { get; set; }

        public string Remarks { get; set; }
    }

    public class InvoiceRentVM
    {
        public string TransactionCode { get; set; }
        [Required(ErrorMessage = "Year is required.")]
        public int CurrentYear { get; set; }
        [Required(ErrorMessage = "Month is required.")]
        public int CurrentMonth { get; set; }


    
        [Range(0, int.MaxValue, ErrorMessage = "Tax cannot below 0.")]
        public int Tax { get; set; }

        public string CreatedBy { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:ii:ss}")]
        public DateTime CreatedAt { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
        public DateTime StartPeriod { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
        public DateTime LastPeriod { get; set; }

        public string Type { get; set; }


        public string TotalPallet { get; set; }
        public string TotalPrice { get; set; }
        public string SubTotal { get; set; }
        public string GrandTotal { get; set; }

        public string Remarks { get; set; }

    }
}