using Microsoft.Data.SqlClient;
using System.Data;

namespace DAL
{
    // Lớp LichBayDAL thuộc tầng Truy xuất dữ liệu (Data Access Layer) chuyên biệt xử lý các nghiệp vụ liên quan đến bảng Schedules (Lịch bay)
    // Mọi thao tác thêm lịch, sửa giờ, xóa chuyến bay hoặc tra cứu chuyến bay phức tạp đều sẽ được gọi từ đây xuống Database
    // Lớp này sử dụng đối tượng DatabaseHelper để tái sử dụng mã nguồn thiết lập kết nối tới hệ quản trị CSDL SQL Server
    public class LichBayDAL
    {
        // Phương thức công khai dùng để truy vấn và lấy về toàn bộ danh sách các chuyến bay hiện đang có trong CSDL
        // Phục vụ cho các màn hình quản trị nội bộ cần xem danh sách tổng hợp (Không yêu cầu truyền tham số)
        public DataTable HienThi() => DatabaseHelper.ExecuteQuery("sp_LichBay_HienThi");

        // Phương thức chịu trách nhiệm chèn một dòng lịch trình bay mới vào cơ sở dữ liệu
        // Các tham số truyền vào bao gồm Số hiệu chuyến bay, Ngày bay, Giờ bay, Mã máy bay thực hiện, Mã lộ trình, Giá vé phổ thông và Trạng thái xác nhận
        // Phương thức này không trả về dữ liệu (void) mà chỉ thực thi câu lệnh INSERT thông qua thủ tục sp_LichBay_Them
        public void Them(string flightNumber, DateTime date, TimeSpan time, int aircraftId, int routeId, decimal economyPrice, bool confirmed)
        {
            // Gọi hàm ExecuteNonQuery của DatabaseHelper để chạy Stored Procedure thực hiện lưu thông tin
            DatabaseHelper.ExecuteNonQuery("sp_LichBay_Them", new[] {
                
                // Đóng gói mã định danh của chuyến bay (Ví dụ: VN-254) vào tham số @FlightNumber
                new SqlParameter("@FlightNumber", flightNumber),
                
                // Đóng gói giá trị Ngày cất cánh dự kiến vào tham số @Date
                new SqlParameter("@Date", date),
                
                // Đóng gói khoảng thời gian cụ thể (Giờ cất cánh) theo định dạng TimeSpan vào tham số @Time
                new SqlParameter("@Time", time),
                
                // Đóng gói mã định danh của chiếc máy bay (AircraftID) được điều động cho chuyến bay này vào tham số @AircraftID
                new SqlParameter("@AircraftID", aircraftId),
                
                // Đóng gói mã của tuyến đường bay (RouteID - quy định điểm xuất phát và điểm đến) vào tham số @RouteID
                new SqlParameter("@RouteID", routeId),
                
                // Đóng gói mức giá tiền dành cho vé Hạng Phổ thông (Economy Class) làm mốc tính giá các hạng khác vào tham số @EconomyPrice
                new SqlParameter("@EconomyPrice", economyPrice),
                
                // Đóng gói trạng thái chuyến bay đã được chốt (Confirmed) hay chưa vào tham số @Confirmed
                new SqlParameter("@Confirmed", confirmed)
            });
        }

        // Phương thức đảm nhận việc cập nhật lại thông tin của một chuyến bay đã tồn tại trong CSDL
        // Yêu cầu bắt buộc là phải truyền vào tham số ID (khóa chính) để xác định chính xác dòng dữ liệu lịch bay cần sửa đổi
        // Có thể thay đổi giờ bay do thời tiết (time, date), đổi tàu bay do bảo trì (aircraftId) hoặc cập nhật giá vé (economyPrice)
        public void CapNhat(int id, string flightNumber, DateTime date, TimeSpan time, int aircraftId, int routeId, decimal economyPrice, bool confirmed)
        {
            // Kích hoạt thủ tục sp_LichBay_CapNhat dưới Database thông qua hàm ExecuteNonQuery
            DatabaseHelper.ExecuteNonQuery("sp_LichBay_CapNhat", new[] {
                
                // Truyền mã ID định danh của chuyến bay cần sửa vào tham số @ID (Đây là điều kiện WHERE bắt buộc trong truy vấn SQL)
                new SqlParameter("@ID", id),
                
                // Truyền Số hiệu chuyến bay mới (nếu có đổi) vào tham số @FlightNumber
                new SqlParameter("@FlightNumber", flightNumber),
                
                // Truyền Ngày cất cánh mới cập nhật vào tham số @Date
                new SqlParameter("@Date", date),
                
                // Truyền Giờ khởi hành mới (có thể do bị delay/hoãn) vào tham số @Time
                new SqlParameter("@Time", time),
                
                // Truyền ID của chiếc máy bay mới được điều động thay thế vào tham số @AircraftID
                new SqlParameter("@AircraftID", aircraftId),
                
                // Truyền ID tuyến bay mới vào tham số @RouteID
                new SqlParameter("@RouteID", routeId),
                
                // Truyền mức giá vé phổ thông mới cập nhật vào tham số @EconomyPrice
                new SqlParameter("@EconomyPrice", economyPrice),
                
                // Truyền trạng thái chuyến bay (đã chốt hoặc đang dự kiến) vào tham số @Confirmed
                new SqlParameter("@Confirmed", confirmed)
            });
        }

