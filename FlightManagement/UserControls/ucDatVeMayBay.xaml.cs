using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text.RegularExpressions;
using BUS;
using FlightManagement.Services;

namespace FlightManagement.UserControls
{
    // Lớp ucDatVeMayBay điều khiển giao diện Đặt vé máy bay (Booking Form)
    // Nơi nhân viên thu thập thông tin cá nhân của khách hàng, cấu hình ghế ngồi và chốt xuất vé
    public partial class ucDatVeMayBay : UserControl
    {
        // Khai báo các đối tượng giao tiếp với tầng BUS (Nghiệp vụ)
        private readonly LichBayBUS lichBayBus = new();
        private readonly VeBUS veBus = new();
        private readonly HangGheBUS hangGheBus = new();
        private readonly DichVuBUS dichVuBus = new();
        private readonly QuocGiaBUS quocGiaBus = new();
                // Khai báo Dịch vụ gửi Email thông báo (Dùng MailKit)
        private readonly EmailNotificationService emailService = new();
        
        // Cửa sổ cha (MainWindow) chứa chức năng điều hướng trang
        private MainWindow _mainWindow;
        
        // Tab cha (ucTongQuanVe) chứa chức năng quản lý chung và màn hình Loading
        private ucTongQuanVe _parentView;
        
        // Các biến lưu trữ trạng thái phiên làm việc hiện tại
        // _userId: ID nhân viên đang thao tác. _scheduleId: ID chuyến bay đang đặt. _selectedCabinId: Hạng ghế khách chọn (Mặc định = 0 là chưa chọn)
        private int _userId, _scheduleId, _selectedCabinId = 0;
        
        // Biến lưu giá gốc hạng Phổ thông của chuyến bay này (dùng làm cơ sở tính giá cho các hạng cao hơn)
        private decimal _economyPrice;
        
        // Biến lưu mã ghế cụ thể khách chọn (Ví dụ: "A12")
        private string _selectedSeat = "";
        
        // Biến lưu mẫu máy bay của chuyến bay để cấu hình sơ đồ ghế
        private string _aircraftName = "";
 
        // Hàm khởi tạo giao diện Đặt vé. 
        // Có thể được gọi từ Sidebar (không có scheduleId) hoặc được gọi từ màn Tìm kiếm (được truyền sẵn scheduleId)
        public ucDatVeMayBay(MainWindow mainWindow, int userId, int scheduleId = 0, ucTongQuanVe parentView = null)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _userId = userId;
            _scheduleId = scheduleId;
            _parentView = parentView;

            // Kiểm tra xem nhân viên đã có mục tiêu chuyến bay để đặt chưa
            if (scheduleId > 0)
            {
                // Nếu đã chọn chuyến bay -> Ẩn màn hình cảnh báo, Hiện form điền thông tin
                pnlNoFlight.Visibility = Visibility.Collapsed;
                pnlBookingForm.Visibility = Visibility.Visible;
                
                // Nạp danh sách Quốc tịch vào ComboBox cho hành khách
                LoadQuocTich();
                
                // Nạp danh sách tùy chọn Hạng ghế (Economy, Business...)
                LoadHangGhe();
                
                // Nạp thông tin chi tiết của chuyến bay đó lên góc trên cùng (Giờ bay, điểm đến, v.v)
                LoadFlightInfo(scheduleId);
            }
            else
            {
                // Nếu nhân viên bấm thẳng từ Menu mà chưa chọn chuyến bay -> Ẩn form, Hiện cảnh báo yêu cầu qua bên màn Tìm kiếm
                pnlNoFlight.Visibility = Visibility.Visible;
                pnlBookingForm.Visibility = Visibility.Collapsed;
            }
        }

        // Xử lý sự kiện bấm nút "Quay lại Tìm kiếm" khi đang ở màn hình cảnh báo chưa chọn chuyến bay
        private void btnQuayLaiTimKiem_Click(object sender, RoutedEventArgs e)
        {
            // Nhờ MainWindow đổi thẻ (Tab) sang trang Tìm kiếm
            _mainWindow.NavigateToTimKiem();
        }

