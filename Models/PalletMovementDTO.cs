using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models
{
    public class PalletMovementDTO
    {
        public string TagId { get; set; }
        public string PalletName { get; set; }
        public string WarehouseName { get; set; }
        public string TransactionCode { get; set; }
        public string ScannedDate { get; set; }
        public string ScannedBy { get; set; }
        public string TransactionDate { get; set; }
    }
}