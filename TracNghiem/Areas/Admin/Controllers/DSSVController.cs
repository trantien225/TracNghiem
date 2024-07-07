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
    public class DSSVController : Controller
    {
        // GET: Admin/DSSV
        THITRACNGHIEM_ONL onl = new THITRACNGHIEM_ONL();
        public ActionResult Index(string search)
        {
            List<SINHVIEN> SV;
            if (string.IsNullOrEmpty(search))
            {
                SV = onl.SINHVIENs.ToList();
            }
            else
            {
                SV = onl.SINHVIENs.Where(t => t.MSSV.Contains(search)).ToList();
            }
            return View(SV);
        }
        public ActionResult Add()
        {
            List<LOPHOCPHAN> LHP = onl.LOPHOCPHANs.ToList();
            return View(LHP);
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

                        var existingStudent = context.SINHVIENs.FirstOrDefault(s => s.MSSV == column1);

                        if (existingStudent == null)
                        {
                            // Nếu sinh viên chưa tồn tại, thêm mới
                            SINHVIEN SV = new SINHVIEN
                            {
                                MSSV = column1,
                                HOTEN = column2,
                                GIOITINH = column3,
                                NGAYSINH = column4 ?? DateTime.Now, // Sử dụng ngày hiện tại nếu null
                                IMG = column5,
                                SDT = column6,
                            };

                            try
                            {
                                context.SINHVIENs.Add(SV);
                                context.SaveChanges(); // Lưu sinh viên mới vào cơ sở dữ liệu để có thể sử dụng MSSV ngay lập tức
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error inserting row {row}: {ex.Message}");
                            }
                        }

                        // Thêm hoặc cập nhật quan hệ LOPHOCPHAN_SINHVIEN
                        var existingLHP_SV = context.LOPHOCPHAN_SINHVIEN.FirstOrDefault(lhp_sv => lhp_sv.MSSV == column1 && lhp_sv.MALOPHOCPHAN == maLopHocPhan);

                        if (existingLHP_SV == null)
                        {
                            LOPHOCPHAN_SINHVIEN LHP_SV = new LOPHOCPHAN_SINHVIEN
                            {
                                MSSV = column1,
                                MALOPHOCPHAN = maLopHocPhan,
                            };

                            try
                            {
                                context.LOPHOCPHAN_SINHVIEN.Add(LHP_SV);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error inserting LOPHOCPHAN_SINHVIEN row {row}: {ex.Message}");
                            }
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