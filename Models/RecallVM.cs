using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Facelift_App.Models
{
    public class RecallVM
    {
        public string TransactionCode { get; set; }
        public string Remarks { get; set; }
        [Required(ErrorMessage = "Warehouse is required.")]
        [Remote("IsWarehouseExist", "WarehouseValidation")]
        public string WarehouseId { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }

       
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

        public List<LogRecallHeader> logs { get; set; }
        public List<LogRecallDocument> versions { get; set; }

    }

}