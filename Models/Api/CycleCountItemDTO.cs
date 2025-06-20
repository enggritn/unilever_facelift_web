using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models.Api
{
    public class CycleCountItemDTO
    {
        public string TransactionItemId { get; set; }
        public string TagId { get; set; }
        public string PalletCondition { get; set; }
        public string PalletTypeName { get; set; }
        public string PalletOwner { get; set; }
        public string PalletProducer { get; set; }
        public string PalletMovementStatus { get; set; }
        public string ScannedBy { get; set; }
        public string ScannedAt { get; set; }
    }
}