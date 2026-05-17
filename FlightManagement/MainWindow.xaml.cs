using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using BUS;
using FlightManagement.UserControls;

namespace FlightManagement
{
    // Lớp MainWindow kế thừa từ Window, đóng vai trò là giao diện chính của ứng dụng
    public partial class MainWindow : Window
    {
        // Khai báo biến cục bộ để lưu trữ mã ID của người dùng đang đăng nhập
        private int _userId;
        
        // Khai báo biến cục bộ để lưu trữ họ và tên đầy đủ của người dùng
        private string _hoTen;
        
        // Khai báo biến cục bộ để lưu trữ tên vai trò của người dùng trong hệ thống
        private string _vaiTro;
        
        // Khai báo biến cục bộ để lưu trữ ID phân quyền của người dùng
        private int _roleId;
        
        // Khai báo biến cục bộ để lưu trữ tên văn phòng nơi người dùng đang làm việc
        private string _vanPhong;

        // Khai báo biến cục bộ để lưu trữ email của người dùng
        private string _email;

        // Biến đánh dấu trạng thái đang đăng xuất để tránh lặp vô tận khi đóng cửa sổ
        private bool _isLoggingOut = false;

        // Khởi tạo một Dictionary dùng chung để ánh xạ giữa mã định danh trang và tiêu đề hiển thị tương ứng
        private static readonly Dictionary<string, string> PageTitles = new()
        {
            // Định nghĩa cặp khóa và giá trị cho màn hình Trang chủ
            {"TrangChu", "Trang chủ"}, 
            
            // Định nghĩa cặp khóa và giá trị cho màn hình Quản lý Nhân viên
            {"NhanVien", "Quản lý Nhân viên"},
            
            // Định nghĩa cặp khóa và giá trị cho màn hình Quản lý Lịch bay
            {"LichBay", "Quản lý Lịch bay"}, 
            
            // Định nghĩa cặp khóa và giá trị cho màn hình Tìm kiếm Chuyến bay
            {"TimKiem", "Tìm kiếm Chuyến bay"},
            
            // Định nghĩa cặp khóa và giá trị cho màn hình Đặt vé Máy bay
            {"DatVe", "Đặt vé Máy bay"}, 
            
            // Định nghĩa cặp khóa và giá trị cho màn hình Quản lý Dịch vụ
            {"DichVu", "Quản lý Dịch vụ"},
            
            // Định nghĩa cặp khóa và giá trị cho màn hình Quản lý Vé & Đặt chỗ
            {"QuanLyVe", "Vé & Đặt chỗ"}, 
            
            // Định nghĩa cặp khóa và giá trị cho màn hình Quản lý Tuyến bay
            {"TuyenBay", "Quản lý Tuyến bay"},
            
            // Định nghĩa cặp khóa và giá trị cho màn hình Chăm sóc Khách hàng
            {"CSKH", "Chăm sóc Khách hàng"},
            
            // Định nghĩa cặp khóa và giá trị cho màn hình Quản lý Đội bay
            {"DoiBay", "Quản lý Đội bay"}, 
            
            // Định nghĩa cặp khóa và giá trị cho màn hình Báo cáo và Thống kê
            {"BaoCao", "Báo cáo & Thống kê"},
            
            // Định nghĩa cặp khóa và giá trị cho màn hình Cấu hình Hạng ghế
            {"HangGhe", "Cấu hình Hạng ghế"}, 
            
            // Định nghĩa cặp khóa và giá trị cho màn hình Lịch sử Hệ thống
            {"LichSu", "Lịch sử Hệ thống"}
        };

