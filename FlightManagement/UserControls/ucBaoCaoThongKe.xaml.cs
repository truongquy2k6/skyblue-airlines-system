using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using BUS;

namespace FlightManagement.UserControls
{
    public partial class ucBaoCaoThongKe : UserControl
    {
        private readonly BaoCaoBUS bus = new();
        private DataTable dtChuyenBay;
        
        // Các biến điều khiển phân trang Danh sách hành khách
        private DataTable dtHanhKhach;
        private int _currentPageHK = 1;
        private int _pageSizeHK = 10;
        private int _totalPagesHK = 1;

        public ucBaoCaoThongKe() 
        { 
            InitializeComponent(); 
            LoadData(); 
        }

        private int _selectedOfficeId = 1;
        private bool _isFilterChanging = false;

        private void LoadData()
        {
            try
            {
                // Đổ dữ liệu vào ComboBox để người dùng chọn chuyến bay cần xem danh sách hành khách
                dtChuyenBay = bus.ChuyenBayCombo();
                cboChuyenBay.ItemsSource = dtChuyenBay.DefaultView;
                
                // Khởi tạo bộ lọc báo cáo Văn phòng & Chi tiết
                InitDateFilter();
                LoadOfficeSummaries();
                SelectOffice(1);

                // Tải mặc định toàn bộ danh sách hành khách trong hệ thống ban đầu (truyền tham số 0)
                dtHanhKhach = bus.DanhSachHanhKhach(0);
                _currentPageHK = 1;
                txtInfo.Text = $"Tổng hành khách: {dtHanhKhach?.Rows.Count ?? 0}";

                // Khởi tạo trạng thái ban đầu của bộ phân trang hành khách
                UpdatePaginationHK();
            }
            catch { }
        }

