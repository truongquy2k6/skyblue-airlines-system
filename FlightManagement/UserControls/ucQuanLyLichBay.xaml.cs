using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BUS;
using Microsoft.Win32;

namespace FlightManagement.UserControls
{
    // Lớp ucQuanLyLichBay chịu trách nhiệm quản lý toàn bộ các chuyến bay dự kiến của hãng (Schedules)
    // Bao gồm các tác vụ: Thêm chuyến bay mới, điều chỉnh giờ bay, đổi máy bay, hoặc hủy chuyến bay
    public partial class ucQuanLyLichBay : UserControl
    {
        // Khởi tạo các đối tượng BUS (Business Logic Layer) để thao tác với cơ sở dữ liệu
        private readonly LichBayBUS bus = new();
        private readonly TuyenBayBUS tuyenBus = new();
        private readonly MayBayBUS mayBayBus = new();
        private readonly LichSuBUS lichSuBus = new();
        
        // _data: Lưu trữ toàn bộ bảng dữ liệu Lịch bay nguyên gốc lấy từ SQL Server
        private DataTable _data = new();
        
        // _filteredView: Một khung nhìn ảo (Virtual View) dùng để lọc dữ liệu trực tiếp trên RAM mà không cần chọc lại DB
        private DataView _filteredView;
        
        // _userId: Lưu lại mã ID của nhân viên đang thao tác để ghi log bảo mật
        // _editId: Biến cờ đánh dấu trạng thái form. Nếu = -1 nghĩa là đang Thêm mới. Nếu > 0 nghĩa là đang Sửa chuyến bay có ID đó.
        private int _userId, _editId = -1;
        
        // Cấu hình tính năng Phân trang (Pagination) trên giao diện C#
        private int _currentPage = 1; // Trang hiện tại, khởi điểm là 1
        private int _pageSize = 10;    // Hiển thị tối đa 10 chuyến bay trên mỗi trang để fill màn hình đẹp mắt
        
        // Biến lưu tổng số trang được tính toán ra sau khi lọc dữ liệu
        private int _totalPages = 1;

        // Hàm khởi tạo giao diện Quản Lý Lịch Bay. Chạy ngay khi nhân viên bấm vào tab "Quản lý Lịch bay"
        public ucQuanLyLichBay(int userId) 
        { 
            InitializeComponent(); 
            _userId = userId; 
            
            // Tải danh sách các ComboBox trước (Như danh sách Tuyến bay, Máy bay)
            LoadFormCombos(); 
            
            // Tải bảng danh sách chuyến bay lên DataGrid
            LoadData(); 
        }

        // Lấy dữ liệu Tuyến bay và Máy bay từ Database để đổ vào các ô ComboBox (danh sách xổ xuống) trên giao diện.
        private void LoadFormCombos()
        {
            try
            {
                // Nạp danh sách Tuyến bay (Ví dụ: SGN -> HAN)
                DataTable routes = tuyenBus.HienThi();
                var routeItems = new List<object>();
                foreach (DataRow r in routes.Rows)
                {
                    // Ghép chữ cho đẹp (Ví dụ: SGN → HAN)
                    routeItems.Add(new { ID = r["ID"], Display = r["MaDi"] + " → " + r["MaDen"] });
                }
                cboTuyenForm.ItemsSource = routeItems;

                // Nạp danh sách Đội tàu bay hiện có (Ví dụ: Airbus A321 - VN-A123)
                DataTable aircraft = mayBayBus.HienThi();
                var acItems = new List<object>();
                foreach (DataRow r in aircraft.Rows)
                {
                    acItems.Add(new { ID = r["ID"], Display = r["TenMayBay"] + " - " + r["Model"] });
                }
                cboMayBayForm.ItemsSource = acItems;

                // Chuẩn bị các ComboBox dùng cho Bộ lọc (Filter) bên ngoài màn hình chính
                cboTuyen.Items.Clear(); 
                cboTuyen.Items.Add("Tất cả");
                foreach (var i in routeItems) cboTuyen.Items.Add(((dynamic)i).Display);
                cboTuyen.SelectedIndex = 0;

                cboMayBay.Items.Clear(); 
                cboMayBay.Items.Add("Tất cả");
                foreach (DataRow r in aircraft.Rows) cboMayBay.Items.Add(r["TenMayBay"].ToString());
                cboMayBay.SelectedIndex = 0;

                cboTrangThai.Items.Clear();
                cboTrangThai.Items.Add("Tất cả"); 
                cboTrangThai.Items.Add("Đã xác nhận"); 
                cboTrangThai.Items.Add("Đã hủy");
                cboTrangThai.SelectedIndex = 0;
            }
            catch { }
        }

