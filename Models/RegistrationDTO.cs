using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models
{
    public class RegistrationDTO
    {
        public string TransactionId { get; set; }
        public string TransactionCode { get; set; }
        public string DeliveryNote { get; set; }
        public string Description { get; set; }
        public string PalletName { get; set; }
        public string WarehouseName { get; set; }
        public string PalletOwner { get; set; }
        public string PalletProducer { get; set; }
        public string ProducedDate { get; set; }
        public string TransactionStatus { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedAt { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedAt { get; set; }
    }
}