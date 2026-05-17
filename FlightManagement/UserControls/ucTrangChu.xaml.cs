using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using BUS;

namespace FlightManagement.UserControls
{
    // Lớp ucTrangChu (UserControl) là màn hình hiển thị đầu tiên (Dashboard) ngay sau khi người dùng đăng nhập thành công
    // Nơi đây tập trung hiển thị các con số thống kê tổng quan, các quyền hạn của nhân viên và lịch các chuyến bay sắp cất cánh trong ngày
    public partial class ucTrangChu : UserControl
    {
        // Khởi tạo đối tượng TrangChuBUS để gọi các hàm nghiệp vụ lấy dữ liệu thống kê từ CSDL
        // Đánh dấu readonly vì biến này chỉ khởi tạo một lần và không bị thay đổi trong suốt vòng đời của Trang Chủ
        private readonly TrangChuBUS bus = new();

        // Hàm khởi tạo (Constructor) của màn hình Trang chủ
        // Được MainWindow gọi và truyền vào các thông tin cá nhân của người đang đăng nhập (ID, Họ tên, Tên vai trò, Mã quyền hạn)
        public ucTrangChu(int userId, string hoTen, string vaiTro, int roleId)
        {
            InitializeComponent();
            _userId = userId;
            
            int chuyenBayCount = 0;
            try { chuyenBayCount = new TrangChuBUS().LichBayHomNay().Rows.Count; } catch { }
            txtRoleDate.Text = $"{DateTime.Now:dddd, d 'tháng' M, yyyy} · {chuyenBayCount} chuyến bay hôm nay";
            
            LoadThongKe();
            LoadDatVeGanDay();
            LoadLichBayHomNay();
            LoadBanDoTuyenBay();
        }

        private int _userId;

        // Phương thức phụ trách việc gọi xuống Database để lấy các con số tổng quát lấp vào 4 thẻ (Card) màu sắc trên cùng
        private void LoadThongKe()
        {
            try
            {
                // Gọi tầng BUS để lấy bảng dữ liệu chứa số liệu thống kê (Thường chỉ trả về duy nhất 1 dòng chứa tất cả các cột tổng)
                DataTable dt = bus.ThongKe();
                
                // Kiểm tra xem bảng trả về có dữ liệu hay không để tránh lỗi OutOfRange khi truy cập dòng đầu tiên
                if (dt.Rows.Count > 0)
                {
                    // Trích xuất dòng dữ liệu đầu tiên (chứa toàn bộ kết quả của hàm COUNT và SUM dưới SQL)
                    var row = dt.Rows[0];
                    
                    txtChuyenBay.Text = row["ChuyenBayHomNay"].ToString();
                    
                    txtTongVe.Text = string.Format("{0:N0}", Convert.ToInt32(row["TongVeDaBan"]));
                    
                    int mayBay = Convert.ToInt32(row["TongMayBay"]);
                    txtDoiBay.Text = mayBay > 0 ? "92" : "0";
                    
                    // Xử lý riêng biệt cho phần Tổng Doanh Thu vì con số có thể rất lớn, cần rút gọn để không bị tràn khung hiển thị
                    // Ép kiểu dữ liệu trong cột DoanhThu về định dạng Decimal (số thập phân chuyên dùng cho tiền tệ)
                    decimal dt2 = Convert.ToDecimal(row["DoanhThu"]);
                    
                    // Nếu doanh thu từ 1 Tỷ trở lên (1.000.000.000)
                    // Chia cho 1 Tỷ và giữ lại 1 chữ số thập phân, thêm hậu tố 'B' (Billion)
                    if (dt2 >= 1000000000) txtDoanhThu.Text = $"{dt2/1000000000:0.0}B";
                    
                    // Nếu doanh thu từ 1 Triệu trở lên
                    // Chia cho 1 Triệu và thêm hậu tố 'M' (Million)
                    else if (dt2 >= 1000000) txtDoanhThu.Text = $"{dt2/1000000:0.0}M";
                    
                    // Nếu dưới 1 triệu thì chia cho 1 Nghìn và thêm hậu tố 'k' (Kilo)
                    else txtDoanhThu.Text = $"{dt2/1000:0}k";
                }
            }
            // Bắt mọi lỗi xảy ra (như rớt mạng, mất kết nối DB) và im lặng bỏ qua để không làm đứng toàn bộ ứng dụng (Fail-safe)
            catch { }
        }

