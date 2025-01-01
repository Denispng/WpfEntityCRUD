//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WpfEntityCRUD
{
    using System;
    using System.Collections.Generic;
    
    public partial class Aisle
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Aisle()
        {
            this.Cells = new HashSet<Cell>();
        }
    
        public int AisleID { get; set; }
        public string Name { get; set; }
        public Nullable<int> LayerCount { get; set; }
        public Nullable<int> CellCount { get; set; }
        public Nullable<int> RowCount { get; set; }
        public int company_id { get; set; }
        public Nullable<int> warehousesId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Cell> Cells { get; set; }
        public virtual Warehouse Warehouse { get; set; }
        public virtual Company Company { get; set; }
    }
}