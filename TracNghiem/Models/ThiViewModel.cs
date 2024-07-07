using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TracNghiem.Models
{
    public class ThiViewModel
    {
        public SINHVIEN Student {  get; set; }
        public List<Class2> Questions { get; set; }
        public HOCPHAN HOCPHAN { get; set; }
        public string MABODETHI { get; set; }
        public string tenhocphan { get; set; }
        public int thoiluongthi { get; set; }
    }
}