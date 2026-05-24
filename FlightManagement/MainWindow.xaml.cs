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

        // Dictionary để lưu trữ cache các trang đã được tải
        private readonly Dictionary<string, UserControl> _pageCache = new();

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
            // Reset hiển thị tất cả các nút điều hướng và tiêu đề nhóm chức năng
            btnNhanVien.Visibility = Visibility.Visible;
            btnLichSu.Visibility = Visibility.Visible;
            btnHangGhe.Visibility = Visibility.Visible;
            btnLichBay.Visibility = Visibility.Visible;
            btnTuyenBay.Visibility = Visibility.Visible;
            btnDichVu.Visibility = Visibility.Visible;
            btnBaoCao.Visibility = Visibility.Visible;
            btnTimKiem.Visibility = Visibility.Visible;
            btnQuanLyVe.Visibility = Visibility.Visible;
            btnCSKH.Visibility = Visibility.Visible;
            
            headerDieuHanhBay.Visibility = Visibility.Visible;
            headerHeThong.Visibility = Visibility.Visible;

            // Phân quyền cho Điều hành viên / Quản lý (Operator - RoleID = 2)
            if (_roleId == 2)
            {
                // Ẩn các chức năng nghiệp vụ của Nhân viên bán vé (Agent)
                btnTimKiem.Visibility = Visibility.Collapsed;
                btnQuanLyVe.Visibility = Visibility.Collapsed;
                btnCSKH.Visibility = Visibility.Collapsed;

                // Ẩn các quyền cấu trị hệ thống tối cao của Admin
                btnNhanVien.Visibility = Visibility.Collapsed;
                btnLichSu.Visibility = Visibility.Collapsed;
                btnHangGhe.Visibility = Visibility.Collapsed;

                // Ẩn tiêu đề nhóm HỆ THỐNG vì không có chức năng nào hiển thị
                headerHeThong.Visibility = Visibility.Collapsed;
            }
            // Phân quyền cho Nhân viên bán vé (Agent - RoleID = 3)
            else if (_roleId == 3)
            {
                // Ẩn các chức năng vận hành kỹ thuật & cấu hình của Operator
                btnLichBay.Visibility = Visibility.Collapsed;
                btnTuyenBay.Visibility = Visibility.Collapsed;
                btnDichVu.Visibility = Visibility.Collapsed;
                btnBaoCao.Visibility = Visibility.Collapsed;
                btnHangGhe.Visibility = Visibility.Collapsed;

                // Ẩn các quyền quản trị tối cao của Admin
                btnNhanVien.Visibility = Visibility.Collapsed;
                btnLichSu.Visibility = Visibility.Collapsed;

                // Ẩn tiêu đề nhóm ĐIỀU HÀNH BAY & HỆ THỐNG vì không còn chức năng nào thuộc 2 nhóm này
                headerDieuHanhBay.Visibility = Visibility.Collapsed;
                headerHeThong.Visibility = Visibility.Collapsed;
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
            UserControl? page = null;

            // Kiểm tra xem trang đã được khởi tạo và lưu trong Cache chưa
            if (_pageCache.TryGetValue(pageId, out UserControl? cachedPage))
            {
                page = cachedPage;
            }
            else
            {
                // Sử dụng câu lệnh switch để xử lý nhiều trường hợp điều hướng khác nhau dựa vào mã trang
                switch (pageId)
                {
                    // Xử lý khi mã trang tương ứng với yêu cầu mở Trang chủ
                    case "TrangChu":
                        page = new ucTrangChu(_userId, _hoTen, _vaiTro, _roleId);
                        break;
                        
                    // Xử lý khi mã trang tương ứng với yêu cầu mở Quản lý Nhân viên
                    case "NhanVien":
                        page = new ucQuanLyNhanVien(_userId);
                        break;
                        
                    // Xử lý khi mã trang tương ứng với yêu cầu mở Quản lý Lịch bay
                    case "LichBay":
                        page = new ucQuanLyLichBay(_userId);
                        break;
                        
                    // Xử lý khi mã trang tương ứng với yêu cầu mở Tìm kiếm Chuyến bay
                    case "TimKiem":
                        page = new ucTimKiemChuyenBay(this, _userId);
                        break;
                        
                    // Xử lý khi mã trang tương ứng với yêu cầu mở Đặt vé Máy bay
                    case "DatVe":
                        page = new ucTongQuanVe(this, _userId, 0);
                        break;
                        
                    // Xử lý khi mã trang tương ứng với yêu cầu mở Quản lý Dịch vụ
                    case "DichVu":
                        page = new ucQuanLyDichVu(_userId);
                        break;
                        
                    // Xử lý khi mã trang tương ứng với yêu cầu mở Quản lý Vé
                    case "QuanLyVe":
                        page = new ucTongQuanVe(this, _userId);
                        break;
                        
                    // Xử lý khi mã trang tương ứng với yêu cầu mở Quản lý Tuyến bay
                    case "TuyenBay":
                        page = new ucTuyenBayDoiBay(_userId);
                        break;
                        
                    // Xử lý khi mã trang tương ứng với yêu cầu mở Quản lý Đội bay
                    case "DoiBay":
                        page = new ucTuyenBayDoiBay(_userId);
                        break;
                        
                    // Xử lý khi mã trang tương ứng với yêu cầu mở Báo cáo & Thống kê
                    case "BaoCao":
                        page = new ucBaoCaoThongKe(_userId);
                        break;
                        
                    // Xử lý khi mã trang tương ứng với yêu cầu mở Cấu hình Hạng ghế
                    case "HangGhe":
                        page = new ucCauHinhHangGhe(_userId);
                        break;
                        
                    // Xử lý khi mã trang tương ứng với yêu cầu mở Chăm sóc Khách hàng (CSKH)
                    case "CSKH":
                        page = new ucCSKH(this, _userId);
                        break;
                        
                    // Xử lý khi mã trang tương ứng với yêu cầu mở Lịch sử Hệ thống
                    case "LichSu":
                        page = new ucLichSuHeThong();
                        break;
                }

                if (page != null)
                {
                    _pageCache[pageId] = page;
                }
            }

            if (page != null)
            {
                // Thêm hoạt họa mờ tỏ (Fade-in) khi nạp trang để tạo cảm giác mượt mà
                pageContent.Opacity = 0;
                pageContent.Content = page;
                
                var fadeIn = new System.Windows.Media.Animation.DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.2))
                {
                    EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut }
                };
                pageContent.BeginAnimation(UIElement.OpacityProperty, fadeIn);

                // Sử dụng Dispatcher với độ ưu tiên Background để trì hoãn việc tải dữ liệu nặng từ Database.
                // Giúp luồng giao diện hoàn thành mượt mà hoạt họa chuyển trang trước, tránh bị khựng giật.
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    TriggerPageRefresh(page);
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        // Tự động tìm và gọi hàm LoadData() có sẵn trong UserControl để đảm bảo dữ liệu mới nhất
        private static void TriggerPageRefresh(UserControl page)
        {
            try
            {
                var loadDataMethod = page.GetType().GetMethod("LoadData", 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Public);
                
                if (loadDataMethod != null)
                {
                    loadDataMethod.Invoke(page, null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi làm mới dữ liệu trang: " + ex.Message);
            }
        }

        // Phương thức công khai dùng để điều hướng ứng dụng sang giao diện Tìm kiếm chuyến bay một cách trực tiếp
        public void NavigateToTimKiem()
        {
            // Thiết lập trạng thái của nút radio Tìm kiếm thành được chọn (để nổi bật giao diện đang active)
            btnTimKiem.IsChecked = true;
            LoadPage("TimKiem");
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
            
            // Tạo mới để truyền tham số scheduleId
            var page = new ucTongQuanVe(this, _userId, scheduleId);
            _pageCache["QuanLyVe"] = page; // Cập nhật lại cache

            pageContent.Opacity = 0;
            pageContent.Content = page;

            var fadeIn = new System.Windows.Media.Animation.DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.2))
            {
                EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut }
            };
            pageContent.BeginAnimation(UIElement.OpacityProperty, fadeIn);
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