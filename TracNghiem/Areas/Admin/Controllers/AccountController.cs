using Microsoft.Ajax.Utilities;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TracNghiem.Models;


namespace TracNghiem.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        // GET: Admin/Account
        THITRACNGHIEM_ONL onl = new THITRACNGHIEM_ONL();
        public ActionResult Index(string maLopHocPhan)
        {
            var viewModel = new Class4();

            // Lấy danh sách các lớp học phần từ cơ sở dữ liệu và chuyển đổi thành SelectListItem
            viewModel.LOPHOCPHANs = onl.LOPHOCPHANs.ToList();

            // Lưu giá trị maLopHocPhan vào ViewBag để hiển thị lại giá trị đã chọn
            ViewBag.SelectedLopHocPhan = maLopHocPhan;

            // Khởi tạo danh sách sinh viên
            if (!string.IsNullOrEmpty(maLopHocPhan))
            {
                // Lọc theo mã lớp học phần
                viewModel.TAIKHOANs = onl.TAIKHOANs
                    .Where(tk => tk.LOAITK == "SinhVien" && onl.LOPHOCPHAN_SINHVIEN
                        .Any(lhs => lhs.MALOPHOCPHAN == maLopHocPhan && lhs.MSSV == tk.TENTK))
                    .ToList();
            }
            else
            {
                // Không có mã lớp học phần được chọn, lấy tất cả sinh viên
                viewModel.TAIKHOANs = onl.TAIKHOANs
                    .Where(tk => tk.LOAITK == "SinhVien")
                    .ToList();
            }

            // Tính toán số lượng sinh viên theo trạng thái
            ViewBag.LoggedInCount = viewModel.TAIKHOANs.Count(tk => tk.TRANGTHAI == true);
            ViewBag.NotLoggedInCount = viewModel.TAIKHOANs.Count(tk => tk.TRANGTHAI == false);
            ViewBag.CompletedCount = viewModel.TAIKHOANs.Count(tk => tk.TRANGTHAI == null);

            return View(viewModel);
        }
        public ActionResult Admin(string search)
        {
            List<TAIKHOAN> giangVienList;
            if (string.IsNullOrEmpty(search))
            {
                giangVienList = onl.TAIKHOANs.Where(t => t.LOAITK == "GiangVien" || t.LOAITK == "Admin").ToList();
            }
            else
            {
                giangVienList = onl.TAIKHOANs.Where(t => (t.LOAITK == "GiangVien" || t.LOAITK == "Admin") && t.TENTK.Contains(search)).ToList();
            }
            return View(giangVienList);

        }
        public ActionResult GiangVienIndex(string search)
        {
            List<TAIKHOAN> giangVienList;
            if (string.IsNullOrEmpty(search))
            {
                giangVienList = onl.TAIKHOANs.Where(t => t.LOAITK == "GiangVien").ToList();
            }
            else
            {
                giangVienList = onl.TAIKHOANs.Where(t => (t.LOAITK == "GiangVien") && t.TENTK.Contains(search)).ToList();
            }
            return View(giangVienList);
        }
        public ActionResult Create()
        {
            return View();
        }

        // POST: /admin/Account/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TAIKHOAN taikhoan)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    onl.TAIKHOANs.Add(taikhoan);
                    onl.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }
            return View(taikhoan);
        }

        // POST: /admin/Account/ResetPassword
        [HttpPost]
        public JsonResult ResetPassword(string username)
        {
            var user = onl.TAIKHOANs.FirstOrDefault(t => t.TENTK == username);
            if (user != null)
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.TENTK);
                user.MATKHAU = hashedPassword; // Đặt lại mật khẩu thành tên đăng nhập

                onl.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        [HttpPost]
        public JsonResult ResetPasswordAdmin(string username, string currentPassword, string newPassword)
        {
            try
            {
                var user = onl.TAIKHOANs.FirstOrDefault(t => t.TENTK == username);
                if (user != null)
                {
                    // Kiểm tra mật khẩu hiện tại
                    if (user.MATKHAU == currentPassword)
                    {
                        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                        user.MATKHAU = hashedPassword; // Cập nhật mật khẩu mới
                        onl.SaveChanges();
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Mật khẩu hiện tại không đúng." });
                    }
                }
                return Json(new { success = false, message = "Không tìm thấy tài khoản." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /admin/Account/Delete
        [HttpPost]
        public ActionResult Delete(string username)
        {
            try
            {
                var user = onl.TAIKHOANs.FirstOrDefault(t => t.TENTK == username);
                if (user != null)
                {
                    onl.TAIKHOANs.Remove(user);
                    onl.SaveChanges();

                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Không tìm thấy tài khoản" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public ActionResult Detail(string id)
        {
            var student = onl.SINHVIENs.FirstOrDefault(u => u.MSSV == id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }
        public ActionResult AddGV()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AddGV(HttpPostedFileBase excelFile, string subjectName)
        {
            if (excelFile != null && excelFile.ContentLength > 0)
            {
                string path = Path.Combine(Server.MapPath("~/App_Data/Uploads"), Path.GetFileName(excelFile.FileName));
                excelFile.SaveAs(path);

                SaveExcelToDatabaseGV(path, subjectName);

                TempData["Message"] = "File uploaded and data saved successfully!";
            }
            else
            {
                TempData["Message"] = "Please select a valid Excel file.";
            }
            // Kiểm tra xem có lỗi ModelState nào không và truyền nó đến View
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
            }

            return RedirectToAction("Add");
        }

        private void SaveExcelToDatabaseGV(string filePath, string maLopHocPhan)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            FileInfo fileInfo = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // Lấy sheet đầu tiên
                int rowCount = worksheet.Dimension.Rows;

                using (var context = new THITRACNGHIEM_ONL())
                {
                    for (int row = 2; row <= rowCount; row++) // Bỏ qua hàng tiêu đề
                    {
                        string TENTK = worksheet.Cells[row, 2].Value?.ToString().Trim();
                        string MATKHAU = worksheet.Cells[row, 3].Value?.ToString().Trim();
                        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(MATKHAU);
                        bool TRANGTHAI;
                        string column4Str = worksheet.Cells[row, 4].Value?.ToString().Trim();
                        bool.TryParse(column4Str, out TRANGTHAI);
                        string MSSV = worksheet.Cells[row, 5].Value?.ToString().Trim();
                        string MSGV = worksheet.Cells[row, 6].Value?.ToString().Trim();
                        // Kiểm tra xem TENTK đã tồn tại hay chưa
                        if (!context.TAIKHOANs.Any(t => t.TENTK == TENTK))
                        {
                            // Mã hóa mật khẩu trước khi lưu vào cơ sở dữ liệu
                            TAIKHOAN TK = new TAIKHOAN
                            {
                                LOAITK = "giangvien",
                                TENTK = TENTK,
                                MATKHAU = hashedPassword,
                                TRANGTHAI = TRANGTHAI,
                                MSSV = null,
                                MAGV = MSGV
                            };

                            try
                            {
                                context.TAIKHOANs.Add(TK);
                                context.SaveChanges(); // Lưu thay đổi ngay sau khi thêm thành công
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error inserting row {row}: {ex.Message}");
                            }
                        }
                        else
                        {
                            // Nếu TENTK đã tồn tại, thông báo lỗi cho người dùng
                            Console.WriteLine($"Error: TENTK '{TENTK}' already exists.");
                            ModelState.AddModelError("", $"Error: TENTK '{TENTK}' already exists.");
                        }
                    }
                }
            }
        }
        public ActionResult Add()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Add(HttpPostedFileBase excelFile, string subjectName)
        {
            if (excelFile != null && excelFile.ContentLength > 0)
            {
                string path = Path.Combine(Server.MapPath("~/App_Data/Uploads"), Path.GetFileName(excelFile.FileName));
                excelFile.SaveAs(path);

                SaveExcelToDatabase(path, subjectName);

                TempData["Message"] = "File uploaded and data saved successfully!";
            }
            else
            {
                TempData["Message"] = "Please select a valid Excel file.";
            }
            // Kiểm tra xem có lỗi ModelState nào không và truyền nó đến View
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
            }

            return RedirectToAction("Add");
        }

        private void SaveExcelToDatabase(string filePath, string maLopHocPhan)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            FileInfo fileInfo = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // Lấy sheet đầu tiên
                int rowCount = worksheet.Dimension.Rows;

                using (var context = new THITRACNGHIEM_ONL())
                {
                    for (int row = 2; row <= rowCount; row++) // Bỏ qua hàng tiêu đề
                    {
                        string TENTK = worksheet.Cells[row, 2].Value?.ToString().Trim();
                        string MATKHAU = worksheet.Cells[row, 3].Value?.ToString().Trim();
                        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(MATKHAU);
                        bool TRANGTHAI;
                        string column4Str = worksheet.Cells[row, 4].Value?.ToString().Trim();
                        bool.TryParse(column4Str, out TRANGTHAI);
                        string MSSV = worksheet.Cells[row, 5].Value?.ToString().Trim();
                        string MSGV = worksheet.Cells[row, 6].Value?.ToString().Trim();
                        // Kiểm tra xem TENTK đã tồn tại hay chưa
                        if (!context.TAIKHOANs.Any(t => t.TENTK == TENTK))
                        {
                            // Mã hóa mật khẩu trước khi lưu vào cơ sở dữ liệu
                            TAIKHOAN TK = new TAIKHOAN
                            {
                                LOAITK = "sinhvien",
                                TENTK = TENTK,
                                MATKHAU = hashedPassword,
                                TRANGTHAI = TRANGTHAI,
                                MSSV=MSSV,
                                MAGV=null
                            };

                            try
                            {
                                context.TAIKHOANs.Add(TK);
                                context.SaveChanges(); // Lưu thay đổi ngay sau khi thêm thành công
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error inserting row {row}: {ex.Message}");
                            }
                        }
                            else
                            {
                                // Nếu TENTK đã tồn tại, thông báo lỗi cho người dùng
                                Console.WriteLine($"Error: TENTK '{TENTK}' already exists.");
                                ModelState.AddModelError("", $"Error: TENTK '{TENTK}' already exists.");
                            }
                    }
                }
            }
        }
        public ActionResult PrintResults(string maLopHocPhan)
        {
            // Assuming 'onl' is your DbContext or data context
            var results = (from sv in onl.SINHVIENs
                           join lhs in onl.LOPHOCPHAN_SINHVIEN on sv.MSSV equals lhs.MSSV
                           join lhp in onl.LOPHOCPHANs on lhs.MALOPHOCPHAN equals lhp.MALOPHOCPHAN
                           join kq in onl.KETQUAs on sv.MSSV equals kq.MSSV
                           where lhp.MALOPHOCPHAN == maLopHocPhan
                           select new kq // Replace YourViewModel with an appropriate ViewModel
                           {
                               MSSV = sv.MSSV,
                               HoTen = sv.HOTEN,
                               TenLopHocPhan = lhp.TENLOPHOCPHAN,
                               MaDeThi = kq.MABODETHI,
                               SoCauDung = kq.SOCAUDUNG,
                               DiemThi = kq.DIEMTHI
                           }).ToList();

            return PartialView(results);
        }

    }

}


