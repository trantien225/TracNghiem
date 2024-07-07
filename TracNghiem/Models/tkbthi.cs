using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TracNghiem.Models
{
    public class tkbthi
    {
        public string TenHocPhan { get; set; }
        public string TenLopHocPhan { get; set; }
        public string TenGV1 { get; set; }
        public string TenGV2 { get; set; }
        public System.DateTime? NgayThi { get; set; }
        public string PHONGTHI { get; set; }
        public string THOIGIANTHI { get; set; }
        public string ThoiLuongThi { get; set; }
    }
}