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
    
    public partial class TrxBillingRentHistoryItem
    {
        public string TransactionItemId { get; set; }
        public string TransactionId { get; set; }
        public string AgingId { get; set; }
        public string PalletId { get; set; }
        public int CurrentMonth { get; set; }
        public int CurrentYear { get; set; }
        public int TotalMinutes { get; set; }
        public string BillingId { get; set; }
        public int BillingYear { get; set; }
        public decimal BillingPrice { get; set; }
    
        public virtual MsBillingConfiguration MsBillingConfiguration { get; set; }
        public virtual MsPallet MsPallet { get; set; }
        public virtual MsPalletAging MsPalletAging { get; set; }
        public virtual TrxBillingRentHistoryHeader TrxBillingRentHistoryHeader { get; set; }
    }
}
