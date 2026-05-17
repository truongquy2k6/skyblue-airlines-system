using DAL;
using System.Data;

namespace BUS
{
    // Lớp Nghiệp Vụ (BUS - Business Logic Layer) quản lý các quy tắc liên quan đến Vé máy bay (Tickets).
    // Đóng vai trò cầu nối trung chuyển dữ liệu từ Giao diện (GUI) xuống lớp Truy xuất dữ liệu (DAL).
    public class VeBUS
    {
        // Khởi tạo đối tượng VeDAL thuộc tầng Data Access Layer ở chế độ chỉ đọc (readonly)
        // Đối tượng này sẽ đảm nhận việc thực thi các câu lệnh truy vấn SQL liên quan đến Vé máy bay
        private readonly VeDAL dal = new();
        
        // Phương thức công khai yêu cầu lấy danh sách toàn bộ vé đã được đặt trong hệ thống
        // Trả về một đối tượng DataTable chứa toàn bộ dữ liệu để hiển thị lên lưới dữ liệu (DataGrid) trên giao diện
        public DataTable HienThi() => dal.HienThi();
        
        // Phương thức xử lý nghiệp vụ đặt vé mới (Thêm vé) vào cơ sở dữ liệu hệ thống
        // Nhận vào các tham số chi tiết: ID người đặt, ID lịch bay, ID loại khoang, tên, họ, email, số điện thoại, hộ chiếu, quốc gia cấp, mã đặt chỗ, số ghế
        // Gọi phương thức Them của DAL để chèn dữ liệu vào bảng Tickets và trả về kết quả (thường là ID của vé vừa được tạo)
        public object? Them(int userId, int scheduleId, int cabinTypeId, string firstname, string lastname,
            string email, string phone, string passportNumber, int passportCountryId, string bookingReference, string? seatNumber = null)
            => dal.Them(userId, scheduleId, cabinTypeId, firstname, lastname, email, phone, passportNumber, passportCountryId, bookingReference, seatNumber);
            
        // Phương thức phục vụ chức năng tìm kiếm vé theo một từ khóa bất kỳ (ví dụ: Mã đặt chỗ PNR, Tên hành khách, hoặc Số điện thoại)
        // Truyền từ khóa tìm kiếm xuống tầng DAL và trả về một DataTable chứa danh sách các vé khớp với điều kiện tìm kiếm
        public DataTable TimKiem(string keyword) => dal.TimKiem(keyword);
        
        // Phương thức lấy danh sách các vị trí ghế ngồi đã có hành khách đặt trước trên một chuyến bay cụ thể
        // Dựa vào ID lịch bay (scheduleId) để truy vấn, phục vụ cho việc vô hiệu hóa (tô màu xám) các ghế này trên sơ đồ chọn ghế của giao diện
        public DataTable LayDanhSachGheDaDat(int scheduleId) => dal.LayDanhSachGheDaDat(scheduleId);
        
        // Phương thức xử lý nghiệp vụ hủy một vé máy bay đã được đặt thành công trước đó
        // Nhận vào ID của vé cần hủy và yêu cầu DAL cập nhật trạng thái của vé đó (thường là chuyển cờ Confirmed thành false hoặc đổi Status)
        public void HuyVe(int ticketId) => dal.HuyVe(ticketId);

        // Phương thức mới: Lấy danh sách tuyến bay cho Combobox Lọc
        public DataTable DanhSachTuyenBay() => dal.DanhSachTuyenBay();

        // Phương thức mới: Hỗ trợ phân trang Server-side
        public DataTable HienThiPhanTrang(string keyword, string tuyenBay, int pageNumber, int pageSize)
            => dal.HienThiPhanTrang(keyword, tuyenBay, pageNumber, pageSize);
    }

    // Lớp Nghiệp Vụ quản lý các quy tắc liên quan đến Dịch vụ đi kèm (Amenities) của hãng hàng không
    public class DichVuBUS
    {
        // Khởi tạo đối tượng truy xuất dữ liệu chuyên biệt thao tác trực tiếp với bảng Amenities (Dịch vụ)
        private readonly DichVuDAL dal = new();
        
