using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using TracNghiem.Models;

namespace TracNghiem
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            EnsureAdminUserExists();
        }
        private void EnsureAdminUserExists()
        {
            using (var context = new THITRACNGHIEM_ONL()) // Thay thế YourDbContext() bằng context của bạn
            {
                // Kiểm tra xem đã có tài khoản admin chưa
                var Adgv = context.GIANGVIENs.FirstOrDefault(u => u.MAGV == "admin");

                if (Adgv == null)
                {
                    // Tạo tài khoản admin mới
                    Adgv = new GIANGVIEN
                    {
                        MAGV = "admin",
                        HOTEN = "admin",
                        GIOITINH="Nam",
                        NGAYSINH = new DateTime(2002, 1, 1),
                        IMG ="admin.png",
                        SDT = "0978264832"
                        // Các thông tin khác của admin
                    };

                    // Thêm tài khoản admin vào cơ sở dữ liệu
                    context.GIANGVIENs.Add(Adgv);
                    context.SaveChanges();
                }
                // Kiểm tra xem đã có tài khoản admin chưa
                var adminUser = context.TAIKHOANs.FirstOrDefault(u => u.LOAITK == "admin");

                if (adminUser == null)
                {
                    // Tạo tài khoản admin mới
                    adminUser = new TAIKHOAN
                    {
                        LOAITK = "admin",
                        TENTK = "Admin",
                        MATKHAU = "$2a$10$vlA6znwq9.fdg4DsWwP8b.SFgJgE.6JVSc.anIUfEBGCWtyUT8796", // Thay bằng mật khẩu thực tế (hoặc sử dụng hàm băm để lưu trữ)
                        TRANGTHAI=true,
                        MAGV="admin"
                        // Các thông tin khác của admin
                    };

                    // Thêm tài khoản admin vào cơ sở dữ liệu
                    context.TAIKHOANs.Add(adminUser);
                    context.SaveChanges();
                }
            }
        }
    }
}
