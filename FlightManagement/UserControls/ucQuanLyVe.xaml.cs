using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BUS;

namespace FlightManagement.UserControls
{
    // Lớp ucQuanLyVe là một UserControl, đóng vai trò như một thành phần giao diện nhỏ có thể nhúng vào MainWindow
    // Nó chịu trách nhiệm hiển thị và quản lý danh sách các vé máy bay đã được đặt trong hệ thống
    public partial class ucQuanLyVe : UserControl
    {
        // Khởi tạo đối tượng VeBUS thuộc tầng Nghiệp Vụ (BUS) để giao tiếp và lấy dữ liệu vé từ cơ sở dữ liệu
        private readonly VeBUS bus = new();
        
        // Khởi tạo bảng dữ liệu DataTable để lưu trữ tạm thời danh sách vé của trang hiện tại
        private DataTable _data = new();
        
        // Khai báo biến lưu trữ số thứ tự của trang dữ liệu hiện tại đang hiển thị trên lưới (mặc định là trang 1)
        private int _currentPage = 1;
        
        // Khai báo hằng số quy định số lượng vé tối đa được hiển thị trên mỗi trang
        private int _pageSize = 10;
        
        // Khai báo biến lưu trữ tổng số trang tính toán được sau khi đã lọc dữ liệu (ít nhất là 1 trang)
        private int _totalPages = 1;

        // Khai báo DispatcherTimer để debounce chức năng tìm kiếm nhập tay (Tránh lag Database)
        private readonly System.Windows.Threading.DispatcherTimer _searchTimer;
        
        // Hàm khởi tạo của UserControl quản lý vé
        public ucQuanLyVe() 
        { 
            // Gọi phương thức tự động sinh của WPF để khởi tạo các nút bấm, lưới dữ liệu đã thiết kế bên file XAML
            InitializeComponent(); 
            
            // Khởi tạo DispatcherTimer để debounce (tránh gọi Database liên tục khi hành khách đang gõ phím)
            _searchTimer = new System.Windows.Threading.DispatcherTimer();
            _searchTimer.Interval = TimeSpan.FromMilliseconds(400); // Chờ 400ms sau khi ngừng gõ mới truy vấn DB
            _searchTimer.Tick += SearchTimer_Tick;

            // Ngay sau khi giao diện tải xong, gọi hàm LoadData để nạp dữ liệu vé từ database lên màn hình
            LoadData(); 
        }

        // Phương thức chịu trách nhiệm nạp dữ liệu từ database và thiết lập các bộ lọc mặc định ban đầu
        public void LoadData()
        {
            try
            {
                // Nạp danh sách các tuyến bay vào ComboBox
                cboChuyenBay.SelectionChanged -= cboChuyenBay_Changed; // Tạm ngắt sự kiện để tránh gọi 2 lần
                cboChuyenBay.Items.Clear();
                cboChuyenBay.Items.Add("Tất cả chuyến bay");
                
                DataTable dtTuyenBay = bus.DanhSachTuyenBay();
                foreach (DataRow r in dtTuyenBay.Rows)
                {
                    cboChuyenBay.Items.Add(r["TuyenBay"].ToString());
                }
                cboChuyenBay.SelectedIndex = 0;
                cboChuyenBay.SelectionChanged += cboChuyenBay_Changed;

                // Lấy dữ liệu phân trang cho trang 1
                _currentPage = 1;
                LoadDataPage();
            }
            catch { }
        }

        // Phương thức gọi xuống Database để lấy đúng 1 trang dữ liệu dựa trên bộ lọc
        private void LoadDataPage()
        {
            try
            {
                string keyword = txtSearch.Text.Trim();

                string tuyenBay = "";
                if (cboChuyenBay.SelectedIndex > 0 && cboChuyenBay.SelectedItem != null)
                {
                    tuyenBay = cboChuyenBay.SelectedItem.ToString()!;
                }

                _data = bus.HienThiPhanTrang(keyword, tuyenBay, _currentPage, _pageSize);
                
                if (_data.Rows.Count > 0)
                {
                    int totalRecords = Convert.ToInt32(_data.Rows[0]["TotalRecords"]);
                    _totalPages = (int)Math.Ceiling((double)totalRecords / _pageSize);
                    txtCount.Text = $"✈ Danh sách vé - Tìm thấy {totalRecords} vé";
                }
                else
                {
                    _totalPages = 1;
                    txtCount.Text = $"✈ Danh sách vé - Tìm thấy 0 vé";
                }

                if (_currentPage > _totalPages) _currentPage = _totalPages;
                txtPageInfo.Text = $"Trang {_currentPage}/{_totalPages}";
                
                dgVe.ItemsSource = _data.DefaultView;
                
                btnPrev.IsEnabled = _currentPage > 1;
                btnNext.IsEnabled = _currentPage < _totalPages;
            }
            catch { }
        }

