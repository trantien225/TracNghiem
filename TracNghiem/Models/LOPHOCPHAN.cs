//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TracNghiem.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class LOPHOCPHAN
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public LOPHOCPHAN()
        {
            this.LICHTHIs = new HashSet<LICHTHI>();
            this.LOPHOCPHAN_SINHVIEN = new HashSet<LOPHOCPHAN_SINHVIEN>();
        }
    
        public string MALOPHOCPHAN { get; set; }
        public string TENLOPHOCPHAN { get; set; }
        public string MAHOCPHAN { get; set; }
    
        public virtual HOCPHAN HOCPHAN { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LICHTHI> LICHTHIs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LOPHOCPHAN_SINHVIEN> LOPHOCPHAN_SINHVIEN { get; set; }
    }
}
