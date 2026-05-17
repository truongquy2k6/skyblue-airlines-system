using Microsoft.Data.SqlClient;
using System.Data;

namespace DAL
{
    // Lớp TrangChuDAL chuyên trách việc tổng hợp dữ liệu để hiển thị lên bảng điều khiển (Dashboard) của màn hình Trang chủ
    // Các thao tác này thường yêu cầu xử lý logic thống kê phức tạp ở dưới Stored Procedure
    public class TrangChuDAL
    {
        // Phương thức lấy các chỉ số thống kê tổng quan (Ví dụ: Tổng số chuyến bay, số vé đã bán, tổng doanh thu trong ngày...)
        public DataTable ThongKe() => DatabaseHelper.ExecuteQuery("sp_TrangChu_ThongKe");
        
        // Phương thức trích xuất danh sách các chuyến bay sẽ cất cánh trong ngày hôm nay để nhân viên tiện theo dõi tiến độ
        public DataTable LichBayHomNay() => DatabaseHelper.ExecuteQuery("sp_TrangChu_LichBayHomNay");
    }

    // Lớp QuocGiaDAL đảm nhiệm việc lấy danh mục các Quốc gia (Countries) từ cơ sở dữ liệu
    // Dùng để làm nguồn dữ liệu cho các ComboBox chọn quốc tịch khi điền hộ chiếu hành khách hoặc tạo sân bay mới
    public class QuocGiaDAL
    {
        // Phương thức gọi thủ tục sp_QuocGia_HienThi để lấy bảng chứa ID và Tên quốc gia
        public DataTable HienThi() => DatabaseHelper.ExecuteQuery("sp_QuocGia_HienThi");
    }

    // Lớp VanPhongDAL chịu trách nhiệm thao tác với danh mục Văn phòng chi nhánh (Offices) của hãng bay
    // Dùng để phân bổ nhân sự làm việc vào các văn phòng cụ thể (như Văn phòng TP.HCM, Văn phòng Hà Nội)
    public class VanPhongDAL
    {
        // Phương thức lấy danh sách các văn phòng đang hoạt động
        public DataTable HienThi() => DatabaseHelper.ExecuteQuery("sp_VanPhong_HienThi");
    }

    // Lớp VaiTroDAL quản lý danh mục các Quyền hạn/Vai trò (Roles) trong hệ thống
    // Ví dụ: Vai trò 1 - Administrator, Vai trò 2 - Booking Agent
    public class VaiTroDAL
    {
        // Phương thức lấy toàn bộ danh sách các chức danh/vai trò hiện có để hiển thị trên màn hình Thêm nhân viên
        public DataTable HienThi() => DatabaseHelper.ExecuteQuery("sp_VaiTro_HienThi");
    }
}
