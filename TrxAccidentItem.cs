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
    
    public partial class TrxAccidentItem
    {
        public string TransactionItemId { get; set; }
        public string TransactionId { get; set; }
        public string TagId { get; set; }
        public string ReasonType { get; set; }
        public string ReasonName { get; set; }
        public string ScannedBy { get; set; }
        public Nullable<System.DateTime> ScannedAt { get; set; }
    
        public virtual TrxAccidentHeader TrxAccidentHeader { get; set; }
    }
}