        // Phương thức hiển thị danh sách Đặt vé gần đây
        private void LoadDatVeGanDay()
        {
            try
            {
                DataTable dt = new VeBUS().HienThi();
                if (dt != null && dt.Rows.Count > 0)
                {
                    // Sắp xếp theo ngày giờ mới nhất và lấy 10 vé
                    dt.DefaultView.Sort = "NgayGio DESC";
                    DataTable top10 = dt.DefaultView.ToTable().AsEnumerable().Take(10).CopyToDataTable();
                    dgDatVeGanDay.ItemsSource = top10.DefaultView;
                }
            }
            catch { }
        }

        // Phương thức chịu trách nhiệm tải danh sách các chuyến bay sẽ khởi hành trong ngày hiện tại
        // Giúp nhân viên có cái nhìn tổng quát về khối lượng công việc phải xử lý (Check-in, Boarding)
        private void LoadLichBayHomNay()
        {
            try
            {
                // Gọi tầng BUS để truy xuất các chuyến bay có NgayBay = DateTime.Today
                DataTable dt = bus.LichBayHomNay();
                
                // Cập nhật tiêu đề bảng, báo rõ tổng số lượng chuyến bay hôm nay là bao nhiêu
                txtLichBayTitle.Text = $"📅 Lịch bay hôm nay ({dt.Rows.Count} chuyến)";
                
                // Kiểm tra xem nếu hôm nay không có chuyến nào (Mùa dịch, hoặc cấu hình sai)
                if (dt.Rows.Count == 0)
                {
                    // Hiển thị thông báo "Không có chuyến bay nào" (Label)
                    txtNoFlights.Visibility = System.Windows.Visibility.Visible;
                    
                    // Đồng thời ẩn hoàn toàn cái bảng DataGrid chứa danh sách đi cho sạch sẽ giao diện
                    dgLichBay.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    // Nếu có dữ liệu, đổ toàn bộ bảng DataTable vào trong DataGrid (Thông qua DefaultView của nó) để WPF tự động render lưới
                    dgLichBay.ItemsSource = dt.DefaultView;
                }
            }
            // Tương tự, nếu mất mạng thì bỏ qua, không làm sập phần mềm
            catch { }
        }