        // Hàm khởi tạo của cửa sổ MainWindow, nhận vào các thông tin người dùng được truyền từ màn hình Login
        public MainWindow(int userId, string hoTen, string vaiTro, int roleId, string vanPhong, string email)
        {
            // Gọi hàm khởi tạo các thành phần giao diện đồ họa được thiết kế trong file XAML
            InitializeComponent();
            
            // Gán ID của người dùng vào biến cục bộ để sử dụng trong toàn bộ lớp
            _userId = userId;
            
            // Gán họ tên của người dùng vào biến cục bộ để sử dụng sau này
            _hoTen = hoTen;
            
            // Gán tên vai trò của người dùng vào biến cục bộ
            _vaiTro = vaiTro;
            
            // Gán ID phân quyền của người dùng vào biến cục bộ để phục vụ cho hàm xử lý phân quyền
            _roleId = roleId;
            
            // Gán thông tin tên văn phòng vào biến cục bộ
            _vanPhong = vanPhong;

            // Gán email của người dùng vào biến cục bộ
            _email = email;

            // Hiển thị tên người dùng lên một TextBlock trên giao diện chính
            txtUserName.Text = hoTen;

            // Hiển thị email của người dùng lên giao diện chính
            txtUserEmail.Text = email;
            
            // Hiển thị huy hiệu đại diện cho vai trò của người dùng lên giao diện
            txtRoleBadge.Text = vaiTro;
            
            // Hiển thị huy hiệu đại diện cho văn phòng làm việc lên giao diện
            txtOfficeBadge.Text = vanPhong;
            
            // Thiết lập giá trị hiển thị thời gian hiện tại, sử dụng chuẩn văn hóa của Việt Nam để định dạng ngôn ngữ
            txtDate.Text = DateTime.Now.ToString("dddd, d 'tháng' M, yyyy", new CultureInfo("vi-VN"));

            // Thiết lập thông tin người dùng trên Header (Avatar + Tên + Vai trò)
            txtHeaderUserName.Text = hoTen;
            txtHeaderRole.Text = vaiTro;
            // Tạo chữ viết tắt cho Avatar (lấy chữ cái đầu của Họ và Tên)
            string[] nameParts = hoTen.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length >= 2)
                txtAvatar.Text = (nameParts[0][0].ToString() + nameParts[nameParts.Length - 1][0].ToString()).ToUpper();
            else if (nameParts.Length == 1)
                txtAvatar.Text = nameParts[0][0].ToString().ToUpper();

            // Kích hoạt hàm xử lý phân quyền để thay đổi giao diện theo vai trò của người dùng
            ApplyPermissions();
            
            // Mặc định tải giao diện Trang chủ ngay khi cửa sổ MainWindow được mở
            LoadPage("TrangChu");
        }

        // Phương thức chịu trách nhiệm ẩn và hiện các nút chức năng dựa vào ID quyền của người dùng
        private void ApplyPermissions()
        {
            // Kiểm tra xem ID quyền có bằng 2 (tức là vai trò Operator / Quản lý) hay không
            if (_roleId == 2)
            {
                // Nếu là Operator, ẩn nút điều hướng đến chức năng quản lý nhân sự
                btnNhanVien.Visibility = Visibility.Collapsed;
                
                // Ẩn nút điều hướng đến chức năng theo dõi lịch sử hệ thống
                btnLichSu.Visibility = Visibility.Collapsed;
                
                // Ẩn nút điều hướng đến chức năng cấu hình giá trị cốt lõi của hạng ghế
                btnHangGhe.Visibility = Visibility.Collapsed;
            }
            // Nếu không phải là Operator, kiểm tra tiếp xem có phải là Agent (Nhân viên bán vé) hay không
            else if (_roleId == 3)
            {
                // Nếu là Agent, ẩn nút quản lý nhân viên vì nhân viên bán vé không có quyền truy cập
                btnNhanVien.Visibility = Visibility.Collapsed;
                
                // Ẩn nút quản lý lịch bay để ngăn nhân viên bán vé thay đổi thời gian bay
                btnLichBay.Visibility = Visibility.Collapsed;
                
                // Ẩn nút quản lý cấu trúc các tuyến bay
                btnTuyenBay.Visibility = Visibility.Collapsed;
                
                // Ẩn nút quản lý danh sách các tàu bay thuộc sở hữu của hãng
                // btnDoiBay.Visibility = Visibility.Collapsed; (Đã gộp vào Tuyến bay)
                
                // Ẩn nút xem báo cáo thống kê doanh thu bảo mật của công ty
                btnBaoCao.Visibility = Visibility.Collapsed;
                
                // Ẩn nút chức năng cấu hình hạng ghế
                btnHangGhe.Visibility = Visibility.Collapsed;
                
                // Ẩn nút chức năng theo dõi lịch sử hoạt động chung của hệ thống
                btnLichSu.Visibility = Visibility.Collapsed;
            }
        }

