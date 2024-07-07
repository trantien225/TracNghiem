using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using TracNghiem.Models;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace TracNghiem.Areas.Admin.Controllers
{
    public class TKBController : Controller
    {
        private THITRACNGHIEM_ONL onl = new THITRACNGHIEM_ONL();

        // GET: Admin/TKB
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Hocphan()
        {
            List<HOCPHAN> HP = onl.HOCPHANs.ToList();
            return View(HP);
        }

        // GET: Admin/TKB/Add
        public ActionResult Add()
        {
            var model = new Class4
            {
                LOPHOCPHANs = onl.LOPHOCPHANs.ToList(),
                HOCPHANs = onl.HOCPHANs.ToList(),
                GIANGVIENs = onl.GIANGVIENs.ToList(),
                LICHTHI = new LICHTHI()
            };
            ViewBag.PhongThiList = GetPhongThiList();
            return View(model);
        }
        private List<String> GetPhongThiList()
        {
            return new List<String>
            {
                "A101", "A102","A103","A104","A105",
                "B101", "B102","B103","B104", "B105"
            // Thêm các phòng thi khác nếu cần
            };
        }
        // CHECK LỊCH TRÙNG TRƯỚC KHI THÊM
        private bool IsScheduleConflict(LICHTHI newSchedule)
        {
            using (var dbContext = new THITRACNGHIEM_ONL())
            {
                var conflictingSchedules = dbContext.LICHTHIs
                    .Where(l =>
                        l.ID != newSchedule.ID &&
                        l.NGAYTHI == newSchedule.NGAYTHI &&
                        l.PHONGTHI == newSchedule.PHONGTHI)
                    .ToList();

                foreach (var schedule in conflictingSchedules)
                {
                    TimeSpan existingStartTime = TimeSpan.Parse(schedule.THOIGIANTHI);
                    TimeSpan newStartTime = TimeSpan.Parse(newSchedule.THOIGIANTHI);

                    // Chuyển đổi THOILUONGTHI thành số nguyên
                    int existingDuration = int.Parse(schedule.THOILUONGTHI);
                    int newDuration = int.Parse(newSchedule.THOILUONGTHI);

                    TimeSpan existingEndTime = existingStartTime.Add(TimeSpan.FromMinutes(existingDuration));
                    TimeSpan newEndTime = newStartTime.Add(TimeSpan.FromMinutes(newDuration));

                    if ((newStartTime >= existingStartTime && newStartTime < existingEndTime) ||
                        (newEndTime > existingStartTime && newEndTime <= existingEndTime) ||
                        (existingStartTime >= newStartTime && existingStartTime < newEndTime))
                    {
                        return true;
                    }
                }

                return false;
            }
        }


        //LÊNH LỊCH LẤY MÃ HỌC PHẦN LỌC LỚP HỌC PHẦN
        [HttpGet]
        public JsonResult GetLopHocPhansByHocPhan(string maHocPhan)
        {
            var lopHocPhans = onl.LOPHOCPHANs
                                .Where(l => l.MAHOCPHAN == maHocPhan)
                                .Select(l => new { l.MALOPHOCPHAN, l.TENLOPHOCPHAN })
                                .ToList();

            return Json(lopHocPhans, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Add(Class4 model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (var dbContext = new THITRACNGHIEM_ONL())
                    {
                        // Kiểm tra lịch thi trùng
                        if (IsScheduleConflict(model.LICHTHI))
                        {
                            ViewBag.Error = "Lịch thi bị trùng, vui lòng chọn thời gian hoặc phòng thi khác.";
                            return View(model);
                        }

                        // Kiểm tra xem có phải chỉnh sửa hay thêm mới
                        if (model.LICHTHI.ID > 0)
                        {
                            var existingEntity = dbContext.LICHTHIs.FirstOrDefault(l => l.ID == model.LICHTHI.ID);
                            if (existingEntity != null)
                            {
                                // Cập nhật thông tin từ model vào entity đã tồn tại
                                existingEntity.MAHOCPHAN = model.LICHTHI.MAHOCPHAN;
                                existingEntity.MALOPHOCPHAN = model.LICHTHI.MALOPHOCPHAN;
                                existingEntity.GV01 = model.LICHTHI.GV01;
                                existingEntity.GV02 = model.LICHTHI.GV02;
                                existingEntity.PHONGTHI = model.LICHTHI.PHONGTHI;
                                existingEntity.NGAYTHI = model.LICHTHI.NGAYTHI;
                                existingEntity.THOIGIANTHI = model.LICHTHI.THOIGIANTHI;
                                existingEntity.THOILUONGTHI = model.LICHTHI.THOILUONGTHI;
                                existingEntity.GHICHU = model.LICHTHI.GHICHU;
                            }
                            else
                            {
                                throw new Exception("Không tìm thấy bản ghi cần chỉnh sửa.");
                            }
                        }
                        else
                        {
                            // Thêm entity mới vào DbContext
                            dbContext.LICHTHIs.Add(model.LICHTHI);
                        }

                        // Lưu các thay đổi vào cơ sở dữ liệu
                        dbContext.SaveChanges();

                        ViewBag.Message = "Thêm/chỉnh sửa lịch thi thành công!";
                        return RedirectToAction("PhanCong", "GV", new { area = "admin" });
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Đã xảy ra lỗi: " + ex.Message;
                //return RedirectToAction("Error", "Shared", new { area = "" });
            }

            // Nếu có lỗi hoặc ModelState không hợp lệ, trả lại view để người dùng sửa lại thông tin
            using (var dbContext = new THITRACNGHIEM_ONL())
            {
                model.LOPHOCPHANs = dbContext.LOPHOCPHANs.ToList();
                model.HOCPHANs = dbContext.HOCPHANs.ToList();
                model.GIANGVIENs = dbContext.GIANGVIENs.ToList();
                ViewBag.PhongThiList = GetPhongThiList();
            }

            return View(model);
        }

        //[HttpPost]
        //public ActionResult Remove(string id)
        //{
        //    var code = new { Success = false, msg = "", code = -1, count = 0 };
        //    var checkCategory = onl.HOCPHANs.FirstOrDefault(x => x.MAHOCPHAN == id);
        //    if (checkCategory != null)
        //    {
        //        onl.HOCPHANs.Remove(checkCategory);
        //        onl.SaveChanges();
        //        code = new { Success = true, msg = "", code = 1, count = onl.HOCPHANs.Count() };
        //    }
        //    return Json(code);
        //}
        //cho phép hiện đề
        public ActionResult AllowExam()
        {
            var exams = onl.BODETHIs.ToList();
            return View(exams);
        }
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

        [HttpPost]
        public ActionResult DisallowExam(string bodethiId)
        {
            var exam = onl.BODETHIs.SingleOrDefault(b => b.MABODETHI == bodethiId);
            if (exam != null)
            {
                exam.IsAllowed = false;
                onl.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }



        public ActionResult GenerateSchedules(Class4 model, DateTime? StartDate, DateTime? EndDate)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (StartDate == null || EndDate == null || StartDate > EndDate)
                    {
                        ViewBag.Error = "Vui lòng chọn khoảng thời gian hợp lệ.";
                        return View(model);
                    }
                    using (var dbContext = new THITRACNGHIEM_ONL())
                    {
                        // Kiểm tra lịch thi trùng
                        if (IsScheduleConflict(model.LICHTHI))
                        {
                            ViewBag.Error = "Lịch thi bị trùng, vui lòng chọn thời gian hoặc phòng thi khác.";
                            return View(model);
                        }

                        // Logic để tạo lịch thi
                        var listLopHocPhan = dbContext.LOPHOCPHANs.Where(l => l.MAHOCPHAN == model.LICHTHI.MAHOCPHAN).ToList();

                        // Ví dụ: Gán ngẫu nhiên phòng thi cho mỗi lớp học phần
                        var random = new Random();

                        // Lấy danh sách phòng thi
                        var phongThiList = GetPhongThiList();

                        // Random ngày thi trong khoảng được chọn
                        DateTime randomDate = StartDate.Value.AddDays(random.Next((EndDate - StartDate).Value.Days + 1));
                        model.LICHTHI.NGAYTHI = randomDate;

                        // Random thời gian thi
                        var thoiGianThiDict = new Dictionary<string, TimeSpan>();

                        foreach (var lopHocPhan in listLopHocPhan)
                        {
                            string selectedPhongThi;
                            bool foundUniqueRoom = false;

                            // Chọn phòng thi ngẫu nhiên và đảm bảo không trùng phòng thi với các lớp học phần khác của cùng môn
                            do
                            {
                                selectedPhongThi = phongThiList[random.Next(phongThiList.Count)];

                                // Kiểm tra xem phòng thi đã được sử dụng cho các lớp học phần khác của cùng môn hay chưa
                                var isConflict = dbContext.LICHTHIs.Any(l =>
                                                    l.NGAYTHI == model.LICHTHI.NGAYTHI &&
                                                    l.THOIGIANTHI == model.LICHTHI.THOIGIANTHI &&
                                                    l.PHONGTHI == selectedPhongThi &&
                                                    l.MAHOCPHAN == model.LICHTHI.MAHOCPHAN &&
                                                    l.MALOPHOCPHAN != lopHocPhan.MALOPHOCPHAN);

                                if (!isConflict)
                                {
                                    foundUniqueRoom = true;
                                }

                            } while (!foundUniqueRoom);

                            var selectedGVs = GetRandomGiangViens(dbContext, selectedPhongThi, model.LICHTHI.NGAYTHI, model.LICHTHI.THOIGIANTHI);
                            TimeSpan thoiGianThi;
                            if (!thoiGianThiDict.ContainsKey(model.LICHTHI.MAHOCPHAN))
                            {
                                thoiGianThi = GetRandomTime();
                                thoiGianThiDict[model.LICHTHI.MAHOCPHAN] = thoiGianThi;
                            }
                            else
                            {
                                thoiGianThi = thoiGianThiDict[model.LICHTHI.MAHOCPHAN];
                            }

                            // Tạo và lưu thực thể LICHTHI cho mỗi lớp học phần
                            var newSchedule = new LICHTHI
                            {
                                MAHOCPHAN = model.LICHTHI.MAHOCPHAN,
                                MALOPHOCPHAN = lopHocPhan.MALOPHOCPHAN,
                                GV01 = selectedGVs[0].MAGV,
                                GV02 = selectedGVs[1].MAGV,
                                PHONGTHI = selectedPhongThi,
                                NGAYTHI = model.LICHTHI.NGAYTHI,
                                THOIGIANTHI = thoiGianThi.ToString(),
                                THOILUONGTHI = model.LICHTHI.THOILUONGTHI,
                                GHICHU = model.LICHTHI.GHICHU
                            };

                            // Lưu newSchedule vào cơ sở dữ liệu
                            dbContext.LICHTHIs.Add(newSchedule);
                            dbContext.SaveChanges();
                        }

                        ViewBag.Message = $"Đã tạo thành công {listLopHocPhan.Count} lịch thi!";
                        return RedirectToAction("PhanCong", "GV", new { area = "admin" });
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Đã xảy ra lỗi: " + ex.Message;
            }

            // Nếu có lỗi hoặc ModelState không hợp lệ, trở về view Add với dữ liệu model
            using (var dbContext = new THITRACNGHIEM_ONL())
            {
                model.LOPHOCPHANs = dbContext.LOPHOCPHANs.ToList();
                model.HOCPHANs = dbContext.HOCPHANs.ToList();
                model.GIANGVIENs = dbContext.GIANGVIENs.ToList();
                ViewBag.PhongThiList = GetPhongThiList();
            }

            return View("Add", model);
        }

        // Hàm random phòng thi
        private string GetRandomPhongThi()
        {
            var phongThiList = GetPhongThiList();
            var random = new Random();
            int index = random.Next(phongThiList.Count);
            return phongThiList[index];
        }

        // Hàm random giảng viên cho phòng thi
        private List<GIANGVIEN> GetRandomGiangViens(THITRACNGHIEM_ONL dbContext, string phongThi, DateTime ngayThi, string thoiGianThi)
        {
            var giangViens = dbContext.GIANGVIENs.ToList();
            var random = new Random();

            // Chuyển đổi TimeSpan thành chuỗi để so sánh với THOIGIANTHI trong cơ sở dữ liệu
            string thoiGianThiString = thoiGianThi;

            // Tìm danh sách giảng viên chưa được phân công vào phòng thi khác vào cùng ngày và cùng thời gian
            var availableGVs = giangViens.Where(gv =>
                gv.MAGV != "Admin" && !dbContext.LICHTHIs.Any(l =>
                    l.NGAYTHI == ngayThi &&
                    l.THOIGIANTHI == thoiGianThiString &&
                    (l.GV01 == gv.MAGV || l.GV02 == gv.MAGV) &&
                    l.PHONGTHI != phongThi // Phòng thi khác
                )).ToList();

            // Xáo trộn danh sách giảng viên có sẵn và chọn ngẫu nhiên 2 giảng viên
            availableGVs = availableGVs.OrderBy(gv => random.Next()).ToList();

            return availableGVs.Take(2).ToList();
        }


        private TimeSpan GetRandomTime()
        {
            Random random = new Random();
            int period = random.Next(0, 3); // Random chọn khoảng thời gian

            int hour, minute;

            switch (period)
            {
                case 0:
                    // Khoảng thời gian 7h-12h
                    hour = random.Next(7, 12); // Random giờ từ 7h đến 11h
                    minute = (random.Next(0, 12)) * 5; // Random phút chia hết cho 5 từ 0 đến 55
                    break;
                case 1:
                    // Khoảng thời gian 12h30-17h30
                    hour = random.Next(12, 18); // Random giờ từ 12h đến 17h
                    if (hour == 12)
                    {
                        minute = (random.Next(6, 12)) * 5; // Nếu là 12h, random phút chia hết cho 5 từ 30 đến 55
                    }
                    else
                    {
                        minute = (random.Next(0, 12)) * 5; // Random phút chia hết cho 5 từ 0 đến 55
                    }
                    break;
                case 2:
                    // Khoảng thời gian 18h-21h45
                    hour = random.Next(18, 22); // Random giờ từ 18h đến 21h
                    if (hour == 21)
                    {
                        minute = (random.Next(0, 10)) * 5; // Nếu là 21h, random phút chia hết cho 5 từ 0 đến 45
                    }
                    else
                    {
                        minute = (random.Next(0, 12)) * 5; // Random phút chia hết cho 5 từ 0 đến 55
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new TimeSpan(hour, minute, 0); // Trả về TimeSpan với giờ và phút ngẫu nhiên
        }

    }
}


