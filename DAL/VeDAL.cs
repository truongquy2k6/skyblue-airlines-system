using Microsoft.Data.SqlClient;
using System.Data;

namespace DAL
{
    // Lớp VeDAL thuộc tầng Truy xuất dữ liệu (Data Access Layer) chuyên biệt xử lý các giao dịch nghiệp vụ liên quan đến đối tượng Vé máy bay
    // Trách nhiệm duy nhất của nó là chuẩn bị dữ liệu đầu vào và gọi lớp DatabaseHelper để tương tác trực tiếp với Stored Procedure của SQL Server
    // Lớp này không chứa các logic kiểm tra (validate) nghiệp vụ, nhiệm vụ đó thuộc về tầng BUS nằm phía trên
    public class VeDAL
    {
        // Phương thức công khai dùng để truy xuất toàn bộ danh sách các vé máy bay đã từng được đặt trên hệ thống
        // Không yêu cầu truyền tham số đầu vào. Phương thức gọi thẳng đến Stored Procedure tên là "sp_Ve_HienThi"
        // Do chỉ cần đọc dữ liệu, nó sử dụng hàm ExecuteQuery của DatabaseHelper để nhận về một đối tượng DataTable hoàn chỉnh
        public DataTable HienThi() => DatabaseHelper.ExecuteQuery("sp_Ve_HienThi");

        // Phương thức chịu trách nhiệm đẩy dữ liệu của một giao dịch đặt vé mới xuống cơ sở dữ liệu để lưu trữ
        // Nhận vào một danh sách dài các tham số chi tiết của vé: Thông tin tài khoản người đặt, Lịch bay, Loại ghế, Thông tin cá nhân hành khách và Số ghế
        // Kiểu trả về là object? (có thể null) vì ta mong đợi SQL Server sẽ trả về một con số ID (Mã tự sinh) của cái vé vừa được INSERT thành công
        public object? Them(int userId, int scheduleId, int cabinTypeId, string firstname, string lastname,
            string email, string phone, string passportNumber, int passportCountryId, string bookingReference, string? seatNumber = null)
        {
            // Bắt đầu gọi hàm ExecuteScalar (dùng để trả về 1 giá trị duy nhất là ID vé) và truyền vào tên Stored Procedure xử lý việc thêm vé
            // Khai báo và truyền vào một mảng chứa tập hợp các đối tượng SqlParameter
            return DatabaseHelper.ExecuteScalar("sp_Ve_Them", new[] {
                
                // Gói giá trị ID của người nhân viên thực hiện thao tác đặt vé vào tham số @UserID để gửi cho SQL
                new SqlParameter("@UserID", userId),
                
                // Gói ID của lịch trình chuyến bay mà khách muốn đi vào tham số @ScheduleID
                new SqlParameter("@ScheduleID", scheduleId),
                
                // Gói ID của hạng ghế (Economy, Business...) mà khách đã mua vào tham số @CabinTypeID
                new SqlParameter("@CabinTypeID", cabinTypeId),
                
                // Gói Tên (Firstname) của hành khách sẽ bay vào tham số @Firstname
                new SqlParameter("@Firstname", firstname),
                
                // Gói Họ (Lastname) của hành khách vào tham số @Lastname
                new SqlParameter("@Lastname", lastname),
                
                // Gói địa chỉ Email liên hệ của người đặt vé vào tham số @Email
                new SqlParameter("@Email", email),
                
                // Gói Số điện thoại di động dùng để nhận tin nhắn thay đổi lịch bay vào tham số @Phone
                new SqlParameter("@Phone", phone),
                
                // Gói số Hộ chiếu (Passport) của hành khách dùng để làm thủ tục check-in vào tham số @PassportNumber
                new SqlParameter("@PassportNumber", passportNumber),
                
                // Gói ID Quốc gia cấp hộ chiếu cho hành khách vào tham số @PassportCountryID
                new SqlParameter("@PassportCountryID", passportCountryId),
                
                // Gói chuỗi Mã đặt chỗ (PNR - Booking Reference, một chuỗi gồm 6 ký tự ngẫu nhiên) vào tham số @BookingReference
                new SqlParameter("@BookingReference", bookingReference),
                
                // Gói Số ghế (Ví dụ: 12A, 14B) vào tham số @SeatNumber.
                // Nếu khách hàng bỏ qua bước chọn ghế (biến seatNumber bị rỗng - null), toán tử ?? sẽ bắt trường hợp đó 
                // và chuyển thành giá trị DBNull.Value (một giá trị rỗng chuẩn hóa riêng của CSDL SQL Server) để đẩy xuống mạng
                new SqlParameter("@SeatNumber", (object?)seatNumber ?? DBNull.Value)
            });
        }