        // Hàm đổ dữ liệu Quốc gia vào ComboBox Hộ chiếu
        private void LoadQuocTich()
        {
            try 
            { 
                cboQuocTich.ItemsSource = quocGiaBus.HienThi().DefaultView; 
                cboQuocTich.SelectedIndex = 0; 
            } 
            catch { }
        }

        // Hàm vẽ động (Dynamic render) các lựa chọn Hạng ghế (Radio Buttons) lên màn hình
        private void LoadHangGhe()
        {
            try
            {
                DataTable dt = hangGheBus.HienThi();
                pnlHangGhe.Children.Clear();
                
                foreach (DataRow r in dt.Rows)
                {
                    // Trích xuất ID, Tên hạng ghế, Hệ số giá (Ví dụ: Economy = 1, Business = 2.5)
                    int id = Convert.ToInt32(r["ID"]);
                    string name = r["TenHangGhe"].ToString()!;
                    double mult = Convert.ToDouble(r["HeSoGia"]);
                    
                    // Tạo một nút RadioButton cho hạng ghế đó
                    var rb = new RadioButton
                    {
                        GroupName = "cabin", 
                        Tag = id, 
                        Margin = new Thickness(5),
                        // Vô hiệu hóa việc tự do click chọn Hạng ghế ở đây. Phải chọn thông qua Sơ đồ ghế (Seat Map)
                        IsHitTestVisible = false, 
                        
                        // Chèn hai cục TextBlock vào trong nút Radio: 1 cục chữ to (Tên hạng), 1 cục chữ nhỏ màu xanh (Giá tiền tương ứng)
                        Content = new StackPanel
                        {
                            Children = {
                                new TextBlock { Text = name, FontWeight = FontWeights.Bold, FontSize = 14 },
                                new TextBlock { Text = $"{(_economyPrice * (decimal)mult):N0} đ", Foreground = Brushes.DodgerBlue, FontSize = 13 }
                            }
                        }
                    };
                    
                    // Đóng gói cái RadioButton đó vào một cái Border bo tròn cho đẹp
                    var border = new Border
                    {
                        BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(8), Padding = new Thickness(15, 10, 15, 10),
                        Margin = new Thickness(5), Child = rb
                    };
                    
                    // Khi cái Radio này được (phần mềm) gạt sang trạng thái Checked, tự động tính lại tổng tiền và cập nhật danh sách tiện ích
                    rb.Checked += (s, e) => { _selectedCabinId = id; UpdateTotal(); LoadCabinAmenities(); };
                    
                    // Gắn khung chọn này vào Panel
                    pnlHangGhe.Children.Add(border);
                }
            }
            catch { }
        }

        // Hàm tải và điền thông tin chi tiết của chuyến bay được chọn lên góc trên cùng của Form
        private void LoadFlightInfo(int scheduleId)
        {
            try
            {
                DataTable dt = lichBayBus.HienThi();
                var rows = dt.Select($"ID = {scheduleId}");
                if (rows.Length > 0)
                {
                    var r = rows[0];
                    txtSoHieu.Text = r["SoHieu"].ToString();
                    txtTuyenBay.Text = r["TuyenBay"].ToString();
                    txtNgayBay.Text = Convert.ToDateTime(r["NgayBay"]).ToString("dd/MM/yyyy");
                    txtGioBay.Text = r["GioBay"].ToString();
                    _aircraftName = r["MayBay"].ToString() ?? "";
                    
                    // Cực kỳ quan trọng: Lấy giá gốc của vé Economy để làm mốc tính cho các Hạng ghế cao hơn
                    _economyPrice = Convert.ToDecimal(r["GiaEconomy"]);
                    
                    // Refresh lại khu vực Hạng ghế để cập nhật giá tiền lên các nút bấm
                    LoadHangGhe(); 
                    
                    // Refresh Tổng tiền bên góc dưới
                    UpdateTotal();
                    
                    // Nạp danh sách các dịch vụ miễn phí của hạng ghế hiện tại (Ví dụ: Hành lý 20kg)
                    LoadCabinAmenities();
                }
            }
            catch { }
        }

