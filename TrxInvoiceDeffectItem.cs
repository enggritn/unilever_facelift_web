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
    
    public partial class TrxInvoiceDeffectItem
    {
        public string TransactionItemId { get; set; }
        public string TransactionId { get; set; }
        public string PalletId { get; set; }
        public string WarehouseId { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public string DeffectType { get; set; }
    
        public virtual TrxInvoiceDeffect TrxInvoiceDeffect { get; set; }
    }
}
