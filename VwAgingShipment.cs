//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Facelift_App
{
    using System;
    using System.Collections.Generic;
    
    public partial class VwAgingShipment
    {
        public string DeliveryNumber { get; set; }
        public string OriginId { get; set; }
        public string OriginCode { get; set; }
        public string OriginName { get; set; }
        public string DestinationId { get; set; }
        public string DestinationCode { get; set; }
        public string DestinationName { get; set; }
        public string TransactionStatus { get; set; }
        public string ShipmentStatus { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public Nullable<int> AgingDay { get; set; }
        public Nullable<int> AgingMin { get; set; }
    }
}
