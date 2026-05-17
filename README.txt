========================================================================
✈️ SKYBLUE AIRLINE - HỆ THỐNG QUẢN LÝ HÀNG KHÔNG TOÀN DIỆN
========================================================================

SkyBlue Airline là một giải pháp quản lý hàng không hiện đại, bảo mật và 
hiệu quả dành cho nền tảng Windows. Ứng dụng mang đến trải nghiệm điều hành
bay trực quan, quản lý vé thông minh, tự động hóa quy trình phân ca phi
hành đoàn và tối ưu hóa doanh thu thông qua hệ thống báo cáo phân tích mạnh mẽ.

🔗 Trang web chính thức & Tải ứng dụng: https://skyblue-airline.netlify.app/
(Tại đây bạn có thể tải xuống bộ cài đặt chính thức SkyBlueAirline_Setup_v1.0.5.exe dành cho Windows)

------------------------------------------------------------------------
🌟 TÍNH NĂNG NỔI BẬT
------------------------------------------------------------------------

Hệ thống được phát triển với đầy đủ các phân hệ chức năng chuyên sâu, 
phục vụ tối đa nhu cầu vận hành của một hãng hàng không thực tế:

1. TRANG CHỦ & TỔNG QUAN: Giao diện điều khiển (Dashboard) trực quan 
   hiển thị nhanh các thống kê quan trọng, biểu đồ và phím tắt nhanh 
   đến các tính năng khác.
   
2. QUẢN LÝ LỊCH BAY: Lên lịch trình bay, điểm xuất phát/điểm đến, 
   thời gian bay, gán tàu bay tự động và tối ưu hóa giờ bay để 
   hạn chế xung đột.
   
3. TUYẾN BAY & ĐỘI BAY: Quản lý mạng lưới đường bay toàn cầu, thông 
   tin chi tiết từng máy bay trong đội bay và thiết lập thông số kỹ thuật.
   
4. CẤU HÌNH HẠNG GHẾ: Thiết lập các hạng vé (Thương gia, Phổ thông,...),
   định hình giá vé cơ bản và các quy tắc áp dụng phụ phí linh hoạt.
   
5. TÌM KIẾM CHUYẾN BAY: Bộ lọc tìm kiếm thông minh giúp tra cứu nhanh 
   chuyến bay theo địa điểm, thời gian, số hiệu bay chỉ trong vài mili-giây.
   
6. ĐẶT VÉ & CHỌN GHẾ: Quy trình đặt vé hiện đại với Sơ đồ chọn ghế 
   trực quan (Seat Selection Map) theo thời gian thực, đảm bảo không 
   trùng ghế.
   
7. QUẢN LÝ & XUẤT VÉ: Theo dõi trạng thái vé, hủy vé, thay đổi thông 
   tin và tự động hóa quy trình xuất/in vé PDF chất lượng cao.
   
8. QUẢN LÝ DỊCH VỤ: Quản lý dịch vụ đi kèm như hành lý ký gửi, suất ăn
   trên máy bay, phòng chờ thương gia nhằm tăng doanh thu phụ trợ.
   
9. QUẢN LÝ NHÂN SỰ: Phân công phi công, tiếp viên hàng không vào phi 
   hành đoàn của từng chuyến bay, quản lý hồ sơ nhân viên và ca trực.
   
10. CSKH & GỬI EMAIL: Dịch vụ gửi email thông báo tự động (thông tin 
    đặt vé, thay đổi lịch bay) và tiếp nhận ý kiến phản hồi của khách hàng.
    
11. BÁO CÁO & THỐNG KÊ: Trực quan hóa doanh thu, tỷ lệ lấp đầy ghế 
    (occupancy rate) qua biểu đồ cột/đường chuyên nghiệp phục vụ 
    ban quản trị.
    
12. LỊCH SỬ HỆ THỐNG: Ghi nhận chi tiết lịch sử hoạt động (Audit Log) 
    của nhân viên để tăng tính minh bạch và bảo mật dữ liệu.

------------------------------------------------------------------------
📐 KIẾN TRÚC DỰ ÁN (3-LAYER ARCHITECTURE)
------------------------------------------------------------------------

Dự án tuân thủ nghiêm ngặt mô hình kiến trúc 3 lớp (3-Layer Architecture) 
chuẩn hóa trong phát triển phần mềm doanh nghiệp, giúp hệ thống có tính 
độc lập cao, dễ bảo trì và mở rộng:

                   [ Lớp Giao diện - GUI/Presentation ]
                                   │
                                   ▼
                 [ Lớp Xử lý Nghiệp vụ - BUS/Business ]
                                   │
                                   ▼
                [ Lớp Truy cập Dữ liệu - DAL/Data Access ]
                                   │
                                   ▼
                 [( Cơ sở dữ liệu - SQL Server Database )]

