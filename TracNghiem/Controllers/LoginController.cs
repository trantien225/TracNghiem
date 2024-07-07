using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;
using BCrypt.Net;
using TracNghiem.Models;

namespace TracNghiem.Controllers
{
    public class LoginController : Controller
    {
        private THITRACNGHIEM_ONL ONL = new THITRACNGHIEM_ONL();

        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]//chặn tấn công token
        public ActionResult Index(string Username, string Password)
        {
            try
            {
                // Tìm tài khoản trong cơ sở dữ liệu
                var user = ONL.TAIKHOANs.FirstOrDefault(u => u.TENTK == Username);
                 if (user != null && BCrypt.Net.BCrypt.Verify(Password, user.MATKHAU))
                {
                    // Đăng nhập thành công
                    // Cập nhật trạng thái đăng nhập
                    user.TRANGTHAI = true;
                    ONL.SaveChanges();
                    Session["CurrentUser"] = user;
                    // Kiểm tra quyền hạn và chuyển hướng
                    if (user.LOAITK == "admin")
                    {
                        return RedirectToAction("Index", "HomeAdmin", new { area = "Admin" });
                    }
                    else if(user.LOAITK == "sinhvien")
                    {
                        var student = ONL.SINHVIENs.FirstOrDefault(s => s.MSSV == user.TENTK);
                        if (student != null)
                        {
                            Session["CurrentUser"] = student; // Lưu thông tin sinh viên vào session
                            return RedirectToAction("Info", new { mssv = student.MSSV });
                        }
                    }
                    else if (user.LOAITK == "giangvien")
                    {
                        // Giả sử có một view riêng cho 'giamthi'
                        return RedirectToAction("PhanCong", "GV", new { area = "Admin" });
                    }
                }

                // Đăng nhập không thành công, hiển thị thông báo lỗi
                ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu";
                return View();
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ
                ViewBag.Error = "Đã xảy ra lỗi khi đăng nhập: " + ex.Message;
                return RedirectToAction("Error", "Shared", new { area = "" });
            }
        }

        public ActionResult Logout()
        {
            var username = Session["CurrentUser"] as string;
            if (username != null)
            {
                var user = ONL.TAIKHOANs.FirstOrDefault(u => u.TENTK == username);
                if (user != null)
                {
                    // Cập nhật trạng thái đăng xuất
                    user.TRANGTHAI = false;
                    ONL.SaveChanges();
                }
                Session["CurrentUser"] = null;
            }
            return RedirectToAction("Index", "Login");
        }

        public ActionResult Info(string mssv)
        {
            var query = from lt in ONL.LICHTHIs
                        join hp in ONL.HOCPHANs on lt.MAHOCPHAN equals hp.MAHOCPHAN
                        select new infolichthi
                        {
                            PHONGTHI = lt.PHONGTHI,
                            TENHOCPHAN = hp.TENHOCPHAN,
                            THOILUONGTHI = lt.THOILUONGTHI,
                            THOIGIANTHI = lt.THOIGIANTHI,
                            NGAYTHI = lt.NGAYTHI // Bao gồm NGAYTHI
                        };

            ViewBag.LICHTHI = query.ToList(); // Lưu dữ liệu vào ViewBag
            if (string.IsNullOrEmpty(mssv))
            {
                return RedirectToAction("Index", "Login");
            }

            //var student = ONL.SINHVIEN.FirstOrDefault(s => s.MSSV == mssv);
            // Lấy thông tin sinh viên từ session
            var student = Session["CurrentUser"] as SINHVIEN;

            if (student == null)
            {
                return HttpNotFound();
            }

            return View(student);
        }
    }
}