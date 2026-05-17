using Microsoft.Data.SqlClient;
using System.Data;

namespace DAL
{
    // Lớp DatabaseHelper là một lớp tiện ích (Utility/Helper) dùng chung cho toàn bộ tầng DAL
    // Lớp này đóng vai trò như một bộ máy trung tâm chuyên trách việc thiết lập kết nối đến máy chủ SQL Server
    // Tất cả các lớp DAL khác (như VeDAL, NhanVienDAL...) đều phải gọi qua lớp này để tương tác với Cơ sở dữ liệu (CSDL)
    // Việc gom chung logic kết nối vào một nơi giúp dễ dàng bảo trì, đổi chuỗi kết nối hoặc xử lý lỗi tập trung
    public class DatabaseHelper
    {
        // Khai báo một chuỗi kết nối (Connection String) tĩnh và chỉ đọc (readonly)
        // Chuỗi này chứa toàn bộ thông tin cấu hình cần thiết để ứng dụng C# có thể đăng nhập vào máy chủ SQL Server trên Azure
        // Cụ thể: Server (địa chỉ máy chủ), Initial Catalog (tên database), User ID (tên đăng nhập), Password (mật khẩu)
        // Các tham số khác như Encrypt=True đảm bảo dữ liệu truyền tải trên mạng được mã hóa an toàn
        private static readonly string connectionString =
            @"Server=tcp:flight-management-sever.database.windows.net,1433;Initial Catalog=FlightManagementDB;Persist Security Info=False;User ID=admin_flight;Password=MatKhau@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        // Phương thức tĩnh nội bộ chịu trách nhiệm tạo ra một đường ống kết nối mới tới CSDL
        // Các hàm khác sẽ gọi hàm này mỗi khi cần mở một phiên làm việc (session) với SQL Server
        public static SqlConnection GetConnection()
        {
            // Trả về một đối tượng SqlConnection mới tinh, được nạp sẵn chuỗi cấu hình kết nối ở trên
            return new SqlConnection(connectionString);
        }

        // Phương thức ExecuteQuery chuyên dùng để thực thi các Stored Procedure có tính chất ĐỌC (SELECT) dữ liệu
        // Hàm này nhận vào tên của Stored Procedure (spName) và một mảng các tham số đầu vào (parameters) nếu có
        // Kết quả trả về luôn là một bảng dữ liệu (DataTable) chứa các dòng record đọc được từ CSDL
        public static DataTable ExecuteQuery(string spName, SqlParameter[]? parameters = null)
        {
            // Khởi tạo một đối tượng DataTable rỗng để chuẩn bị hứng dữ liệu đổ về từ máy chủ
            DataTable dt = new DataTable();
            
            // Khởi tạo một kết nối tới cơ sở dữ liệu trong khối using
            // Khối using đảm bảo rằng ngay khi khối lệnh thực thi xong, kết nối sẽ tự động được đóng lại (Dispose) để giải phóng tài nguyên mạng
            using (SqlConnection conn = GetConnection())
            {
                // Mở luồng kết nối tới máy chủ SQL Server
                conn.Open();
                
                // Khởi tạo đối tượng SqlCommand để mang câu lệnh hoặc tên Stored Procedure gửi sang máy chủ
                // Đối tượng này được liên kết chặt chẽ với đường ống kết nối (conn) vừa mở
                using (SqlCommand cmd = new SqlCommand(spName, conn))
                {
                    // Chỉ định rõ ràng rằng cái tên lệnh (spName) mà ta đang gửi đi là một Stored Procedure chứ không phải chuỗi SQL thô (Text)
                    // Việc dùng Stored Procedure giúp bảo mật cao hơn, chống lại các cuộc tấn công SQL Injection
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    // Kiểm tra xem mảng tham số truyền vào có tồn tại hay không
                    if (parameters != null) 
                    {
                        // Nếu có tham số (ví dụ: tham số ID khách hàng cần tìm), nạp toàn bộ mảng tham số này vào đối tượng SqlCommand
                        cmd.Parameters.AddRange(parameters);
                    }
                    
                    // Khởi tạo một SqlDataAdapter, công cụ này đóng vai trò như một "chiếc phễu"
                    // Nó sẽ tự động thực thi SqlCommand ở trên và hứng luồng dữ liệu trả về
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        // Dùng phễu (DataAdapter) để đổ (Fill) toàn bộ dữ liệu vừa nhận được vào bảng DataTable rỗng đã tạo lúc đầu
                        da.Fill(dt);
                    }
                }
            }
            
