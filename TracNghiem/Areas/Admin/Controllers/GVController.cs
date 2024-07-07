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
    public class GVController : Controller
    {
        // GET: Admin/GV
        THITRACNGHIEM_ONL onl = new THITRACNGHIEM_ONL();
        public ActionResult Index(string search)
        {
            List<GIANGVIEN> GV;
            if (string.IsNullOrEmpty(search))
            {
                GV = onl.GIANGVIENs.ToList();
            }
            else
            {
                GV = onl.GIANGVIENs.Where(t => t.MAGV.Contains(search)).ToList();
            }
            return View(GV);
        }
        public ActionResult PhanCong()
        {
            List<tkbthi> lichThiViewModelList = new List<tkbthi>(); // Danh sách ViewModel để hiển thị

            // Đoạn code để lấy dữ liệu từ cơ sở dữ liệu, ví dụ:
            using (THITRACNGHIEM_ONL db = new THITRACNGHIEM_ONL()) // Thay YourDbContext bằng tên DbContext của bạn
            {
                // Thực hiện truy vấn để lấy thông tin từ các bảng
                var query = from lt in db.LICHTHIs
                            join lp in db.LOPHOCPHANs on lt.MALOPHOCPHAN equals lp.MALOPHOCPHAN
                            join hp in db.HOCPHANs on lt.MAHOCPHAN equals hp.MAHOCPHAN // Thêm join với bảng HOCPHAN
                            join gv1 in db.GIANGVIENs on lt.GV01 equals gv1.MAGV into gv1Group
                            from gv1 in gv1Group.DefaultIfEmpty()
                            join gv2 in db.GIANGVIENs on lt.GV02 equals gv2.MAGV into gv2Group
                            from gv2 in gv2Group.DefaultIfEmpty()
                            select new tkbthi
                            {
                                TenLopHocPhan = lp.TENLOPHOCPHAN,
                                TenGV1 = gv1.HOTEN,
                                TenGV2 = gv2.HOTEN,
                                TenHocPhan = hp.TENHOCPHAN, // Lấy thông tin TenHocPhan từ HOCPHAN
                                NgayThi = lt.NGAYTHI,
                                PHONGTHI = lt.PHONGTHI,
                                THOIGIANTHI = lt.THOIGIANTHI.ToString(),
                                ThoiLuongThi = lt.THOILUONGTHI.ToString()
                            };

                lichThiViewModelList = query.ToList(); // Chuyển kết quả thành danh sách ViewModel
            }

            // Trả về View với danh sách lịch thi và các thông tin liên quan
            return View(lichThiViewModelList);
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
                int colCount = worksheet.Dimension.Columns;

                using (var context = new THITRACNGHIEM_ONL())
                {
                    for (int row = 2; row <= rowCount; row++) // Bỏ qua hàng tiêu đề
                    {
                        string column1 = worksheet.Cells[row, 1].Value?.ToString().Trim();
                        string column2 = worksheet.Cells[row, 2].Value?.ToString().Trim();
                        string column3 = worksheet.Cells[row, 3].Value?.ToString().Trim();
                        DateTime? column4 = worksheet.Cells[row, 4].GetValue<DateTime?>();
                        string column5 = worksheet.Cells[row, 5].Value?.ToString().Trim();
                        string column6 = worksheet.Cells[row, 6].Value?.ToString().Trim();

                        GIANGVIEN SV = new GIANGVIEN
                        {
                            MAGV = column1,
                            HOTEN = column2,
                            GIOITINH = column3,
                            NGAYSINH = (DateTime)column4,
                            IMG = column5,
                            SDT = column6,


                        };

                        try
                        {
                            context.GIANGVIENs.Add(SV);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error inserting row {row}: {ex.Message}");
                        }
                    }

                    try
                    {
                        context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error saving changes: {ex.Message}");
                    }
                }
            }
        }
    }
}