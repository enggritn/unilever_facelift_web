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
    
    public partial class TrxShipmentItem
    {
        public string TransactionItemId { get; set; }
        public string TransactionId { get; set; }
        public string TagId { get; set; }
        public string ScannedBy { get; set; }
        public System.DateTime ScannedAt { get; set; }
        public string DispatchedBy { get; set; }
        public Nullable<System.DateTime> DispatchedAt { get; set; }
        public string ReceivedBy { get; set; }
        public Nullable<System.DateTime> ReceivedAt { get; set; }
        public string PalletMovementStatus { get; set; }
    
        public virtual TrxShipmentHeader TrxShipmentHeader { get; set; }
    }
}
