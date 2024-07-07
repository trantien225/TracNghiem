using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TracNghiem.Models;

namespace TracNghiem.Areas.Admin.Controllers
{
    public class TKDIEMController : Controller
    {
        // GET: Admin/TKDIEM
        THITRACNGHIEM_ONL db = new THITRACNGHIEM_ONL();

        public ActionResult Index()
        {
            try
            {
                var query = from ketqua in db.KETQUAs
                            join bodethi in db.BODETHIs on ketqua.MABODETHI equals bodethi.MABODETHI
                            join monthi in db.HOCPHANs on bodethi.MAHOCPHAN equals monthi.MAHOCPHAN
                            group new { monthi, ketqua } by new { monthi.TENHOCPHAN } into grp
                            select new Class3
                            {
                                TENHOCPHAN = grp.Key.TENHOCPHAN,
                                TONGDIEM = grp.Average(x => x.ketqua.DIEMTHI)
                            };

                // Chuyển dữ liệu sang View để vẽ biểu đồ
                return View(query.ToList());
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Đã xảy ra lỗi: " + ex.Message;
                return RedirectToAction("Error", "Shared", new { area = "" });
            }
        }
    }
}
