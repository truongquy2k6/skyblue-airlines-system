using Microsoft.Data.SqlClient;
using System.Data;

namespace DAL
{
    // Lớp SanBayDAL thuộc tầng DAL chịu trách nhiệm trực tiếp tương tác với bảng Airports (Sân bay) trong CSDL
    // Mọi thao tác cấu hình, thêm mới hoặc chỉnh sửa thông tin sân bay đều đi qua lớp này
    public class SanBayDAL
    {
        // Phương thức hiển thị toàn bộ danh sách sân bay có trên hệ thống để người dùng có thể chọn khi tạo lịch bay
        // Gọi thủ tục sp_SanBay_HienThi thông qua DatabaseHelper và trả về một DataTable
        public DataTable HienThi() => DatabaseHelper.ExecuteQuery("sp_SanBay_HienThi");

        // Phương thức thêm mới một sân bay vào danh mục dùng chung
        // Cần truyền vào mã IATA (ví dụ: SGN, HAN), Tên sân bay và ID của Quốc gia chứa sân bay đó
        // Không trả về dữ liệu nên hàm dùng void kết hợp với hàm ExecuteNonQuery
        public void Them(string iataCode, string name, int countryId)
        {
            // Kích hoạt thủ tục sp_SanBay_Them và truyền đầy đủ các tham số cần thiết
            DatabaseHelper.ExecuteNonQuery("sp_SanBay_Them", new[] {
                
                // Đóng gói mã vắn tắt của sân bay theo chuẩn IATA vào tham số @IATACode
                new SqlParameter("@IATACode", iataCode),
                
                // Đóng gói tên đầy đủ của sân bay vào tham số @Name
                new SqlParameter("@Name", name),
                
                // Đóng gói ID của quốc gia (liên kết với bảng Countries) vào tham số @CountryID
                new SqlParameter("@CountryID", countryId)
            });
        }

        // Phương thức cập nhật lại thông tin của một sân bay đã tồn tại trong hệ thống (thường dùng khi sân bay đổi tên hoặc có sai sót khi nhập liệu)
        // Yêu cầu phải có ID của sân bay để SQL Server biết cần Update dòng nào
        public void CapNhat(int id, string iataCode, string name, int countryId)
        {
            // Thực thi Stored Procedure sp_SanBay_CapNhat để ghi đè dữ liệu mới
            DatabaseHelper.ExecuteNonQuery("sp_SanBay_CapNhat", new[] {
                
                // Tham số khóa chính ID để xác định sân bay
                new SqlParameter("@ID", id),
                
                // Mã IATA mới (nếu có cập nhật)
                new SqlParameter("@IATACode", iataCode),
                
                // Tên sân bay mới
                new SqlParameter("@Name", name),
                
                // ID Quốc gia mới
                new SqlParameter("@CountryID", countryId)
            });
        }

        // Phương thức xóa một sân bay khỏi hệ thống dựa trên ID cung cấp
        // Chỉ nên sử dụng nếu sân bay này chưa từng được dùng trong bất kỳ lịch bay nào (ràng buộc khóa ngoại)
        public void Xoa(int id) => DatabaseHelper.ExecuteNonQuery("sp_SanBay_Xoa", new[] { new SqlParameter("@ID", id) });
    }

    // Lớp TuyenBayDAL nằm chung file vì nó có mối quan hệ rất mật thiết với Sân Bay
    // Chuyên quản lý bảng Routes (Tuyến bay), tức là quy định đường bay từ sân bay A đến sân bay B
    public class TuyenBayDAL
    {
        // Phương thức truy xuất danh sách toàn bộ các tuyến bay hiện có (Từ đâu đến đâu, mất bao lâu, khoảng cách bao xa)
        public DataTable HienThi() => DatabaseHelper.ExecuteQuery("sp_TuyenBay_HienThi");

        // Phương thức thêm một tuyến đường bay mới kết nối hai sân bay
        // Nhận vào ID sân bay cất cánh (DepID), ID sân bay hạ cánh (ArrID), Khoảng cách địa lý (Distance) và Thời gian bay dự kiến (FlightTime)
        public void Them(int depId, int arrId, int distance, int flightTime)
        {
            // Gọi thủ tục sp_TuyenBay_Them và nạp các thông số chi tiết
            DatabaseHelper.ExecuteNonQuery("sp_TuyenBay_Them", new[] {
                
                // Truyền ID của sân bay điểm đi (Departure) vào tham số @DepID
                new SqlParameter("@DepID", depId),
                
                // Truyền ID của sân bay điểm đến (Arrival) vào tham số @ArrID
                new SqlParameter("@ArrID", arrId),
                
                // Truyền thông tin khoảng cách tuyến bay (Đơn vị: km hoặc mile) vào tham số @Distance
                new SqlParameter("@Distance", distance),
                
                // Truyền tổng thời gian bay dự kiến (Đơn vị: phút) vào tham số @FlightTime
                new SqlParameter("@FlightTime", flightTime)
            });
        }

        // Phương thức thay đổi thông tin của một tuyến bay (Ví dụ do đo đạc lại khoảng cách hoặc cập nhật lại thời gian bay tiêu chuẩn)
        // Cần truyền ID của tuyến bay cần cập nhật
        public void CapNhat(int id, int depId, int arrId, int distance, int flightTime)
        {
            // Thực thi lệnh cập nhật qua Stored Procedure sp_TuyenBay_CapNhat
            DatabaseHelper.ExecuteNonQuery("sp_TuyenBay_CapNhat", new[] {
                
                // Truyền ID khóa chính
                new SqlParameter("@ID", id),
                
                // Cập nhật sân bay đi
                new SqlParameter("@DepID", depId),
                
                // Cập nhật sân bay đến
                new SqlParameter("@ArrID", arrId),
                
                // Cập nhật khoảng cách mới
                new SqlParameter("@Distance", distance),
                
                // Cập nhật thời gian bay mới
                new SqlParameter("@FlightTime", flightTime)
            });
        }

        // Phương thức xóa bỏ tuyến bay dựa trên ID
        // Việc xóa này có thể không được phép thực hiện nếu đã có lịch trình bay (Schedules) nào đó đang sử dụng tuyến bay này
        public void Xoa(int id) => DatabaseHelper.ExecuteNonQuery("sp_TuyenBay_Xoa", new[] { new SqlParameter("@ID", id) });
    }
}
