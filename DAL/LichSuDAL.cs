using Microsoft.Data.SqlClient;
using System.Data;

namespace DAL
{
    // Lớp LichSuDAL là module quan trọng chịu trách nhiệm ghi log (nhật ký) mọi hoạt động của người dùng trên hệ thống
    // Chức năng này giúp Admin kiểm tra được ai đã đăng nhập lúc nào, hoặc ai đã thực hiện thay đổi dữ liệu nào để phục vụ việc bảo mật (Audit)
    public class LichSuDAL
    {
        // Phương thức lấy ra danh sách lịch sử truy cập phân trang từ SQL Server
        public DataTable LayTruyCap(int pageNumber, int pageSize)
        {
            return DatabaseHelper.ExecuteQuery("sp_LichSu_LayTruyCap", new[] {
                new SqlParameter("@PageNumber", pageNumber),
                new SqlParameter("@PageSize", pageSize)
            });
        }
        
        // Phương thức lấy ra danh sách các hành động thay đổi dữ liệu phân trang từ SQL Server
        public DataTable LayChinhSua(int pageNumber, int pageSize)
        {
            return DatabaseHelper.ExecuteQuery("sp_LichSu_LayChinhSua", new[] {
                new SqlParameter("@PageNumber", pageNumber),
                new SqlParameter("@PageSize", pageSize)
            });
        }

        // Phương thức ghi nhận sự kiện một nhân viên vừa đăng nhập thành công vào phần mềm
        // Yêu cầu truyền ID của nhân viên đó và địa chỉ IP của máy tính đang sử dụng
        public void GhiNhanTruyCap(int userId, string ip)
        {
            // Thực thi Stored Procedure để chèn một dòng nhật ký truy cập với trạng thái là Đăng nhập
            DatabaseHelper.ExecuteNonQuery("sp_LichSu_GhiNhanTruyCap", new[] {
                
                // Tham số ID nhân viên
                new SqlParameter("@UserID", userId),
                
                // Tham số chứa địa chỉ mạng (Ví dụ: 192.168.1.10)
                new SqlParameter("@IP", ip)
            });
        }

        // Phương thức ghi nhận sự kiện nhân viên bấm nút Đăng xuất (Logout) hoặc tắt phần mềm an toàn
        // Giúp hệ thống tính toán được tổng thời gian nhân viên làm việc trong ngày dựa trên giờ đăng nhập và đăng xuất
        public void GhiNhanDangXuat(int userId)
        {
            DatabaseHelper.ExecuteNonQuery("sp_LichSu_GhiNhanDangXuat", new[] { new SqlParameter("@UserID", userId) });
        }

        // Phương thức ghi nhận chi tiết một thao tác làm thay đổi cơ sở dữ liệu của người dùng
        // Được gọi ở tầng BUS mỗi khi người dùng Thêm/Sửa/Xóa thành công một đối tượng nào đó
        public void GhiNhanChinhSua(int userId, string hanhDong, string doiTuong, string noiDung)
        {
            DatabaseHelper.ExecuteNonQuery("sp_LichSu_GhiNhanChinhSua", new[] {
                
                // ID của nhân viên trực tiếp thực hiện thao tác
                new SqlParameter("@UserID", userId),
                
                // Tên hành động (Ví dụ: "Thêm mới", "Cập nhật", "Xóa", "Hủy")
                new SqlParameter("@HanhDong", hanhDong),
                
                // Đối tượng bị tác động (Ví dụ: "Vé máy bay", "Chuyến bay VN-123")
                new SqlParameter("@DoiTuong", doiTuong),
                
                // Nội dung chi tiết của thay đổi (Ví dụ: "Sửa giá vé từ 1tr thành 1.5tr")
                new SqlParameter("@NoiDung", noiDung)
            });
        }

        // Phương thức dọn dẹp/xóa trắng toàn bộ nhật ký đăng nhập (Dùng để giải phóng bộ nhớ DB sau một khoảng thời gian dài)
        public void XoaTruyCap() => DatabaseHelper.ExecuteNonQuery("sp_LichSu_XoaTruyCap");
        
        // Phương thức dọn dẹp toàn bộ nhật ký chỉnh sửa dữ liệu
        public void XoaChinhSua() => DatabaseHelper.ExecuteNonQuery("sp_LichSu_XoaChinhSua");
    }

    // Lớp BaoCaoDAL cung cấp dữ liệu thô phục vụ cho các chức năng kết xuất Báo cáo (Reports) của hệ thống
    public class BaoCaoDAL
    {
        // Phương thức lấy danh sách chi tiết tất cả hành khách (Passenger Manifest) trên một chuyến bay cụ thể
        // Dùng để nhân viên in danh sách ra giấy trước khi thực hiện check-in hoặc boarding cho khách
        public DataTable DanhSachHanhKhach(int scheduleId)
        {
            // Yêu cầu truyền vào ID của lịch bay
            return DatabaseHelper.ExecuteQuery("sp_BaoCao_DanhSachHanhKhach", new[] { new SqlParameter("@ScheduleID", scheduleId) });
        }

        // Phương thức xuất báo cáo Thống kê doanh thu theo từng tuyến bay cụ thể (Ví dụ Tuyến SGN-HAN thu được bao nhiêu tiền)
        public DataTable ChuyenBayCombo() => DatabaseHelper.ExecuteQuery("sp_BaoCao_ChuyenBayCombo");
        
        // Phương thức xuất báo cáo Thống kê số lượng vé bán được chia theo từng Văn phòng giao dịch
        public DataTable ThongKeVanPhong() => DatabaseHelper.ExecuteQuery("sp_BaoCao_ThongKeVanPhong");

        // Phương thức truy xuất thông tin chi tiết của một chuyến bay (bao gồm giờ cất cánh thực tế, tàu bay, tổng số vé đã bán...)
        public DataTable ChiTietChuyenBay(int scheduleId)
        {
            return DatabaseHelper.ExecuteQuery("sp_BaoCao_ChiTietChuyenBay", new[] { new SqlParameter("@ScheduleID", scheduleId) });
        }

        // Phương thức lấy báo cáo doanh thu bán hàng theo tuần của Văn phòng và nhân viên
        public DataTable DoanhThuTuanVanPhong(int officeId, int userId, System.DateTime startDate, System.DateTime endDate)
        {
            return DatabaseHelper.ExecuteQuery("sp_BaoCao_DoanhThuTuanVanPhong", new[] {
                new SqlParameter("@OfficeID", officeId),
                new SqlParameter("@UserID", userId),
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
            });
        }

        // Phương thức lấy báo cáo doanh thu bán hàng theo tháng của Văn phòng và nhân viên
        public DataTable DoanhThuThangVanPhong(int officeId, int userId, System.DateTime startDate, System.DateTime endDate)
        {
            return DatabaseHelper.ExecuteQuery("sp_BaoCao_DoanhThuThangVanPhong", new[] {
                new SqlParameter("@OfficeID", officeId),
                new SqlParameter("@UserID", userId),
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
            });
        }

        // Phương thức lấy danh sách nhân viên thuộc Văn phòng cụ thể để phục vụ chọn lọc báo cáo
        public DataTable NhanVienTheoVanPhong(int officeId)
        {
            return DatabaseHelper.ExecuteQuery("sp_BaoCao_NhanVienTheoVanPhong", new[] {
                new SqlParameter("@OfficeID", officeId)
            });
        }
    }
}
