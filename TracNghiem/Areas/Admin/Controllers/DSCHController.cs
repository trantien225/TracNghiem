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
    public class DSCHController : Controller
    {
        // GET: Admin/DSCH
        THITRACNGHIEM_ONL onl = new THITRACNGHIEM_ONL();
        //hiển thị ds câu hỏi
        public ActionResult Index(string MAHOCPHAN)
        {
            ViewBag.HocPhans = onl.HOCPHANs.ToList(); // Retrieve list of courses for the dropdown

            List<CAUHOI> CH;
            if (!string.IsNullOrEmpty(MAHOCPHAN))
            {
                CH = onl.CAUHOIs.Where(ch => ch.MAHOCPHAN == MAHOCPHAN).ToList();
            }
            else
            {
                CH = onl.CAUHOIs.ToList();
            }

            return View(CH);
        }
        //học phần
        public ActionResult Hocphan()
        {
            List<HOCPHAN> HP = onl.HOCPHANs.ToList();
            return View(HP);
        }
        [HttpPost]
        public JsonResult Remove(string id)
        {
            try
            {
                // Kiểm tra xem học phần có được sử dụng trong bảng LOPHOCPHAN hay không
                var lophoc = onl.LOPHOCPHANs.FirstOrDefault(l => l.MAHOCPHAN == id);
                if (lophoc != null)
                {                
                    return Json(new { success = false });
                }

                var checkCategory = onl.HOCPHANs.FirstOrDefault(x => x.MAHOCPHAN == id);
                if (checkCategory != null)
                {
                    // Xóa các câu hỏi liên quan trước khi xóa học phần
                    var questions = onl.CAUHOIs.Where(q => q.MAHOCPHAN == id).ToList();
                    foreach (var question in questions)
                    {
                        onl.CAUHOIs.Remove(question);
                    }

                    onl.HOCPHANs.Remove(checkCategory);
                    onl.SaveChanges();

                    // Kiểm tra lại sau khi xóa
                    checkCategory = onl.HOCPHANs.FirstOrDefault(x => x.MAHOCPHAN == id);
                    if (checkCategory == null)
                    {
                        return Json(new { success = true });
                    }
                }
                return Json(new { success = false });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
        }
        //Thêm câu hỏi
        public ActionResult Add()
        {
            List<HOCPHAN> HP = onl.HOCPHANs.ToList();
            return View(HP);
        }

        [HttpPost]
        public ActionResult Add(HttpPostedFileBase excelFile, string subjectName)
        {
            if (excelFile != null && excelFile.ContentLength > 0 && !string.IsNullOrEmpty(subjectName))
            {
                string path = Path.Combine(Server.MapPath("~/App_Data/Uploads"), Path.GetFileName(excelFile.FileName));
                excelFile.SaveAs(path);

                var result = SaveExcelToDatabase(path, subjectName);
                if (result == null)
                {
                    return Json(new { success = true });
                }
                TempData["Message"] = "File uploaded and data saved successfully!";
            }
            else
            {
                TempData["Message"] = "Please select a valid Excel file and a subject.";
            }
            return Json(new { success = false });
        }

        private string SaveExcelToDatabase(string filePath, string subjectName)
        {
            try
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
                            string MACAUHOI = worksheet.Cells[row, 1].Value?.ToString().Trim();
                            string TENCAUHOI = worksheet.Cells[row, 2].Value?.ToString().Trim();
                            string DAPAN_A = worksheet.Cells[row, 3].Value?.ToString().Trim();
                            string DAPAN_B = worksheet.Cells[row, 4].Value?.ToString().Trim();
                            string DAPAN_C = worksheet.Cells[row, 5].Value?.ToString().Trim();
                            string DAPAN_D = worksheet.Cells[row, 6].Value?.ToString().Trim();
                            string DAPANDUNG = worksheet.Cells[row, 7].Value?.ToString().Trim();

                            CAUHOI ch = new CAUHOI
                            {
                                MACAUHOI = MACAUHOI,
                                TENCAUHOI = TENCAUHOI,
                                DAPAN_A = DAPAN_A,
                                DAPAN_B = DAPAN_B,
                                DAPAN_C = DAPAN_C,
                                DAPAN_D = DAPAN_D,
                                DAPANDUNG = DAPANDUNG,
                                MAHOCPHAN = subjectName // Gán tên học phần đã chọn
                            };

                            try
                            {
                                context.CAUHOIs.Add(ch);
                            }
                            catch (Exception ex)
                            {
                                return $"Lỗi khi chèn hàng {row}: {ex.Message}";
                            }
                        }

                        try
                        {
                            context.SaveChanges();
                            // Trả về null nếu thành công
                            return null;
                        }
                        catch (Exception ex)
                        {
                            return $"Lỗi trong quá trình lưu dữ liệu vào cơ sở dữ liệu: {ex.Message}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Lỗi khi xử lý file Excel: {ex.Message}";
            }
        }
    }
}