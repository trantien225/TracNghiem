using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using TracNghiem.Models;

namespace TracNghiem.Areas.Admin.Controllers
{
    public class TopicController : Controller
    {
        // GET: Admin/Topic
        THITRACNGHIEM_ONL onl = new THITRACNGHIEM_ONL();
        public ActionResult Index()
        {
            var courses = onl.HOCPHANs.ToList();
            ViewBag.Courses = new SelectList(courses, "MAHOCPHAN", "TENHOCPHAN");
            return View();
        }

        // Action để lấy danh sách bộ đề thi dựa trên mã học phần
        public ActionResult GetTopicsByCourse(string courseId)
        {
            var topics = onl.BODETHIs.Where(b => b.MAHOCPHAN == courseId).ToList();
            return PartialView("_TopicList", topics);
        }
        public PartialViewResult GetQuestionsByTopic(string bodethiId)
        {
            var questions = (from cauhoi in onl.CAUHOIs
                             join bc in onl.BODETHI_CAUHOI on cauhoi.MACAUHOI equals bc.MACAUHOI
                             join bd in onl.BODETHIs on bc.MABODETHI equals bd.MABODETHI
                             where bd.MABODETHI == bodethiId
                             select new Class2 // ViewModel để chứa thông tin câu hỏi cần thiết
                             {
                                 TENCAUHOI = cauhoi.TENCAUHOI,
                                 DAPAN_A = cauhoi.DAPAN_A,
                                 DAPAN_B = cauhoi.DAPAN_B,
                                 DAPAN_C = cauhoi.DAPAN_C,
                                 DAPAN_D = cauhoi.DAPAN_D,
                                 DAPANDUNG = cauhoi.DAPANDUNG
                                 // Các thông tin khác của câu hỏi nếu cần thiết
                             }).ToList();

            return PartialView("_QuestionList", questions);
        }

        public ActionResult ADDBODE()
        {
            var courses = onl.HOCPHANs.ToList(); // Lấy danh sách các HOCPHAN từ cơ sở dữ liệu

            // Tạo SelectList cho dropdown
            SelectList courseList = new SelectList(courses, "MAHOCPHAN", "TENHOCPHAN");

            // Tạo một đối tượng Class1 và gán các thuộc tính
            Class1 data = new Class1
            {
                BODETHI = new BODETHI(), // Khởi tạo BODETHI mới
                HOCPHANs = courses,      // Gán danh sách HOCPHANs
                CourseList = courseList  // Gán SelectList cho dropdownlist
            };

            // Truyền dữ liệu sang view
            return View(data);
        }

        [HttpPost]
        public ActionResult ADDBODE(BODETHI bodethi, string MAHOCPHAN)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Gán mã học phần từ tham số vào đối tượng bodethi
                    bodethi.MAHOCPHAN = MAHOCPHAN;

                    // Thêm đối tượng bodethi vào DbContext
                    onl.BODETHIs.Add(bodethi);

                    // Lưu các thay đổi vào cơ sở dữ liệu
                    onl.SaveChanges();