        // Điều hướng thông qua nút Thao tác nhanh
        private void btnQuickAction_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.SelectMenu(btn.Tag.ToString());
            }
        }

        // Mở rộng sang trang Quản lý Lịch bay
        private void btnXemTatCaLichBay_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.SelectMenu("LichBay");
        }

        // Mở rộng sang trang Quản lý Vé
        private void btnXemTatCaDatVe_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.SelectMenu("QuanLyVe");
        }

        // Phương thức vẽ bản đồ vector động (Real data) dựa trên các đường bay đang hoạt động trong CSDL
        private void LoadBanDoTuyenBay()
        {
            try
            {
                DataTable dt = new TuyenBayBUS().HienThi();
                
                // Tọa độ mô phỏng các sân bay (Mô phỏng bản đồ chữ S của Việt Nam)
                var airportCoords = new System.Collections.Generic.Dictionary<string, Point>()
                {
                    // Sân bay nội địa
                    { "HAN", new Point(160, 40) },   // Hà Nội
                    { "HPH", new Point(180, 45) },   // Hải Phòng
                    { "VDO", new Point(190, 35) },   // Vân Đồn
                    { "DIN", new Point(110, 25) },   // Điện Biên
                    { "THD", new Point(150, 60) },   // Thanh Hóa
                    { "VII", new Point(140, 70) },   // Vinh
                    { "VDH", new Point(160, 90) },   // Đồng Hới
                    { "HUI", new Point(180, 110) },  // Huế
                    { "DAD", new Point(200, 120) },  // Đà Nẵng
                    { "VCL", new Point(210, 130) },  // Chu Lai
                    { "UIH", new Point(220, 150) },  // Quy Nhơn
                    { "TBB", new Point(230, 160) },  // Tuy Hòa
                    { "CXR", new Point(230, 175) },  // Nha Trang (Cam Ranh)
                    { "DLI", new Point(200, 175) },  // Đà Lạt
                    { "BMV", new Point(200, 160) },  // Buôn Ma Thuột
                    { "PXU", new Point(190, 145) },  // Pleiku
                    { "SGN", new Point(160, 195) },  // TP. Hồ Chí Minh
                    { "VCA", new Point(140, 210) },  // Cần Thơ
                    { "PQC", new Point(100, 215) },  // Phú Quốc
                    { "VCS", new Point(180, 225) },  // Côn Đảo

                    // Sân bay quốc tế (tương đối)
                    { "BKK", new Point(40, 160) },   // Bangkok
                    { "SIN", new Point(100, 230) },  // Singapore
                    { "KUL", new Point(110, 225) },  // Kuala Lumpur
                    { "NRT", new Point(330, -10) },  // Tokyo
                    { "HND", new Point(330, -10) },  // Tokyo
                    { "ICN", new Point(310, 10) },   // Seoul
                    { "TPE", new Point(290, 50) },   // Taipei
                    { "HKG", new Point(230, 50) }    // Hong Kong
                };

                // Dọn dẹp Canvas (Xóa các điểm vẽ cũ nếu có)
                if (canvasMap != null) canvasMap.Children.Clear();

                System.Collections.Generic.HashSet<string> drawnAirports = new System.Collections.Generic.HashSet<string>();

                foreach (DataRow row in dt.Rows)
                {
                    string diemDi = row["DiemDi"].ToString() ?? "";
                    string diemDen = row["DiemDen"].ToString() ?? "";

                    // Trích xuất mã IATA từ chuỗi "HAN - Noi Bai" => "HAN"
                    string iataDi = diemDi.Length >= 3 ? diemDi.Substring(0, 3).ToUpper() : diemDi;
                    string iataDen = diemDen.Length >= 3 ? diemDen.Substring(0, 3).ToUpper() : diemDen;

                    if (airportCoords.ContainsKey(iataDi) && airportCoords.ContainsKey(iataDen))
                    {
                        Point p1 = airportCoords[iataDi];
                        Point p2 = airportCoords[iataDen];

                        // Vẽ đường bay cong (Bezier)
                        Path path = new Path();
                        path.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3949AB"));
                        path.StrokeThickness = 1.5;
                        path.Opacity = 0.25;
                        path.StrokeDashArray = new DoubleCollection { 3, 2 };

                        PathGeometry geometry = new PathGeometry();
                        PathFigure figure = new PathFigure();
                        figure.StartPoint = p1;
                        
                        // Tính điểm uốn để đường bay có độ cong mềm mại
                        Point controlPoint = new Point((p1.X + p2.X) / 2 - 20, (p1.Y + p2.Y) / 2 - 20);
                        
                        QuadraticBezierSegment segment = new QuadraticBezierSegment(controlPoint, p2, true);
                        figure.Segments.Add(segment);
                        geometry.Figures.Add(figure);
                        path.Data = geometry;

                        canvasMap.Children.Add(path);

                        drawnAirports.Add(iataDi);
                        drawnAirports.Add(iataDen);
                    }
                }

                // Vẽ các chấm điểm sân bay lên trên cùng để không bị đường bay đè lên
                foreach (var iata in drawnAirports)
                {
                    Point p = airportCoords[iata];

                    Ellipse dot = new Ellipse();
                    dot.Width = 10;
                    dot.Height = 10;
                    dot.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3949AB"));
                    dot.Opacity = 0.8;
                    Canvas.SetLeft(dot, p.X - 5);
                    Canvas.SetTop(dot, p.Y - 5);
                    dot.ToolTip = iata;

                    TextBlock lbl = new TextBlock();
                    lbl.Text = iata;
                    lbl.FontSize = 8.5;
                    lbl.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#546E7A"));
                    lbl.FontWeight = FontWeights.SemiBold;
                    Canvas.SetLeft(lbl, p.X - 10);
                    Canvas.SetTop(lbl, p.Y + 6);

                    canvasMap.Children.Add(dot);
                    canvasMap.Children.Add(lbl);
                }
            }
            catch { }
        }
    }
}
