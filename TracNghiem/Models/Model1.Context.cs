﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class THITRACNGHIEM_ONL : DbContext
    {
        public THITRACNGHIEM_ONL()
            : base("name=THITRACNGHIEM_ONL")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<BODETHI> BODETHIs { get; set; }
        public virtual DbSet<BODETHI_CAUHOI> BODETHI_CAUHOI { get; set; }
        public virtual DbSet<CAUHOI> CAUHOIs { get; set; }
        public virtual DbSet<GIANGVIEN> GIANGVIENs { get; set; }
        public virtual DbSet<HOCPHAN> HOCPHANs { get; set; }
        public virtual DbSet<KETQUA> KETQUAs { get; set; }
        public virtual DbSet<LOPHOCPHAN> LOPHOCPHANs { get; set; }
        public virtual DbSet<LOPHOCPHAN_SINHVIEN> LOPHOCPHAN_SINHVIEN { get; set; }
        public virtual DbSet<SINHVIEN> SINHVIENs { get; set; }
        public virtual DbSet<TAIKHOAN> TAIKHOANs { get; set; }
        public virtual DbSet<LICHTHI> LICHTHIs { get; set; }
    }
}
