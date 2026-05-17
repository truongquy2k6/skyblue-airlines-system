using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BUS;

namespace FlightManagement.UserControls
{
    // Lớp ucTuyenBayDoiBay là giao diện 2-trong-1 (Two-in-one).
    // Nửa màn hình bên trái quản lý danh mục các SÂN BAY (Ví dụ: Nội Bài, Tân Sơn Nhất).
    // Nửa màn hình bên phải quản lý các TUYẾN BAY nối hai sân bay lại với nhau (Ví dụ: Tuyến HAN-SGN).
    public partial class ucTuyenBayDoiBay : UserControl
    {
        // Khởi tạo các lớp nghiệp vụ (BUS) tương ứng
        private readonly SanBayBUS sanBayBus = new();
        private readonly TuyenBayBUS tuyenBayBus = new();
        private readonly MayBayBUS mayBayBus = new();
        private readonly QuocGiaBUS quocGiaBus = new();
        private readonly LichSuBUS lichSuBus = new();
        
        // Cờ lưu ID thao tác: _editSBId (Sân bay), _editTBId (Tuyến bay), _editMBId (Máy bay)
        // Bằng -1 nghĩa là trạng thái Thêm mới.
        private int _userId, _editSBId = -1, _editTBId = -1, _editMBId = -1;

        // Lưu trữ bảng dữ liệu nguyên gốc
        private DataTable _dataSB = new();
        private DataTable _dataTB = new();
        private DataTable _dataMB = new();

        // Biến phân trang
        private int _currentPageSB = 1;
        private int _pageSizeSB = 10;
        private int _totalPagesSB = 1;

        private int _currentPageTB = 1;
        private int _pageSizeTB = 10;
        private int _totalPagesTB = 1;

        private int _currentPageMB = 1;
        private int _pageSizeMB = 10;
        private int _totalPagesMB = 1;

        // Hàm khởi tạo giao diện
        public ucTuyenBayDoiBay(int userId) 
        { 
            InitializeComponent(); 
            _userId = userId; 
            
            // Tải danh sách quốc gia vào form nhập Sân bay
            LoadFormCombos(); 
            
            // Tải danh sách sân bay và tuyến bay lên hai cái bảng lớn (DataGrid)
            LoadData(); 
        }

        // Phương thức lấy dữ liệu Quốc gia nạp vào ComboBox khi tạo Sân bay mới (để biết sân bay này nằm ở nước nào)
        private void LoadFormCombos()
        {
            try
            {
                cboQuocGiaSB.ItemsSource = quocGiaBus.HienThi().DefaultView;
            }
            catch { }
        }

        // Phương thức lấy danh sách các Sân bay đã tạo nạp vào hai ô ComboBox "Điểm đi" và "Điểm đến" của Form tạo Tuyến bay
        private void LoadAirportComboForRoutes()
        {
            try
            {
                DataTable dt = sanBayBus.HienThi();
                var items = new List<object>();
                
                // Lặp để tạo chuỗi hiển thị đẹp mắt (Mã IATA - Tên Sân Bay, ví dụ: "SGN - Tân Sơn Nhất")
                foreach (DataRow r in dt.Rows) items.Add(new { ID = r["ID"], Display = r["IATACode"] + " - " + r["TenSanBay"] });
                
                cboSBDi.ItemsSource = items;
                
                // .ToList() để tạo bản sao tránh việc 2 ô ComboBox xài chung một tham chiếu danh sách
                cboSBDen.ItemsSource = items.ToList();
            }
            catch { }
        }