        // Phương thức lấy danh sách toàn bộ các dịch vụ khả dụng trong hệ thống từ cơ sở dữ liệu
        public DataTable HienThi() => dal.HienThi();
        
        // Phương thức trích xuất dữ liệu thống kê tổng hợp liên quan đến mức độ sử dụng các dịch vụ (số lượng mua, tổng doanh thu)
        public DataTable ThongKe() => dal.ThongKe();
        
        // Phương thức truy vấn nhanh danh sách 3 dịch vụ được khách hàng ưa chuộng và đăng ký mua nhiều nhất để hiển thị báo cáo
        public DataTable Top3() => dal.Top3();
        
        // Phương thức thêm mới một dịch vụ vào danh mục dịch vụ cung cấp của hãng bay với các thông tin: tên dịch vụ và mức giá áp dụng
        public void Them(string service, decimal price) => dal.Them(service, price);
        
        // Phương thức cập nhật thông tin (tên dịch vụ, đơn giá) của một dịch vụ đã tồn tại trong hệ thống dựa vào mã định danh (id) của nó
        public void CapNhat(int id, string service, decimal price) => dal.CapNhat(id, service, price);
        
        // Phương thức xóa bỏ hoàn toàn một dịch vụ khỏi hệ thống dựa vào mã định danh (id), thường sẽ cần kiểm tra các ràng buộc dữ liệu trước khi xóa
        public void Xoa(int id) => dal.Xoa(id);
        
        // Phương thức lấy danh sách các dịch vụ đi kèm mà một hành khách (xác định qua ID vé - ticketId) đã đăng ký mua thêm cho chuyến bay của họ
        public DataTable LayTheoVe(int ticketId) => dal.LayTheoVe(ticketId);
        
        // Phương thức thực hiện nghiệp vụ gán (đăng ký thêm) một dịch vụ cụ thể vào một vé máy bay với mức giá áp dụng tại thời điểm bán
        // Ghi nhận sự kết nối này vào bảng chi tiết dịch vụ của vé (ví dụ: bảng AmenitiesTickets)
        public void GanChoVe(int amenityId, int ticketId, decimal price) => dal.GanChoVe(amenityId, ticketId, price);
        
        // Phương thức hủy bỏ (xóa) một dịch vụ đã được gắn vào vé máy bay trước đó trong trường hợp hành khách yêu cầu hoàn hoặc hủy dịch vụ
        public void XoaKhoiVe(int amenityId, int ticketId) => dal.XoaKhoiVe(amenityId, ticketId);
    }

    // Lớp Nghiệp Vụ chịu trách nhiệm quản lý và cấu hình các Hạng ghế (Cabin Types) trên máy bay
    public class HangGheBUS
    {
        // Khởi tạo đối tượng kết nối và thao tác với dữ liệu Hạng ghế tại tầng truy xuất dữ liệu DAL
        private readonly HangGheDAL dal = new();
        
        // Phương thức trích xuất danh sách tất cả các hạng ghế đang được hãng bay cấu hình (ví dụ: Economy, Business, First Class...)
        public DataTable HienThi() => dal.HienThi();
        
        // Phương thức lấy danh sách các dịch vụ mặc định được cung cấp miễn phí (đã bao gồm trong giá vé) đối với một hạng ghế cụ thể
        public DataTable LayCauHinh(int cabinTypeId) => dal.LayCauHinh(cabinTypeId);
        
        // Phương thức thiết lập cấu hình: Gán một dịch vụ cụ thể (amenityId) trở thành dịch vụ mặc định đi kèm cho một hạng ghế (cabinTypeId)
        public void GanDichVu(int cabinTypeId, int amenityId) => dal.GanDichVu(cabinTypeId, amenityId);
        