        // Tải lại toàn bộ dữ liệu lịch bay từ Database, sau đó gọi ApplyFilter để áp dụng bộ lọc đang chọn, và tính toán các con số Thống kê.
        private void LoadData()
        {
            try
            {
                // Gọi lớp BUS để lấy toàn bộ dữ liệu Lịch Bay từ SQL Server mang lên bộ nhớ (_data)
                _data = bus.HienThi();
                
                // Gọi hàm lọc để tìm kiếm theo các điều kiện người dùng đang chọn trên màn hình
                ApplyFilter();
                
                // --- ĐOẠN CODE TÍNH TOÁN THỐNG KÊ (DÃY THẺ MÀU TRÊN CÙNG) ---
                
                // Đếm tổng số dòng (số chuyến bay) hiện có
                int total = _data.Rows.Count;
                
                // Dùng hàm Select của DataTable để tìm và đếm các chuyến bay có trạng thái 'Đã xác nhận'
                int confirmed = _data.Select("TrangThai = 'Đã xác nhận'").Length;
                
                // Số chuyến bị hủy đơn giản là: Tổng số - Số đã xác nhận
                int cancelled = total - confirmed;
                
                // Tìm các chuyến bay sắp cất cánh (Ngày bay >= Ngày hôm nay) VÀ Trạng thái Đã xác nhận
                var upcoming = _data.Select($"NgayBay >= '{DateTime.Today:yyyy-MM-dd}' AND TrangThai = 'Đã xác nhận'");
                
                // Cập nhật các con số vừa tính toán được lên giao diện (vào các TextBlock tương ứng trên các thẻ Card)
                txtTong.Text = total.ToString();
                txtXacNhan.Text = confirmed.ToString();
                txtHuy.Text = cancelled.ToString();
                txtSapKH.Text = upcoming.Length.ToString();
                
                // Tính giá vé trung bình của tất cả chuyến bay
                if (total > 0)
                {
                    // Lấy cột GiaEconomy, chuyển sang kiểu Decimal và dùng LINQ để tính trung bình cộng (Average)
                    decimal avg = _data.AsEnumerable().Average(r => Convert.ToDecimal(r["GiaEconomy"]));
                    
                    // Rút gọn định dạng tiền tệ cho đẹp mắt (Ví dụ: 1.500.000 đ -> 1.5M, 500.000 đ -> 500k)
                    txtGiaTB.Text = avg >= 1000000 ? $"{avg / 1000000:0.0}M" : $"{avg / 1000:0}k";
                }
                
                // Đổi tiêu đề cho giao diện để hiển thị tổng số chuyến bay
                txtTitle.Text = $"Quản lý Lịch bay ({total} chuyến)";
                
                // Gọi hàm vẽ lại danh sách biểu đồ Top Tuyến Bay và Top Máy Bay
                LoadDistributions();
            }
            catch (Exception ex) { ShowDialogMessage("Lỗi: " + ex.Message, "Lỗi hệ thống"); }
        }

        // Hàm thống kê nhanh xem Tuyến bay nào có nhiều chuyến nhất, Máy bay nào bay nhiều nhất (Dùng LINQ)
        private void LoadDistributions()
        {
            lstTopRoutes.Items.Clear();
            
            // Gom nhóm bảng dữ liệu theo Tên Tuyến Bay, Đếm số lượng, Sắp xếp giảm dần, và chỉ Lấy 4 top đầu
            var routeGroups = _data.AsEnumerable().GroupBy(r => r["TuyenBay"].ToString()).OrderByDescending(g => g.Count()).Take(4);
            foreach (var g in routeGroups)
            {
                // Tạo một thanh ngang (StackPanel) chứa Tên tuyến và Số lượng để nhét vào ListBox
                var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
                sp.Children.Add(new TextBlock { Text = g.Key, Width = 180 });
                sp.Children.Add(new TextBlock { Text = $"{g.Count()} chuyến", Foreground = System.Windows.Media.Brushes.DodgerBlue, FontWeight = FontWeights.Bold });
                lstTopRoutes.Items.Add(sp);
            }

            lstAircraftUsage.Items.Clear();
            
            // Tương tự, thống kê tần suất bay của từng chiếc máy bay (Để dễ lên kế hoạch bảo trì)
            var acGroups = _data.AsEnumerable().GroupBy(r => r["MayBay"].ToString()).OrderByDescending(g => g.Count());
            foreach (var g in acGroups)
            {
                var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
                sp.Children.Add(new TextBlock { Text = g.Key, Width = 180 });
                sp.Children.Add(new TextBlock { Text = $"{g.Count()} chuyến", Foreground = System.Windows.Media.Brushes.OrangeRed, FontWeight = FontWeights.Bold });
                lstAircraftUsage.Items.Add(sp);
            }
        }

