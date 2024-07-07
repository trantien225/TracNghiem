using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TracNghiem.Models
{
    public class Class1
    {
        public BODETHI BODETHI { get; set; }
        public List<HOCPHAN> HOCPHANs { get; set; }
        public SelectList CourseList { get; set; } // Thêm thuộc tính này

        // Constructor mặc định (nếu cần)
        public Class1()
        {
            CourseList = new SelectList(new List<SelectListItem>()); // Khởi tạo SelectList để tránh lỗi null
        }
    }
}