                    // Chuyển hướng về trang danh sách sau khi thêm thành công
                    return RedirectToAction("Index", "Topic"); // Điều hướng về Action "Index" trong Controller "Topic"
                }
            }
            catch (Exception ex)
            {
                // Log lỗi hoặc hiển thị thông báo lỗi
                ModelState.AddModelError("", "Có lỗi xảy ra khi thêm đề thi: " + ex.Message);
            }

            // Nếu có lỗi, trả lại view để người dùng sửa lại thông tin
            var model = new Class1
            {
                CourseList = new SelectList(onl.HOCPHANs, "MAHOCPHAN", "TENHOCPHAN"),
                BODETHI = bodethi // Truyền lại đối tượng bodethi để giữ lại thông tin người dùng đã nhập
            };

            return View("CreateBoDe", model); // Trả về view "CreateBoDe" với model để người dùng có thể sửa lại thông tin
        }

        //xóa
        [HttpPost]
        public ActionResult Remove(string id)
        {
            var code = new { Success = false, msg = "", code = -1, count = 0 };

            // Tìm bài kiểm tra (BODETHI) để xóa
            var exam = onl.BODETHIs.FirstOrDefault(x => x.MABODETHI == id);

            if (exam != null)
            {
                // Tìm tất cả ID câu hỏi liên quan đến bài kiểm tra này
                var questionIds = onl.BODETHI_CAUHOI.Where(bc => bc.MABODETHI == id).Select(bc => bc.MACAUHOI).ToList();

                // Xóa tất cả câu hỏi liên quan với bài kiểm tra này từ bảng CAUHOI
                foreach (var questionId in questionIds)
                {
                    var question = onl.CAUHOIs.FirstOrDefault(q => q.MACAUHOI == questionId);
                    if (question != null)
                    {
                        onl.CAUHOIs.Remove(question);
                    }
                }

                // Lưu các thay đổi vào bảng CAUHOI
                onl.SaveChanges();

                // Sau đó xóa bài kiểm tra từ bảng BODETHI
                onl.BODETHIs.Remove(exam);
                onl.SaveChanges();

                // Cập nhật mã trả về
                code = new { Success = true, msg = "Đã xóa bài kiểm tra và các câu hỏi liên quan thành công.", code = 1, count = onl.HOCPHANs.Count() };
            }
            else
            {
                code = new { Success = false, msg = "Không tìm thấy bài kiểm tra.", code = -1, count = 0 };
            }

            // Trả về phản hồi dưới dạng JSON
            return Json(code);
        }

        //thêm đề
        public ActionResult Add()
        {
            List<BODETHI> BD = onl.BODETHIs.ToList();
            return View(BD);
        }

        [HttpPost]
        public ActionResult Add(HttpPostedFileBase excelFile, string subjectName)
        {
            if (excelFile != null && excelFile.ContentLength > 0 && !string.IsNullOrEmpty(subjectName))
            {
                try
                {
                    string path = Path.Combine(Server.MapPath("~/App_Data/Uploads"), Path.GetFileName(excelFile.FileName));
                    excelFile.SaveAs(path);

                    SaveExcelToDatabase(path, subjectName);

                    TempData["Message"] = "File uploaded and data saved successfully!";
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "An error occurred while processing the file: " + ex.Message;
                }
            }
            else
            {
                TempData["Message"] = "Please select a valid Excel file and a subject.";
            }

            return RedirectToAction("Add");
        }

        private void SaveExcelToDatabase(string filePath, string subjectName)
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
                        try
                        {
                            string MACAUHOI = worksheet.Cells[row, 1].Value?.ToString().Trim();
                            BODETHI_CAUHOI ch = new BODETHI_CAUHOI
                            {
                                MACAUHOI = MACAUHOI,
                                MABODETHI = subjectName // Gán mã bộ câu hỏi được chọn
                            };

                            context.BODETHI_CAUHOI.Add(ch);
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
        //////////////////////////////////////////////////////////////////////////////////////////////////////


        [HttpGet]
        public ActionResult CreateBoDe()
        {
            //List<BODETHI> BD = onl.BODETHIs.ToList();
            //return View(BD);
            var emptyExams = onl.BODETHIs
            .Where(b => !onl.BODETHI_CAUHOI.Any(bc => bc.MABODETHI == b.MABODETHI))
            .ToList();

            return View(emptyExams);
        }

        // Hành động để lấy câu hỏi ngẫu nhiên cho một bộ đề cụ thể
        [HttpGet]
        public ActionResult GetRandomQuestions(string examId)
        {
            try
            {
                // Lấy mã học phần từ bộ đề thi
                var subjectId = onl.BODETHIs
                    .Where(b => b.MABODETHI == examId)
                    .Select(b => b.MAHOCPHAN)
                    .FirstOrDefault();

                if (subjectId == null)
                {
                    return Json(new { error = "Exam ID not found" }, JsonRequestBehavior.AllowGet);
                }

                // Lấy số lượng câu hỏi cần lấy từ BODETHI
                int numberOfQuestionsToFetch = onl.BODETHIs
                    .Where(b => b.MABODETHI == examId)
                    .Select(b => b.SOLUONGCAUHOI)
                    .FirstOrDefault();

                // Lấy danh sách câu hỏi ngẫu nhiên từ CAUHOIs
                var randomQuestions = onl.CAUHOIs
                    .Where(q => q.MAHOCPHAN == subjectId)
                    .AsEnumerable() // Chuyển sang enumerable để có thể dùng GroupBy và Guid.NewGuid()
                    .GroupBy(q => q.MACAUHOI) // Nhóm câu hỏi theo mã câu hỏi để loại bỏ trùng lặp
                    .Select(g => g.FirstOrDefault()) // Lấy câu hỏi đầu tiên của mỗi nhóm
                    .OrderBy(x => Guid.NewGuid()) // Sắp xếp ngẫu nhiên
                    .Take(numberOfQuestionsToFetch)
                    .Select(q => new
                    {
                        QuestionId = q.MACAUHOI,
                        QuestionText = q.TENCAUHOI,
                        AnswerA = q.DAPAN_A,
                        AnswerB = q.DAPAN_B,
                        AnswerC = q.DAPAN_C,
                        AnswerD = q.DAPAN_D,
                    })
                    .ToList();

                return Json(randomQuestions, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần thiết
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        // Hành động để xử lý tạo đề thi
        [HttpPost]
        public ActionResult CreateBoDe(string subjectName, List<string> selectedQuestions)
        {
            try
            {
                // Tạo bộ đề mới
                var bodeThi = new BODETHI
                {
                    MABODETHI = subjectName // Lấy MABODETHI từ form
                };

                // Lưu các câu hỏi vào BODETHI_CAUHOI
                foreach (var questionId in selectedQuestions)
                {
                    var bodeThiCauHoi = new BODETHI_CAUHOI
                    {
                        MACAUHOI = questionId,
                        MABODETHI = subjectName // Đặt MABOCAUHOI là MABODETHI
                    };

                    onl.BODETHI_CAUHOI.Add(bodeThiCauHoi);
                }

                // Lưu vào cơ sở dữ liệu
                onl.SaveChanges();

                // Redirect hoặc thông báo thành công
                return RedirectToAction("Index", "HomeAdmin", new { area = "admin" });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần thiết
                ViewBag.Error = "Đã xảy ra lỗi: " + ex.Message;
                return RedirectToAction("Error", "Shared", new { area = "" });
            }
        }

        //cho phép đề thi
        [HttpPost]
        public ActionResult AllowExam(string bodethiId)
        {
            var exam = onl.BODETHIs.SingleOrDefault(b => b.MABODETHI == bodethiId);
            if (exam != null)
            {
                exam.IsAllowed = true;
                onl.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}
