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
    
    public partial class KETQUA
    {
        public int ID { get; set; }
        public string MSSV { get; set; }
        public string MABODETHI { get; set; }
        public int SOCAUDUNG { get; set; }
        public double DIEMTHI { get; set; }
        public string MAHOCPHAN { get; set; }
    
        public virtual SINHVIEN SINHVIEN { get; set; }
        public virtual BODETHI BODETHI { get; set; }
        public virtual HOCPHAN HOCPHAN { get; set; }
    }
}
