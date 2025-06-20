using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models.Api
{
    public class ShipmentItemDTO
    {
        public string TransactionItemId { get; set; }
        public string TagId { get; set; }
        public string PalletTypeName { get; set; }
        public string PalletOwner { get; set; }
        public string PalletProducer { get; set; }
        public string PalletMovementStatus { get; set; }
        public string ScannedBy { get; set; }
        public string ScannedAt { get; set; }
        public string DispatchedBy { get; set; }
        public string DispatchedAt { get; set; }
        public string ReceivedBy { get; set; }
        public string ReceivedAt { get; set; }
    }
}