using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models.Api
{
    public class Dashboard
    {
        public int totalRegistration { get; set; }
        public int totalOutbound { get; set; }
        public int totalInbound { get; set; }
        public int totalInspection { get; set; }
        public int totalCycleCount { get; set; }
        public int totalRecall { get; set; }
    }
}