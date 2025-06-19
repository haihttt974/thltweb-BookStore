# BookStore ASP.NET

Đây là dự án website bán sách được xây dựng bằng ASP.NET Core MVC. Ứng dụng sử dụng Entity Framework Core, ASP.NET Identity và Session để quản lý người dùng và dữ liệu.

## Cài đặt

1. Cài đặt .NET SDK 9.0 hoặc mới hơn.
2. Mở solution `web_0799.sln` bằng Visual Studio hoặc chạy dòng lệnh:
   ```bash
   cd web_0799
   dotnet build
   ```
3. Cập nhật chuỗi kết nối có tên `ST5` trong tệp cấu hình hoặc user secrets.
4. Áp dụng các migrations để tạo cơ sở dữ liệu:
   ```bash
   dotnet ef database update
   ```
5. Khởi chạy ứng dụng:
   ```bash
   dotnet run
   ```

## Các tính năng chính

- Quản lý sản phẩm, danh mục và đơn hàng.
- Hỗ trợ đăng ký, đăng nhập người dùng.
- Giỏ hàng và đặt hàng trực tuyến.

## Cấu trúc thư mục

- `Areas/Admin` - Trang quản trị dành cho quản trị viên.
- `Controllers` - Các controller của trang người dùng.
- `Models` - Định nghĩa các lớp mô hình và DbContext.
- `Repositories` - Lớp truy cập dữ liệu.
- `Views`, `ViewComponents` - Giao diện Razor hiển thị dữ liệu.

