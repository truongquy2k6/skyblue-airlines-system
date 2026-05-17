using Microsoft.Data.SqlClient;
using System.Data;

namespace DAL
{
    // Lớp NhanVienDAL đóng vai trò là cầu nối giao tiếp trực tiếp với cơ sở dữ liệu chuyên biệt cho bảng NhanVien (hoặc bảng Users)
    // Toàn bộ các thao tác thêm, sửa, xóa, lấy danh sách hoặc kiểm tra đăng nhập của nhân viên đều được định nghĩa tại đây
    // Lớp này sử dụng chung đường ống kết nối được cung cấp bởi lớp DatabaseHelper để tối ưu hóa mã nguồn
    public class NhanVienDAL
    {
        // Phương thức chịu trách nhiệm thực thi giao dịch kiểm tra thông tin đăng nhập của người dùng
        // Nhận vào chuỗi email và password nguyên bản từ tầng nghiệp vụ (BUS) truyền xuống
        // Phương thức này sẽ trả về một bảng dữ liệu (DataTable) chứa thông tin của người dùng nếu tài khoản hợp lệ
        public DataTable DangNhap(string email, string password)
        {
            // Gọi phương thức ExecuteQuery của DatabaseHelper để chạy thủ tục có tên "sp_DangNhap" trong CSDL
            // Truyền vào một mảng chứa 2 tham số là @Email và @Password để SQL Server so khớp với dữ liệu trong bảng Users
            return DatabaseHelper.ExecuteQuery("sp_DangNhap", new[] {
                
                // Đóng gói giá trị email do người dùng nhập vào tham số @Email
                new SqlParameter("@Email", email),
                
                // Đóng gói giá trị mật khẩu vào tham số @Password (Lưu ý: trong thực tế mật khẩu thường được băm (hash) trước khi truyền xuống đây)
                new SqlParameter("@Password", password)
            });
        }

        // Phương thức công khai dùng để trích xuất toàn bộ hồ sơ của các nhân viên đang có trong hệ thống
        // Không yêu cầu truyền bất kỳ tham số nào. Kết quả trả về là một DataTable để tầng UI đổ vào lưới hiển thị (DataGrid)
        public DataTable HienThi() => DatabaseHelper.ExecuteQuery("sp_NhanVien_HienThi");

        // Phương thức thực hiện nghiệp vụ tạo mới một tài khoản nhân viên và lưu vào cơ sở dữ liệu
        // Cần cung cấp đầy đủ các thông tin nhân sự cơ bản: Vai trò, Email, Mật khẩu, Tên, Họ, Văn phòng trực thuộc và Ngày sinh
        // Vì đây là thao tác ghi dữ liệu (không cần trả về bảng), nên hàm sử dụng kiểu trả về là void (không có giá trị trả về)
        public void Them(int roleId, string email, string password, string firstName, string lastName, int officeId, DateTime birthdate)
        {
            // Gọi phương thức ExecuteNonQuery của DatabaseHelper để kích hoạt Stored Procedure "sp_NhanVien_Them"
            // Truyền vào một mảng các đối tượng SqlParameter mang theo dữ liệu chi tiết của nhân viên mới
            DatabaseHelper.ExecuteNonQuery("sp_NhanVien_Them", new[] {
                
                // Truyền ID phân quyền (Ví dụ: 1 là Admin, 2 là Manager) vào tham số @RoleID
                new SqlParameter("@RoleID", roleId),
                
                // Truyền địa chỉ Email (thường dùng làm tên đăng nhập) vào tham số @Email
                new SqlParameter("@Email", email),
                
                // Truyền chuỗi Mật khẩu vào tham số @Password
                new SqlParameter("@Password", password),
                
                // Truyền Tên thật của nhân viên vào tham số @FirstName
                new SqlParameter("@FirstName", firstName),
                
                // Truyền Họ của nhân viên vào tham số @LastName
                new SqlParameter("@LastName", lastName),
                
                // Truyền mã định danh của Văn phòng chi nhánh nơi nhân viên này làm việc vào tham số @OfficeID
                new SqlParameter("@OfficeID", officeId),
                
                // Truyền ngày tháng năm sinh của nhân viên vào tham số @Birthdate để lưu trữ
                new SqlParameter("@Birthdate", birthdate)
            });
        }

        // Phương thức thực thi lệnh cập nhật hồ sơ của một nhân viên đã tồn tại trong hệ thống
        // Cần phải truyền vào tham số ID của nhân viên đó để SQL Server biết chính xác cần sửa dòng dữ liệu nào
        // Ngoài ra, hàm này còn cập nhật thêm trạng thái hoạt động (active) của nhân viên (ví dụ: bị khóa tài khoản hoặc đã nghỉ việc)
        public void CapNhat(int id, int roleId, string email, string password, string firstName, string lastName, int officeId, DateTime birthdate, bool active)
        {
            // Sử dụng ExecuteNonQuery để chạy lệnh cập nhật, bỏ qua giá trị số dòng bị ảnh hưởng do C# không yêu cầu bắt lại ở đây
            DatabaseHelper.ExecuteNonQuery("sp_NhanVien_CapNhat", new[] {
                
                // Truyền ID của tài khoản nhân viên cần chỉnh sửa vào tham số @ID. Đây là tham số bắt buộc làm khóa chính (Primary Key)
                new SqlParameter("@ID", id),
                
                // Truyền ID vai trò mới (nếu có thay đổi) vào tham số @RoleID
                new SqlParameter("@RoleID", roleId),
                
                // Truyền địa chỉ Email mới vào tham số @Email
                new SqlParameter("@Email", email),
                
                // Truyền mật khẩu mới (nếu người dùng có đổi) vào tham số @Password
                new SqlParameter("@Password", password),
                
                // Truyền tên mới vào tham số @FirstName
                new SqlParameter("@FirstName", firstName),
                
                // Truyền họ mới vào tham số @LastName
                new SqlParameter("@LastName", lastName),
                
                // Truyền chi nhánh văn phòng mới (nếu có luân chuyển công tác) vào tham số @OfficeID
                new SqlParameter("@OfficeID", officeId),
                
                // Truyền ngày sinh mới cập nhật vào tham số @Birthdate
                new SqlParameter("@Birthdate", birthdate),
                
                // Truyền trạng thái hoạt động của tài khoản vào tham số @Active. True nghĩa là đang làm việc (kích hoạt), False là đã nghỉ việc (vô hiệu hóa)
                new SqlParameter("@Active", active)
            });
        }

        // Phương thức thực hiện nghiệp vụ xóa bỏ hoàn toàn một tài khoản nhân viên khỏi cơ sở dữ liệu
        // Nhận vào duy nhất mã định danh (ID) của nhân viên cần xóa. Stored Procedure bên dưới thường sẽ xóa luôn cả các dữ liệu liên quan nếu có (Cascade Delete)
        public void Xoa(int id) => DatabaseHelper.ExecuteNonQuery("sp_NhanVien_Xoa", new[] { new SqlParameter("@ID", id) });
    }
}
