using Microsoft.Data.SqlClient;
using System.Data;

namespace DAL
{
    // Lớp DichVuDAL chịu trách nhiệm xử lý các thao tác dữ liệu liên quan đến bảng Amenities (Dịch vụ tiện ích mở rộng)
    // Bao gồm các dịch vụ như: Mua thêm hành lý, chọn suất ăn đặc biệt, mua quyền ưu tiên làm thủ tục...
    public class DichVuDAL
    {
        // Phương thức lấy danh sách tất cả các dịch vụ có sẵn trong hệ thống để quản lý hoặc hiển thị cho khách chọn
        public DataTable HienThi() => DatabaseHelper.ExecuteQuery("sp_DichVu_HienThi");
        
        // Phương thức tổng hợp số liệu thống kê liên quan đến dịch vụ (Ví dụ: Dịch vụ nào mang lại doanh thu cao nhất)
        public DataTable ThongKe() => DatabaseHelper.ExecuteQuery("sp_DichVu_ThongKe");
        
        // Phương thức lấy ra Top 3 dịch vụ được hành khách mua nhiều nhất để đề xuất trên màn hình đặt vé
        public DataTable Top3() => DatabaseHelper.ExecuteQuery("sp_DichVu_Top3");

        // Phương thức thêm một loại dịch vụ mới vào danh mục hệ thống
        // Nhận vào Tên dịch vụ và Giá tiền niêm yết
        public void Them(string service, decimal price)
        {
            // Gọi Stored Procedure sp_DichVu_Them để thực hiện câu lệnh INSERT
            DatabaseHelper.ExecuteNonQuery("sp_DichVu_Them", new[] {
                
                // Tham số chứa Tên dịch vụ
                new SqlParameter("@Service", service),
                
                // Tham số chứa Giá dịch vụ
                new SqlParameter("@Price", price)
            });
        }

        // Phương thức cập nhật tên hoặc giá của một dịch vụ đã có (dựa vào ID)
        public void CapNhat(int id, string service, decimal price)
        {
            // Thực thi Stored Procedure sp_DichVu_CapNhat
            DatabaseHelper.ExecuteNonQuery("sp_DichVu_CapNhat", new[] {
                
                // ID của dịch vụ cần sửa
                new SqlParameter("@ID", id),
                
                // Tên mới của dịch vụ
                new SqlParameter("@Service", service),
                
                // Giá trị mới của dịch vụ (Có thể tăng giá hoặc giảm giá)
                new SqlParameter("@Price", price)
            });
        }

        // Phương thức xóa một dịch vụ khỏi danh mục dùng chung (Không cho phép mua nữa)
        // Lưu ý: Nếu dịch vụ này đã từng được khách hàng mua, SQL Server có thể báo lỗi Foreign Key
        public void Xoa(int id) => DatabaseHelper.ExecuteNonQuery("sp_DichVu_Xoa", new[] { new SqlParameter("@ID", id) });

        // Phương thức truy xuất danh sách các dịch vụ mà một khách hàng cụ thể ĐÃ MUA kèm theo vé của họ
        // Cần truyền vào ID của vé (TicketID). Kết quả dùng để in lên Boarding Pass hoặc hóa đơn
        public DataTable LayTheoVe(int ticketId)
        {
            return DatabaseHelper.ExecuteQuery("sp_DichVu_LayTheoVe", new[] { new SqlParameter("@TicketID", ticketId) });
        }

        // Phương thức gán/mua thêm một dịch vụ cho một tấm vé máy bay cụ thể
        // Khi khách quyết định trả tiền mua thêm dịch vụ, hàm này sẽ ghi nhận vào bảng AmenitiesTickets
        public void GanChoVe(int amenityId, int ticketId, decimal price)
        {
            // Kích hoạt thủ tục thêm liên kết giữa Vé và Dịch vụ
            DatabaseHelper.ExecuteNonQuery("sp_DichVu_GanChoVe", new[] {
                
                // ID của loại dịch vụ khách chọn
                new SqlParameter("@AmenityID", amenityId),
                
                // ID của vé máy bay mà khách đang sở hữu
                new SqlParameter("@TicketID", ticketId),
                
                // Mức giá thực tế tại thời điểm mua (Vì giá danh mục có thể thay đổi trong tương lai, phải chốt giá lúc mua)
                new SqlParameter("@Price", price)
            });
        }

        // Phương thức hủy bỏ (xóa) một dịch vụ ra khỏi một tấm vé (Trong trường hợp khách hàng đổi ý và yêu cầu hoàn tiền dịch vụ)
        public void XoaKhoiVe(int amenityId, int ticketId)
        {
            DatabaseHelper.ExecuteNonQuery("sp_DichVu_XoaKhoiVe", new[] {
                
                // ID của dịch vụ cần hủy
                new SqlParameter("@AmenityID", amenityId),
                
                // ID của vé chứa dịch vụ đó
                new SqlParameter("@TicketID", ticketId)
            });
        }
    }

    // Lớp HangGheDAL chuyên quản lý danh mục và cấu hình của các Hạng ghế (Cabin Types) như: Economy, Business, First Class
    public class HangGheDAL
    {
        // Phương thức lấy ra danh sách các Hạng ghế cơ bản có trong hệ thống
        public DataTable HienThi() => DatabaseHelper.ExecuteQuery("sp_HangGhe_HienThi");

        // Phương thức truy xuất danh sách các dịch vụ được MIỄN PHÍ hoặc ĐI KÈM MẶC ĐỊNH cho một Hạng ghế cụ thể
        // Ví dụ: Hạng Business được tự động miễn phí dịch vụ "Phòng chờ thương gia" và "Wifi trên máy bay"
        public DataTable LayCauHinh(int cabinTypeId)
        {
            return DatabaseHelper.ExecuteQuery("sp_HangGhe_LayCauHinh", new[] { new SqlParameter("@CabinTypeID", cabinTypeId) });
        }

        // Phương thức thiết lập (gắn) một dịch vụ mặc định vào một hạng ghế
        // Ví dụ: Admin cấu hình cho Hạng Economy được tặng kèm suất ăn nhẹ (Snack)
        public void GanDichVu(int cabinTypeId, int amenityId)
        {
            // Lưu thông tin vào bảng trung gian AmenitiesCabinType
            DatabaseHelper.ExecuteNonQuery("sp_HangGhe_GanDichVu", new[] {
                
                // ID của hạng ghế
                new SqlParameter("@CabinTypeID", cabinTypeId),
                
                // ID của dịch vụ miễn phí đi kèm
                new SqlParameter("@AmenityID", amenityId)
            });
        }

        // Phương thức tháo bỏ/hủy một dịch vụ mặc định khỏi một hạng ghế (Cắt giảm chi phí)
        // Ví dụ: Không miễn phí suất ăn nóng cho hạng Phổ thông nữa
        public void GoDichVu(int cabinTypeId, int amenityId)
        {
            DatabaseHelper.ExecuteNonQuery("sp_HangGhe_GoDichVu", new[] {
                
                // ID của hạng ghế
                new SqlParameter("@CabinTypeID", cabinTypeId),
                
                // ID của dịch vụ bị cắt giảm
                new SqlParameter("@AmenityID", amenityId)
            });
        }
    }
}
