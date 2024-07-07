using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TracNghiem.Models;

namespace TracNghiem.Areas.Admin.Controllers
{
    public class LopController : Controller
    {
        // GET: Admin/Lop
        THITRACNGHIEM_ONL onl = new THITRACNGHIEM_ONL();
        public ActionResult Index()
        {
            List<LOPHOCPHAN> LOP_HP = onl.LOPHOCPHANs.ToList();
            return View(LOP_HP);
        }
        public ActionResult dssv()
        {
            List<SINHVIEN> DSSV = onl.SINHVIENs.ToList();
            return View(DSSV);
        }
    }
}