        // Tải toàn bộ dữ liệu 2 bảng lên màn hình
        private void LoadData()
        {
            try
            {
                // Bảng bên trái: Tải danh mục Sân bay
                _dataSB = sanBayBus.HienThi();
                txtSBCount.Text = $"Danh sách Sân bay ({_dataSB.Rows.Count})";
                
                // Thống kê Sân bay
                txtSBTong.Text = _dataSB.Rows.Count.ToString();
                int quocNoi = 0;
                foreach (DataRow r in _dataSB.Rows)
                {
                    string qg = r["QuocGia"]?.ToString().Trim().ToLower();
                    if (qg == "việt nam" || qg == "viet nam") quocNoi++;
                }
                txtSBNMD.Text = quocNoi.ToString();
                txtSBQT.Text = (_dataSB.Rows.Count - quocNoi).ToString();
                UpdatePaginationSB();

                // Bảng bên phải: Tải danh mục Tuyến bay
                _dataTB = tuyenBayBus.HienThi();
                txtTBCount.Text = $"Danh sách Tuyến bay ({_dataTB.Rows.Count})";
                
                // Thống kê Tuyến bay
                txtTBTong.Text = _dataTB.Rows.Count.ToString();
                int maxDist = 0;
                int totalTime = 0;
                foreach (DataRow r in _dataTB.Rows)
                {
                    int dist = Convert.ToInt32(r["KhoangCach"]);
                    if (dist > maxDist) maxDist = dist;
                    string timeStr = r["ThoiGianBay"]?.ToString()?.Replace(" phút", "") ?? "0";
                    int.TryParse(timeStr, out int time);
                    totalTime += time;
                }
                txtTBMaxDist.Text = maxDist > 0 ? maxDist.ToString("N0") + " km" : "0 km";
                txtTBAvgTime.Text = _dataTB.Rows.Count > 0 ? (totalTime / _dataTB.Rows.Count).ToString() + " phút" : "0 phút";
                UpdatePaginationTB();

                // Tab Đội bay: Tải danh sách Máy bay
                _dataMB = mayBayBus.HienThi();
                txtTong.Text = _dataMB.Rows.Count.ToString();
                txtTitle.Text = $"Danh sách Đội bay ({_dataMB.Rows.Count})";
                
                // Thống kê Đội bay
                int narrow = 0, wide = 0;
                foreach (DataRow r in _dataMB.Rows)
                {
                    int seats = Convert.ToInt32(r["TongGhe"]);
                    if (seats >= 200) wide++;
                    else narrow++;
                }
                txtNarrowBody.Text = narrow.ToString();
                txtWideBody.Text = wide.ToString();
                UpdatePaginationMB();

                // Nạp lại danh sách điểm đi/đến, vì nếu bên trái vừa tạo thêm 1 sân bay mới, bên phải phải có cái đó để tạo tuyến
                LoadAirportComboForRoutes();
            }
            catch (Exception ex) { ShowDialogMessage("Lỗi: " + ex.Message, "Lỗi hệ thống"); }
        }

        // ================= KHU VỰC THAO TÁC CRUD VỚI SÂN BAY (NỬA TRÁI) =================
        
        // Mở popup Thêm sân bay
        private void btnThemSB_Click(object s, RoutedEventArgs e) 
        { 
            _editSBId = -1; // Cờ Thêm mới
            txtFormTitleSB.Text = "Thêm sân bay mới"; 
            txtIATA.Text = ""; 
            txtTenSB.Text = ""; 
            pnlFormSB.Visibility = Visibility.Visible; 
        }
        
        // Mở popup Sửa sân bay
        private void btnSuaSB_Click(object s, RoutedEventArgs e)
        {
            if (s is Button btn && btn.DataContext is DataRowView row)
            {
                // Lưu ID sân bay cần sửa
                _editSBId = Convert.ToInt32(row["ID"]);
                txtFormTitleSB.Text = $"Sửa - {row["IATACode"]}";
                
                // Móc dữ liệu cũ đổ vào ô nhập
                txtIATA.Text = row["IATACode"].ToString();
                txtTenSB.Text = row["TenSanBay"].ToString();
                if (row["CountryID"] != DBNull.Value) cboQuocGiaSB.SelectedValue = Convert.ToInt32(row["CountryID"]);
                
                pnlFormSB.Visibility = Visibility.Visible;
            }
        }
        