        // Sự kiện xảy ra khi DispatcherTimer hoàn thành thời gian chờ (người dùng đã ngừng gõ đủ 400ms)
        private void SearchTimer_Tick(object? sender, EventArgs e)
        {
            _searchTimer.Stop();
            _currentPage = 1;
            LoadDataPage();
        }

        // Sự kiện tự động kích hoạt mỗi khi người dùng gõ thêm hoặc xóa bớt một ký tự trong thanh tìm kiếm
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchTimer.Stop(); // Reset timer cũ nếu còn đang đếm ngược
            _searchTimer.Start(); // Bắt đầu đếm ngược 400ms
        }

        // Sự kiện tự động kích hoạt mỗi khi người dùng chọn một mục khác trong danh sách tuyến bay
        private void cboChuyenBay_Changed(object sender, SelectionChangedEventArgs e)
        {
            _currentPage = 1;
            LoadDataPage();
        }

        // Sự kiện kích hoạt khi người dùng nhấn vào nút "In vé" trên giao diện
        private void btnInVe_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra xem người dùng đã dùng chuột nhấp chọn một dòng cụ thể nào trên bảng danh sách vé hay chưa
            DataRowView? row = dgVe.SelectedItem as DataRowView;
            
            // Nếu không có Item nào được chọn cả dòng, thử lấy từ ô đang được chọn (Do SelectionUnit = CellOrRowHeader)
            if (row == null && dgVe.SelectedCells.Count > 0)
            {
                row = dgVe.SelectedCells[0].Item as DataRowView;
            }

            if (row == null)
            {
                // Nếu chưa chọn, bật lên một hộp thoại cảnh báo yêu cầu người dùng phải chọn một vé trước
                ShowDialogMessage("Vui lòng chọn một vé từ danh sách để in!", "Chưa chọn vé");
                
                // Thoát ngang khỏi sự kiện, không thực hiện tiếp đoạn code bên dưới
                return;
            }
            
            // Đọc cột "TenKhach" từ dòng dữ liệu và chuyển thành chuỗi để lấy tên khách hàng
            string tenKhach = row["TenKhach"].ToString()!;
            
            // Đọc cột "TuyenBay" từ dòng dữ liệu để lấy lộ trình bay (VD: SGN-HAN)
            string tuyenBay = row["TuyenBay"].ToString()!;
            
            // Đọc cột "SoHieu" từ dòng dữ liệu để lấy mã chuyến bay (VD: VN123)
            string soHieu = row["SoHieu"].ToString()!;
            
            // Xử lý đọc ngày bay: Bảng dữ liệu có thể có cột NgayBay riêng hoặc cột NgayGio chung. Ta kiểm tra xem cột nào tồn tại.
            // Nếu có cột NgayBay, ta parse nó sang kiểu DateTime và định dạng thành chuỗi chuẩn "dd MMM yyyy"
            // Nếu không có, ta thử parse cột NgayGio. Nếu cả hai không có, trả về chuỗi rỗng
            string ngayBay = row.Row.Table.Columns.Contains("NgayBay") ? Convert.ToDateTime(row["NgayBay"]).ToString("dd/MM/yyyy") : 
                             (row.Row.Table.Columns.Contains("NgayGio") ? Convert.ToDateTime(row["NgayGio"]).ToString("dd/MM/yyyy") : "");
                             
            // Xử lý đọc giờ bay tương tự như cách xử lý ngày bay ở trên
            // Nếu có cột GioBay thì lấy luôn chuỗi đó, nếu không thì lấy thuộc tính giờ từ cột NgayGio
            string gioBay = row.Row.Table.Columns.Contains("GioBay") ? row["GioBay"].ToString()! : 
                            (row.Row.Table.Columns.Contains("NgayGio") ? Convert.ToDateTime(row["NgayGio"]).ToString("HH:mm") : "");
            
            // Xử lý đọc số ghế: Kiểm tra xem bảng có trả về cột SeatNumber không và giá trị của nó có bị NULL (DBNull) hay không
            // Nếu khách đã chọn ghế thì lưu vào biến, nếu không thì để chuỗi rỗng
            string ghe = row.Row.Table.Columns.Contains("SeatNumber") && row["SeatNumber"] != DBNull.Value ? row["SeatNumber"].ToString()! : "";
            
            // Đọc cột "MaDatCho" từ dòng dữ liệu để lấy mã PNR (Booking Reference) in lên vé
            string bookingRef = row["MaDatCho"].ToString()!;
            
