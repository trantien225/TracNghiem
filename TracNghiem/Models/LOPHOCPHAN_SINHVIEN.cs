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
    
    public partial class LOPHOCPHAN_SINHVIEN
    {
        public int ID { get; set; }
        public string MSSV { get; set; }
        public string MALOPHOCPHAN { get; set; }
    
        public virtual LOPHOCPHAN LOPHOCPHAN { get; set; }
        public virtual SINHVIEN SINHVIEN { get; set; }
    }
}