        // Hàm tính toán và cập nhật số tiền khách phải trả hiển thị dưới góc màn hình
        private void UpdateTotal()
        {
            try
            {
                if (_selectedCabinId == 0)
                {
                    txtTongTien.Text = "0 đ";
                    return;
                }

                DataTable dt = hangGheBus.HienThi();
                var rows = dt.Select($"ID = {_selectedCabinId}");
                if (rows.Length > 0)
                {
                    // Lấy hệ số giá. Ví dụ: Giá gốc là 1tr, khách chọn Business có hệ số 2.5 -> Tổng bằng 2.5tr
                    double mult = Convert.ToDouble(rows[0]["HeSoGia"]);
                    txtTongTien.Text = $"{(_economyPrice * (decimal)mult):N0} đ";
                }
            }
            catch { }
        }

        // Hàm nạp danh sách các Tiện ích (Suất ăn, Hành lý) được tặng kèm theo hạng ghế khách đang chọn
        private void LoadCabinAmenities()
        {
            try
            {
                pnlTienIchHangGhe.Children.Clear(); // Dọn dẹp các tiện ích cũ
                if (_selectedCabinId == 0)
                {
                    txtNoAmenities.Visibility = Visibility.Visible;
                    return;
                }

                // Lấy bảng cấu hình dịch vụ của hạng ghế hiện tại
                DataTable dt = hangGheBus.LayCauHinh(_selectedCabinId);

                // Dùng LINQ để chắt lọc ra các dịch vụ được đánh dấu True (Được tặng) trong hệ thống
                var selectedAmenities = dt.AsEnumerable()
                    .Where(r => Convert.ToBoolean(r["DuocChon"]))
                    .ToList();

                // Nếu không có dịch vụ nào, hiện chữ "Không có tiện ích miễn phí". Nếu có, ẩn nó đi
                txtNoAmenities.Visibility = selectedAmenities.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                
                // Vẽ giao diện cho từng Tiện ích một
                foreach (DataRow row in selectedAmenities)
                {
                    string amenityName = row["TenDichVu"].ToString() ?? "";
                    decimal price = Convert.ToDecimal(row["Gia"]);

                    // Đóng gói từng dịch vụ vào trong một tấm thẻ viền xanh dương
                    var card = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(227, 242, 253)),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(63, 81, 181)),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(6),
                        Padding = new Thickness(8, 6, 8, 6),
                        Margin = new Thickness(0, 0, 8, 8)
                    };

                    // Chèn nội dung văn bản vào thẻ
                    var panel = new StackPanel();
                    panel.Children.Add(new TextBlock
                    {
                        Text = "• " + amenityName,
                        FontWeight = FontWeights.SemiBold,
                        FontSize = 12
                    });
                    panel.Children.Add(new TextBlock
                    {
                        Text = $"Giá dịch vụ: {price:N0} đ",
                        Foreground = Brushes.Gray,
                        FontSize = 11
                    });
                    
                    card.Child = panel;
                    