            // Trả về bảng dữ liệu đã chứa đầy thông tin cho các tầng bên trên (BUS/GUI) sử dụng
            return dt;
        }

        // Phương thức ExecuteNonQuery chuyên dùng để thực thi các lệnh THAY ĐỔI dữ liệu (INSERT, UPDATE, DELETE)
        // Phương thức này không trả về dữ liệu bảng, mà chỉ trả về một con số nguyên biểu thị cho SỐ DÒNG BỊ ẢNH HƯỞNG
        public static int ExecuteNonQuery(string spName, SqlParameter[]? parameters = null)
        {
            // Tương tự, tạo và quản lý vòng đời của đối tượng kết nối bằng khối using
            using (SqlConnection conn = GetConnection())
            {
                // Mở đường kết nối vật lý tới CSDL
                conn.Open();
                
                // Chuẩn bị túi chứa lệnh (SqlCommand) với tên Stored Procedure và đường kết nối
                using (SqlCommand cmd = new SqlCommand(spName, conn))
                {
                    // Đánh dấu đây là lệnh gọi Stored Procedure
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    // Nếu có tham số đi kèm (ví dụ: các thông tin tên tuổi, ngày sinh để INSERT), nhồi nó vào lệnh
                    if (parameters != null) 
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    
                    // Yêu cầu máy chủ SQL thực thi lệnh thay đổi dữ liệu
                    // Hàm ExecuteNonQuery() của SQL Server sẽ trả về số lượng dòng (rows) đã được thêm, sửa hoặc xóa thành công
                    // Ta ném thẳng con số này về lại cho hàm gọi (thường để kiểm tra xem > 0 tức là thao tác thành công hay không)
                    return cmd.ExecuteNonQuery(); 
                }
            }
        }

        // Phương thức ExecuteScalar chuyên dùng để lấy về MỘT GIÁ TRỊ DUY NHẤT (Vô hướng)
        // Ví dụ cực kỳ phổ biến: Đếm tổng số khách (COUNT(*)) trả về 1 con số, 
        // Hoặc thêm mới một dòng vào CSDL rồi cần lấy ngay cái ID (Khóa chính) vừa sinh tự động (SELECT SCOPE_IDENTITY())
        public static object? ExecuteScalar(string spName, SqlParameter[]? parameters = null)
        {
            // Khởi tạo đường ống kết nối an toàn với CSDL
            using (SqlConnection conn = GetConnection())
            {
                // Bắt đầu mở cổng giao tiếp
                conn.Open();
                
                // Chuẩn bị đối tượng mệnh lệnh
                using (SqlCommand cmd = new SqlCommand(spName, conn))
                {
                    // Thông báo lệnh này là gọi thủ tục nội trú (Stored Proc)
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    // Nạp thêm các điều kiện hoặc tham số đầu vào nếu được cung cấp
                    if (parameters != null) 
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    
                    // Thực thi câu lệnh trên máy chủ và chỉ bốc lấy giá trị ở Ô ĐẦU TIÊN (Dòng 1, Cột 1) của tập kết quả trả về
                    // Mọi dữ liệu ở các cột và dòng phía sau (nếu lỡ có) đều bị hệ thống bỏ qua để tối ưu tốc độ
                    // Giá trị trả về có kiểu chung là 'object' nên có thể chứa int, string, datetime tùy theo ngữ cảnh, tầng gọi sẽ tự ép kiểu sau
                    return cmd.ExecuteScalar(); 
                }
            }
        }
    }
}