        // Lưu thông tin Sân bay (Insert hoặc Update)
        private void btnLuuSB_Click(object s, RoutedEventArgs e)
        {
            // Kiểm tra rỗng
            if (string.IsNullOrWhiteSpace(txtIATA.Text) || string.IsNullOrWhiteSpace(txtTenSB.Text) || cboQuocGiaSB.SelectedValue == null)
            { ShowDialogMessage("Nhập đầy đủ thông tin!", "Thiếu thông tin"); return; }
            
            try
            {
                int countryId = Convert.ToInt32(cboQuocGiaSB.SelectedValue);
                if (_editSBId == -1) 
                { 
                    // ToUpper mã IATA vì chuẩn quốc tế mã sân bay phải viết Hoa (vd: SGN)
                    sanBayBus.Them(txtIATA.Text.Trim().ToUpper(), txtTenSB.Text.Trim(), countryId); 
                    lichSuBus.GhiNhanChinhSua(_userId, "Thêm", "Sân bay", $"Thêm {txtIATA.Text}"); 
                }
                else 
                { 
                    sanBayBus.CapNhat(_editSBId, txtIATA.Text.Trim().ToUpper(), txtTenSB.Text.Trim(), countryId); 
                    lichSuBus.GhiNhanChinhSua(_userId, "Sửa", "Sân bay", $"Sửa {txtIATA.Text}"); 
                }
                // Ẩn form và Load lại bảng
                pnlFormSB.Visibility = Visibility.Collapsed; 
                LoadData();
                ShowDialogMessage(_editSBId == -1 ? "Thêm sân bay thành công!" : "Cập nhật sân bay thành công!", "Thành công");
            }
            catch (Exception ex) { ShowDialogMessage(ex.Message, "Lỗi hệ thống"); }
        }
        
        private void btnHuySB_Click(object s, RoutedEventArgs e) { pnlFormSB.Visibility = Visibility.Collapsed; }
        
        // Xóa Sân bay
        private void btnXoaSB_Click(object s, RoutedEventArgs e)
        {
            if (s is Button btn && btn.DataContext is DataRowView row)
                ShowConfirmDialog($"Xóa sân bay {row["IATACode"]}?", "Xác nhận", () =>
                { 
                    try 
                    { 
                        sanBayBus.Xoa(Convert.ToInt32(row["ID"])); 
                        lichSuBus.GhiNhanChinhSua(_userId, "Xóa", "Sân bay", $"Xóa {row["IATACode"]}"); 
                        LoadData(); 
                    } 
                    catch (Exception ex) 
                    { 
                        ShowDialogMessage("Không thể xóa sân bay đang được sử dụng trong các tuyến bay!", "Ràng buộc dữ liệu"); 
                    } 
                });
        }

        // ================= KHU VỰC THAO TÁC CRUD VỚI TUYẾN BAY (NỬA PHẢI) =================
        
        private void btnThemTB_Click(object s, RoutedEventArgs e) 
        { 
            _editTBId = -1; // Cờ thêm mới
            txtFormTitleTB.Text = "Thêm tuyến bay mới"; 
            txtKhoangCach.Text = ""; 
            txtThoiGian.Text = ""; 
            pnlFormTB.Visibility = Visibility.Visible; 
        }
        
        private void btnSuaTB_Click(object s, RoutedEventArgs e)
        {
            if (s is Button btn && btn.DataContext is DataRowView row)
            {
                _editTBId = Convert.ToInt32(row["ID"]);
                txtFormTitleTB.Text = $"Sửa tuyến bay #{_editTBId}";
                if (row["DepartureAirportID"] != DBNull.Value) cboSBDi.SelectedValue = Convert.ToInt32(row["DepartureAirportID"]);
                if (row["ArrivalAirportID"] != DBNull.Value) cboSBDen.SelectedValue = Convert.ToInt32(row["ArrivalAirportID"]);
                txtKhoangCach.Text = row["KhoangCach"]?.ToString() ?? "";
                
                // Mẹo: Cắt bỏ chữ " phút" do truy vấn SQL nối vào để lấy lại số nguyên gốc
                txtThoiGian.Text = row["ThoiGianBay"]?.ToString()?.Replace(" phút", "") ?? "";
                
                pnlFormTB.Visibility = Visibility.Visible;
            }
        }
        