                    // Thêm thẻ này vào khung Panel chung trên giao diện
                    pnlTienIchHangGhe.Children.Add(card);
                }
            }
            catch
            {
                txtNoAmenities.Visibility = Visibility.Visible;
            }
        }

        // Sự kiện khi nhân viên bấm nút [Chọn ghế] màu xanh rêu
        private void btnChonGhe_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra xem người dùng đã chọn chuyến bay nào chưa (nếu đến thẳng màn hình này mà chưa qua Tìm kiếm)
            if (_scheduleId == 0)
            {
                ShowDialogMessage("Vui lòng chọn chuyến bay trước!", "Thông báo");
                return;
            }

            // Mặc định thiết lập số lượng ghế cho các khoang
            int ecoSeats = 60, bizSeats = 20, firstSeats = 0; 
            try
            {
                // Truy vấn bảng Máy Bay để lấy đúng cấu hình số lượng ghế của chiếc máy bay đang bay chuyến này
                DataTable dtMayBay = new DAL.MayBayDAL().HienThi();
                var mbRow = dtMayBay.Select($"TenMayBay = '{_aircraftName}'");
                if (mbRow.Length > 0)
                {
                    ecoSeats = Convert.ToInt32(mbRow[0]["GheEconomy"]);
                    bizSeats = Convert.ToInt32(mbRow[0]["GheBusiness"]);
                    // Kiểm tra xem database có cột hỗ trợ ghế FirstClass không để tránh lỗi
                    firstSeats = mbRow[0].Table.Columns.Contains("GheFirstClass") ? Convert.ToInt32(mbRow[0]["GheFirstClass"]) : 0;
                }
            }
            catch { }

            // Khởi tạo danh sách các mã ghế đã bị người khác đặt trước đó trên chuyến bay này (ví dụ: A1, B3)
            List<string> bookedSeats = new List<string>();
            try
            {
                DataTable dtBooked = veBus.LayDanhSachGheDaDat(_scheduleId);
                foreach (DataRow row in dtBooked.Rows)
                {
                    bookedSeats.Add(row["SeatNumber"].ToString()!);
                }
            }
            catch { }

            // Bật cửa sổ popup Chọn Ghế (SeatSelectionWindow), nạp vào số lượng ghế các khoang và mảng ghế đã đặt để nó vẽ sơ đồ và bôi đỏ
            string passengerName = $"{txtHo.Text} {txtTen.Text}".Trim();
            if (string.IsNullOrEmpty(passengerName)) passengerName = "Hành khách";
            string route = txtTuyenBay.Text;
            
            SeatSelectionWindow seatWindow = new SeatSelectionWindow(ecoSeats, bizSeats, firstSeats, bookedSeats, passengerName, route);
            
            // Lệnh ShowDialog() làm nổi cửa sổ lên trên và bắt buộc người dùng thao tác xong mới đi tiếp. Trả về True nếu chọn ghế thành công.
            if (seatWindow.ShowDialog() == true)
            {
                // Lấy mã ghế khách vừa chọn (ví dụ: "C5") từ cửa sổ đó ra và in lên ô Giao diện
                _selectedSeat = seatWindow.SelectedSeat;
                txtGheDaChon.Text = _selectedSeat;
                
                // Thuật toán quét qua các nút RadioButton Hạng ghế trên màn hình,
                // ép gạt nút (Checked) sang cái hạng ghế tương ứng với khu vực ghế vừa được chọn trên sơ đồ.
                foreach (var child in pnlHangGhe.Children)
                {
                    if (child is Border b && b.Child is RadioButton rb)
                    {
                        if (Convert.ToInt32(rb.Tag) == seatWindow.SelectedCabinId)
                        {
                            rb.IsChecked = true;
                            break;
                        }
                    }
                }
            }
        }

        private void btnDoiChuyen_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateToTimKiem();
        }

        // Sự kiện khi nhân viên bấm nút [Hoàn tất Đặt vé]
        private async void btnDatVe_Click(object sender, RoutedEventArgs e)
        {
            // Bước 1: Kiểm duyệt toàn bộ dữ liệu form bằng hàm Validation (Bắt rỗng, sai định dạng)
            if (!ValidatePassengerInfo()) return;
            
            // Kiểm tra ràng buộc chọn ghế ngồi trước khi đặt vé
            if (string.IsNullOrEmpty(_selectedSeat) || _selectedCabinId == 0)
            {
                ShowDialogMessage("Vui lòng chọn ghế ngồi trước khi xác nhận đặt vé!", "Thiếu thông tin");
                return;
            }
            
            // Bước 2: Kiểm tra chốt chặn an toàn, chống việc hacker hoặc lỗi giao diện khiến scheduleId rỗng
            if (_scheduleId == 0)
            {
                ShowDialogMessage("Vui lòng chọn chuyến bay từ Tìm kiếm Chuyến bay!", "Thông báo");
                return;
            }

            // Hiển thị UI Loading toàn phần
            if (_parentView != null) _parentView.ShowLoading(true);
            if (sender is Button btnStart) btnStart.IsEnabled = false;

            try
            {
                // Bước 3: Sinh ra một mã đặt chỗ (Booking Reference/ PNR) ngẫu nhiên 6 ký tự
                string bookingRef = GenerateBookingRef();
                
                // Lấy ID quốc tịch, nếu chưa có thì gán mặc định là số 1 (Việt Nam)
                int countryId = cboQuocTich.SelectedValue != null ? Convert.ToInt32(cboQuocTich.SelectedValue) : 1;
                
                // Thu thập dữ liệu UI
                string ho = txtHo.Text.Trim();
                string ten = txtTen.Text.Trim();
                string emailKhach = txtEmailKhach.Text.Trim();
                string sdt = txtSDT.Text.Trim();
                string hoChieu = txtHoChieu.Text.Trim();
                string soHieu = txtSoHieu.Text.Trim();
                string tuyenBay = txtTuyenBay.Text.Trim();
                string ngayBay = txtNgayBay.Text.Trim();
                string gioBay = txtGioBay.Text.Trim();
                int currentUserId = _userId;
                int scheduleId = _scheduleId;
                int selectedCabinId = _selectedCabinId;
                string selectedSeat = _selectedSeat;

                string message = "";

                // Chạy ngầm các tác vụ nặng
                await System.Threading.Tasks.Task.Run(() =>
                {
                    // Bước 4: Gọi hàm thêm Vé vào cơ sở dữ liệu ở tầng BUS. Hàm này insert thông tin và nhả về số ID của vé đó.
                    object? newTicketIdObj = veBus.Them(currentUserId, scheduleId, selectedCabinId, ten, ho,
                        emailKhach, sdt, hoChieu, countryId, bookingRef, selectedSeat);
                        
                    int newTicketId = Convert.ToInt32(newTicketIdObj ?? 0);
                    
                    // Nếu vé tạo thành công dưới DB
                    if (newTicketId > 0)
                    {
                        // Tự động chèn các dịch vụ đi kèm của hạng ghế (nếu có) vào vé này trong bảng AmenitiesTickets
                        GanDichVuMacDinhChoVe(newTicketId, selectedCabinId);
                    }
                    
                    // Bước 5: Ghi lưu lại hành động vào hệ thống Audit Log (Lịch sử)
                    new LichSuBUS().GhiNhanChinhSua(currentUserId, "Đặt vé", "Vé máy bay", $"Đặt vé {bookingRef} cho {ho} {ten}");
                    
                    message = $"Đặt vé thành công!\nMã đặt chỗ: {bookingRef}";
                    
                    // Bước 6: Đẩy vé vào hàng đợi Mail CSKH thay vì gửi trực tiếp
                    if (newTicketId > 0)
                    {
                        new CSKHBUS().ThemMailQueue(newTicketId);
                        message += "\n(Vé đã được đưa vào Hàng đợi gửi Email CSKH)";
                    }
                });
                
                // Báo kết quả cuối cùng
                ShowDialogMessage(message, "Thành công");
                
                // Xóa trắng mọi thứ để form gọn gàng
                ClearForm();
            }
            catch (Exception ex) { ShowDialogMessage("Lỗi: " + ex.Message, "Lỗi hệ thống"); }
            finally
            {
                // Tắt UI Loading toàn phần
                if (_parentView != null) _parentView.ShowLoading(false);
                if (sender is Button btnEnd) btnEnd.IsEnabled = true;
            }
        }

        // Hàm nội bộ chạy ngầm sau khi vé tạo thành công để copy các Dịch vụ tiện ích sang cho vé đó
        private void GanDichVuMacDinhChoVe(int ticketId, int selectedCabinId)
        {
            try
            {
                DataTable dtAmenityConfig = hangGheBus.LayCauHinh(selectedCabinId);
                foreach (DataRow row in dtAmenityConfig.Rows)
                {
                    // Chỉ lấy những dịch vụ có giá trị DuocChon là True
                    if (!Convert.ToBoolean(row["DuocChon"])) continue;

                    int amenityId = Convert.ToInt32(row["AmenityID"]);
                    decimal price = Convert.ToDecimal(row["Gia"]);
                    // Ghi nhận trực tiếp xuống DB
                    dichVuBus.GanChoVe(amenityId, ticketId, price);
                }
            }
            catch
            {
                // Cố ý không quăng lỗi ra ngoài nếu gán tiện ích bị hỏng, để không làm vỡ tiến trình xuất vé
            }
        }

        // Hàm bao bọc nghiệp vụ gửi Email, giúp bắt try/catch và trả về kết quả True/False
        private bool TrySendConfirmationEmail(string email, string hoTen, string bookingRef, string soHieu, string tuyenBay, string ngayBay, string gioBay, string selectedSeat)
        {
            try
            {
                emailService.SendBookingConfirmation(
                    email, hoTen, bookingRef, soHieu, tuyenBay, ngayBay, gioBay, selectedSeat);
                return emailService.IsConfigured;
            }
            catch
            {
                return false;
            }
        }

        // Hàm tiện ích sinh mã PNR ngẫu nhiên gồm 6 ký tự chữ hoa và số
        private string GenerateBookingRef()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var rng = new Random();
            // Lấy 6 ký tự ngẫu nhiên trong chuỗi chars và ghép lại thành string
            return new string(Enumerable.Range(0, 6).Select(_ => chars[rng.Next(chars.Length)]).ToArray());
        }

        // Hàm rào chắn kiểm tra tính đúng đắn của dữ liệu đầu vào (Validation)
        private bool ValidatePassengerInfo()
        {
            // Kiểm tra rỗng cho các trường bắt buộc
            if (string.IsNullOrWhiteSpace(txtHo.Text) || string.IsNullOrWhiteSpace(txtTen.Text) ||
                string.IsNullOrWhiteSpace(txtSDT.Text) || string.IsNullOrWhiteSpace(txtHoChieu.Text) || string.IsNullOrWhiteSpace(txtEmailKhach.Text))
            {
                ShowDialogMessage("Vui lòng nhập đầy đủ thông tin hành khách!", "Thiếu thông tin");
                return false;
            }
            
            // Dùng Biểu thức chính quy (Regex) để ép chuẩn định dạng Email (Bắt buộc phải chứa @ và dấu chấm ở phía sau)
            if (!Regex.IsMatch(txtEmailKhach.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ShowDialogMessage("Email không hợp lệ (sai định dạng)!", "Lỗi định dạng");
                return false;
            }
            
            // Dùng Regex kiểm tra SDT: Buộc phải là số, không chứa chữ, độ dài linh hoạt từ 10 đến 14 ký tự
            if (!Regex.IsMatch(txtSDT.Text, @"^\d{10,14}$"))
            {
                ShowDialogMessage("Số điện thoại không hợp lệ (chỉ chứa 10-14 chữ số)!", "Lỗi định dạng");
                return false;
            }
            
            // Dùng Regex kiểm tra Hộ chiếu/CMND: Cho phép chữ cái và số, độ dài từ 6 đến 9 ký tự
            if (!Regex.IsMatch(txtHoChieu.Text, @"^[a-zA-Z0-9]{6,9}$"))
            {
                ShowDialogMessage("Hộ chiếu không hợp lệ (6-9 ký tự, chỉ gồm chữ và số)!", "Lỗi định dạng");
                return false;
            }

            return true;
        }

        // Hàm dọn vệ sinh form (Reset) sau khi đặt vé xong
        private void ClearForm()
        {
            txtHo.Text = ""; txtTen.Text = ""; txtEmailKhach.Text = ""; txtSDT.Text = ""; txtHoChieu.Text = "";
            txtGheDaChon.Text = "Chưa chọn"; _selectedSeat = ""; _selectedCabinId = 0;
            
            // Bỏ chọn tất cả radio buttons
            foreach (var child in pnlHangGhe.Children)
            {
                if (child is Border b && b.Child is RadioButton rb)
                {
                    rb.IsChecked = false;
                }
            }
            
            UpdateTotal();
            LoadCabinAmenities();
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
