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
    
    public partial class MsTransporterAccess
    {
        public int TransporterAccessId { get; set; }
        public string WarehouseId { get; set; }
        public string TransporterId { get; set; }
    
        public virtual MsTransporter MsTransporter { get; set; }
        public virtual MsWarehouse MsWarehouse { get; set; }
    }
}
