using Microsoft.Data.SqlClient;
using System.Data;

namespace DAL
{
    // Lớp MayBayDAL chuyên trách việc giao tiếp với CSDL để quản lý danh mục Đội tàu bay (Aircrafts)
    // Các thao tác nhập tàu bay mới, cập nhật sơ đồ ghế hoặc loại bỏ tàu bay cũ sẽ được xử lý tại đây
    public class MayBayDAL
    {
        // Phương thức hiển thị danh sách toàn bộ các máy bay hiện có mà hãng hàng không đang sở hữu hoặc vận hành
        // Kết quả trả về là một bảng dữ liệu (DataTable) thường được dùng để bind lên DataGrid trong màn hình quản trị Đội bay
        public DataTable HienThi() => DatabaseHelper.ExecuteQuery("sp_MayBay_HienThi");

        // Phương thức Thêm một chiếc máy bay mới vào cơ sở dữ liệu
        // Các tham số đầu vào bao gồm: Tên gọi, Mẫu mã chế tạo và cấu trúc số lượng ghế (Tổng số, Phổ thông, Thương gia, Hạng nhất)
        // Số ghế hạng nhất (FirstClass) mặc định là 0 nếu không được truyền vào (Dùng tham số mặc định của C#)
        public void Them(string name, string makeModel, int totalSeats, int economySeats, int businessSeats, int firstClassSeats = 0)
        {
            // Thực thi Stored Procedure sp_MayBay_Them bằng cách gửi mảng các tham số chi tiết
            DatabaseHelper.ExecuteNonQuery("sp_MayBay_Them", new[] {
                
                // Đóng gói Tên của máy bay (Ví dụ: Boeing 787 Dreamliner) vào tham số @Name
                new SqlParameter("@Name", name),
                
                // Đóng gói Dòng máy bay/Hãng sản xuất (Ví dụ: B787-9) vào tham số @MakeModel
                new SqlParameter("@MakeModel", makeModel),
                
                // Đóng gói Tổng số lượng ghế tối đa mà tàu bay này có thể chở vào tham số @TotalSeats
                new SqlParameter("@TotalSeats", totalSeats),
                
                // Đóng gói Số lượng ghế thiết kế cho Khoang Phổ thông (Economy) vào tham số @EconomySeats
                new SqlParameter("@EconomySeats", economySeats),
                
                // Đóng gói Số lượng ghế thiết kế cho Khoang Thương gia (Business) vào tham số @BusinessSeats
                new SqlParameter("@BusinessSeats", businessSeats),
                
                // Đóng gói Số lượng ghế thiết kế cho Khoang Hạng nhất (First Class) vào tham số @FirstClassSeats
                new SqlParameter("@FirstClassSeats", firstClassSeats)
            });
        }

        // Phương thức cập nhật lại thông tin của một máy bay đã có trong hệ thống (thường áp dụng khi máy bay được cải tạo lại cabin, tăng giảm số lượng ghế)
        // Yêu cầu phải truyền vào tham số ID của máy bay để định vị dòng cần sửa trong Database
        public void CapNhat(int id, string name, string makeModel, int totalSeats, int economySeats, int businessSeats, int firstClassSeats = 0)
        {
            // Gọi thủ tục cập nhật thông qua ExecuteNonQuery
            DatabaseHelper.ExecuteNonQuery("sp_MayBay_CapNhat", new[] {
                
                // Truyền khóa chính ID
                new SqlParameter("@ID", id),
                
                // Truyền tên máy bay mới (nếu có đổi tên)
                new SqlParameter("@Name", name),
                
                // Truyền mẫu máy bay mới
                new SqlParameter("@MakeModel", makeModel),
                
                // Cập nhật lại tổng số ghế
                new SqlParameter("@TotalSeats", totalSeats),
                
                // Cập nhật số ghế phổ thông
                new SqlParameter("@EconomySeats", economySeats),
                
                // Cập nhật số ghế thương gia
                new SqlParameter("@BusinessSeats", businessSeats),
                
                // Cập nhật số ghế hạng nhất
                new SqlParameter("@FirstClassSeats", firstClassSeats)
            });
        }

        // Phương thức xóa một hồ sơ máy bay khỏi hệ thống dựa trên tham số ID
        // Tương tự các danh mục khác, thao tác xóa thường sẽ bị SQL Server chặn lại (Throw Exception) nếu chiếc máy bay này đã từng thực hiện bất kỳ lịch bay nào trước đó (do dính ràng buộc khóa ngoại)
        public void Xoa(int id) => DatabaseHelper.ExecuteNonQuery("sp_MayBay_Xoa", new[] { new SqlParameter("@ID", id) });
    }
}