* Lưu ý: Lớp Đối tượng Truyền tải dữ liệu - DTO (Data Transfer Object) 
chứa các cấu trúc lớp mô hình hóa dữ liệu (Model) dùng chung để truyền tải
dữ liệu trực tiếp xuyên suốt các tầng kiến trúc GUI -> BUS -> DAL.

------------------------------------------------------------------------
💻 CÔNG NGHỆ SỬ DỤNG
------------------------------------------------------------------------

- Công nghệ cốt lõi: C#, .NET Framework / .NET Core, WPF.
- Giao diện & UI/UX: 
  + Material Design in XAML Toolkit (UI hiện đại, Material icons, DialogHost).
  + LiveCharts (Biểu đồ thống kê).
  + Phông chữ: Outfit (Google Fonts) kết hợp thiết kế Glassmorphism trên web.
- Cơ sở dữ liệu: Microsoft SQL Server (sử dụng Stored Procedure tăng tốc).
- Thông báo & Email: MailKit & MimeKit để gửi email thông báo tự động.
- Web Landing Page: HTML5, CSS3, JavaScript, host trên Netlify.

------------------------------------------------------------------------
📁 CẤU TRÚC THƯ MỤC DỰ ÁN
------------------------------------------------------------------------

FlightManagement_Project_Update/
├── FlightManagement/                      # Mã nguồn ứng dụng WPF Desktop
│   ├── FlightManagement.sln               # File Solution của Visual Studio
│   ├── FlightManagement/                  # Dự án chính (GUI, Assets, App.xaml)
│   │   ├── UserControls/                  # Phân hệ chức năng ứng dụng
│   │   ├── Services/                      # Các dịch vụ (Email, PDF,...)
│   │   └── Helpers/                       # Hàm bổ trợ
│   ├── BUS/                               # Business Logic Layer (Nghiệp vụ)
│   ├── DAL/                               # Data Access Layer (Truy xuất DB)
│   ├── DTO/                               # Data Transfer Object (Model)
│   ├── Database/                          # File script SQL tạo cơ sở dữ liệu
│   └── PrintTicket/                       # Phân hệ xử lý in vé
│
└── Web/                                   # Mã nguồn trang tải ứng dụng
    ├── index.html                         # Giao diện Landing Page
    └── SkyBlueAirline_Setup_v1.0.5.exe    # Bộ cài đặt Windows chính thức

------------------------------------------------------------------------
🚀 HƯỚNG DẪN CÀI ĐẶT & KHỞI CHẠY
------------------------------------------------------------------------

A. DÀNH CHO NGƯỜI DÙNG CUỐI:
1. Truy cập trang web: https://skyblue-airline.netlify.app/
2. Click nút "Tải xuống" để tải bộ cài đặt "SkyBlueAirline_Setup_v1.0.5.exe".
3. Mở tệp cài đặt và tiến hành cài đặt ứng dụng theo hướng dẫn.
4. Mở ứng dụng từ màn hình Desktop và bắt đầu trải nghiệm!

B. DÀNH CHO LẬP TRÌNH VIÊN (DEVELOPERS):
Yêu cầu hệ thống: Visual Studio 2022+, SQL Server.

1. Clone dự án:
   git clone https://github.com/truongquy2k6/skyblue-airlines-system.git
   
2. Thiết lập Database:
   Mở SQL Server Management Studio (SSMS), chạy file script SQL trong thư
   mục FlightManagement/Database/ để khởi tạo database cùng Stored Procedures.
   
3. Cấu hình Connection String:
   Cập nhật chuỗi kết nối trong file cấu hình dự án (App.config) để trỏ đến
   SQL Server của bạn.
   
4. Chạy dự án:
   Mở Visual Studio, mở solution FlightManagement.sln, nhấn F5 để khởi chạy.

------------------------------------------------------------------------
📞 THÔNG TIN HỖ TRỢ & LIÊN HỆ
------------------------------------------------------------------------

Nếu bạn có bất kỳ câu hỏi nào liên quan đến ứng dụng hoặc kỹ thuật:

* Email Hỗ trợ: support@skyblueairline.com
* Hotline Hỗ trợ 24/7: 1900 6789 (Nhấn phím 1)
* Website Dự án: https://skyblue-airline.netlify.app/

------------------------------------------------------------------------
© 2026 SkyBlue Airline. All rights reserved. Developed for Excellence.