        // Lọc dữ liệu chuyến bay trên bộ nhớ (DataView) thay vì gọi lại Database. Tạo ra câu lệnh RowFilter.
        private void ApplyFilter()
        {
            // Biến f (filter) chứa chuỗi điều kiện lọc (có cú pháp giống hệt mệnh đề WHERE trong câu lệnh SQL)
            string f = "";
            
            // Nếu người dùng có gõ chữ vào ô Tìm kiếm (Ô tìm kiếm hỗ trợ tra cứu theo Mã chuyến hoặc Tuyến bay)
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                f += $"(SoHieu LIKE '%{txtSearch.Text}%' OR TuyenBay LIKE '%{txtSearch.Text}%')";
                
            // Nếu người dùng có chọn Tuyến bay trong ComboBox (SelectedIndex > 0 tức là bỏ qua ô "Tất cả")
            if (cboTuyen.SelectedIndex > 0)
                f += (f.Length > 0 ? " AND " : "") + $"TuyenBay LIKE '%{cboTuyen.SelectedItem}%'";
                
            // Nếu người dùng có chọn Máy bay
            if (cboMayBay.SelectedIndex > 0)
                f += (f.Length > 0 ? " AND " : "") + $"MayBay = '{cboMayBay.SelectedItem}'";
                
            // Nếu người dùng có chọn Trạng thái (Xác nhận/Hủy)
            if (cboTrangThai.SelectedIndex > 0)
                f += (f.Length > 0 ? " AND " : "") + $"TrangThai = '{cboTrangThai.SelectedItem}'";
                
            // Nếu người dùng có chọn Ngày bay cụ thể trên lịch
            if (dpFilterNgay.SelectedDate.HasValue)
                f += (f.Length > 0 ? " AND " : "") + $"NgayBay = '{dpFilterNgay.SelectedDate:yyyy-MM-dd}'";
                
            // Khởi tạo một khung nhìn ảo (DataView) từ bảng dữ liệu gốc
            _filteredView = _data.DefaultView;
            
            // Áp dụng bộ lọc vào khung nhìn ảo này. Nó sẽ tự động giấu đi các dòng không thỏa mãn điều kiện.
            _filteredView.RowFilter = f;
            
            // Cập nhật lại câu thông báo trên góc màn hình để báo số lượng kết quả
            txtCount.Text = $"📋 Danh sách lịch bay - {_filteredView.Count} chuyến";
            
            // Ép trang hiện tại về 1 để tránh lỗi hiển thị nếu số lượng kết quả lọc ra ít hơn tổng số trang hiện tại
            _currentPage = 1;
            
            // Cập nhật lại logic phân trang và nhét dữ liệu vào bảng DataGrid
            UpdatePagination();
        }

        // Cắt khúc dữ liệu để hiển thị từng trang (Pagination)
        private void UpdatePagination()
        {
            if (_filteredView == null) return;
            
            // Tính số trang bằng Tổng số kết quả / 5 (làm tròn lên)
            _totalPages = (int)Math.Ceiling((double)_filteredView.Count / _pageSize);
            if (_totalPages == 0) _totalPages = 1; // Bét nhất cũng phải có 1 trang
            
            // Hiển thị trạng thái phân trang (Ví dụ: Trang 1/3)
            txtPageInfo.Text = $"Trang {_currentPage}/{_totalPages}";
            
            // Tạo một bảng DataTable trống nhưng giữ nguyên cấu trúc cột (Clone)
            DataTable pageTable = _filteredView.Table.Clone();
            
            // Tính toán vị trí bắt đầu và kết thúc của trang hiện tại (Trang 1 thì từ dòng 0 đến 5, Trang 2 thì từ dòng 5 đến 10)
            int startIndex = (_currentPage - 1) * _pageSize;
            int endIndex = Math.Min(startIndex + _pageSize, _filteredView.Count);
            
            // Lặp qua khung nhìn ảo (_filteredView) và copy từng dòng nhét vào cái bảng hiển thị (pageTable)
            for (int i = startIndex; i < endIndex; i++)
            {
                pageTable.ImportRow(_filteredView[i].Row);
            }
            
            // Cuối cùng nhét nguyên cái bảng cắt xén này vào giao diện
            dgLichBay.ItemsSource = pageTable.DefaultView;
            
            // Bật tắt nút Tiến Lùi (Vô hiệu hóa nút Lùi nếu đang ở trang 1)
            btnPrev.IsEnabled = _currentPage > 1;
            btnNext.IsEnabled = _currentPage < _totalPages;
        }

