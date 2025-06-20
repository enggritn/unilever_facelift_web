
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models
{
    public class RegistrationItemDTO
    {
        public string TransactionItemId { get; set; }
        public string TagId { get; set; }
        public string Status { get; set; }
        public string InsertedBy { get; set; }
        public string InsertedAt { get; set; }
        public string InsertMethod { get; set; }
    }
}