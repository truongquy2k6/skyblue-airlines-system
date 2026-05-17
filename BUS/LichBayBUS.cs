using DAL;
using System.Data;

namespace BUS
{
    /// <summary>
    /// Lớp Nghiệp Vụ (BUS) xử lý logic liên quan đến Lịch bay trước khi giao tiếp với DAL.
    /// Hiện tại lớp này đóng vai trò trung chuyển dữ liệu (pass-through) giữa GUI và DAL.
    /// </summary>
    public class LichBayBUS
    {
        // Khởi tạo đối tượng DAL để chuẩn bị gọi lệnh truy xuất CSDL
        private readonly LichBayDAL dal = new();
        
        /// <summary>
        /// Chuyển tiếp yêu cầu lấy danh sách lịch bay xuống DAL.
        /// </summary>
        public DataTable HienThi() => dal.HienThi();
        
        /// <summary>
        /// Chuyển tiếp yêu cầu thêm chuyến bay mới xuống DAL.
        /// Có thể thêm logic kiểm tra hợp lệ (Validation) tại đây trong tương lai.
        /// </summary>
        public void Them(string flightNumber, DateTime date, TimeSpan time, int aircraftId, int routeId, decimal economyPrice, bool confirmed)
            => dal.Them(flightNumber, date, time, aircraftId, routeId, economyPrice, confirmed);
            
        /// <summary>
        /// Chuyển tiếp yêu cầu cập nhật chuyến bay xuống DAL.
        /// </summary>
        public void CapNhat(int id, string flightNumber, DateTime date, TimeSpan time, int aircraftId, int routeId, decimal economyPrice, bool confirmed)
            => dal.CapNhat(id, flightNumber, date, time, aircraftId, routeId, economyPrice, confirmed);
            
        /// <summary>
        /// Chuyển tiếp yêu cầu xóa chuyến bay xuống DAL.
        /// </summary>
        public void Xoa(int id) => dal.Xoa(id);
        
        /// <summary>
        /// Nhận tham số tìm kiếm từ giao diện, chuyển xuống DAL để query CSDL.
        /// </summary>
        public DataTable TimKiem(int? sanBayDi, int? sanBayDen, DateTime? ngayTu, DateTime? ngayDen, int pageNumber, int pageSize) 
            => dal.TimKiem(sanBayDi, sanBayDen, ngayTu, ngayDen, pageNumber, pageSize);
    }
}