        // Phương thức được kích hoạt khi một mục trên thanh menu điều hướng được click
        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra xem thành phần phát sinh sự kiện click có phải là RadioButton hay không
            if (sender is RadioButton rb)
            {
                // Nếu đúng, đọc thuộc tính Uid của RadioButton đó và truyền vào hàm tải trang tương ứng
                LoadPage(rb.Uid);
            }
        }

        // Phương thức xử lý việc tạo và hiển thị nội dung cho từng trang riêng biệt trong ứng dụng
        public void LoadPage(string pageId)
        {
            // Sử dụng câu lệnh switch để xử lý nhiều trường hợp điều hướng khác nhau dựa vào mã trang
            switch (pageId)
            {
                // Xử lý khi mã trang tương ứng với yêu cầu mở Trang chủ
                case "TrangChu":
                    // Tạo một thể hiện mới của UserControl Trang chủ và nhúng nó vào ContentControl của trang chính
                    pageContent.Content = new ucTrangChu(_userId, _hoTen, _vaiTro, _roleId);
                    // Dừng cấu trúc switch
                    break;
                    
                // Xử lý khi mã trang tương ứng với yêu cầu mở Quản lý Nhân viên
                case "NhanVien":
                    // Tạo và nhúng UserControl quản lý nhân sự vào vùng nội dung chính
                    pageContent.Content = new ucQuanLyNhanVien(_userId);
                    // Dừng cấu trúc switch
                    break;
                    
                // Xử lý khi mã trang tương ứng với yêu cầu mở Quản lý Lịch bay
                case "LichBay":
                    // Tạo và nhúng UserControl quản lý lịch bay vào vùng nội dung chính
                    pageContent.Content = new ucQuanLyLichBay(_userId);
                    // Dừng cấu trúc switch
                    break;
                    
                // Xử lý khi mã trang tương ứng với yêu cầu mở Tìm kiếm Chuyến bay
                case "TimKiem":
                    // Tạo UserControl Tìm kiếm chuyến bay. Lưu ý truyền biến this (MainWindow) để UserControl có thể tương tác ngược lại
                    pageContent.Content = new ucTimKiemChuyenBay(this, _userId);
                    // Dừng cấu trúc switch
                    break;
                    
                // Xử lý khi mã trang tương ứng với yêu cầu mở Đặt vé Máy bay
                case "DatVe":
                    // Chuyển hướng sang module Tổng quan vé
                    pageContent.Content = new ucTongQuanVe(this, _userId, 0);
                    break;
                    
                // Xử lý khi mã trang tương ứng với yêu cầu mở Quản lý Dịch vụ
                case "DichVu":
                    // Tạo và nhúng UserControl quản lý các dịch vụ đi kèm vào vùng nội dung
                    pageContent.Content = new ucQuanLyDichVu(_userId);
                    // Dừng cấu trúc switch
                    break;
                    
                // Xử lý khi mã trang tương ứng với yêu cầu mở Quản lý Vé
                case "QuanLyVe":
                    // Tạo và nhúng module chung quản lý vé và đặt vé
                    pageContent.Content = new ucTongQuanVe(this, _userId);
                    // Dừng cấu trúc switch
                    break;
                    
                // Xử lý khi mã trang tương ứng với yêu cầu mở Quản lý Tuyến bay
                case "TuyenBay":
                    // Tạo và nhúng UserControl dùng để thiết lập lộ trình các tuyến bay
                    pageContent.Content = new ucTuyenBayDoiBay(_userId);
                    // Dừng cấu trúc switch
                    break;
                    
                // Xử lý khi mã trang tương ứng với yêu cầu mở Quản lý Đội bay
                case "DoiBay":
                    // Redirect về Quản lý Tuyến bay (Vì đã gộp)
                    pageContent.Content = new ucTuyenBayDoiBay(_userId);
                    break;
                    
                // Xử lý khi mã trang tương ứng với yêu cầu mở Báo cáo & Thống kê
                case "BaoCao":
                    // Tạo và nhúng UserControl trình diễn biểu đồ thống kê
                    pageContent.Content = new ucBaoCaoThongKe();
                    // Dừng cấu trúc switch
                    break;
                    
                // Xử lý khi mã trang tương ứng với yêu cầu mở Cấu hình Hạng ghế
                case "HangGhe":
                    // Tạo và nhúng UserControl cấu hình quy tắc và giá của hạng ghế
                    pageContent.Content = new ucCauHinhHangGhe(_userId);
                    // Dừng cấu trúc switch
                    break;
                    
                // Xử lý khi mã trang tương ứng với yêu cầu mở Chăm sóc Khách hàng (CSKH)
                case "CSKH":
                    // Tạo và nhúng UserControl CSKH
                    pageContent.Content = new ucCSKH(this, _userId);
                    // Dừng cấu trúc switch
                    break;
                    
                // Xử lý khi mã trang tương ứng với yêu cầu mở Lịch sử Hệ thống
                case "LichSu":
                    // Tạo và nhúng UserControl nhật ký hệ thống
                    pageContent.Content = new ucLichSuHeThong();
                    // Dừng cấu trúc switch
                    break;
            }
        }

        // Phương thức công khai dùng để điều hướng ứng dụng sang giao diện Tìm kiếm chuyến bay một cách trực tiếp
        public void NavigateToTimKiem()
        {
            // Thiết lập trạng thái của nút radio Tìm kiếm thành được chọn (để nổi bật giao diện đang active)
            btnTimKiem.IsChecked = true;
            
            // Gọi trực tiếp bộ tạo giao diện của trang Tìm kiếm và nạp nó vào vùng trung tâm màn hình
            pageContent.Content = new ucTimKiemChuyenBay(this, _userId);
        }

        // Phương thức công khai dùng để điều hướng ứng dụng và đánh dấu active trên thanh menu trái
        public void SelectMenu(string uId)
        {
            var rb = (RadioButton)this.FindName("btn" + uId);
            if (rb != null) rb.IsChecked = true;
            LoadPage(uId);
        }

        // Phương thức công khai dùng để điều hướng ứng dụng sang giao diện Đặt vé máy bay và tự động chọn một lịch bay cụ thể
        public void NavigateToDatVe(int scheduleId)
        {
            // Bật sáng nút "Vé & Đặt chỗ" trên Sidebar
            btnQuanLyVe.IsChecked = true;
            
            // Khởi tạo module chung và nạp tham số scheduleId để tự động bật Tab "Đặt vé mới"
            pageContent.Content = new ucTongQuanVe(this, _userId, scheduleId);
        }

        // Phương thức được gọi tự động khi người dùng nhấp vào nút đăng xuất tài khoản
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Sự kiện xảy ra khi cửa sổ đang đóng (qua nút X, Alt+F4 hoặc lệnh Close)
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_isLoggingOut) return;

            _isLoggingOut = true;
            
            try 
            { 
                // Ghi log đăng xuất
                new LichSuBUS().GhiNhanDangXuat(_userId); 
            } 
            catch { }

            // Hiển thị lại màn hình đăng nhập
            LoginWindow login = new LoginWindow();
            login.Show();
        }

        // Các hàm điều khiển cửa sổ tùy chỉnh
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                btnMaxRestore.Content = "□";
                mainBorder.CornerRadius = new CornerRadius(16);
                mainBorder.Margin = new Thickness(0);
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                btnMaxRestore.Content = "❐";
                mainBorder.CornerRadius = new CornerRadius(0);
                mainBorder.Margin = new Thickness(7);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Header_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
        }
    }
}