        private void btnLuuTB_Click(object s, RoutedEventArgs e)
        {
            // Bắt rỗng
            if (cboSBDi.SelectedValue == null || cboSBDen.SelectedValue == null || string.IsNullOrWhiteSpace(txtKhoangCach.Text) || string.IsNullOrWhiteSpace(txtThoiGian.Text))
            { ShowDialogMessage("Nhập đầy đủ thông tin!", "Thiếu dữ liệu"); return; }
            
            try
            {
                int depId = Convert.ToInt32(cboSBDi.SelectedValue);
                int arrId = Convert.ToInt32(cboSBDen.SelectedValue);
                
                // Validation Logic: Điểm đi và điểm đến phải khác nhau. Chặn việc tạo tuyến HAN-HAN
                if (depId == arrId) { ShowDialogMessage("Điểm đi và điểm đến không được trùng!", "Lỗi điểm đi/đến"); return; }
                
                // Validation Logic: Cự ly bay và thời lượng bay phải là số và lớn hơn 0
                if (!int.TryParse(txtKhoangCach.Text, out int dist) || dist <= 0 ||
                    !int.TryParse(txtThoiGian.Text, out int time) || time <= 0)
                {
                    ShowDialogMessage("Khoảng cách và thời gian bay phải là số nguyên dương hợp lệ!", "Lỗi định dạng"); return;
                }
                
                if (_editTBId == -1) 
                { 
                    tuyenBayBus.Them(depId, arrId, dist, time); 
                    lichSuBus.GhiNhanChinhSua(_userId, "Thêm", "Tuyến bay", $"Thêm tuyến bay mới"); 
                }
                else 
                { 
                    tuyenBayBus.CapNhat(_editTBId, depId, arrId, dist, time); 
                    lichSuBus.GhiNhanChinhSua(_userId, "Sửa", "Tuyến bay", $"Sửa tuyến bay #{_editTBId}"); 
                }
                pnlFormTB.Visibility = Visibility.Collapsed; 
                LoadData();
                ShowDialogMessage(_editTBId == -1 ? "Thêm tuyến bay thành công!" : "Cập nhật tuyến bay thành công!", "Thành công");
            }
            catch (Exception ex) { ShowDialogMessage(ex.Message, "Lỗi hệ thống"); }
        }
        
        private void btnHuyTB_Click(object s, RoutedEventArgs e) { pnlFormTB.Visibility = Visibility.Collapsed; }
        
        private void btnXoaTB_Click(object s, RoutedEventArgs e)
        {
            if (s is Button btn && btn.DataContext is DataRowView row)
                ShowConfirmDialog($"Xóa tuyến bay {row["DiemDi"]} → {row["DiemDen"]}?", "Xác nhận", () =>
                { 
                    try 
                    { 
                        tuyenBayBus.Xoa(Convert.ToInt32(row["ID"])); 
                        lichSuBus.GhiNhanChinhSua(_userId, "Xóa", "Tuyến bay", $"Xóa tuyến bay #{row["ID"]}"); 
                        LoadData(); 
                    } 
                    catch (Exception ex) 
                    { 
                        ShowDialogMessage("Không thể xóa Tuyến bay vì đang có lịch bay vận hành trên tuyến này!", "Ràng buộc dữ liệu"); 
                    } 
                });
        }

        // ================= KHU VỰC THAO TÁC CRUD VỚI ĐỘI BAY (TAB MÁY BAY) =================
        
        private void btnThem_Click(object s, RoutedEventArgs e) 
        { 
            _editMBId = -1; 
            txtFormTitle.Text = "Thêm máy bay mới"; 
            ClearForm(); 
            txtTenMB.Text = "VN-A" + new Random().Next(100, 999).ToString();
            txtTenMB.IsReadOnly = true;
            pnlForm.Visibility = Visibility.Visible; 
        }