        // Phương thức xóa bỏ hoàn toàn một dòng dữ liệu lịch bay khỏi hệ thống dựa trên ID cung cấp
        // Chỉ nhận một tham số là mã ID, thường hệ thống sẽ có cơ chế xóa ràng buộc khóa ngoại (ví dụ xóa luôn vé của chuyến đó) nếu thiết kế Database yêu cầu
        public void Xoa(int id) => DatabaseHelper.ExecuteNonQuery("sp_LichBay_Xoa", new[] { new SqlParameter("@ID", id) });

        // Phương thức thực thi logic tìm kiếm lịch bay thông minh có tích hợp tính năng phân trang (Pagination) ở cấp độ Database
        // Cho phép tìm kiếm chuyến bay dựa trên 4 bộ lọc tùy chọn (Sân bay xuất phát, Sân bay đến, Khoảng thời gian từ ngày nào đến ngày nào)
        // Việc phân trang dưới DB giúp chỉ kéo về đúng 10-20 dòng dữ liệu mỗi lần truy vấn, tiết kiệm băng thông và RAM cho ứng dụng
        public DataTable TimKiem(int? sanBayDi, int? sanBayDen, DateTime? ngayTu, DateTime? ngayDen, int pageNumber, int pageSize)
        {
            // Gọi thủ tục sp_LichBay_TimKiem và nhận về kết quả là một bảng DataTable. Bảng này ngoài danh sách chuyến bay còn đính kèm một cột TotalRecords (tổng số chuyến)
            return DatabaseHelper.ExecuteQuery("sp_LichBay_TimKiem", new[] {
                
                // Nếu tham số sanBayDi bị trống (null do người dùng không chọn lọc), toán tử ?? sẽ bắt nó và chuyển thành DBNull.Value.
                // Điều này báo cho SQL Server biết là bỏ qua điều kiện lọc theo sân bay đi. Ngược lại thì gói ID sân bay đó vào tham số @SanBayDi
                new SqlParameter("@SanBayDi", (object?)sanBayDi ?? DBNull.Value),
                
                // Tương tự, đóng gói tham số ID sân bay đến vào tham số @SanBayDen, hỗ trợ xử lý rỗng bằng DBNull.Value
                new SqlParameter("@SanBayDen", (object?)sanBayDen ?? DBNull.Value),
                
                // Đóng gói mốc thời gian bắt đầu giới hạn tìm kiếm vào tham số @NgayTu. SQL sẽ tìm các chuyến bay cất cánh từ ngày này trở đi
                new SqlParameter("@NgayTu", (object?)ngayTu ?? DBNull.Value),
                
                // Đóng gói mốc thời gian kết thúc giới hạn tìm kiếm vào tham số @NgayDen. SQL sẽ tìm các chuyến bay cất cánh trước hoặc trong ngày này
                new SqlParameter("@NgayDen", (object?)ngayDen ?? DBNull.Value),
                
                // Đóng gói số thứ tự của trang dữ liệu mà người dùng muốn xem (ví dụ: Trang 2) vào tham số @PageNumber để tính Offset
                new SqlParameter("@PageNumber", pageNumber),
                
                // Đóng gói số lượng bản ghi (chuyến bay) được phép hiển thị tối đa trên một trang (ví dụ: 10 chuyến/trang) vào tham số @PageSize để tính Fetch Next
                new SqlParameter("@PageSize", pageSize)
            });
        }
    }
}
