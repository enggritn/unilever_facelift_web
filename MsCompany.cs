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
    
    public partial class MsCompany
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MsCompany()
        {
            this.MsPallets = new HashSet<MsPallet>();
            this.MsWarehouseCompanies = new HashSet<MsWarehouseCompany>();
            this.TrxRegistrationHeaders = new HashSet<TrxRegistrationHeader>();
        }
    
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAbb { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedAt { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MsPallet> MsPallets { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MsWarehouseCompany> MsWarehouseCompanies { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TrxRegistrationHeader> TrxRegistrationHeaders { get; set; }
    }
}
