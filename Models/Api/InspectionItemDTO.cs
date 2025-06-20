using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models.Api
{
    public class InspectionItemDTO
    {
        public string TransactionItemId { get; set; }
        public string TransactionId { get; set; }
        public string TagId { get; set; }
        public string ScannedBy { get; set; }
        public string ScannedAt { get; set; }
    }
}