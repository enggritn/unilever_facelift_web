using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models.Api
{
    public class AccidentItemDTO
    {
        public string TransactionItemId { get; set; }
        public string TagId { get; set; }
        public string PalletTypeName { get; set; }
        public string ReasonType { get; set; }
        public string ReasonName { get; set; }
        public string ScannedBy { get; set; }
        public string ScannedAt { get; set; }
    }
}