        // Phương thức thay đổi cấu hình: Gỡ bỏ một dịch vụ (amenityId) ra khỏi danh sách các dịch vụ mặc định của một hạng ghế (cabinTypeId)
        public void GoDichVu(int cabinTypeId, int amenityId) => dal.GoDichVu(cabinTypeId, amenityId);
    }

    // Lớp Nghiệp Vụ chuyên cung cấp các dữ liệu tổng hợp để hiển thị trực quan trên màn hình Dashboard (Trang chủ)
    public class TrangChuBUS
    {
        // Khởi tạo đối tượng TrangChuDAL để thực thi các thủ tục và hàm truy vấn báo cáo nhanh dưới Database
        private readonly TrangChuDAL dal = new();
        
        // Phương thức lấy các con số thống kê tổng quan (như Tổng số vé bán ra, Doanh thu trong ngày, Số lượng chuyến bay...) để hiển thị trên Dashboard
        public DataTable ThongKe() => dal.ThongKe();
        
        // Phương thức truy xuất danh sách chi tiết lịch trình các chuyến bay chuẩn bị khởi hành hoặc hạ cánh trong ngày hôm nay
        public DataTable LichBayHomNay() => dal.LichBayHomNay();
    }

    // Lớp Nghiệp Vụ đóng vai trò truy xuất danh mục quản lý Quốc gia (Countries)
    public class QuocGiaBUS
    {
        // Khởi tạo đối tượng thao tác trực tiếp với dữ liệu bảng Countries
        private readonly QuocGiaDAL dal = new();
        
        // Phương thức lấy toàn bộ danh sách các quốc gia, phục vụ cho việc đổ dữ liệu vào các ComboBox chọn quốc tịch hoặc nơi cấp hộ chiếu
        public DataTable HienThi() => dal.HienThi();
    }

    // Lớp Nghiệp Vụ đóng vai trò truy xuất danh mục Văn phòng chi nhánh (Offices)
    public class VanPhongBUS
    {
        // Khởi tạo đối tượng truy cập và lấy dữ liệu bảng Offices
        private readonly VanPhongDAL dal = new();
        
        // Phương thức lấy danh sách toàn bộ các văn phòng hoặc đại lý của hãng bay để hiển thị trong các danh mục lựa chọn (ví dụ: Quản lý nhân viên)
        public DataTable HienThi() => dal.HienThi();
    }

    // Lớp Nghiệp Vụ đóng vai trò truy xuất danh mục Vai trò và Phân quyền (Roles)
    public class VaiTroBUS
    {
        // Khởi tạo đối tượng truy cập dữ liệu liên quan đến bảng Roles
        private readonly VaiTroDAL dal = new();
        
        // Phương thức lấy danh sách các vai trò (như Administrator, Operator, Agent...) để phục vụ việc cấp quyền truy cập cho tài khoản nhân viên
        public DataTable HienThi() => dal.HienThi();
    }

    // Lớp Nghiệp Vụ quản lý hệ thống nhật ký hoạt động (Logs), theo dõi lịch sử thao tác của tất cả người dùng
    public class LichSuBUS
    {
        // Khởi tạo đối tượng chịu trách nhiệm thao tác với dữ liệu Lịch sử (Logs) tại tầng DAL
        private readonly LichSuDAL dal = new();
        
        // Phương thức lấy danh sách lịch sử chi tiết các phiên đăng nhập, đăng xuất phân trang
        public DataTable LayTruyCap(int pageNumber, int pageSize) => dal.LayTruyCap(pageNumber, pageSize);
        
        // Phương thức lấy danh sách các bản ghi nhật ký chỉnh sửa dữ liệu phân trang
        public DataTable LayChinhSua(int pageNumber, int pageSize) => dal.LayChinhSua(pageNumber, pageSize);
        
        // Phương thức ghi nhận một sự kiện người dùng đăng nhập thành công vào phần mềm, tiến hành lưu lại ID người dùng và địa chỉ IP của máy khách
        public void GhiNhanTruyCap(int userId, string ip) => dal.GhiNhanTruyCap(userId, ip);
        
