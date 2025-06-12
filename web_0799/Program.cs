using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using web_0799.Models;
using web_0799.Repositories;

namespace web_0799
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Thêm dịch vụ session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian timeout của session
                options.Cookie.HttpOnly = true; // Cookie chỉ cho HTTP
                options.Cookie.IsEssential = true; // Cookie thiết yếu (cho GDPR)
            });

            // Cấu hình DbContext
            builder.Services.AddDbContext<ProductDBContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ST5")));

            // Cấu hình Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddDefaultTokenProviders()
                .AddDefaultUI()
                .AddEntityFrameworkStores<ProductDBContext>();

            // Cấu hình cookie cho Identity
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/Identity/Account/Login";
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
            });

            builder.Services.AddRazorPages();

            // Đăng ký repository
            builder.Services.AddScoped<IProductRepository, EFProductRepository>();
            builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseExceptionHandler("/Error");
            app.UseStatusCodePagesWithReExecute("/Error/{0}");
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Sử dụng session middleware (phải gọi sau UseStaticFiles và trước UseRouting)
            app.UseSession();

            app.UseRouting();
            app.UseAuthorization();
            app.MapRazorPages();
            app.MapStaticAssets();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}