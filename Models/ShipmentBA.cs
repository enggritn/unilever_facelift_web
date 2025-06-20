using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Facelift_App.Models
{
    public class ShipmentBA
    {
        public ShipmentVM shipment { get; set; }
        public string TransactionCode { get; set; }
        public string RefNumber { get; set; }
        public string Remarks { get; set; }

        [Required(ErrorMessage = "Warehouse is required.")]
        [Remote("IsWarehouseExist", "WarehouseValidation")]
        public string WarehouseId { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }


        public string AccidentType { get; set; }

        public string TransactionStatus { get; set; }


        public string CreatedBy { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:ii:ss}")]
        public DateTime CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:ii:ss}")]
        public DateTime? ModifiedAt { get; set; }
        public string ApprovedBy { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:ii:ss}")]
        public DateTime? ApprovedAt { get; set; }

        public List<LogAccidentHeader> logs { get; set; }
    }
}