        private void btnSua_Click(object s, RoutedEventArgs e)
        {
            if (s is Button btn && btn.DataContext is DataRowView row)
            {
                _editMBId = Convert.ToInt32(row["ID"]);
                txtFormTitle.Text = $"Sửa - {row["TenMayBay"]}";
                txtTenMB.Text = row["TenMayBay"].ToString();
                txtModel.Text = row["Model"].ToString();
                txtTongGheForm.Text = row["TongGhe"].ToString();
                txtEcoForm.Text = row["GheEconomy"].ToString();
                txtBizForm.Text = row["GheBusiness"].ToString();
                txtFirstClassForm.Text = row.Row.Table.Columns.Contains("GheFirstClass") ? row["GheFirstClass"].ToString() : "0";
                pnlForm.Visibility = Visibility.Visible;
            }
        }

        private void btnLuu_Click(object s, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTenMB.Text) || string.IsNullOrWhiteSpace(txtModel.Text) ||
                string.IsNullOrWhiteSpace(txtTongGheForm.Text) || string.IsNullOrWhiteSpace(txtEcoForm.Text) || string.IsNullOrWhiteSpace(txtBizForm.Text) || string.IsNullOrWhiteSpace(txtFirstClassForm.Text))
            { ShowDialogMessage("Nhập đầy đủ thông tin!", "Thiếu thông tin"); return; }
            try
            {
                if (!int.TryParse(txtTongGheForm.Text, out int total) || total <= 0 ||
                    !int.TryParse(txtEcoForm.Text, out int eco) || eco < 0 ||
                    !int.TryParse(txtBizForm.Text, out int biz) || biz < 0 ||
                    !int.TryParse(txtFirstClassForm.Text, out int first) || first < 0)
                {
                    ShowDialogMessage("Số lượng ghế phải là số nguyên (có thể bằng 0)!", "Lỗi định dạng"); return;
                }
                if (eco + biz + first != total) 
                { 
                    ShowDialogMessage($"Tổng ghế ({total}) phải bằng Economy ({eco}) + Business ({biz}) + First Class ({first})!", "Lỗi phân bổ ghế"); 
                    return; 
                }
                
                if (_editMBId == -1) 
                { 
                    mayBayBus.Them(txtTenMB.Text.Trim(), txtModel.Text.Trim(), total, eco, biz, first); 
                    lichSuBus.GhiNhanChinhSua(_userId, "Thêm", "Máy bay", $"Thêm {txtTenMB.Text}"); 
                }
                else 
                { 
                    mayBayBus.CapNhat(_editMBId, txtTenMB.Text.Trim(), txtModel.Text.Trim(), total, eco, biz, first); 
                    lichSuBus.GhiNhanChinhSua(_userId, "Sửa", "Máy bay", $"Sửa {txtTenMB.Text}"); 
                }
                pnlForm.Visibility = Visibility.Collapsed; 
                LoadData();
                ShowDialogMessage(_editMBId == -1 ? "Thêm thành công!" : "Cập nhật thành công!", "Thành công");
            }
            catch (Exception ex) { ShowDialogMessage(ex.Message, "Lỗi hệ thống"); }
        }

        private void btnHuy_Click(object s, RoutedEventArgs e) { pnlForm.Visibility = Visibility.Collapsed; }

        private void btnXoa_Click(object s, RoutedEventArgs e)
        {
            if (s is Button btn && btn.DataContext is DataRowView row)
                ShowConfirmDialog($"Xóa máy bay {row["TenMayBay"]}?", "Xác nhận", () =>
                { 
                    try 
                    { 
                        mayBayBus.Xoa(Convert.ToInt32(row["ID"])); 
                        lichSuBus.GhiNhanChinhSua(_userId, "Xóa", "Máy bay", $"Xóa {row["TenMayBay"]}"); 
                        LoadData(); 
                    } 
                    catch (Exception ex) 
                    { 
                        ShowDialogMessage(ex.Message, "Lỗi hệ thống"); 
                    } 
                });
        }

        private void ClearForm() 
        { 
            txtTenMB.Text = ""; 
            txtModel.Text = ""; 
            txtTongGheForm.Text = ""; 
            txtEcoForm.Text = ""; 
            txtBizForm.Text = ""; 
            txtFirstClassForm.Text = "0"; 
            txtTenMB.IsReadOnly = false; 
        }

        // ================= PHÂN TRANG (PAGINATION) =================

        private void UpdatePaginationSB()
        {
            if (_dataSB == null) return;
            _totalPagesSB = (int)Math.Ceiling((double)_dataSB.Rows.Count / _pageSizeSB);
            if (_totalPagesSB == 0) _totalPagesSB = 1;
            if (_currentPageSB > _totalPagesSB) _currentPageSB = _totalPagesSB;
            if (_currentPageSB < 1) _currentPageSB = 1;

            txtPageInfoSB.Text = $"Trang {_currentPageSB}/{_totalPagesSB}";

            DataTable pageTable = _dataSB.Clone();
            int startIndex = (_currentPageSB - 1) * _pageSizeSB;
            int endIndex = Math.Min(startIndex + _pageSizeSB, _dataSB.Rows.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                pageTable.ImportRow(_dataSB.Rows[i]);
            }

            dgSanBay.ItemsSource = pageTable.DefaultView;
            btnPrevSB.IsEnabled = _currentPageSB > 1;
            btnNextSB.IsEnabled = _currentPageSB < _totalPagesSB;
        }

        private void btnPrevSB_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPageSB > 1) { _currentPageSB--; UpdatePaginationSB(); }
        }

        private void btnNextSB_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPageSB < _totalPagesSB) { _currentPageSB++; UpdatePaginationSB(); }
        }

        private void UpdatePaginationTB()
        {
            if (_dataTB == null) return;
            _totalPagesTB = (int)Math.Ceiling((double)_dataTB.Rows.Count / _pageSizeTB);
            if (_totalPagesTB == 0) _totalPagesTB = 1;
            if (_currentPageTB > _totalPagesTB) _currentPageTB = _totalPagesTB;
            if (_currentPageTB < 1) _currentPageTB = 1;

            txtPageInfoTB.Text = $"Trang {_currentPageTB}/{_totalPagesTB}";

            DataTable pageTable = _dataTB.Clone();
            int startIndex = (_currentPageTB - 1) * _pageSizeTB;
            int endIndex = Math.Min(startIndex + _pageSizeTB, _dataTB.Rows.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                pageTable.ImportRow(_dataTB.Rows[i]);
            }

            dgTuyenBay.ItemsSource = pageTable.DefaultView;
            btnPrevTB.IsEnabled = _currentPageTB > 1;
            btnNextTB.IsEnabled = _currentPageTB < _totalPagesTB;
        }

        private void btnPrevTB_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPageTB > 1) { _currentPageTB--; UpdatePaginationTB(); }
        }

        private void btnNextTB_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPageTB < _totalPagesTB) { _currentPageTB++; UpdatePaginationTB(); }
        }

        private void UpdatePaginationMB()
        {
            if (_dataMB == null) return;
            _totalPagesMB = (int)Math.Ceiling((double)_dataMB.Rows.Count / _pageSizeMB);
            if (_totalPagesMB == 0) _totalPagesMB = 1;
            if (_currentPageMB > _totalPagesMB) _currentPageMB = _totalPagesMB;
            if (_currentPageMB < 1) _currentPageMB = 1;

            txtPageInfoMB.Text = $"Trang {_currentPageMB}/{_totalPagesMB}";

            DataTable pageTable = _dataMB.Clone();
            int startIndex = (_currentPageMB - 1) * _pageSizeMB;
            int endIndex = Math.Min(startIndex + _pageSizeMB, _dataMB.Rows.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                pageTable.ImportRow(_dataMB.Rows[i]);
            }

            dgMayBay.ItemsSource = pageTable.DefaultView;
            btnPrevMB.IsEnabled = _currentPageMB > 1;
            btnNextMB.IsEnabled = _currentPageMB < _totalPagesMB;
        }

        private void btnPrevMB_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPageMB > 1) { _currentPageMB--; UpdatePaginationMB(); }
        }

        private void btnNextMB_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPageMB < _totalPagesMB) { _currentPageMB++; UpdatePaginationMB(); }
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
