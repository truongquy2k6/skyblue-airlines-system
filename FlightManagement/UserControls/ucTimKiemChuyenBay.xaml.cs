using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BUS;

namespace FlightManagement.UserControls
{
    // Lớp ucTimKiemChuyenBay là trái tim của hệ thống đặt vé, cho phép nhân viên đại lý (Agent) tìm kiếm các chuyến bay hiện hành
    // Màn hình này áp dụng kỹ thuật hiển thị nhóm theo từng ngày (Group By Date) và tự động tạo DataGrid linh hoạt trên giao diện
    public partial class ucTimKiemChuyenBay : UserControl
    {
        // Khởi tạo các đối tượng BUS để kết nối với cơ sở dữ liệu
        // LichBayBUS dùng để tìm chuyến bay, SanBayBUS dùng để lấy danh sách sân bay đổ vào bộ lọc ComboBox
        private readonly LichBayBUS bus = new();
        private readonly SanBayBUS sanBayBus = new();
        
        // Biến lưu trữ tham chiếu đến màn hình cửa sổ chính (MainWindow)
        // Mục đích là để sau khi tìm xong, người dùng bấm nút Đặt vé thì nó gọi hàm điều hướng của MainWindow
        private MainWindow _mainWindow;
        
        // Biến lưu ID người dùng (nhân viên) hiện tại, thường dùng để ghi lịch sử hoặc truyền vào các hàm đặt vé sau này
        private int _userId;
        
        // Các biến phục vụ chức năng Phân trang (Pagination) ở tầng CSDL
        // Thiết lập trang hiện tại khởi điểm là trang 1
        private int _currentPage = 1;
        
        // Thiết lập số lượng bản ghi hiển thị trên mỗi trang là 10 chuyến bay để fill màn hình đẹp mắt
        private int _pageSize = 10;

        // Hàm khởi tạo (Constructor) của UserControl Tìm Kiếm
        // Được MainWindow gọi mỗi khi nhân viên bấm vào nút chức năng "Tìm kiếm chuyến bay" trên Sidebar
        public ucTimKiemChuyenBay(MainWindow mainWindow, int userId)
        {
            // Lệnh tiêu chuẩn của WPF để vẽ giao diện (Khởi tạo các TextBlock, Button, ComboBox...)
            InitializeComponent();
            
            // Giữ lại đối tượng MainWindow cha truyền vào
            _mainWindow = mainWindow; 
            
            // Giữ lại ID người dùng
            _userId = userId;         

            // Bắt đầu chuỗi thao tác tải dữ liệu lần đầu tiên (First Load)
            LoadSanBay(); // Tải danh mục Sân bay vào các ComboBox chọn điểm đi/đến
            LoadData();   // Tải toàn bộ danh sách chuyến bay lên màn hình với trang 1
        }

        // Phương thức lấy danh sách sân bay từ CSDL và đổ vào hai thẻ ComboBox làm bộ lọc
        private void LoadSanBay()
        {
            try
            {
                // Gọi tầng BUS lấy bảng danh sách sân bay
                DataTable dt = sanBayBus.HienThi();
                
                // Khởi tạo một danh sách ảo (chứa các đối tượng có ID và Tên hiển thị)
                // Đặc biệt nhét thêm một đối tượng ảo có ID là Null với chữ "Tất cả" lên đầu tiên (Giúp người dùng bỏ chọn bộ lọc)
                var items = new List<object> { new { ID = (object)DBNull.Value, Display = "Tất cả" } };
                
                // Duyệt qua từng dòng sân bay trong bảng trả về
                foreach (DataRow r in dt.Rows)
                {
                    // Ghép mã IATA và Tên sân bay lại với nhau (Ví dụ: SGN - Sân bay Tân Sơn Nhất) và đưa vào danh sách
                    items.Add(new { ID = (object)r["ID"], Display = r["IATACode"] + " - " + r["TenSanBay"] });
                }
                
                // Gán danh sách sân bay này vào ComboBox Nơi Đi và tự động chọn mục đầu tiên (Tất cả)
                cboSanBayDi.ItemsSource = items; 
                cboSanBayDi.SelectedIndex = 0;
                
                // Sao chép y chang danh sách này gán vào ComboBox Nơi Đến
                cboSanBayDen.ItemsSource = items.ToList(); 
                cboSanBayDen.SelectedIndex = 0;
            }
            // Bỏ qua ngoại lệ nếu có lỗi mạng
            catch { }
        }

