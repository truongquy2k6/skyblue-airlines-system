using DAL;
using System.Data;

namespace BUS
{
    /// <summary>
    /// Lớp Nghiệp vụ (BUS) xử lý logic liên quan đến tài khoản Nhân viên (Users).
    /// </summary>
    public class NhanVienBUS
    {
        private readonly NhanVienDAL dal = new();
        
        /// <summary> Xử lý đăng nhập, gọi DAL để đối chiếu email và mật khẩu </summary>
        public DataTable DangNhap(string email, string password) => dal.DangNhap(email, password);
        
        /// <summary> Lấy danh sách toàn bộ nhân viên </summary>
        public DataTable HienThi() => dal.HienThi();
        
        /// <summary> Gọi DAL để tạo mới một tài khoản nhân viên </summary>
        public void Them(int roleId, string email, string password, string firstName, string lastName, int officeId, DateTime birthdate)
            => dal.Them(roleId, email, password, firstName, lastName, officeId, birthdate);
            
        /// <summary> Gọi DAL để cập nhật thông tin nhân viên đang có </summary>
        public void CapNhat(int id, int roleId, string email, string password, string firstName, string lastName, int officeId, DateTime birthdate, bool active)
            => dal.CapNhat(id, roleId, email, password, firstName, lastName, officeId, birthdate, active);
            
        /// <summary> Xóa tài khoản nhân viên theo ID </summary>
        public void Xoa(int id) => dal.Xoa(id);
    }
}