            // Đọc cột "HangGhe" để hiển thị lên vé
            string hangGhe = row["HangGhe"].ToString()!;

            // Khởi tạo một đối tượng cửa sổ TicketWindow (giao diện in vé Boarding Pass) và truyền toàn bộ các tham số vừa lấy được vào hàm tạo
            TicketWindow ticketWindow = new TicketWindow(tenKhach, tuyenBay, soHieu, ngayBay, gioBay, ghe, bookingRef, hangGhe);
            
            // Tìm kiếm đối tượng cửa sổ cha (Window) đang chứa UserControl này (tức là cửa sổ MainWindow)
            var ownerWindow = Window.GetWindow(this);
            
            // Nếu tìm thấy cửa sổ cha thành công
            if (ownerWindow != null)
            {
                // Gán thuộc tính Owner của cửa sổ in vé thành cửa sổ MainWindow
                // Điều này giúp cửa sổ in vé luôn nổi lên trên cùng và nằm chính giữa MainWindow
                ticketWindow.Owner = ownerWindow;

                // Bật hiệu ứng làm mờ (Blur) cho cửa sổ chính để làm nổi bật tấm vé
                var blurEffect = new System.Windows.Media.Effects.BlurEffect { Radius = 10, RenderingBias = System.Windows.Media.Effects.RenderingBias.Quality };
                ownerWindow.Effect = blurEffect;

                // Hiển thị cửa sổ in vé dưới dạng Dialog (ngăn người dùng tương tác với MainWindow bên dưới)
                ticketWindow.ShowDialog();

                // Sau khi cửa sổ in vé đóng lại, ta gỡ bỏ hiệu ứng làm mờ để MainWindow trở lại bình thường
                ownerWindow.Effect = null;
            }
            else
            {
                // Nếu không tìm thấy owner, chỉ hiện cửa sổ lên bình thường
                ticketWindow.ShowDialog();
            }
        }

        // Đã xóa hàm ApplyFilters và UpdatePagination vì đã gộp vào LoadDataPage

        // Sự kiện kích hoạt khi người dùng nhấn vào nút "Trang trước"
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra an toàn: Nếu đang ở trang 1 hoặc nhỏ hơn thì bỏ qua không lùi trang được nữa
            if (_currentPage <= 1) return;
            
            // Giảm chỉ số trang hiện tại đi 1 đơn vị
            _currentPage--;
            
            // Gọi hàm cập nhật phân trang để tải trang mới từ Database
            LoadDataPage();
        }

        // Sự kiện kích hoạt khi người dùng nhấn vào nút "Trang kế"
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra an toàn: Nếu đang ở trang cuối cùng rồi thì bỏ qua không nhảy trang được nữa
            if (_currentPage >= _totalPages) return;
            
            // Tăng chỉ số trang hiện hành lên 1 đơn vị
            _currentPage++;
            
            // Gọi hàm cập nhật phân trang để tải trang mới từ Database
            LoadDataPage();
        }

        // Sự kiện xảy ra khi người dùng nhấp vào nút "Hoàn vé" (Biểu tượng hoàn tiền màu cam)
        private void btnHoanVe_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.DataContext is not DataRowView row) return;
            
            if (!CanManageTicket(row, out string reason))
            {
                ShowDialogMessage(reason, "Không thể thao tác");
                return;
            }

            ShowConfirmDialog($"Bạn có chắc muốn HOÀN VÉ {row["MaDatCho"]} và HOÀN TIỀN lại cho khách hàng không?", "Xác nhận hoàn vé", () =>
            {
                try
                {
                    int ticketId = Convert.ToInt32(row["ID"]);
                    bus.HuyVe(ticketId);
                    LoadData();
                    ShowDialogMessage("Đã hoàn vé và ghi nhận hoàn tiền thành công.", "Thành công");
                }
                catch (Exception ex)
                {
                    ShowDialogMessage("Không thể hoàn vé: " + ex.Message, "Lỗi");
                }
            });
        }

        // Sự kiện xảy ra khi người dùng nhấp vào nút "Hủy vé" (Biểu tượng thùng rác) nằm trên một dòng cụ thể của lưới DataGrid
        private void btnHuyVe_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.DataContext is not DataRowView row) return;
            
            if (!CanManageTicket(row, out string reason))
            {
                ShowDialogMessage(reason, "Không thể thao tác");
                return;
            }

            ShowConfirmDialog($"Bạn có chắc muốn hủy vé {row["MaDatCho"]}?", "Xác nhận hủy vé", () =>
            {
                try
                {
                    int ticketId = Convert.ToInt32(row["ID"]);
                    bus.HuyVe(ticketId);
                    LoadData();
                    ShowDialogMessage("Đã hủy vé thành công.", "Thành công");
                }
                catch (Exception ex)
                {
                    ShowDialogMessage("Không thể hủy vé: " + ex.Message, "Lỗi");
                }
            });
        }

        // Phương thức kiểm tra toàn diện các quy tắc kinh doanh xem một vé máy bay cụ thể có được quyền hủy hay không
        private bool CanManageTicket(DataRowView row, out string reason)
        {
            // Trích xuất trạng thái hiện tại của vé từ dòng dữ liệu
            string status = row["TrangThai"]?.ToString() ?? "";
            
            // Kiểm tra quy tắc 1: Nếu vé đó vốn dĩ đã bị hủy từ trước
            if (status == "Đã hủy")
            {
                // Gán lý do từ chối vào biến out và trả về false (không cho phép)
                reason = "Vé đã hủy nên không thể thao tác.";
                return false;
            }

            // Gọi hàm cố gắng đọc và parse thời gian bay từ dòng dữ liệu
            // Nếu không thể đọc được ngày giờ vì dữ liệu bị hỏng hoặc trống
            if (!TryGetFlightDateTime(row, out DateTime flightDateTime))
            {
                // Trả về false và báo lỗi không có ngày giờ bay
                reason = "Không đọc được thông tin thời gian chuyến bay.";
                return false;
            }

            // Kiểm tra quy tắc 2: Đối chiếu giờ khởi hành của chuyến bay với thời gian thực tế hiện tại
            // Nếu thời gian khởi hành đã trôi qua (nghĩa là máy bay đã cất cánh hoặc đã hạ cánh rồi)
            if (flightDateTime <= DateTime.Now)
            {
                // Từ chối việc hủy vé vì khách hàng không thể hủy vé một chuyến bay đã diễn ra
                reason = "Chuyến bay đã cất cánh hoặc đã bay, không thể hủy vé.";
                return false;
            }

            // Nếu vượt qua tất cả các bài kiểm tra rủi ro trên thì trả về chuỗi rỗng và cấp quyền cho phép hủy (true)
            reason = "";
            return true;
        }

        // Phương thức phụ trợ dùng để cố gắng bóc tách chuỗi Ngày giờ bay từ dòng dữ liệu và chuyển thành kiểu DateTime thực tế
        private bool TryGetFlightDateTime(DataRowView row, out DateTime result)
        {
            // Thiết lập giá trị mặc định cho biến out là giá trị nhỏ nhất có thể của DateTime để khởi tạo
            result = DateTime.MinValue;
            
            // Kiểm tra xem cột NgayGio trên DataRowView có rỗng hay chứa giá trị NULL của CSDL hay không, nếu có thì trả về false ngay
            if (row["NgayGio"] == DBNull.Value) return false;
            
            // Cố gắng phân tích cú pháp chuỗi từ cột NgayGio sang đối tượng DateTime
            // Nếu parse thành công, giá trị sẽ nằm trong biến result và hàm trả về true. Nếu thất bại trả về false.
            return DateTime.TryParse(row["NgayGio"].ToString(), out result);
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

        private void ShowConfirmDialog(string message, string title, Action onConfirm)
        {
            var view = new StackPanel { Margin = new Thickness(25), MinWidth = 350 };
            view.Children.Add(new TextBlock { Text = title, FontWeight = FontWeights.Bold, FontSize = 18, Margin = new Thickness(0, 0, 0, 15), Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)) });
            view.Children.Add(new TextBlock { Text = message, FontSize = 15, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 25), Foreground = new SolidColorBrush(Color.FromRgb(85, 85, 85)) });
            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var btnCancel = new Button { Content = "HỦY", Margin = new Thickness(0, 0, 10, 0), Style = (Style)FindResource("MaterialDesignFlatButton"), Foreground = new SolidColorBrush(Color.FromRgb(120, 120, 120)) };
            btnCancel.Click += (s, ev) => dialogHost.IsOpen = false;
            var btnOk = new Button { Content = "XÁC NHẬN", Style = (Style)FindResource("MaterialDesignFlatButton"), Foreground = new SolidColorBrush(Color.FromRgb(57, 73, 171)) };
            btnOk.Click += (s, ev) => { dialogHost.IsOpen = false; onConfirm(); };
            btnPanel.Children.Add(btnCancel);
            btnPanel.Children.Add(btnOk);
            view.Children.Add(btnPanel);
            dialogHost.DialogContent = view;
            dialogHost.IsOpen = true;
        }
    }
}