        private void txtSearchChuyenBay_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dtChuyenBay == null) return;
            try
            {
                string filter = txtSearchChuyenBay.Text.Trim().Replace("'", "''");
                if (string.IsNullOrEmpty(filter))
                {
                    dtChuyenBay.DefaultView.RowFilter = "";
                }
                else
                {
                    dtChuyenBay.DefaultView.RowFilter = $"Display LIKE '%{filter}%'";
                }
                
                // Tự động mở danh sách gợi ý của ComboBox để người dùng dễ lựa chọn
                cboChuyenBay.IsDropDownOpen = true;
            }
            catch { }
        }

        private void cboChuyenBay_Changed(object sender, SelectionChangedEventArgs e)
        {
            // Bắt sự kiện khi người dùng chọn một chuyến bay cụ thể từ ComboBox
            if (cboChuyenBay.SelectedValue is int id)
            {
                try
                {
                    // Lấy toàn bộ danh sách hành khách đã đặt vé trên chuyến bay đó
                    dtHanhKhach = bus.DanhSachHanhKhach(id);
                    _currentPageHK = 1;
                    UpdatePaginationHK();
                    
                    // Lấy thêm thông tin tổng quan của chuyến bay (ví dụ tổng số khách)
                    DataTable info = bus.ChiTietChuyenBay(id);
                    if (info.Rows.Count > 0)
                        txtInfo.Text = $"Tổng hành khách: {info.Rows[0]["TongHanhKhach"]}";
                }
                catch { }
            }
        }

        // ================= PHÂN TRANG (PAGINATION) DANH SÁCH HÀNH KHÁCH =================

        private void UpdatePaginationHK()
        {
            try
            {
                if (dtHanhKhach == null || dtHanhKhach.Rows.Count == 0)
                {
                    txtPageInfoHK.Text = "Trang 1/1";
                    dgHanhKhach.ItemsSource = null;
                    btnPrevHK.IsEnabled = false;
                    btnPrevHK.Opacity = 0.45;
                    btnNextHK.IsEnabled = false;
                    btnNextHK.Opacity = 0.45;
                    return;
                }

                _totalPagesHK = (int)System.Math.Ceiling((double)dtHanhKhach.Rows.Count / _pageSizeHK);
                if (_totalPagesHK == 0) _totalPagesHK = 1;
                if (_currentPageHK > _totalPagesHK) _currentPageHK = _totalPagesHK;
                if (_currentPageHK < 1) _currentPageHK = 1;

                txtPageInfoHK.Text = $"Trang {_currentPageHK}/{_totalPagesHK}";

                // Tạo bảng tạm chứa các bản ghi thuộc trang hiện tại
                DataTable pageTable = dtHanhKhach.Clone();
                int startIndex = (_currentPageHK - 1) * _pageSizeHK;
                int endIndex = System.Math.Min(startIndex + _pageSizeHK, dtHanhKhach.Rows.Count);

                for (int i = startIndex; i < endIndex; i++)
                {
                    pageTable.ImportRow(dtHanhKhach.Rows[i]);
                }

                dgHanhKhach.ItemsSource = pageTable.DefaultView;
                
                // Cập nhật trạng thái các nút Trang trước / Trang sau
                btnPrevHK.IsEnabled = _currentPageHK > 1;
                btnPrevHK.Opacity = btnPrevHK.IsEnabled ? 1.0 : 0.45;

                btnNextHK.IsEnabled = _currentPageHK < _totalPagesHK;
                btnNextHK.Opacity = btnNextHK.IsEnabled ? 1.0 : 0.45;
            }
            catch { }
        }

        private void btnPrevHK_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_currentPageHK > 1)
            {
                _currentPageHK--;
                UpdatePaginationHK();
            }
        }

        private void btnNextHK_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_currentPageHK < _totalPagesHK)
            {
                _currentPageHK++;
                UpdatePaginationHK();
            }
        }

        // ================= THỐNG KÊ CHI TIẾT VĂN PHÒNG & BÁO CÁO TUẦN (ROLE-BASED & OPTIMIZED UPGRADE) =================

        private void InitDateFilter()
        {
            _isFilterChanging = true;
            try
            {
                // Mặc định chọn ngày hiện tại cho DatePicker chọn Tuần
                System.DateTime today = System.DateTime.Today;
                dtpSelectWeek.SelectedDate = today;

                int diff = (7 + (today.DayOfWeek - System.DayOfWeek.Monday)) % 7;
                System.DateTime startOfWeek = today.AddDays(-1 * diff);
                System.DateTime endOfWeek = startOfWeek.AddDays(6);
                
                dtpStart.SelectedDate = startOfWeek;
                dtpEnd.SelectedDate = endOfWeek;
                
                lblWeekRange.Text = $"Từ {startOfWeek:dd/MM/yyyy} đến {endOfWeek:dd/MM/yyyy}";
            }
            catch { }
            _isFilterChanging = false;
        }

        private void dtpSelectWeek_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isFilterChanging) return;
            try
            {
                if (dtpSelectWeek.SelectedDate is System.DateTime selectedDate)
                {
                    int diff = (7 + (selectedDate.DayOfWeek - System.DayOfWeek.Monday)) % 7;
                    System.DateTime startOfWeek = selectedDate.AddDays(-1 * diff);
                    System.DateTime endOfWeek = startOfWeek.AddDays(6);
                    
                    _isFilterChanging = true;
                    dtpStart.SelectedDate = startOfWeek;
                    dtpEnd.SelectedDate = endOfWeek;
                    _isFilterChanging = false;
                    
                    lblWeekRange.Text = $"Từ {startOfWeek:dd/MM/yyyy} đến {endOfWeek:dd/MM/yyyy}";
                    
                    LoadReport();
                }
            }
            catch { }
        }

        private void LoadOfficeSummaries()
        {
            try
            {
                DataTable dt = bus.ThongKeVanPhong();
                foreach (DataRow row in dt.Rows)
                {
                    string name = row["VanPhong"]?.ToString() ?? "";
                    int soVe = System.Convert.ToInt32(row["SoVe"]);
                    double doanhThu = System.Convert.ToDouble(row["DoanhThu"]);
                    
                    string textVe = $"{soVe:N0} vé";
                    string textDoanhThu = $"{doanhThu:N0} đ";
                    
                    if (name.Contains("Hà Nội"))
                    {
                        txtVeHN.Text = textVe;
                        txtDoanhThuHN.Text = textDoanhThu;
                    }
                    else if (name.Contains("HCM") || name.Contains("Hồ Chí Minh") || name.Contains("TP.HCM") || name.Contains("TP. HCM") || name.Contains("TPHCM"))
                    {
                        txtVeHCM.Text = textVe;
                        txtDoanhThuHCM.Text = textDoanhThu;
                    }
                    else if (name.Contains("Đà Nẵng"))
                    {
                        txtVeDN.Text = textVe;
                        txtDoanhThuDN.Text = textDoanhThu;
                    }
                }
            }
            catch { }
        }

        private void SelectOffice(int officeId)
        {
            _selectedOfficeId = officeId;
            
            // Làm nổi bật văn phòng đang được active (Selected Office Visual Style)
            var activeBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1565C0"));
            var inactiveBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#CFD8DC"));
            var activeBg = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E3F2FD"));
            var inactiveBg = System.Windows.Media.Brushes.White;

            cardHN.BorderThickness = new System.Windows.Thickness(officeId == 1 ? 2.5 : 1);
            cardHN.BorderBrush = officeId == 1 ? activeBrush : inactiveBrush;
            cardHN.Background = officeId == 1 ? activeBg : inactiveBg;

            cardHCM.BorderThickness = new System.Windows.Thickness(officeId == 2 ? 2.5 : 1);
            cardHCM.BorderBrush = officeId == 2 ? activeBrush : inactiveBrush;
            cardHCM.Background = officeId == 2 ? activeBg : inactiveBg;

            cardDN.BorderThickness = new System.Windows.Thickness(officeId == 3 ? 2.5 : 1);
            cardDN.BorderBrush = officeId == 3 ? activeBrush : inactiveBrush;
            cardDN.Background = officeId == 3 ? activeBg : inactiveBg;
            
            // Cập nhật nhãn khu vực
            txtKhuVucReport.Text = officeId == 1 ? "Miền Bắc (Hà Nội)" :
                                   officeId == 2 ? "Miền Nam (TP.HCM)" : "Miền Trung (Đà Nẵng)";
            
            // Tải danh sách nhân viên tương ứng với văn phòng đó
            LoadEmployeesForReport();
            
            // Tải báo cáo tuần
            LoadReport();
        }

        private void LoadEmployeesForReport()
        {
            try
            {
                DataTable dt = bus.NhanVienTheoVanPhong(_selectedOfficeId);
                
                _isFilterChanging = true;
                cboNhanVienReport.ItemsSource = dt.DefaultView;
                if (dt.Rows.Count > 0)
                {
                    cboNhanVienReport.SelectedValue = dt.Rows[0]["ID"];
                }
                else
                {
                    cboNhanVienReport.SelectedValue = null;
                }
                _isFilterChanging = false;
            }
            catch { }
        }

        private void LoadReport()
        {
            if (_isFilterChanging) return;
            try
            {
                int userId = 0;
                if (cboNhanVienReport.SelectedValue is int uid)
                {
                    userId = uid;
                }
                
                System.DateTime startDate = dtpStart.SelectedDate ?? System.DateTime.Today.AddDays(-7);
                System.DateTime endDate = dtpEnd.SelectedDate ?? System.DateTime.Today;
                
                // Sử dụng Stored Procedure doanh thu tuần đã tối ưu hóa, loại bỏ hoàn toàn hiện tượng giật lag
                DataTable dtReport = bus.DoanhThuTuanVanPhong(_selectedOfficeId, userId, startDate, endDate);
                
                // Kiểm tra vai trò của Nhân viên
                int roleId = 3; // Mặc định là Nhân viên bán vé (3) hoặc Tất cả (0)
                if (cboNhanVienReport.SelectedItem is DataRowView selectedRow && selectedRow["RoleID"] != System.DBNull.Value)
                {
                    roleId = System.Convert.ToInt32(selectedRow["RoleID"]);
                }
                
                // Hiển thị DataGrid phù hợp với vai trò
                if (roleId == 2) // Nhân viên điều hành
                {
                    dgBaoCaoBanVe.Visibility = System.Windows.Visibility.Collapsed;
                    dgBaoCaoDieuHanh.Visibility = System.Windows.Visibility.Visible;
                    
                    // Tính dòng "Tổng" cho Nhân viên điều hành
                    DataTable dtDisplay = dtReport.Clone();
                    foreach (DataRow row in dtReport.Rows)
                    {
                        dtDisplay.ImportRow(row);
                    }
                    
                    int totalChuyen = 0;
                    double totalVe = 0;
                    int totalDelay = 0;
                    int totalFeedback = 0;
                    double totalHanhLy = 0;
                    double totalDichVu = 0;
                    
                    foreach (DataRow r in dtReport.Rows)
                    {
                        totalChuyen += System.Convert.ToInt32(r["SoChuyenBay"]);
                        totalVe += System.Convert.ToInt32(r["SoVe"]);
                        totalDelay += System.Convert.ToInt32(r["SoChuyenDelay"]);
                        totalFeedback += System.Convert.ToInt32(r["FeedbackDaXuLy"]);
                        totalHanhLy += System.Convert.ToDouble(r["HanhLyDieuPhoi"]);
                        totalDichVu += System.Convert.ToDouble(r["DoanhSoDichVu"]);
                    }
                    
                    DataRow drTotal = dtDisplay.NewRow();
                    drTotal["Thu"] = "Tổng";
                    drTotal["SoChuyenBay"] = totalChuyen;
                    drTotal["KhachTrungBinh"] = totalChuyen > 0 ? (int)(totalVe / totalChuyen) : 0;
                    drTotal["SoChuyenDelay"] = totalDelay;
                    drTotal["FeedbackDaXuLy"] = totalFeedback;
                    drTotal["HanhLyDieuPhoi"] = totalHanhLy;
                    drTotal["DoanhSoDichVu"] = totalDichVu;
                    drTotal["GhiChu"] = totalDelay == 0 ? "Vận hành hoàn hảo" : $"Tỷ lệ delay: {((double)totalDelay / Math.Max(1, totalChuyen) * 100):N1}%";
                    
                    dtDisplay.Rows.Add(drTotal);
                    dgBaoCaoDieuHanh.ItemsSource = dtDisplay.DefaultView;
                }
                else // Nhân viên bán vé (3) hoặc Tất cả (0)
                {
                    dgBaoCaoBanVe.Visibility = System.Windows.Visibility.Visible;
                    dgBaoCaoDieuHanh.Visibility = System.Windows.Visibility.Collapsed;
                    
                    // Tính dòng "Tổng" cho Nhân viên bán vé
                    DataTable dtDisplay = dtReport.Clone();
                    foreach (DataRow row in dtReport.Rows)
                    {
                        dtDisplay.ImportRow(row);
                    }
                    
                    int totalCold = 0;
                    int totalFollow = 0;
                    int totalEmails = 0;
                    int totalMeetings = 0;
                    int totalVisits = 0;
                    int totalLeads = 0;
                    int totalDeals = 0;
                    int totalVe = 0;
                    double totalRealRev = 0;
                    double totalTargetRev = 0;
                    
                    foreach (DataRow r in dtReport.Rows)
                    {
                        totalCold += System.Convert.ToInt32(r["ColdCalls"]);
                        totalFollow += System.Convert.ToInt32(r["FollowUpCalls"]);
                        totalEmails += System.Convert.ToInt32(r["Emails"]);
                        totalMeetings += System.Convert.ToInt32(r["Meetings"]);
                        totalVisits += System.Convert.ToInt32(r["Visits"]);
                        totalLeads += System.Convert.ToInt32(r["Leads"]);
                        totalDeals += System.Convert.ToInt32(r["Deals"]);
                        totalVe += System.Convert.ToInt32(r["SoVe"]);
                        totalRealRev += System.Convert.ToDouble(r["DoanhThuThucTe"]);
                        totalTargetRev += System.Convert.ToDouble(r["DoanhThuMucTieu"]);
                    }
                    
                    DataRow drTotal = dtDisplay.NewRow();
                    drTotal["Thu"] = "Tổng";
                    drTotal["ColdCalls"] = totalCold;
                    drTotal["FollowUpCalls"] = totalFollow;
                    drTotal["Emails"] = totalEmails;
                    drTotal["Meetings"] = totalMeetings;
                    drTotal["Visits"] = totalVisits;
                    drTotal["Leads"] = totalLeads;
                    drTotal["Deals"] = totalDeals;
                    drTotal["SoVe"] = totalVe;
                    drTotal["DoanhThuThucTe"] = totalRealRev;
                    drTotal["DoanhThuMucTieu"] = totalTargetRev;
                    drTotal["ChenhLech"] = totalRealRev - totalTargetRev;
                    drTotal["IsNegative"] = (totalRealRev - totalTargetRev) < 0 ? 1 : 0;
                    drTotal["GhiChu"] = totalRealRev >= totalTargetRev ? "Đạt chỉ tiêu tuần" : "Cần nỗ lực hơn";
                    
                    dtDisplay.Rows.Add(drTotal);
                    dgBaoCaoBanVe.ItemsSource = dtDisplay.DefaultView;
                }
            }
            catch { }
        }

        private void CardOffice_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string tagStr && int.TryParse(tagStr, out int officeId))
            {
                SelectOffice(officeId);
            }
        }

        private void ReportFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadReport();
        }

        // ================= XUẤT VÀ IN BÁO CÁO (PRINT / EXCEL UPGRADE) =================

        private void btnPrintReport_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    DataGrid activeGrid = dgBaoCaoBanVe.Visibility == System.Windows.Visibility.Visible 
                        ? dgBaoCaoBanVe 
                        : dgBaoCaoDieuHanh;

                    string jobName = dgBaoCaoBanVe.Visibility == System.Windows.Visibility.Visible
                        ? "BaoCaoDoanhThuTuan_BanVe"
                        : "BaoCaoHoatDongTuan_DieuHanh";

                    string staffRole = dgBaoCaoBanVe.Visibility == System.Windows.Visibility.Visible
                        ? "Nhân viên Bán vé (Sales Agent)"
                        : "Nhân viên Điều hành (Flight Operator)";

                    // Tạo vùng in cực kỳ trực quan và chuyên nghiệp
                    StackPanel printContainer = new StackPanel { Margin = new System.Windows.Thickness(30), Background = System.Windows.Media.Brushes.White };

                    // 1. Tiêu đề chính hãng bay
                    TextBlock companyText = new TextBlock
                    {
                        Text = "SKYBLUE AIRLINES - HỆ THỐNG QUẢN TRỊ CHI NHÁNH",
                        FontSize = 11,
                        FontWeight = System.Windows.FontWeights.Bold,
                        Foreground = System.Windows.Media.Brushes.Gray,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        Margin = new System.Windows.Thickness(0, 0, 0, 4)
                    };
                    printContainer.Children.Add(companyText);

                    TextBlock titleText = new TextBlock
                    {
                        Text = dgBaoCaoBanVe.Visibility == System.Windows.Visibility.Visible
                            ? "BÁO CÁO HIỆU SUẤT DOANH THU BÁN VÉ"
                            : "BÁO CÁO HIỆU SUẤT ĐIỀU PHỐI VẬN HÀNH BAY",
                        FontSize = 18,
                        FontWeight = System.Windows.FontWeights.Bold,
                        Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1565C0")),
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        Margin = new System.Windows.Thickness(0, 5, 0, 15)
                    };
                    printContainer.Children.Add(titleText);

                    // 2. Thông tin chung
                    Grid infoGrid = new Grid { Margin = new System.Windows.Thickness(0, 0, 0, 15) };
                    infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(1.2, System.Windows.GridUnitType.Star) });
                    infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });

                    StackPanel leftInfo = new StackPanel();
                    leftInfo.Children.Add(new TextBlock { Text = $"📍 Văn phòng chi nhánh: {txtKhuVucReport.Text}", FontSize = 12, FontWeight = System.Windows.FontWeights.SemiBold, Margin = new System.Windows.Thickness(0, 0, 0, 4) });
                    
                    // Lấy họ tên nhân viên
                    string staffName = "Tất cả nhân viên";
                    if (cboNhanVienReport.SelectedItem is DataRowView rowView)
                    {
                        staffName = rowView["HoTen"]?.ToString() ?? "Tất cả nhân viên";
                    }
                    leftInfo.Children.Add(new TextBlock { Text = $"👤 Nhân viên báo cáo: {staffName}", FontSize = 12, Margin = new System.Windows.Thickness(0, 0, 0, 4) });
                    leftInfo.Children.Add(new TextBlock { Text = $"💼 Vai trò nhân sự: {staffRole}", FontSize = 12, Margin = new System.Windows.Thickness(0, 0, 0, 4) });

                    StackPanel rightInfo = new StackPanel();
                    rightInfo.Children.Add(new TextBlock { Text = $"📅 Tuần báo cáo: {lblWeekRange.Text.Replace("Tuần từ ", "")}", FontSize = 12, FontWeight = System.Windows.FontWeights.SemiBold, Margin = new System.Windows.Thickness(0, 0, 0, 4) });
                    rightInfo.Children.Add(new TextBlock { Text = $"🖨️ Ngày xuất bản: {System.DateTime.Now:dd/MM/yyyy HH:mm}", FontSize = 12, Margin = new System.Windows.Thickness(0, 0, 0, 4) });

                    Grid.SetColumn(leftInfo, 0);
                    Grid.SetColumn(rightInfo, 1);
                    infoGrid.Children.Add(leftInfo);
                    infoGrid.Children.Add(rightInfo);
                    printContainer.Children.Add(infoGrid);

                    // Separator line
                    Border separator = new Border { BorderBrush = System.Windows.Media.Brushes.LightGray, BorderThickness = new System.Windows.Thickness(0, 0, 0, 1), Margin = new System.Windows.Thickness(0, 0, 0, 15) };
                    printContainer.Children.Add(separator);

                    // 3. Sao chép DataGrid dữ liệu thành bảng in đẹp đẽ có viền
                    Border tableBorder = new Border { BorderBrush = System.Windows.Media.Brushes.DarkGray, BorderThickness = new System.Windows.Thickness(1), Margin = new System.Windows.Thickness(0, 0, 0, 20) };
                    Grid tableGrid = new Grid();
                    tableBorder.Child = tableGrid;

                    // Tính toán tổng chiều rộng thực tế của Grid để chia tỷ lệ
                    double totalGridWidth = 0;
                    foreach (var col in activeGrid.Columns)
                    {
                        totalGridWidth += col.ActualWidth > 0 ? col.ActualWidth : 100;
                    }

                    // Độ rộng vùng in khả dụng (trừ đi lề 30 mỗi bên)
                    double printableWidth = printDialog.PrintableAreaWidth > 0 ? (printDialog.PrintableAreaWidth - 60) : 700;
                    tableGrid.Width = printableWidth;

                    // Định nghĩa các cột theo tỷ lệ Star để tự động co giãn vừa vặn trang giấy A4, không bị tràn viền
                    foreach (var col in activeGrid.Columns)
                    {
                        double colWidth = col.ActualWidth > 0 ? col.ActualWidth : 100;
                        double proportion = colWidth / totalGridWidth;
                        tableGrid.ColumnDefinitions.Add(new ColumnDefinition 
                        { 
                            Width = new System.Windows.GridLength(proportion, System.Windows.GridUnitType.Star) 
                        });
                    }

                    // Header Row
                    tableGrid.RowDefinitions.Add(new RowDefinition { Height = System.Windows.GridLength.Auto });
                    int colIdx = 0;
                    foreach (var col in activeGrid.Columns)
                    {
                        Border cellBorder = new Border
                        {
                            Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#ECEFF1")),
                            BorderBrush = System.Windows.Media.Brushes.DarkGray,
                            BorderThickness = new System.Windows.Thickness(0, 0, colIdx < activeGrid.Columns.Count - 1 ? 1 : 0, 1),
                            Padding = new System.Windows.Thickness(6, 6, 6, 6)
                        };
                        TextBlock cellText = new TextBlock
                        {
                            Text = col.Header?.ToString() ?? "",
                            FontWeight = System.Windows.FontWeights.Bold,
                            FontSize = 10,
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                            TextWrapping = System.Windows.TextWrapping.Wrap
                        };
                        cellBorder.Child = cellText;
                        Grid.SetRow(cellBorder, 0);
                        Grid.SetColumn(cellBorder, colIdx);
                        tableGrid.Children.Add(cellBorder);
                        colIdx++;
                    }

                    // Data Rows
                    var itemsSource = activeGrid.ItemsSource as System.Data.DataView;
                    if (itemsSource != null)
                    {
                        int rowIdx = 1;
                        foreach (System.Data.DataRowView itemRow in itemsSource)
                        {
                            tableGrid.RowDefinitions.Add(new RowDefinition { Height = System.Windows.GridLength.Auto });
                            int cellColIdx = 0;
                            foreach (var col in activeGrid.Columns)
                            {
                                Border cellBorder = new Border
                                {
                                    BorderBrush = System.Windows.Media.Brushes.LightGray,
                                    BorderThickness = new System.Windows.Thickness(0, 0, cellColIdx < activeGrid.Columns.Count - 1 ? 1 : 0, 1),
                                    Padding = new System.Windows.Thickness(6, 6, 6, 6)
                                };

                                // Lấy giá trị
                                string val = "";
                                if (col is DataGridTextColumn textCol)
                                {
                                    string bindingPath = (textCol.Binding as System.Windows.Data.Binding)?.Path?.Path;
                                    val = bindingPath != null ? itemRow[bindingPath]?.ToString() ?? "" : "";
                                }
                                else if (col is DataGridTemplateColumn templateCol)
                                {
                                    if (col.Header.ToString().Contains("Chênh lệch"))
                                    {
                                        val = itemRow["ChenhLech"]?.ToString() ?? "";
                                        if (val != "" && itemRow["IsNegative"] != DBNull.Value && Convert.ToInt32(itemRow["IsNegative"]) == 1)
                                        {
                                            val = "-" + val;
                                        }
                                    }
                                }

                                // Căn chỉnh dòng tổng cộng
                                bool isTotalRow = itemRow["Thu"]?.ToString() == "Tổng" || itemRow["Thu"]?.ToString() == "Tổng cộng";

                                TextBlock cellText = new TextBlock
                                {
                                    Text = val,
                                    FontSize = 10,
                                    FontWeight = isTotalRow ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal,
                                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                                    TextWrapping = System.Windows.TextWrapping.Wrap
                                };

                                // Định dạng số tiền căn phải
                                if (col.Header.ToString().Contains("Doanh thu") || col.Header.ToString().Contains("Chênh lệch") || col.Header.ToString().Contains("bổ trợ") || col.Header.ToString().Contains("hành lý"))
                                {
                                    cellText.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                                    if (decimal.TryParse(val, out decimal dVal))
                                    {
                                        cellText.Text = dVal.ToString("N0");
                                    }
                                    if (col.Header.ToString().Contains("Doanh thu") || col.Header.ToString().Contains("Chênh lệch") || col.Header.ToString().Contains("bổ trợ"))
                                    {
                                        cellText.Text += " đ";
                                    }
                                    else if (col.Header.ToString().Contains("hành lý"))
                                    {
                                        cellText.Text += " kg";
                                    }
                                }

                                cellBorder.Child = cellText;
                                Grid.SetRow(cellBorder, rowIdx);
                                Grid.SetColumn(cellBorder, cellColIdx);
                                tableGrid.Children.Add(cellBorder);
                                cellColIdx++;
                            }
                            rowIdx++;
                        }
                    }
                    printContainer.Children.Add(tableBorder);

                    // 4. Phần Nhận xét / Đánh giá
                    string comment = txtNhanXetReport.Text;
                    if (string.IsNullOrWhiteSpace(comment))
                    {
                        comment = "(Không có nhận xét hoặc đánh giá bổ sung cho tuần báo cáo này)";
                    }

                    GroupBox commentBox = new GroupBox
                    {
                        Header = "📝 Ý KIẾN & ĐÁNH GIÁ CỦA QUẢN LÝ VĂN PHÒNG",
                        FontWeight = System.Windows.FontWeights.Bold,
                        FontSize = 11,
                        Margin = new System.Windows.Thickness(0, 5, 0, 25),
                        Padding = new System.Windows.Thickness(12),
                        Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FAFAFA")),
                        BorderBrush = System.Windows.Media.Brushes.LightGray,
                        BorderThickness = new System.Windows.Thickness(1)
                    };
                    TextBlock commentText = new TextBlock
                    {
                        Text = comment,
                        FontWeight = System.Windows.FontWeights.Normal,
                        FontSize = 11,
                        LineHeight = 16,
                        TextWrapping = System.Windows.TextWrapping.Wrap,
                        FontStyle = string.IsNullOrWhiteSpace(txtNhanXetReport.Text) ? System.Windows.FontStyles.Italic : System.Windows.FontStyles.Normal,
                        Foreground = string.IsNullOrWhiteSpace(txtNhanXetReport.Text) ? System.Windows.Media.Brushes.Gray : System.Windows.Media.Brushes.Black
                    };
                    commentBox.Content = commentText;
                    printContainer.Children.Add(commentBox);

                    // 5. Phần chữ ký phê duyệt chuẩn doanh nghiệp
                    Grid signGrid = new Grid { Margin = new System.Windows.Thickness(0, 10, 0, 0) };
                    signGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
                    signGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });

                    StackPanel operatorSign = new StackPanel { HorizontalAlignment = System.Windows.HorizontalAlignment.Center };
                    operatorSign.Children.Add(new TextBlock { Text = "Người lập báo cáo", FontWeight = System.Windows.FontWeights.Bold, FontSize = 11, HorizontalAlignment = System.Windows.HorizontalAlignment.Center });
                    operatorSign.Children.Add(new TextBlock { Text = "(Ký và ghi rõ họ tên)", FontStyle = System.Windows.FontStyles.Italic, FontSize = 9.5, Foreground = System.Windows.Media.Brushes.Gray, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, Margin = new System.Windows.Thickness(0, 2, 0, 45) });
                    operatorSign.Children.Add(new TextBlock { Text = staffName, FontWeight = System.Windows.FontWeights.Bold, FontSize = 11, HorizontalAlignment = System.Windows.HorizontalAlignment.Center });

                    StackPanel managerSign = new StackPanel { HorizontalAlignment = System.Windows.HorizontalAlignment.Center };
                    managerSign.Children.Add(new TextBlock { Text = "Ban Giám Đốc chi nhánh", FontWeight = System.Windows.FontWeights.Bold, FontSize = 11, HorizontalAlignment = System.Windows.HorizontalAlignment.Center });
                    managerSign.Children.Add(new TextBlock { Text = "(Ký, đóng dấu duyệt)", FontStyle = System.Windows.FontStyles.Italic, FontSize = 9.5, Foreground = System.Windows.Media.Brushes.Gray, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, Margin = new System.Windows.Thickness(0, 2, 0, 45) });
                    managerSign.Children.Add(new TextBlock { Text = "..................................................", FontSize = 11, HorizontalAlignment = System.Windows.HorizontalAlignment.Center });

                    Grid.SetColumn(operatorSign, 0);
                    Grid.SetColumn(managerSign, 1);
                    signGrid.Children.Add(operatorSign);
                    signGrid.Children.Add(managerSign);
                    printContainer.Children.Add(signGrid);

                    // Đo đạc kích thước in ấn
                    Size pageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
                    printContainer.Measure(pageSize);
                    printContainer.Arrange(new Rect(0, 0, printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight));

                    // Tiến hành in visual container
                    printDialog.PrintVisual(printContainer, jobName);
                    ShowDialogMessage("Đã gửi lệnh in báo cáo tuần kèm nhận xét và sơ đồ phê duyệt thành công!", "In Báo Cáo");
                }
            }
            catch (System.Exception ex)
            {
                ShowDialogMessage($"Lỗi trong quá trình in báo cáo: {ex.Message}", "Lỗi hệ thống");
            }
        }

        private void btnExportExcel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                DataGrid activeGrid = dgBaoCaoBanVe.Visibility == System.Windows.Visibility.Visible 
                    ? dgBaoCaoBanVe 
                    : dgBaoCaoDieuHanh;

                // Tự động ánh xạ mã văn phòng viết tắt tiếng Việt không dấu cho tên file
                string officeCode = _selectedOfficeId switch
                {
                    1 => "HN",
                    2 => "HCM",
                    3 => "DN",
                    _ => "VP"
                };

                string fileName = dgBaoCaoBanVe.Visibility == System.Windows.Visibility.Visible
                    ? $"BaoCaoDoanhThuTuan_{officeCode}_{System.DateTime.Today:yyyyMMdd}.xlsx"
                    : $"BaoCaoHoatDongTuan_{officeCode}_{System.DateTime.Today:yyyyMMdd}.xlsx";

                string title = dgBaoCaoBanVe.Visibility == System.Windows.Visibility.Visible
                    ? $"BÁO CÁO DOANH THU BÁN VÉ TUẦN - VĂN PHÒNG {txtKhuVucReport.Text.ToUpper()}"
                    : $"BÁO CÁO HOẠT ĐỘNG ĐIỀU PHỐI TUẦN - VĂN PHÒNG {txtKhuVucReport.Text.ToUpper()}";

                FlightManagement.Helpers.ExcelExporter.ExportDataGrid(activeGrid, title, fileName);
            }
            catch (System.Exception ex)
            {
                ShowDialogMessage($"Lỗi trong quá trình xuất Excel: {ex.Message}", "Lỗi hệ thống");
            }
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