        // Phương thức cốt lõi xử lý mọi tác vụ: Lấy điều kiện lọc, Gọi hàm tìm kiếm, Tính số trang, và Tạo giao diện bảng chuyến bay
        private void LoadData()
        {
            try
            {
                // Bước 1: Thu thập bộ lọc từ giao diện (UI)
                // Kiểm tra xem SelectedValue của ComboBox có phải là số (ID) hay không. Nếu chọn "Tất cả" thì nó gán giá trị rỗng (null)
                int? di = cboSanBayDi.SelectedValue is int d ? d : null;
                int? den = cboSanBayDen.SelectedValue is int a ? a : null;
                
                // Nhặt giá trị Ngày đi và Ngày đến từ 2 bộ lịch (DatePicker) trên màn hình
                DateTime? ngayTu = dpNgayTu.SelectedDate;
                DateTime? ngayDen = dpNgayDen.SelectedDate;

                // Bước 2: Gọi xuống tầng BUS để tìm kiếm với các tham số trên và tham số phân trang
                DataTable dt = bus.TimKiem(di, den, ngayTu, ngayDen, _currentPage, _pageSize);
                
                // Gọi hàm bổ trợ để tính toán xem mỗi chuyến bay còn bao nhiêu ghế trống (Do Stored Procedure tìm kiếm có thể không chứa cột này)
                EnsureRemainingSeatsColumn(dt);

                // Bước 3: Tính toán thuật toán Phân trang (Pagination)
                // Khởi tạo biến lưu tổng số dòng mà DB đếm được (Chưa bị giới hạn bởi PageSize)
                int totalRecords = 0; 
                
                // Kiểm tra xem bảng trả về có cột TotalRecords (do thủ tục SQL trả kèm) hay không
                if (dt.Rows.Count > 0 && dt.Columns.Contains("TotalRecords"))
                {
                    totalRecords = Convert.ToInt32(dt.Rows[0]["TotalRecords"]);
                }

                // Lấy tổng số kết quả chia cho số dòng trên 1 trang để ra Tổng số trang. Math.Ceiling giúp làm tròn lên (Ví dụ: 12 dòng / 5 = 2.4 -> Làm tròn thành 3 trang)
                int totalPages = (int)Math.Ceiling((double)totalRecords / _pageSize);
                
                // Chặn trường hợp bị 0 trang nếu không có kết quả nào
                if (totalPages == 0) totalPages = 1;

                // Nếu không có chuyến bay nào thì ẩn hẳn khu vực chứa nút bấm Tiến/Lùi trang đi
                pnlPagination.Visibility = totalRecords > 0 ? Visibility.Visible : Visibility.Collapsed;
                
                // Cập nhật dòng Text báo trạng thái (Ví dụ: Trang 1 / 3)
                txtPageInfo.Text = $"Trang {_currentPage} / {totalPages}"; 
                
                // Khóa nút "Trang trước" nếu đang đứng ở Trang 1
                btnPrevPage.IsEnabled = _currentPage > 1;
                btnPrevPage.Opacity = btnPrevPage.IsEnabled ? 1.0 : 0.45;
                
                // Khóa nút "Trang sau" nếu đang đứng ở Trang cuối cùng
                btnNextPage.IsEnabled = _currentPage < totalPages;
                btnNextPage.Opacity = btnNextPage.IsEnabled ? 1.0 : 0.45;

                // Bước 4: Làm sạch giao diện (Xóa trắng vùng chứa bảng)
                pnlResults.Children.Clear();
                
                // Bắt đầu dùng LINQ để gom nhóm (Group) các dòng chuyến bay theo NGÀY BAY (Loại bỏ thành phần Giờ bằng .Date)
                // Đồng thời sắp xếp (Order) các ngày từ sớm nhất đến muộn nhất
                var grouped = dt.AsEnumerable()
                    .GroupBy(r => Convert.ToDateTime(r["NgayBay"]).Date)
                    .OrderBy(g => g.Key);

                // Bước 5: Render (vẽ) bảng lên màn hình bằng code C# hoàn toàn (Không dùng XAML)
                // Vòng lặp duyệt qua từng cụm Ngày bay (Ví dụ: Nhóm ngày 15/05, sau đó là nhóm 16/05)
                foreach (var group in grouped)
                {
                    // Lấy ra chuỗi mô tả thân thiện (Ví dụ: "Hôm nay", "Ngày mai") thông qua hàm tự viết
                    string dayLabel = GetDayLabel(group.Key);
                    
                    // Đếm số lượng chuyến bay cất cánh trong ngày hôm đó
                    int count = group.Count();

                    // Khởi tạo một thanh Header (StackPanel) hiển thị tiêu đề ngày bay
                    string thu = "";
                    switch (group.Key.DayOfWeek)
                    {
                        case DayOfWeek.Monday: thu = "Thứ Hai"; break;
                        case DayOfWeek.Tuesday: thu = "Thứ Ba"; break;
                        case DayOfWeek.Wednesday: thu = "Thứ Tư"; break;
                        case DayOfWeek.Thursday: thu = "Thứ Năm"; break;
                        case DayOfWeek.Friday: thu = "Thứ Sáu"; break;
                        case DayOfWeek.Saturday: thu = "Thứ Bảy"; break;
                        case DayOfWeek.Sunday: thu = "Chủ Nhật"; break;
                    }
                    string vietnameseDate = $"{thu}, {group.Key.Day} tháng {group.Key.Month}, {group.Key.Year}";
                    string countText = count == 1 ? "1 chuyến bay" : $"{count} chuyến bay";

                    var headerPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(0, 15, 0, 10),
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    var calendarIcon = new MaterialDesignThemes.Wpf.PackIcon
                    {
                        Kind = MaterialDesignThemes.Wpf.PackIconKind.CalendarMonth,
                        Width = 18,
                        Height = 18,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 8, 0),
                        Foreground = Brushes.White
                    };

                    var headerText = new TextBlock
                    {
                        Text = $"{dayLabel} - {vietnameseDate} - {countText}",
                        FontSize = 14.5,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.White,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    headerPanel.Children.Add(calendarIcon);
                    headerPanel.Children.Add(headerText);

                    pnlResults.Children.Add(headerPanel);

                    // Bắt đầu nhào nặn một cái DataGrid (Bảng) mới tinh để đựng chuyến bay của ngày đó
                    var dg = new DataGrid
                    {
                        AutoGenerateColumns = false, // Tắt tự động tạo cột để code bên dưới tự chỉnh bằng tay
                        IsReadOnly = true, // Khóa bảng không cho người dùng nháy đúp sửa chữ
                        CanUserAddRows = false, // Chặn hiển thị dòng trống cuối bảng
                        GridLinesVisibility = DataGridGridLinesVisibility.Horizontal, // Chỉ hiện vạch kẻ ngang giữa các dòng
                        BorderThickness = new Thickness(0), // Viền do Border bọc ngoài đảm nhận
                        Margin = new Thickness(0),
                        Background = Brushes.White,
                        ColumnHeaderHeight = 42, // Chiều cao hàng tiêu đề
                        RowHeight = 48, // Chiều cao mỗi dòng dữ liệu
                        FontSize = 13.5, // Cỡ chữ chuẩn
                        AlternatingRowBackground = new SolidColorBrush(Color.FromRgb(244, 246, 249)),
                        ColumnHeaderStyle = (Style)FindResource("DefaultColumnHeaderStyle"),
                        CellStyle = (Style)FindResource("DefaultCellStyle"),
                        SelectionUnit = DataGridSelectionUnit.FullRow
                    };

                    // Đăng ký sự kiện lăn chuột trực tiếp để cuộn ScrollViewer cha bên ngoài một cách mượt mà
                    dg.PreviewMouseWheel += (sender, args) =>
                    {
                        if (args.Handled) return;
                        
                        var parent = VisualTreeHelper.GetParent(dg);
                        while (parent != null)
                        {
                            if (parent is ScrollViewer sv)
                            {
                                args.Handled = true;
                                double newOffset = sv.VerticalOffset - (args.Delta * 0.4);
                                sv.ScrollToVerticalOffset(newOffset);
                                break;
                            }
                            parent = VisualTreeHelper.GetParent(parent);
                        }
                    };

                    // Bắt đầu nối các Cột (Column) vào DataGrid vừa tạo. Mỗi cột ràng buộc (Binding) với một trường trong DB
                    // Cột Số Hiệu chuyến bay, áp dụng Badge xanh cực đẹp
                    var soHieuCol = new DataGridTemplateColumn
                    {
                        Header = "Số hiệu",
                        Width = 95,
                        MinWidth = 95,
                        HeaderStyle = (Style)FindResource("CenteredHeaderStyle")
                    };
                    var shFactory = new FrameworkElementFactory(typeof(Border));
                    shFactory.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromRgb(227, 242, 253))); // #E3F2FD
                    shFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
                    shFactory.SetValue(Border.PaddingProperty, new Thickness(6, 3, 6, 3));
                    shFactory.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                    shFactory.SetValue(Border.VerticalAlignmentProperty, VerticalAlignment.Center);

                    var tbFactory = new FrameworkElementFactory(typeof(TextBlock));
                    tbFactory.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("SoHieu"));
                    tbFactory.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Color.FromRgb(21, 101, 192))); // #1565C0
                    tbFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
                    tbFactory.SetValue(TextBlock.FontSizeProperty, 12.5);

                    shFactory.AppendChild(tbFactory);
                    soHieuCol.CellTemplate = new DataTemplate { VisualTree = shFactory };
                    dg.Columns.Add(soHieuCol);
                    
                    // Cột Sân bay đi
                    dg.Columns.Add(new DataGridTextColumn 
                    { 
                        Header = "Điểm đi", 
                        Binding = new System.Windows.Data.Binding("SanBayDi"), 
                        Width = new DataGridLength(1.4, DataGridLengthUnitType.Star), 
                        MinWidth = 150,
                        ElementStyle = MakeValStyle(HorizontalAlignment.Left)
                    });
                    
                    // Cột Giờ khởi hành
                    dg.Columns.Add(new DataGridTextColumn 
                    { 
                        Header = "Giờ đi", 
                        Binding = new System.Windows.Data.Binding("GioBay") { StringFormat = @"hh\:mm" }, 
                        Width = 75, 
                        MinWidth = 75,
                        HeaderStyle = (Style)FindResource("CenteredHeaderStyle"),
                        ElementStyle = MakeValStyle(HorizontalAlignment.Center)
                    });
                    
                    // Cột Sân bay đến
                    dg.Columns.Add(new DataGridTextColumn 
                    { 
                        Header = "Điểm đến", 
                        Binding = new System.Windows.Data.Binding("SanBayDen"), 
                        Width = new DataGridLength(1.4, DataGridLengthUnitType.Star), 
                        MinWidth = 150,
                        ElementStyle = MakeValStyle(HorizontalAlignment.Left)
                    });
                    
                    // Cột Thời lượng bay
                    dg.Columns.Add(new DataGridTextColumn 
                    { 
                        Header = "Thời gian bay", 
                        Binding = new System.Windows.Data.Binding("ThoiGianBay"), 
                        Width = 110, 
                        MinWidth = 110,
                        HeaderStyle = (Style)FindResource("CenteredHeaderStyle"),
                        ElementStyle = MakeValStyle(HorizontalAlignment.Center)
                    });
                    
                    // Cột Dòng máy bay khai thác
                    dg.Columns.Add(new DataGridTextColumn 
                    { 
                        Header = "Máy bay", 
                        Binding = new System.Windows.Data.Binding("MayBay"), 
                        Width = 100, 
                        MinWidth = 100,
                        HeaderStyle = (Style)FindResource("CenteredHeaderStyle"),
                        ElementStyle = MakeValStyle(HorizontalAlignment.Center)
                    });
                    
                    // Cột báo số Ghế trống còn lại
                    dg.Columns.Add(new DataGridTextColumn
                    {
                        Header = "Ghế trống",
                        Binding = new System.Windows.Data.Binding("GheTrong"),
                        Width = 90,
                        MinWidth = 90,
                        HeaderStyle = (Style)FindResource("CenteredHeaderStyle"),
                        ElementStyle = MakeValStyle(HorizontalAlignment.Center, Color.FromRgb(38, 50, 56), true)
                    });
                    
                    // Cột hiển thị Giá vé hạng Phổ thông
                    dg.Columns.Add(new DataGridTextColumn
                    {
                        Header = "Giá Phổ Thông",
                        Binding = new System.Windows.Data.Binding("GiaEconomy") { StringFormat = "{0:N0} đ" },
                        Width = 135,
                        MinWidth = 135,
                        HeaderStyle = (Style)FindResource("CenteredHeaderStyle"),
                        ElementStyle = MakeValStyle(HorizontalAlignment.Center, Color.FromRgb(230, 74, 25), true)
                    });
                    
                    // Cột hiển thị Giá vé hạng Thương gia
                    dg.Columns.Add(new DataGridTextColumn
                    {
                        Header = "Giá Thương Gia",
                        Binding = new System.Windows.Data.Binding("GiaBusiness") { StringFormat = "{0:N0} đ" },
                        Width = 135,
                        MinWidth = 135,
                        HeaderStyle = (Style)FindResource("CenteredHeaderStyle"),
                        ElementStyle = MakeValStyle(HorizontalAlignment.Center, Color.FromRgb(123, 31, 162), true)
                    });

                    // Cột hiển thị Giá vé hạng Nhất
                    dg.Columns.Add(new DataGridTextColumn
                    {
                        Header = "Giá Hạng Nhất",
                        Binding = new System.Windows.Data.Binding("GiaFirstClass") { StringFormat = "{0:N0} đ" },
                        Width = 135,
                        MinWidth = 135,
                        HeaderStyle = (Style)FindResource("CenteredHeaderStyle"),
                        ElementStyle = MakeValStyle(HorizontalAlignment.Center, Color.FromRgb(198, 163, 0), true)
                    });
 
                    // Bắt đầu tạo một cột đặc biệt: Cột chứa Nút Bấm "Đặt vé" (Template Column)
                    var btnCol = new DataGridTemplateColumn 
                    { 
                        Header = "Thao tác", 
                        Width = 100, 
                        MinWidth = 100,
                        HeaderStyle = (Style)FindResource("CenteredHeaderStyle")
                    };
                    
                    // FrameworkElementFactory giúp tự động nặn ra các cục Button cho từng dòng trong bảng
                    var factory = new FrameworkElementFactory(typeof(Button));
                    
                    // Ép nội dung chữ của nút là "Đặt vé"
                    factory.SetValue(Button.ContentProperty, "Đặt vé");
                    factory.SetValue(Button.FontSizeProperty, 13.0);
                    factory.SetValue(Button.FontWeightProperty, FontWeights.Bold);
                    factory.SetValue(Button.HeightProperty, 32.0);
                    factory.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                    factory.SetValue(Button.PaddingProperty, new Thickness(12, 0, 12, 0));
                    
                    // Tô nền nút thành màu Xanh nước biển thương hiệu
                    factory.SetValue(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(63, 81, 181)));
                    factory.SetValue(Button.ForegroundProperty, Brushes.White);
                    factory.SetValue(Button.BorderThicknessProperty, new Thickness(0));
                    
                    // Chỉnh icon con trỏ chuột biến thành Bàn tay (Hand) khi di chuột qua nút
                    factory.SetValue(Button.CursorProperty, System.Windows.Input.Cursors.Hand);
                    
                    // Gắn hàm sự kiện btnDatVe_Click vào hành động Bấm nút (ClickEvent)
                    factory.AddHandler(Button.ClickEvent, new RoutedEventHandler(btnDatVe_Click));
                    
                    // Liên kết thuộc tính IsEnabled với cột IsBookable để vô hiệu hóa nút nếu chuyến bay đã qua
                    factory.SetBinding(Button.IsEnabledProperty, new System.Windows.Data.Binding("IsBookable"));
                    
                    // Hoàn thiện cái khuôn đúc cột và nhét nó vào DataGrid
                    btnCol.CellTemplate = new DataTemplate { VisualTree = factory };
                    dg.Columns.Add(btnCol);
 
                    // Xử lý dữ liệu: Tạo một phiên bản (Clone) của cấu trúc bảng dữ liệu tổng ban đầu
                    DataTable groupTable = dt.Clone();
                    
                    // Hút từng dòng chuyến bay thuộc Nhóm ngày hiện tại châm vào bảng sao chép này
                    foreach (var row in group) groupTable.ImportRow(row);
                    
                    // Nạp nguồn dữ liệu đã chia nhóm này vào DataGrid
                    dg.ItemsSource = groupTable.DefaultView;
 
                    // Tạo một khung viền (Border) bo góc cong ôm trọn cái DataGrid cho giao diện mềm mại, hiện đại
                    var border = new Border
                    {
                        Background = Brushes.White,
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(10),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                        BorderThickness = new Thickness(1),
                        Margin = new Thickness(0, 0, 0, 15), // Cách lề dưới để không dính vào ngày tiếp theo
                        VerticalAlignment = VerticalAlignment.Top
                    };
                    
                    // Đặt DataGrid vào trong lòng cái Border
                    border.Child = dg;
                    
                    pnlResults.Children.Add(border);
                }

                // Nếu vòng lặp gom nhóm phát hiện không có bất kỳ kết quả nào (Any() = false)
                if (!grouped.Any())
                {
                    // Chèn một dòng thông báo lỗi thân thiện vào giữa màn hình
                    pnlResults.Children.Add(new TextBlock
                    {
                        Text = "⚠ Không tìm thấy chuyến bay nào phù hợp.",
                        FontSize = 16,
                        Foreground = Brushes.Gray,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 40, 0, 40)
                    });
                }
            }
            catch (Exception ex) { ShowDialogMessage("Lỗi: " + ex.Message, "Lỗi hệ thống"); }
        }

        // Hàm tiện ích phân tích Ngày tháng do DateTime cung cấp để chuyển đổi thành các chuỗi văn bản thân thiện với con người
        private string GetDayLabel(DateTime date)
        {
            // Nếu ngày truyền vào khớp với ngày hiện hành trên máy tính -> Trả về "Hôm nay"
            if (date.Date == DateTime.Today) return "Hôm nay";
            
            // Nếu cộng ngày hiện hành lên 1 mà khớp -> "Ngày mai"
            if (date.Date == DateTime.Today.AddDays(1)) return "Ngày mai";
            
            // Nếu trừ đi 1 ngày mà khớp -> "Hôm qua"
            if (date.Date == DateTime.Today.AddDays(-1)) return "Hôm qua";
            
            // Nếu xa quá thì trả về định dạng chuẩn dd/MM/yyyy
            return date.ToString("dd/MM/yyyy");
        }

        // Hàm tạo Style (Định dạng) tự động giúp tô đậm chữ và đổi thành màu xanh nước biển cho cột DataGrid
        private Style MakeBoldBlue()
        {
            // Báo cho WPF biết là tao đang tạo một gói định dạng dành riêng cho cục TextBlock
            var s = new Style(typeof(TextBlock));
            s.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.Bold));
            s.Setters.Add(new Setter(TextBlock.ForegroundProperty, new SolidColorBrush(Color.FromRgb(63, 81, 181))));
            return s;
        }

        // Hàm tạo Style màu chữ tùy chỉnh dựa theo bảng màu truyền vào (Giúp giảm bớt code bị lặp lại nhiều lần ở trên)
        private Style MakeColor(Color c)
        {
            var s = new Style(typeof(TextBlock));
            s.Setters.Add(new Setter(TextBlock.ForegroundProperty, new SolidColorBrush(c)));
            return s;
        }

        // Phương thức kiểm tra và điền cột thông tin "Ghế trống" còn lại cho từng chuyến bay
        // Rất quan trọng vì tính năng Tìm kiếm SQL chỉ ném về số lượng vé đã đặt chứ chưa trừ đi tổng ghế của tàu bay
        private void EnsureRemainingSeatsColumn(DataTable dt)
        {
            // Thêm cột kiểm tra thời gian bay (nếu chưa có)
            if (!dt.Columns.Contains("IsBookable"))
            {
                dt.Columns.Add("IsBookable", typeof(bool));
            }

            bool missingGheTrong = !dt.Columns.Contains("GheTrong");
            
            // Chèn thêm một cột giả (không có thật dưới DB) tên là GheTrong, kiểu số nguyên (int)
            if (missingGheTrong)
            {
                dt.Columns.Add("GheTrong", typeof(int));
            }
            
            // Triệu hồi hàm HienThi() lấy hết toàn bộ lịch bay thô trong hệ thống
            DataTable allSchedules = null;
            Dictionary<int, int> remainingSeatsBySchedule = null;
            
            if (missingGheTrong)
            {
                allSchedules = bus.HienThi();
                // Dùng sức mạnh của LINQ chuyển bảng lịch bay thô đó thành một Từ điển (Dictionary) siêu tốc độ 
                remainingSeatsBySchedule = allSchedules.AsEnumerable()
                    .ToDictionary(
                        r => Convert.ToInt32(r["ID"]),
                        r => Convert.ToInt32(r["GheTrong"]));
            }

            // Lấy từng dòng trong bảng Tìm kiếm hiện tại
            foreach (DataRow row in dt.Rows)
            {
                if (missingGheTrong)
                {
                    // Bóc mã ID lịch bay ra
                    int scheduleId = Convert.ToInt32(row["ID"]);
                    
                    // Trực tiếp móc cái từ điển ở trên, nếu thấy Key tương ứng thì nhét số ghế dư vào cột. Không thấy thì cho là 0 ghế
                    row["GheTrong"] = remainingSeatsBySchedule.TryGetValue(scheduleId, out int seatsLeft) ? seatsLeft : 0;
                }
                
                // Ràng buộc thời gian: Không cho phép đặt chuyến bay đã qua giờ cất cánh
                DateTime ngayBay = Convert.ToDateTime(row["NgayBay"]);
                TimeSpan gioBay = (TimeSpan)row["GioBay"];
                row["IsBookable"] = ngayBay.Date.Add(gioBay) > DateTime.Now;
            }
        }

        // Sự kiện xảy ra khi nhân viên bấm vào nút [Tìm chuyến bay] hình cái phễu lọc
        private void btnLoc_Click(object sender, RoutedEventArgs e)
        {
            // Đặt cưỡng bức trang về lại số 1 (Để tránh lỗi nếu người dùng đang đứng ở trang 5 mà đổi bộ lọc)
            _currentPage = 1;
            
            // Bắt đầu chu trình chạy lấy lại dữ liệu mới từ Database
            LoadData();
        }

        // Sự kiện xảy ra khi nhân viên bấm vào nút [Hủy lọc] màu xám
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            // Đá hai thanh ComboBox điểm đi điểm đến về mục đầu tiên (Mục chữ "Tất cả")
            cboSanBayDi.SelectedIndex = 0; 
            cboSanBayDen.SelectedIndex = 0; 
            
            // Xóa rỗng luôn hai cái lịch ngày tháng
            dpNgayTu.SelectedDate = null; 
            dpNgayDen.SelectedDate = null;
            
            // Trả về trang 1
            _currentPage = 1;
            
            // Tải lại bảng danh sách không điều kiện
            LoadData();
        }

        // Sự kiện xảy ra khi bấm nút [<] lùi về một trang hiển thị trước đó
        private void btnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            // Xác nhận lại lần nữa là không bị lố về trang 0
            if (_currentPage > 1)
            {
                _currentPage--; // Trừ trang
                LoadData(); // Load lại data
            }
        }

        // Sự kiện xảy ra khi bấm nút [>] tiến lên trang kết quả tiếp theo
        private void btnNextPage_Click(object sender, RoutedEventArgs e)
        {
            _currentPage++; // Cộng trang
            LoadData(); // Load lại data
        }

        // Hành động quan trọng nhất: Khi một nút "Đặt vé" xanh lơ trên bất kỳ dòng nào được nhân viên Click
        private void btnDatVe_Click(object sender, RoutedEventArgs e)
        {
            // Lệnh IF này thực hiện ép kiểu và trích xuất dữ liệu dòng:
            // "sender" là cái nút vừa bị bấm, ép nó thành cục Button. 
            // Sau đó sờ vào bộ xương (DataContext) của cái nút đó, ép thành DataRowView (Cái dòng nằm sau nút bấm đó)
            if (sender is Button btn && btn.DataContext is DataRowView row)
            {
                // Ràng buộc thời gian: Kiểm tra phòng hờ giao diện lỗi
                DateTime ngayBay = Convert.ToDateTime(row["NgayBay"]);
                TimeSpan gioBay = (TimeSpan)row["GioBay"];
                if (ngayBay.Date.Add(gioBay) <= DateTime.Now)
                {
                    ShowDialogMessage("Chuyến bay này đã qua giờ cất cánh. Bạn không thể đặt vé!", "Thông báo");
                    return;
                }

                // Móc lấy trường ID nằm trên cái dòng đó (ID chuyến bay cần mua vé)
                int scheduleId = Convert.ToInt32(row["ID"]);
                
                // Truyền lệnh yêu cầu cửa sổ chính (MainWindow) đá nhân viên sang màn hình Đặt vé chi tiết cùng với mã chuyến bay này
                _mainWindow.NavigateToDatVe(scheduleId);
            }
        }

        private Style MakeValStyle(HorizontalAlignment align, Color? textColor = null, bool bold = false)
        {
            var s = new Style(typeof(TextBlock));
            s.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));
            s.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, align));
            if (textColor.HasValue)
            {
                s.Setters.Add(new Setter(TextBlock.ForegroundProperty, new SolidColorBrush(textColor.Value)));
            }
            if (bold)
            {
                s.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.Bold));
            }
            return s;
        }

        // ================= DIALOG THÔNG BÁO HIỆN ĐẠI (INLINE) =================
        private void ShowDialogMessage(string message, string title = "Thông báo")
        {
            var view = new StackPanel { Margin = new Thickness(25), MinWidth = 350 };
            view.Children.Add(new TextBlock { Text = title, FontWeight = FontWeights.Bold, FontSize = 18, Margin = new Thickness(0, 0, 0, 15), Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)) });
            view.Children.Add(new TextBlock { Text = message, FontSize = 15, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 25), Foreground = new SolidColorBrush(Color.FromRgb(85, 85, 85)) });
            var btnOk = new Button { Content = "XÁC NHẬN", HorizontalAlignment = HorizontalAlignment.Right, Style = (Style)FindResource("MaterialDesignFlatButton"), Foreground = new SolidColorBrush(Color.FromRgb(57, 73, 171)) };
            btnOk.Click += (s, ev) => dialogHost.IsOpen = false;
            view.Children.Add(btnOk);
            dialogHost.DialogContent = view;
            dialogHost.IsOpen = true;
        }
    }
}
