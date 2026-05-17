using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Data.SqlClient;
using BUS;

namespace FlightManagement.UserControls
{
    // Lớp ucQuanLyDichVu quản lý toàn bộ hệ sinh thái Dịch vụ cộng thêm của hãng hàng không.
    // Giao diện gồm 2 Tab: 
    // Tab 1 (GanDichVu): Cho phép tra cứu mã vé của khách và gán thủ công thêm dịch vụ (VD: Khách mua thêm Hành lý tại quầy).
    // Tab 2 (DanhSachDichVu): Quản lý danh mục chung các dịch vụ của hãng (Thêm/Sửa/Xóa Dịch vụ).
    public partial class ucQuanLyDichVu : UserControl
    {
        // Khởi tạo các lớp nghiệp vụ từ tầng BUS
        private readonly DichVuBUS bus = new();
        private readonly VeBUS veBus = new();
        private readonly LichSuBUS lichSuBus = new();
        
        // _userId: Lưu dấu vết người thao tác.
        // _editDVId: Cờ sửa chữa dùng cho Tab 2 (Danh sách dịch vụ).
        // _currentTicketId: Lưu ID của vé đang được tra cứu ở Tab 1 để tiện gán dịch vụ.
        private int _userId, _editDVId = -1, _currentTicketId = -1;
        private System.Windows.Threading.DispatcherTimer _searchTimer;
        private bool _isSelecting = false;

        // Lưu trữ dữ liệu dịch vụ nguyên gốc
        private DataTable _dataDV = new();

        // Biến phân trang cho dịch vụ
        private int _currentPageDV = 1;
        private int _pageSizeDV = 10;
        private int _totalPagesDV = 1;

        // Hàm khởi tạo, chạy lần đầu khi mở UserControl
        public ucQuanLyDichVu(int userId) 
        { 
            InitializeComponent(); 
            _userId = userId; 

            // Cấu hình các thuộc tính liên kết cho combobox chọn vé
            cboVe.DisplayMemberPath = "DisplayText";
            cboVe.SelectedValuePath = "ID";
            cboVe.AddHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent, new TextChangedEventHandler(cboVe_TextChanged));

            LoadData(); 
        }


