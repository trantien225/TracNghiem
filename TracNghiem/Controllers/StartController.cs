using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using TracNghiem.Models;

namespace TracNghiem.Controllers
{
    public class StartController : Controller
    {
        THITRACNGHIEM_ONL db = new THITRACNGHIEM_ONL();

        //private string BODETHI;

        public ActionResult Index()
        {
            try
            {
                var student = Session["CurrentUser"] as SINHVIEN;

                // Kiểm tra nếu session không chứa đối tượng SINHVIEN hợp lệ
                if (student == null)
                {
                    return RedirectToAction("Login", "Account"); // Chuyển hướng đến trang đăng nhập
                }

                string mssv = student.MSSV; // Lấy MSSV của sinh viên từ đối tượng student

                // Lấy danh sách các bộ đề có trạng thái cho phép là true
                //var allowedExams = db.BODETHIs.Where(b => b.IsAllowed == true).ToList();
                var allowedExams = db.LOPHOCPHAN_SINHVIEN
                    .Where(lsv => lsv.MSSV == mssv)
                    .Join(db.LOPHOCPHANs, lsv => lsv.MALOPHOCPHAN, lhp => lhp.MALOPHOCPHAN, (lsv, lhp) => lhp)
                    .Join(db.HOCPHANs, lhp => lhp.MAHOCPHAN, hp => hp.MAHOCPHAN, (lhp, hp) => hp)
                    .Join(db.BODETHIs, hp => hp.MAHOCPHAN, bd => bd.MAHOCPHAN, (hp, bd) => bd)
                    .Where(bd => bd.IsAllowed == true)
                    .ToList();
                // Lấy MABODETHI của BODETHI đầu tiên trong danh sách các bộ đề cho phép (nếu có)
                //string bodethiId = allowedExams.FirstOrDefault()?.MABODETHI;

                if (!allowedExams.Any())
                {
                    TempData["ErrorMessage"] = "Chưa được phép thi.";
                    if (Request.UrlReferrer != null)
                    {
                        return Redirect(Request.UrlReferrer.ToString());
                    }
                    else
                    {
                        return RedirectToAction("Info", "Login");
                    }
                }
                string bodethiId = allowedExams.FirstOrDefault()?.MABODETHI;
                // Lấy danh sách câu hỏi từ database dựa trên MSSV của sinh viên và MABODETHI đã chọn
                var questions = (from sv in db.SINHVIENs
                                 join lsv in db.LOPHOCPHAN_SINHVIEN on sv.MSSV equals lsv.MSSV
                                 join lhp in db.LOPHOCPHANs on lsv.MALOPHOCPHAN equals lhp.MALOPHOCPHAN
                                 join hp in db.HOCPHANs on lhp.MAHOCPHAN equals hp.MAHOCPHAN
                                 join bd in db.BODETHIs on hp.MAHOCPHAN equals bd.MAHOCPHAN
                                 join bdch in db.BODETHI_CAUHOI on bd.MABODETHI equals bdch.MABODETHI
                                 join ch in db.CAUHOIs on bdch.MACAUHOI equals ch.MACAUHOI
                                 where sv.MSSV == mssv && bd.MABODETHI == bodethiId
                                 select new Class2
                                 {
                                     MAHOCPHAN = hp.MAHOCPHAN,
                                     TENHOCPHAN = hp.TENHOCPHAN,
                                     MABODETHI = bd.MABODETHI,
                                     TENCAUHOI = ch.TENCAUHOI,
                                     DAPAN_A = ch.DAPAN_A,
                                     DAPAN_B = ch.DAPAN_B,
                                     DAPAN_C = ch.DAPAN_C,
                                     DAPAN_D = ch.DAPAN_D,
                                     DAPANDUNG = ch.DAPANDUNG
                                 }).ToList();
                // Xáo trộn danh sách câu hỏi
                questions = questions.OrderBy(x => Guid.NewGuid()).ToList();

                string tenHocPhan = questions.FirstOrDefault()?.TENHOCPHAN;
                string BODETHI = questions.FirstOrDefault()?.MABODETHI;

                ////////////////////////////////////////////////////////////
                string maHocPhan = questions.FirstOrDefault()?.MAHOCPHAN; // Lấy mã học phần từ câu hỏi đầu tiên (hoặc từ nguồn dữ liệu phù hợp)

                // Tìm kiếm thông tin lịch thi dựa trên mã học phần
                var lichThiInfo = db.LICHTHIs.FirstOrDefault(lt => lt.MAHOCPHAN == maHocPhan);
                // Lấy thời lượng thi nếu tìm thấy thông tin lịch thi
                int thoiLuongThi = Convert.ToInt32(lichThiInfo.THOILUONGTHI);
                // Sử dụng thời lượng thi ở đây...

                Session["BODETHI"] = BODETHI;
                Session["MAHOCPHAN"] = maHocPhan;
                // Tạo đối tượng ViewModel
                ThiViewModel viewModel = new ThiViewModel
                {
                    Student = student,
                    Questions = questions,
                    tenhocphan = tenHocPhan,
                    thoiluongthi = thoiLuongThi,
                };

                // Chuyển đổi danh sách câu hỏi thành JSON và truyền vào ViewBag
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                ViewBag.QuizData = JsonConvert.SerializeObject(viewModel.Questions, settings);

                // Trả về View và truyền danh sách câu hỏi đã xáo trộn
                return View(viewModel);
            }
            catch (Exception)
            {
                // Truyền thông báo lỗi đến View thông qua TempData
                TempData["ErrorMessage"] = "Sinh viên không thuộc đợt thi.";
                if (Request.UrlReferrer != null)
                {
                    return Redirect(Request.UrlReferrer.ToString());
                }
                else
                {
                    return RedirectToAction("Info", "Login");
                }
            }
        }
        //LƯU TRỮ KẾT QUẢ THI CỦA SINH VIÊN
        [HttpPost]
        public ActionResult SaveResult(string mssv, int soCauDung, string diemThi)
            {
            try
            {
                // Tính điểm
                // Lưu kết quả vào CSDL
                // Code xử lý lưu vào bảng KETQUA ở đây
                //string maBoDeThi = "DETHI01"; // Set cứng mã đề thi
                SaveToDatabase(mssv, soCauDung, diemThi);
                Logout(mssv);
                return Json(new { success = true, message = "Kết quả đã được lưu." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi lưu kết quả." });
            }
        }

        // Hàm lưu kết quả vào CSDL (đây là ví dụ, bạn cần thay thế bằng cách thực tế)
        [HttpPost]
        public JsonResult SaveToDatabase(string mssv, int soCauDung, string diemThi)
        {
            try
            {
                string bodethiId = Session["BODETHI"] as string;
                string mahocphan = Session["MAHOCPHAN"] as string;
                if (!double.TryParse(diemThi, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double diemThiDouble))
                {
                    throw new Exception("Điểm thi không hợp lệ.");
                }
                var ketQua = new KETQUA
                {
                    MSSV = mssv,
                    MAHOCPHAN = mahocphan,
                    MABODETHI = bodethiId,
                    SOCAUDUNG = soCauDung,
                    DIEMTHI = diemThiDouble
                };

                db.KETQUAs.Add(ketQua);
                db.SaveChanges();
                Session.Clear();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public ActionResult Logout(string mssv)
        {
            // Truyền giá trị mssv vào biến Session["CurrentUser"]
            Session["CurrentUser"] = mssv;

            var user = db.TAIKHOANs.FirstOrDefault(u => u.TENTK == mssv);
            if (user != null)
            {
                // Cập nhật trạng thái đăng xuất
                user.TRANGTHAI = null;
                db.SaveChanges();
            }   
            Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}