        // Phương thức ghi nhận một sự kiện người dùng chủ động nhấn nút đăng xuất khỏi hệ thống dựa vào ID tài khoản của họ
        public void GhiNhanDangXuat(int userId) => dal.GhiNhanDangXuat(userId);
        
        // Phương thức ghi lại chi tiết dấu vết thay đổi dữ liệu: Lưu thông tin ai (userId) đã làm thao tác gì (hanhDong) với dữ liệu nào (doiTuong) và nội dung chi tiết ra sao (noiDung)
        public void GhiNhanChinhSua(int userId, string hanhDong, string doiTuong, string noiDung)
            => dal.GhiNhanChinhSua(userId, hanhDong, doiTuong, noiDung);
            
        // Phương thức thực thi chức năng dọn dẹp, xóa toàn bộ dữ liệu lịch sử phiên truy cập (thường chức năng này chỉ dành cho tài khoản Admin cấp cao)
        public void XoaTruyCap() => dal.XoaTruyCap();
        
        // Phương thức thực thi chức năng dọn dẹp, xóa toàn bộ dữ liệu lịch sử các thao tác chỉnh sửa hệ thống nhằm mục đích giải phóng dung lượng cho Database
        public void XoaChinhSua() => dal.XoaChinhSua();
    }

    // Lớp Nghiệp Vụ chuyên biệt phục vụ việc tính toán, truy xuất các số liệu phức tạp để xuất báo cáo
    public class BaoCaoBUS
    {
        // Khởi tạo đối tượng thực thi các câu lệnh truy vấn (Query) phức tạp, kết bảng nhiều lớp để sinh báo cáo ở tầng DAL
        private readonly BaoCaoDAL dal = new();
        
        // Phương thức truy xuất danh sách chi tiết toàn bộ hành khách trên một chuyến bay cụ thể (thông qua scheduleId) để phục vụ việc in ấn danh sách Boarding Pass
        public DataTable DanhSachHanhKhach(int scheduleId) => dal.DanhSachHanhKhach(scheduleId);
        
        // Phương thức lấy danh sách các chuyến bay dưới định dạng rút gọn, phục vụ cho các Dropdown/ComboBox khi người dùng cần lọc báo cáo theo từng chuyến bay
        public DataTable ChuyenBayCombo() => dal.ChuyenBayCombo();
        
        // Phương thức tổng hợp, thống kê và phân tích doanh số vé bán ra nhóm theo từng văn phòng đại lý, phục vụ đánh giá hiệu quả kinh doanh của các chi nhánh
        public DataTable ThongKeVanPhong() => dal.ThongKeVanPhong();
        
        // Phương thức thu thập và trả về tất cả các thông tin chi tiết nhất về một chuyến bay (bao gồm: Giờ khởi hành, Máy bay sử dụng, Đường bay, Tình trạng số chỗ trống...)
        public DataTable ChiTietChuyenBay(int scheduleId) => dal.ChiTietChuyenBay(scheduleId);

        // Phương thức lấy báo cáo doanh thu bán hàng theo tuần của Văn phòng và nhân viên
        public DataTable DoanhThuTuanVanPhong(int officeId, int userId, System.DateTime startDate, System.DateTime endDate) 
            => dal.DoanhThuTuanVanPhong(officeId, userId, startDate, endDate);

        // Phương thức lấy báo cáo doanh thu bán hàng theo tháng của Văn phòng và nhân viên
        public DataTable DoanhThuThangVanPhong(int officeId, int userId, System.DateTime startDate, System.DateTime endDate) 
            => dal.DoanhThuThangVanPhong(officeId, userId, startDate, endDate);

        // Phương thức lấy danh sách nhân viên thuộc Văn phòng cụ thể để phục vụ chọn lọc báo cáo
        public DataTable NhanVienTheoVanPhong(int officeId) => dal.NhanVienTheoVanPhong(officeId);
    }
}