        // Hàm nạp dữ liệu từ CSDL lên giao diện (Bao gồm bảng danh sách và các thống kê)
        private void LoadData()
        {
            try
            {
                // Tải danh sách toàn bộ các loại dịch vụ hiển thị lên DataGrid
                _dataDV = bus.HienThi();
                UpdatePaginationDV();
                
                // Đổ danh sách dịch vụ vào ComboBox ở Tab "Gán dịch vụ" để người dùng chọn
                cboDichVuGan.ItemsSource = _dataDV.DefaultView;
                
                // Tải trước 100 vé mới nhất hiển thị mặc định
                LoadInitialTickets();
                
                // Lấy các số liệu thống kê tổng quát (Từ Procedure Thống kê dưới DB)
                DataTable stats = bus.ThongKe();
                if (stats.Rows.Count > 0)
                {
                    // Cập nhật các thẻ thông số: Tổng số DV, Giá trung bình, Tổng doanh thu thu được từ bán DV
                    txtTong.Text = stats.Rows[0]["TongDichVu"].ToString();
                    
                    decimal avg = Convert.ToDecimal(stats.Rows[0]["GiaTrungBinh"]);
                    txtGiaTB.Text = $"{avg:N0} đ";
                    
                    decimal rev = Convert.ToDecimal(stats.Rows[0]["TongDoanhThu"]);
                    txtDoanhThu.Text = $"{rev:N0} đ";
                }
                
                // Hiển thị danh sách Top 3 Dịch vụ được mua nhiều nhất (Best Sellers)
                lstTop3.Items.Clear();
                DataTable top = bus.Top3();
                int rank = 1; // Biến đánh số thứ hạng
                foreach (DataRow r in top.Rows)
                {
                    // Dùng switch expression (C# 8.0+) để gán icon huy chương tương ứng với thứ hạng 1-2-3
                    string medal = rank switch { 1 => "🥇", 2 => "🥈", 3 => "🥉", _ => "" };
                    var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 3, 0, 3) };
                    
                    // Tạo dòng hiển thị Tên dịch vụ
                    sp.Children.Add(new TextBlock { Text = $"{medal} {r["TenDichVu"]}", Width = 250, FontSize = 14 });
                    // Hiện giá tiền (Chữ màu Xanh)
                    sp.Children.Add(new TextBlock { Text = $"{Convert.ToDecimal(r["Gia"]):N0} đ", Width = 120, Foreground = System.Windows.Media.Brushes.DodgerBlue });
                    // Hiện số lượt đã được khách đặt (Chữ in đậm màu Cam)
                    sp.Children.Add(new TextBlock { Text = $"{r["SoLanDat"]} lần đặt", FontWeight = FontWeights.Bold, Foreground = System.Windows.Media.Brushes.OrangeRed });
                    
                    lstTop3.Items.Add(sp);
                    rank++;
                }
            }
            catch (Exception ex) { ShowDialogMessage("Lỗi: " + ex.Message, "Lỗi hệ thống"); }
        }

        // Tải danh sách 100 vé mới nhất hiển thị mặc định (Vừa nhanh vừa có sẵn dữ liệu duyệt)
        private void LoadInitialTickets()
        {
            try
            {
                DataTable dtVe = veBus.HienThi();
                var ticketsList = new System.Collections.Generic.List<dynamic>();
                int count = 0;
                foreach (DataRow row in dtVe.Rows)
                {
                    if (count >= 100) break; // Chỉ lấy tối đa 100 vé mới nhất
                    ticketsList.Add(new
                    {
                        ID = Convert.ToInt32(row["ID"]),
                        DisplayText = $"{row["MaDatCho"]} - {row["TenKhach"]} ({row["SoHieu"]} - {row["TuyenBay"]})"
                    });
                    count++;
                }
                cboVe.ItemsSource = ticketsList;
            }
            catch { }
        }

        // ================= Tab 1: GÁN DỊCH VỤ THỦ CÔNG CHO VÉ =================
        
        // Sự kiện gõ tìm kiếm vé có debounce delay 500ms để chống lag
        private void cboVe_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isSelecting) return;
            if (cboVe.SelectedItem != null)
            {
                dynamic selectedItem = cboVe.SelectedItem;
                if (cboVe.Text == selectedItem.DisplayText)
                    return;
            }

            if (_searchTimer == null)
            {
                _searchTimer = new System.Windows.Threading.DispatcherTimer();
                _searchTimer.Interval = TimeSpan.FromMilliseconds(500);
                _searchTimer.Tick += SearchTimer_Tick;
            }
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void SearchTimer_Tick(object? sender, EventArgs e)
        {
            _searchTimer.Stop();
            string keyword = cboVe.Text.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                // Nếu rỗng, xóa sạch lựa chọn hiện tại và ẩn bảng tiện ích để tránh tự động điền lại của WPF
                _isSelecting = true;
                cboVe.SelectedIndex = -1;
                cboVe.Text = "";
                pnlDichVuVe.Visibility = Visibility.Collapsed;
                _currentTicketId = -1;
                _isSelecting = false;

                LoadInitialTickets();
                return;
            }

            try
            {
                // Lưu lại nội dung và vị trí con trỏ chuột hiện tại trước khi thay đổi ItemsSource
                string currentText = cboVe.Text;
                int caretIndex = 0;
                var textBox = cboVe.Template.FindName("PART_EditableTextBox", cboVe) as TextBox;
                if (textBox != null)
                {
                    caretIndex = textBox.CaretIndex;
                }

                DataTable dtVe = veBus.TimKiem(keyword);
                var ticketsList = new System.Collections.Generic.List<dynamic>();
                foreach (DataRow row in dtVe.Rows)
                {
                    ticketsList.Add(new
                    {
                        ID = Convert.ToInt32(row["ID"]),
                        DisplayText = $"{row["MaDatCho"]} - {row["TenKhach"]} ({row["SoHieu"]} - {row["TuyenBay"]})"
                    });
                }

                _isSelecting = true; // Chặn TextChanged phát sinh ngoài ý muốn
                cboVe.ItemsSource = ticketsList;
                cboVe.Text = currentText;
                _isSelecting = false;

                // Khôi phục lại nội dung và con trỏ chuột chính xác để người dùng gõ tiếp tục mượt mà
                if (textBox != null)
                {
                    textBox.Text = currentText;
                    textBox.CaretIndex = Math.Min(caretIndex, currentText.Length);
                }

                cboVe.IsDropDownOpen = true;
            }
            catch { }
        }

        // Sự kiện khi nhân viên chọn một vé từ ComboBox tìm kiếm
        private void cboVe_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboVe.SelectedValue == null)
            {
                pnlDichVuVe.Visibility = Visibility.Collapsed;
                _currentTicketId = -1;
                return;
            }
            try
            {
                _isSelecting = true;
                _currentTicketId = Convert.ToInt32(cboVe.SelectedValue);
                
                // Lấy thông tin từ đối tượng được chọn để hiển thị tiêu đề
                dynamic? selectedItem = cboVe.SelectedItem;
                if (selectedItem != null)
                {
                    txtVeInfo.Text = selectedItem.DisplayText;
                }
                
                // Gọi BUS để tìm xem cái vé này ĐANG CÓ những dịch vụ gì rồi, và đổ lên DataGrid nhỏ
                dgDichVuVe.ItemsSource = bus.LayTheoVe(_currentTicketId).DefaultView;
                pnlDichVuVe.Visibility = Visibility.Visible;

                // Tắt bôi đen toàn bộ sau khi người dùng chọn xong vé
                Dispatcher.BeginInvoke(new Action(() => {
                    var textBox = cboVe.Template.FindName("PART_EditableTextBox", cboVe) as TextBox;
                    if (textBox != null)
                    {
                        textBox.SelectionLength = 0;
                        textBox.CaretIndex = textBox.Text.Length;
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
            catch (Exception ex) { ShowDialogMessage(ex.Message, "Lỗi hệ thống"); }
            finally
            {
                _isSelecting = false;
            }
        }

        // Sự kiện khi nhân viên bấm nút [Làm mới] ở cạnh Combobox chọn vé
        private void btnLamMoiVe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _isSelecting = true;
                cboVe.SelectedIndex = -1;
                cboVe.Text = "";
                pnlDichVuVe.Visibility = Visibility.Collapsed;
                _currentTicketId = -1;
                _isSelecting = false;

                LoadInitialTickets();
            }
            catch { }
        }

        // Sự kiện khi nhân viên bấm [Thêm Dịch vụ này]
        private void btnGanDichVu_Click(object s, RoutedEventArgs e)
        {
            // Kiểm tra chốt chặn: Phải có vé đang được chọn và Combobox dịch vụ phải có giá trị
            if (_currentTicketId == -1 || cboDichVuGan.SelectedValue == null) return;
            try
            {
                int amenityId = Convert.ToInt32(cboDichVuGan.SelectedValue);
                
                // Trích xuất dòng dữ liệu đang được chọn trong ComboBox ra thành kiểu DataRowView
                var row = (DataRowView)cboDichVuGan.SelectedItem;
                
                // Đọc ra Cột Giá tiền (Để lưu luôn giá trị tại thời điểm mua, đề phòng tương lai giá đổi)
                decimal price = Convert.ToDecimal(row["Gia"]);
                
                // Thực thi lệnh insert liên kết Dịch vụ - Vé xuống Database (bảng AmenitiesTickets)
                bus.GanChoVe(amenityId, _currentTicketId, price);
                
                // Cập nhật log Audit
                lichSuBus.GhiNhanChinhSua(_userId, "Gán DV", "Vé", $"Gán dịch vụ cho vé #{_currentTicketId}");
                
                // Reload dữ liệu để DataGrid hiển thị ngay lập tức dịch vụ vừa thêm
                LoadData();
                pnlDichVuVe.Visibility = Visibility.Visible;
                dgDichVuVe.ItemsSource = bus.LayTheoVe(_currentTicketId).DefaultView;
            }
            catch (Exception ex) { ShowDialogMessage(ex.Message, "Lỗi hệ thống"); }
        }

        // Sự kiện khi bấm nút [Gỡ bỏ] (Dấu X đỏ) một dịch vụ ra khỏi vé
        private void btnGoDichVuVe_Click(object s, RoutedEventArgs e)
        {
            if (s is Button btn && btn.DataContext is DataRowView row)
            {
                try
                {
                    // Lệnh xóa cần 2 khóa: Khóa Dịch vụ và Khóa Vé
                    bus.XoaKhoiVe(Convert.ToInt32(row["ID"]), _currentTicketId);
                    
                    // Reload UI
                    LoadData();
                    pnlDichVuVe.Visibility = Visibility.Visible;
                    dgDichVuVe.ItemsSource = bus.LayTheoVe(_currentTicketId).DefaultView;
                }
                catch (Exception ex) { ShowDialogMessage(ex.Message, "Lỗi hệ thống"); }
            }
        }

        // ================= Tab 2: CRUD DANH MỤC DỊCH VỤ (THÊM / SỬA / XÓA) =================
        
        // Mở Form để Thêm một loại dịch vụ mới cho hãng
        private void btnThemDV_Click(object s, RoutedEventArgs e) 
        { 
            tcDichVu.SelectedIndex = 1; // Tự động nhảy sang tab Danh sách Dịch vụ
            _editDVId = -1; // Cờ Thêm mới
            txtFormTitleDV.Text = "Thêm dịch vụ mới"; 
            txtTenDV.Text = ""; 
            txtGiaDV.Text = ""; 
            pnlFormDV.Visibility = Visibility.Visible; 
        }
        
        // Mở Form chỉnh sửa Dịch vụ (Ví dụ đổi tên, tăng giá)
        private void btnSuaDV_Click(object s, RoutedEventArgs e)
        {
            if (s is Button btn && btn.DataContext is DataRowView row)
            {
                _editDVId = Convert.ToInt32(row["ID"]);
                txtFormTitleDV.Text = $"Sửa - {row["TenDichVu"]}";
                
                // Load dữ liệu cũ
                txtTenDV.Text = row["TenDichVu"].ToString();
                txtGiaDV.Text = Convert.ToDecimal(row["Gia"]).ToString("0"); // Bỏ bớt số thập phân .00
                
                pnlFormDV.Visibility = Visibility.Visible;
            }
        }
        
        // Lưu thông tin Danh mục
        private void btnLuuDV_Click(object s, RoutedEventArgs e)
        {
            // Bắt lỗi rỗng
            if (string.IsNullOrWhiteSpace(txtTenDV.Text) || string.IsNullOrWhiteSpace(txtGiaDV.Text))
            { ShowDialogMessage("Nhập đầy đủ thông tin!", "Thiếu thông tin"); return; }
            
            // Bắt lỗi Logic: Giá dịch vụ phải là số thực >= 0
            if (!decimal.TryParse(txtGiaDV.Text, out decimal price) || price < 0)
            { ShowDialogMessage("Giá dịch vụ phải là một số lớn hơn hoặc bằng 0!", "Lỗi định dạng"); return; }
            
            try
            {
                if (_editDVId == -1) 
                { 
                    // Chế độ Thêm mới
                    bus.Them(txtTenDV.Text.Trim(), price); 
                    lichSuBus.GhiNhanChinhSua(_userId, "Thêm", "Dịch vụ", $"Thêm DV {txtTenDV.Text}"); 
                }
                else 
                { 
                    // Chế độ Cập nhật
                    bus.CapNhat(_editDVId, txtTenDV.Text.Trim(), price); 
                    lichSuBus.GhiNhanChinhSua(_userId, "Sửa", "Dịch vụ", $"Sửa DV ID={_editDVId}"); 
                }
                
                // Tắt form, refresh dữ liệu
                pnlFormDV.Visibility = Visibility.Collapsed; 
                LoadData();
            }
            catch (Exception ex) { ShowDialogMessage(ex.Message, "Lỗi hệ thống"); }
        }
        
        private void btnHuyDV_Click(object s, RoutedEventArgs e) { pnlFormDV.Visibility = Visibility.Collapsed; }
        
        // Xóa hẳn một dịch vụ khỏi CSDL
        private void btnXoaDV_Click(object s, RoutedEventArgs e)
        {
            if (s is Button btn && btn.DataContext is DataRowView row)
                ShowConfirmDialog($"Xóa {row["TenDichVu"]}?", "Xác nhận", () =>
                {
                    try
                    {
                        bus.Xoa(Convert.ToInt32(row["ID"]));
                        lichSuBus.GhiNhanChinhSua(_userId, "Xóa", "Dịch vụ", $"Xóa {row["TenDichVu"]}");
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        ShowDialogMessage(ex.Message, "Lỗi hệ thống"); 
                    }
                });
        }

        // ================= PHÂN TRANG (PAGINATION) =================

        private void UpdatePaginationDV()
        {
            if (_dataDV == null) return;
            _totalPagesDV = (int)Math.Ceiling((double)_dataDV.Rows.Count / _pageSizeDV);
            if (_totalPagesDV == 0) _totalPagesDV = 1;
            if (_currentPageDV > _totalPagesDV) _currentPageDV = _totalPagesDV;
            if (_currentPageDV < 1) _currentPageDV = 1;

            txtPageInfoDV.Text = $"Trang {_currentPageDV}/{_totalPagesDV}";

            DataTable pageTable = _dataDV.Clone();
            int startIndex = (_currentPageDV - 1) * _pageSizeDV;
            int endIndex = Math.Min(startIndex + _pageSizeDV, _dataDV.Rows.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                pageTable.ImportRow(_dataDV.Rows[i]);
            }

            dgDichVu.ItemsSource = pageTable.DefaultView;
            btnPrevDV.IsEnabled = _currentPageDV > 1;
            btnNextDV.IsEnabled = _currentPageDV < _totalPagesDV;
        }

        private void btnPrevDV_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPageDV > 1) { _currentPageDV--; UpdatePaginationDV(); }
        }

        private void btnNextDV_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPageDV < _totalPagesDV) { _currentPageDV++; UpdatePaginationDV(); }
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
