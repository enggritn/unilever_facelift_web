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
    
    public partial class MsMainMenu
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MsMainMenu()
        {
            this.MsMenus = new HashSet<MsMenu>();
        }
    
        public int MenuId { get; set; }
        public int MenuIndex { get; set; }
        public string MenuName { get; set; }
        public string MenuIcon { get; set; }
        public bool IsActive { get; set; }
        public string MenuType { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MsMenu> MsMenus { get; set; }
    }
}