        // Phương thức truy xuất danh sách các vị trí ghế ngồi hiện tại đã có hành khách khác mua trước trên một chuyến bay cụ thể
        // Cần tham số scheduleId để biết đang kiểm tra chuyến bay nào. Dữ liệu trả về (DataTable) sẽ được giao diện dùng để vô hiệu hóa các nút bấm ghế đó
        public DataTable LayDanhSachGheDaDat(int scheduleId)
        {
            // Gọi thủ tục "sp_Ve_LayDanhSachGheDaDat" thông qua ExecuteQuery và truyền tham số duy nhất là ID của lịch bay
            return DatabaseHelper.ExecuteQuery("sp_Ve_LayDanhSachGheDaDat", new[] { new SqlParameter("@ScheduleID", scheduleId) });
        }

        // Phương thức gửi yêu cầu tìm kiếm vé máy bay xuống CSDL dựa vào một từ khóa văn bản tùy ý do người dùng gõ
        // Từ khóa này có thể là tên hành khách, số điện thoại hoặc mã PNR. Việc so khớp cụ thể LIKE '%...' sẽ do Stored Procedure bên dưới SQL tự lo
        public DataTable TimKiem(string keyword)
        {
            // Đóng gói từ khóa vào tham số @Keyword và gọi ExecuteQuery để nhận lại bảng dữ liệu kết quả tìm kiếm
            return DatabaseHelper.ExecuteQuery("sp_Ve_TimKiem", new[] { new SqlParameter("@Keyword", keyword) });
        }

        // Phương thức thao tác dữ liệu để hủy đi một vé đã từng đặt thành công
        // Quá trình hủy vé trong các hệ thống thật thường không phải là xóa hẳn khỏi ổ cứng (không dùng lệnh DELETE vật lý)
        // Thay vào đó Stored Procedure "sp_Ve_Huy" sẽ chỉ dùng lệnh UPDATE để đổi cột TrangThai thành 'Đã hủy' và có thể giải phóng chỗ ngồi
        public void HuyVe(int ticketId)
        {
            // Do chỉ thay đổi trạng thái chứ không cần trả về bảng hay dữ liệu nào nên ta dùng hàm ExecuteNonQuery
            // Truyền ID của vé cần hủy vào tham số @TicketID để SQL Server biết cần "trảm" dòng dữ liệu nào
            DatabaseHelper.ExecuteNonQuery("sp_Ve_Huy", new[] { new SqlParameter("@TicketID", ticketId) });
        }

        // Phương thức mới: Lấy danh sách tuyến bay cho Combobox Lọc
        public DataTable DanhSachTuyenBay()
        {
            return DatabaseHelper.ExecuteQuery("sp_Ve_DanhSachTuyenBay");
        }

        // Phương thức mới: Hỗ trợ phân trang Server-side trực tiếp từ SQL Server
        public DataTable HienThiPhanTrang(string keyword, string tuyenBay, int pageNumber, int pageSize)
        {
            return DatabaseHelper.ExecuteQuery("sp_Ve_HienThiPhanTrang", new[] {
                new SqlParameter("@Keyword", string.IsNullOrEmpty(keyword) ? DBNull.Value : keyword),
                new SqlParameter("@TuyenBay", string.IsNullOrEmpty(tuyenBay) || tuyenBay == "Tất cả chuyến bay" ? DBNull.Value : tuyenBay),
                new SqlParameter("@PageNumber", pageNumber),
                new SqlParameter("@PageSize", pageSize)
            });
        }
    }
}