        // Sự kiện ấn nút [<] Lùi trang
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1) { _currentPage--; UpdatePagination(); }
        }

        // Sự kiện ấn nút [>] Tiến trang
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages) { _currentPage++; UpdatePagination(); }
        }

        // Sự kiện khi nội dung ô tìm kiếm hoặc bộ lọc thay đổi, tự động lọc liền lập tức mà không cần bấm nút "Lọc"
        private void Filter_Changed(object s, EventArgs e) { if (_data.Rows.Count > 0) ApplyFilter(); }
        
        // Sự kiện Xóa bộ lọc (Hồi phục mặc định)
        private void btnXoaBoLoc_Click(object s, RoutedEventArgs e)
        { txtSearch.Text = ""; cboTuyen.SelectedIndex = 0; cboMayBay.SelectedIndex = 0; cboTrangThai.SelectedIndex = 0; dpFilterNgay.SelectedDate = null; }
        
        // Sự kiện bật/tắt (Collapse/Visible) dải Thống kê báo cáo để giao diện gọn gàng hơn
        private void btnToggleStats_Click(object s, RoutedEventArgs e)
        {
            pnlStats.Visibility = pnlStats.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            btnToggleStats.Content = pnlStats.Visibility == Visibility.Visible ? "📊 Ẩn thống kê" : "📊 Hiện thống kê";
        }
        
        // Chức năng xuất dữ liệu Lịch bay ra file Excel (.xlsx)
        private void btnExportCSV_Click(object s, RoutedEventArgs e)
        {
            try
            {
                DataTable dtExport = new DataTable();
                dtExport.Columns.Add("Số hiệu");
                dtExport.Columns.Add("Tuyến bay");
                dtExport.Columns.Add("Ngày bay");
                dtExport.Columns.Add("Giờ bay");
                dtExport.Columns.Add("Máy bay");
                dtExport.Columns.Add("Giá Phổ Thông (VND)");
                dtExport.Columns.Add("Giá Thương Gia (VND)");
                dtExport.Columns.Add("Giá Hạng Nhất (VND)");
                dtExport.Columns.Add("Trạng thái");

                foreach (DataRowView r in _data.DefaultView)
                {
                    dtExport.Rows.Add(
                        r["SoHieu"],
                        r["TuyenBay"],
                        r["NgayBay"] != DBNull.Value ? Convert.ToDateTime(r["NgayBay"]).ToString("dd/MM/yyyy") : "",
                        r["GioBay"],
                        r["MayBay"],
                        r["GiaEconomy"] != DBNull.Value ? Convert.ToDecimal(r["GiaEconomy"]).ToString("N0") : "0",
                        r["GiaBusiness"] != DBNull.Value ? Convert.ToDecimal(r["GiaBusiness"]).ToString("N0") : "0",
                        r["GiaFirstClass"] != DBNull.Value ? Convert.ToDecimal(r["GiaFirstClass"]).ToString("N0") : "0",
                        r["TrangThai"]
                    );
                }

                FlightManagement.Helpers.ExcelExporter.ExportDataTable(dtExport, "DANH SÁCH LỊCH BAY HÃNG HÀNG KHÔNG SKYBLUE", "DanhSachLichBay.xlsx");
            }
            catch (Exception ex)
            {
                ShowDialogMessage("Lỗi xuất Excel: " + ex.Message, "Lỗi");
            }
        }

        // ================= KHU VỰC THAO TÁC FORM (THÊM / SỬA / LƯU / XÓA) =================

        // Mở form nhưng ở trạng thái THÊM MỚI (Set cờ _editId = -1)
        private void btnThem_Click(object s, RoutedEventArgs e) 
        { 
            _editId = -1; 
            txtFormTitle.Text = "Thêm lịch bay mới"; 
            ClearForm(); 
            // Khóa chọn Trạng thái vì chuyến mới mặc định phải là Đã xác nhận
            cboTrangThaiForm.IsEnabled = false; 
            pnlForm.Visibility = Visibility.Visible; 
        }

        // Mở form nhưng ở trạng thái CHỈNH SỬA
        private void btnSua_Click(object s, RoutedEventArgs e)
        {
            // Bóc tách nút Edit được ấn và lấy ra Dòng dữ liệu (Row) tương ứng chứa cái nút đó
            if (s is Button btn && btn.DataContext is DataRowView row)
            {
                // Lấy ID thật sự của chuyến bay dưới Database gắn vào cờ
                _editId = Convert.ToInt32(row["ID"]);
                
                // Đổi tiêu đề và nhét dữ liệu cũ lên lại giao diện cho người dùng sửa
                txtFormTitle.Text = $"Sửa lịch bay - {row["SoHieu"]}";
                txtSoHieu.Text = row["SoHieu"].ToString();
                cboTuyenForm.SelectedValue = row["RouteID"];
                cboMayBayForm.SelectedValue = row["AircraftID"];
                
                // Đổi giá vé thành dạng số chẵn (không lấy số thập phân)
                txtGia.Text = Convert.ToDecimal(row["GiaEconomy"]).ToString("0");
                dpNgayBay.SelectedDate = Convert.ToDateTime(row["NgayBay"]);
                
                // Cắt lấy 5 ký tự đầu của Giờ bay (Ví dụ "14:30:00" -> "14:30")
                txtGioBay.Text = row["GioBay"]?.ToString()?.Substring(0, 5) ?? "";
                cboTrangThaiForm.SelectedIndex = row["TrangThai"]?.ToString() == "Đã xác nhận" ? 0 : 1;
                
                // Cho phép Sửa Trạng thái (Trường hợp muốn hủy chuyến bay thì chọn 'Đã hủy')
                cboTrangThaiForm.IsEnabled = true;
                
                // Bật Form lên che màn hình cũ đi
                pnlForm.Visibility = Visibility.Visible;
            }
        }

        // Xử lý nút Lưu (Lưu vào DB cho cả trường hợp Thêm và Sửa).
        private void btnLuu_Click(object s, RoutedEventArgs e)
        {
            // --- BƯỚC 1: KIỂM TRA TÍNH HỢP LỆ CỦA DỮ LIỆU ĐẦU VÀO (VALIDATION) ---
            
            // Kiểm tra xem người dùng có để trống ô nào không
            if (string.IsNullOrWhiteSpace(txtSoHieu.Text) || cboTuyenForm.SelectedValue == null || cboMayBayForm.SelectedValue == null || string.IsNullOrWhiteSpace(txtGia.Text) || dpNgayBay.SelectedDate == null || string.IsNullOrWhiteSpace(txtGioBay.Text))
            { ShowDialogMessage("Vui lòng nhập đầy đủ thông tin!", "Thiếu dữ liệu"); return; }
            
            // Kiểm tra độ dài Số hiệu không được quá 10 ký tự (Do DB giới hạn VARCHAR 10)
            if (txtSoHieu.Text.Trim().Length > 10)
            { ShowDialogMessage("Số hiệu chuyến bay không được vượt quá 10 ký tự!", "Lỗi định dạng"); return; }
            
            // Kiểm tra ngày bay không được chọn lùi về quá khứ (Phải là hôm nay hoặc tương lai)
            if (dpNgayBay.SelectedDate.Value.Date < DateTime.Today)
            { ShowDialogMessage("Ngày bay không được nằm trong quá khứ!", "Lỗi ngày bay"); return; }
            
            // Kiểm tra định dạng Giờ bay xem có đúng kiểu thời gian (Ví dụ 14:30) không
            if (!TimeSpan.TryParse(txtGioBay.Text, out TimeSpan time))
            { ShowDialogMessage("Giờ bay không hợp lệ (VD: 14:30)!", "Lỗi giờ bay"); return; }
            
            // Kiểm tra Giá vé phải là số nguyên hoặc số thập phân, và không được là số âm
            if (!decimal.TryParse(txtGia.Text, out decimal price) || price < 0)
            { ShowDialogMessage("Giá vé phải là số lớn hơn hoặc bằng 0!", "Lỗi giá vé"); return; }
            
            // --- BƯỚC 2: TIẾN HÀNH LƯU XUỐNG DATABASE ---
            try
            {
                // Lấy ID của Tuyến bay và Máy bay mà người dùng đã chọn từ ComboBox
                int routeId = Convert.ToInt32(cboTuyenForm.SelectedValue);
                int acId = Convert.ToInt32(cboMayBayForm.SelectedValue);
                
                // Trạng thái: Nếu Index = 0 (Đã xác nhận) thì biến confirmed = true, ngược lại = false
                bool confirmed = cboTrangThaiForm.SelectedIndex == 0;
                
                if (_editId == -1) // Nếu _editId là -1, nghĩa là đang ở chế độ THÊM MỚI
                { 
                    // Gọi hàm Thêm của lớp BUS, truyền dữ liệu để tạo chuyến bay mới
                    bus.Them(txtSoHieu.Text.Trim(), dpNgayBay.SelectedDate!.Value, time, acId, routeId, price, confirmed); 
                    
                    // Lập tức ghi vào bảng Lịch sử (Nhân viên nào vừa Thêm chuyến bay gì)
                    lichSuBus.GhiNhanChinhSua(_userId, "Thêm", "Lịch bay", $"Thêm chuyến {txtSoHieu.Text}"); 
                }
                else // Nếu _editId khác -1, nghĩa là đang ở chế độ SỬA
                { 
                    // Gọi hàm Cập Nhật của lớp BUS, truyền ID của dòng đang sửa
                    bus.CapNhat(_editId, txtSoHieu.Text.Trim(), dpNgayBay.SelectedDate!.Value, time, acId, routeId, price, confirmed); 
                    
                    // Ghi lịch sử là có thao tác Sửa
                    lichSuBus.GhiNhanChinhSua(_userId, "Sửa", "Lịch bay", $"Sửa chuyến {txtSoHieu.Text}"); 
                }
                
                // Ẩn Form nhập liệu đi sau khi lưu thành công
                pnlForm.Visibility = Visibility.Collapsed; 
                
                // Gọi hàm LoadData() để quét lại Database và vẽ giao diện mới nhất
                LoadData();
                
                ShowDialogMessage(_editId == -1 ? "Thêm thành công!" : "Cập nhật thành công!", "Thành công");
            }
            catch (Exception ex) { ShowDialogMessage("Lỗi: " + ex.Message, "Lỗi hệ thống"); }
        }
        
        // Tắt form đi
        private void btnHuyForm_Click(object s, RoutedEventArgs e) { pnlForm.Visibility = Visibility.Collapsed; }
        
        // Sự kiện ấn nút Xóa (Nút viền đỏ chữ đỏ trên bảng)
        private void btnXoa_Click(object s, RoutedEventArgs e)
        {
            if (s is Button btn && btn.DataContext is DataRowView row)
            {
                // Gọi hộp thoại cảnh báo trước khi xóa. Nếu bấm Yes thì mới chạy lệnh.
                ShowConfirmDialog($"Xóa chuyến bay {row["SoHieu"]}?", "Xác nhận xóa", () =>
                { 
                    try 
                    { 
                        // Gọi BUS yêu cầu xóa ID chuyến bay
                        bus.Xoa(Convert.ToInt32(row["ID"])); 
                        lichSuBus.GhiNhanChinhSua(_userId, "Xóa", "Lịch bay", $"Xóa chuyến {row["SoHieu"]}"); 
                        LoadData(); 
                    } 
                    catch (Exception ex) 
                    { 
                        // Nếu chuyến bay đã có người mua vé, SQL Server sẽ ném lỗi Foreign Key Violation để chặn thao tác xóa
                        ShowDialogMessage("Không thể xóa chuyến bay vì đã có khách đặt vé (Ràng buộc dữ liệu)!\nVui lòng chuyển trạng thái thành 'Đã hủy'.", "Không thể xóa"); 
                    } 
                });
            }
        }
        
        // Dọn dẹp Textbox sạch sẽ
        private void ClearForm() { txtSoHieu.Text = ""; txtGia.Text = ""; dpNgayBay.SelectedDate = null; txtGioBay.Text = ""; cboTrangThaiForm.SelectedIndex = 0; }

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
