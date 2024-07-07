using System;
using System.Data.Entity;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using TracNghiem.Models;

namespace TracNghiem.Areas.Admin.Controllers
{
    public class HomeAdminController : Controller
    {
        THITRACNGHIEM_ONL db = new THITRACNGHIEM_ONL();
        // GET: Admin/Home
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
                Console.WriteLine($"Exception: {ex.Message}");
                return View("Error");
            }
        }

        public ActionResult Backup()
        {
            return View();
        }

        // POST: Admin/Home/BackupDatabase
        [HttpPost]
        public ActionResult BackupDatabase(string backupFileName)
        {
            try
            {
                string databaseName = "THITRACNGHIEM_ONL"; // Thay thế bằng tên cơ sở dữ liệu của bạn
                string backupFolder = Server.MapPath("~/Backups/"); // Đường dẫn thư mục lưu trữ sao lưu

                // Tạo thư mục sao lưu nếu nó chưa tồn tại
                if (!Directory.Exists(backupFolder))
                {
                    Directory.CreateDirectory(backupFolder);
                }

                // Lấy tên tệp sao lưu từ người dùng (nếu có)
                if (string.IsNullOrEmpty(backupFileName))
                {
                    // Nếu người dùng không nhập, sử dụng tên mặc định
                    backupFileName = $"{databaseName}_{DateTime.Now:yyyyMMddHHmmss}.bak";
                }

                // Tạo đường dẫn và tên file sao lưu
                string backupFilePath = Path.Combine(backupFolder, backupFileName);

                // Lệnh SQL sao lưu cơ sở dữ liệu
                string backupQuery = $"BACKUP DATABASE [{databaseName}] TO DISK = '{backupFilePath}' WITH INIT";

                // Thực thi lệnh sao lưu
                using (var context = new THITRACNGHIEM_ONL()) // Thay thế bằng tên lớp DbContext của bạn
                {
                    context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, backupQuery);
                }

                // Lưu thông tin kết quả sao lưu vào ViewBag để hiển thị trong view
                ViewBag.Message = $"Đã sao lưu cơ sở dữ liệu {databaseName} vào tệp {backupFileName} thành công vào lúc {DateTime.Now}.";
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi trong quá trình sao lưu cơ sở dữ liệu: {ex.Message}";
                //return RedirectToAction("Error", "Shared", new { area = "" });
                return View("Backupdatabase");
            }

            // Trả về view để hiển thị kết quả sao lưu
            return View("Backupdatabase");
        }

        [HttpPost]
        public ActionResult RestoreDatabase(HttpPostedFileBase backupFile)
        {
            try
            {
                if (backupFile == null || backupFile.ContentLength == 0)
                {
                    ViewBag.Error = "Vui lòng chọn tệp sao lưu để phục hồi.";
                    return View("Index"); // Trả về view để hiển thị thông báo lỗi
                }

                string databaseName = "THITRACNGHIEM_ONL"; // Thay thế bằng tên cơ sở dữ liệu của bạn
                string backupFolder = Server.MapPath("~/Backups/"); // Đường dẫn thư mục lưu trữ sao lưu

                // Tạo thư mục sao lưu nếu nó chưa tồn tại
                if (!Directory.Exists(backupFolder))
                {
                    Directory.CreateDirectory(backupFolder);
                }

                // Lưu tệp sao lưu vào thư mục lưu trữ
                string backupFileName = Path.GetFileName(backupFile.FileName);
                string backupFilePath = Path.Combine(backupFolder, backupFileName);
                backupFile.SaveAs(backupFilePath);

                // Lệnh SQL phục hồi cơ sở dữ liệu với tùy chọn ghi đè (REPLACE)
                string restoreQuery = $"USE master RESTORE DATABASE [{databaseName}] FROM DISK = '{backupFilePath}' WITH REPLACE";

                // Thực thi lệnh phục hồi
                using (var context = new THITRACNGHIEM_ONL()) // Thay thế bằng tên lớp DbContext của bạn
                {
                    context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, restoreQuery);
                }

                ViewBag.Message = $"Phục hồi cơ sở dữ liệu {databaseName} từ tệp '{backupFileName}' thành công vào lúc {DateTime.Now}.";
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi trong quá trình phục hồi cơ sở dữ liệu: {ex.Message}";
                return View("Backupdatabase");
            }

            return RedirectToAction("Index", "Login", new { area = "" });
        }

    }
}
