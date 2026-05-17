using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BUS;
using Microsoft.Win32;

namespace FlightManagement.UserControls
{
    public partial class ucQuanLyNhanVien : UserControl
    {
        private readonly NhanVienBUS bus = new();
        private readonly VanPhongBUS vanPhongBus = new();
        private readonly VaiTroBUS vaiTroBus = new();
        private readonly LichSuBUS lichSuBus = new();
        private DataTable _data = new();
        private int _userId;
        private int _editId = -1; // -1 = add mode, >0 = edit mode

        public ucQuanLyNhanVien(int userId)
        {
            InitializeComponent();
            _userId = userId;
            LoadFilters();
            LoadFormCombos();
            LoadData();
        }

        private void LoadFilters()
        {
            cboVaiTro.Items.Clear(); cboVaiTro.Items.Add("Tất cả");
            try { foreach (DataRow r in vaiTroBus.HienThi().Rows) cboVaiTro.Items.Add(r["TenVaiTro"].ToString()); } catch { }
            cboVaiTro.SelectedIndex = 0;

            cboVanPhong.Items.Clear(); cboVanPhong.Items.Add("Tất cả");
            try { foreach (DataRow r in vanPhongBus.HienThi().Rows) cboVanPhong.Items.Add(r["TenVanPhong"].ToString()); } catch { }
            cboVanPhong.SelectedIndex = 0;

            cboTrangThai.Items.Clear();
            cboTrangThai.Items.Add("Tất cả"); cboTrangThai.Items.Add("Hoạt động"); cboTrangThai.Items.Add("Đã khóa");
            cboTrangThai.SelectedIndex = 0;
        }

        private void LoadFormCombos()
        {
            try
            {
                cboVanPhongForm.ItemsSource = vanPhongBus.HienThi().DefaultView;
                cboVaiTroForm.ItemsSource = vaiTroBus.HienThi().DefaultView;
            }
            catch { }
        }

        private void LoadData()
        {
            try
            {
                // Truy vấn toàn bộ dữ liệu nhân viên từ SQL Server lên bộ nhớ
                _data = bus.HienThi();
                
                // Kích hoạt bộ lọc để chỉ hiển thị những dữ liệu thỏa mãn điều kiện tìm kiếm hiện tại
                ApplyFilter();
                
                // Đếm tổng số lượng nhân viên hiện có trong hệ thống
                int total = _data.Rows.Count;
                
                // Dùng LINQ Select để đếm xem có bao nhiêu tài khoản đang ở trạng thái Hoạt động
                int active = _data.Select("TrangThai = 'Hoạt động'").Length;
                
                // Gắn các con số thống kê lên các thẻ màu trên cùng của màn hình
                txtTong.Text = total.ToString();
                txtHoatDong.Text = active.ToString();
                txtKhoa.Text = (total - active).ToString(); // Số tài khoản bị khóa = Tổng - Hoạt động
                
                // Cập nhật lại các biểu đồ/danh sách thống kê (Nhân viên theo vai trò, theo văn phòng)
                LoadDistributions();
            }
            catch (Exception ex) { ShowDialogMessage("Lỗi: " + ex.Message, "Lỗi hệ thống"); }
        }

        private void LoadDistributions()
        {
            // Distribution by role
            lstVaiTro.Items.Clear();
            var roleGroups = _data.AsEnumerable().GroupBy(r => r["VaiTro"].ToString());
            foreach (var g in roleGroups)
            {
                var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
                sp.Children.Add(new TextBlock { Text = g.Key, Width = 200 });
                sp.Children.Add(new TextBlock { Text = $"{g.Count()} người", Foreground = System.Windows.Media.Brushes.DodgerBlue, FontWeight = FontWeights.Bold });
                lstVaiTro.Items.Add(sp);
            }

            // Distribution by office
            lstVanPhong.Items.Clear();
            var officeGroups = _data.AsEnumerable().GroupBy(r => r["VanPhong"].ToString());
            foreach (var g in officeGroups)
            {
                var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
                sp.Children.Add(new TextBlock { Text = string.IsNullOrEmpty(g.Key) ? "Chưa xác định" : g.Key, Width = 200 });
                sp.Children.Add(new TextBlock { Text = $"{g.Count()} người", Foreground = System.Windows.Media.Brushes.OrangeRed, FontWeight = FontWeights.Bold });
                lstVanPhong.Items.Add(sp);
            }
        }

        private void ApplyFilter()
        {
            var view = _data.DefaultView;
            string filter = "";
            
            // Lọc theo từ khóa gõ vào ô tìm kiếm (tìm trên cột Họ Tên hoặc cột Email)
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                filter += $"(HoTen LIKE '%{txtSearch.Text}%' OR Email LIKE '%{txtSearch.Text}%')";
                
            // Lọc theo ComboBox Vai trò (nếu không chọn 'Tất cả')
            if (cboVaiTro.SelectedIndex > 0)
                filter += (filter.Length > 0 ? " AND " : "") + $"VaiTro = '{cboVaiTro.SelectedItem}'";
                
            // Lọc theo ComboBox Văn phòng
            if (cboVanPhong.SelectedIndex > 0)
                filter += (filter.Length > 0 ? " AND " : "") + $"VanPhong = '{cboVanPhong.SelectedItem}'";
                
            // Lọc theo ComboBox Trạng thái (Hoạt động / Đã khóa)
            if (cboTrangThai.SelectedIndex > 0)
                filter += (filter.Length > 0 ? " AND " : "") + $"TrangThai = '{cboTrangThai.SelectedItem}'";
                
            // Ép chuỗi điều kiện vào DataView, tự động DataGrid sẽ bị co lại chỉ còn những dòng khớp điều kiện
            view.RowFilter = filter;
            dgNhanVien.ItemsSource = view;
            
            // Cập nhật câu thông báo số lượng người tìm thấy
            txtCount.Text = $"👥 Danh sách nhân viên - {view.Count} người";
        }

        private void Filter_Changed(object sender, EventArgs e) { if (_data.Rows.Count > 0) ApplyFilter(); }
        private void btnXoaBoLoc_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = ""; cboVaiTro.SelectedIndex = 0; cboVanPhong.SelectedIndex = 0; cboTrangThai.SelectedIndex = 0;
        }

        private void btnToggleStats_Click(object sender, RoutedEventArgs e)
        {
            if (pnlStats.Visibility == Visibility.Visible)
            { pnlStats.Visibility = Visibility.Collapsed; btnToggleStats.Content = "📊 Hiện thống kê"; }
            else
            { pnlStats.Visibility = Visibility.Visible; btnToggleStats.Content = "📊 Ẩn thống kê"; }
        }

        private void btnExportCSV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtExport = new DataTable();
                dtExport.Columns.Add("Mã NV");
                dtExport.Columns.Add("Họ tên");
                dtExport.Columns.Add("Email");
                dtExport.Columns.Add("Văn phòng");
                dtExport.Columns.Add("Vai trò");
                dtExport.Columns.Add("Ngày sinh");
                dtExport.Columns.Add("Trạng thái");

                foreach (DataRowView r in _data.DefaultView)
                {
                    dtExport.Rows.Add(
                        r["ID"],
                        r["HoTen"],
                        r["Email"],
                        r["VanPhong"],
                        r["VaiTro"],
                        r["NgaySinh"] != DBNull.Value ? Convert.ToDateTime(r["NgaySinh"]).ToString("dd/MM/yyyy") : "",
                        r["TrangThai"]
                    );
                }

                FlightManagement.Helpers.ExcelExporter.ExportDataTable(dtExport, "DANH SÁCH NHÂN VIÊN HÃNG HÀNG KHÔNG SKYBLUE", "DanhSachNhanVien.xlsx");
            }
            catch (Exception ex) 
            { 
                ShowDialogMessage("Lỗi xuất Excel: " + ex.Message, "Lỗi"); 
            }
        }

        // ===== INLINE FORM =====
        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            _editId = -1;
            txtFormTitle.Text = "Thêm nhân viên mới";
            ClearForm();
            pnlForm.Visibility = Visibility.Visible;
        }

        private void btnSua_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is DataRowView row)
            {
                // RÀNG BUỘC BẢO VỆ TÀI KHOẢN ADMIN GỐC: Không cho phép sửa tài khoản Administrator được thêm trực tiếp trong SQL (ID = 1 và ID = 4)
                int targetId = Convert.ToInt32(row["ID"]);
                if (targetId == 1 || targetId == 4)
                {
                    ShowDialogMessage("Không được phép chỉnh sửa tài khoản Administrator gốc được thêm trực tiếp trong SQL!", "Bảo vệ hệ thống");
                    return;
                }

                _editId = targetId;
                txtFormTitle.Text = $"Sửa nhân viên - {row["HoTen"]}";
                txtHo.Text = row["LastName"]?.ToString() ?? "";
                txtTen.Text = row["FirstName"]?.ToString() ?? "";
                txtEmailForm.Text = row["Email"]?.ToString() ?? "";
                txtMatKhau.Password = "";
                if (row["OfficeID"] != DBNull.Value) cboVanPhongForm.SelectedValue = Convert.ToInt32(row["OfficeID"]);
                if (row["RoleID"] != DBNull.Value) cboVaiTroForm.SelectedValue = Convert.ToInt32(row["RoleID"]);
                if (row["NgaySinh"] != DBNull.Value) dpNgaySinh.SelectedDate = Convert.ToDateTime(row["NgaySinh"]);
                pnlForm.Visibility = Visibility.Visible;
            }
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra các trường văn bản không được để trống
            if (string.IsNullOrWhiteSpace(txtHo.Text) || string.IsNullOrWhiteSpace(txtTen.Text) || string.IsNullOrWhiteSpace(txtEmailForm.Text))
            {
                ShowDialogMessage("Vui lòng nhập đầy đủ Họ, Tên và Email!", "Thiếu thông tin");
                return;
            }
            
            // Biểu thức chính quy ép định dạng email phải hợp lệ
            if (!Regex.IsMatch(txtEmailForm.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ShowDialogMessage("Email không hợp lệ (sai định dạng)!", "Lỗi định dạng");
                return;
            }
            try
            {
                // Lấy các giá trị ID từ ComboBox, nếu lỗi thì gán mặc định (3 = Agent, 1 = Văn phòng chính)
                int roleId = cboVaiTroForm.SelectedValue != null ? Convert.ToInt32(cboVaiTroForm.SelectedValue) : 3;
                int officeId = cboVanPhongForm.SelectedValue != null ? Convert.ToInt32(cboVanPhongForm.SelectedValue) : 1;
                DateTime birthdate = dpNgaySinh.SelectedDate ?? DateTime.Today;

                // Kiểm tra tuổi nhân viên (phải đủ 18 tuổi tính đến ngày hôm nay)
                if (DateTime.Today.Year - birthdate.Year < 18 || (DateTime.Today.Year - birthdate.Year == 18 && DateTime.Today.DayOfYear < birthdate.DayOfYear))
                {
                    ShowDialogMessage("Nhân viên phải từ đủ 18 tuổi trở lên!", "Lỗi ngày sinh");
                    return;
                }

                // Mật khẩu cũng phải từ 6 ký tự trở lên để đảm bảo an toàn
                if (string.IsNullOrEmpty(txtMatKhau.Password) || txtMatKhau.Password.Length < 6)
                {
                    ShowDialogMessage("Vui lòng nhập mật khẩu (ít nhất 6 ký tự)!", "Thiếu thông tin");
                    return;
                }

                if (_editId == -1) // Chế độ Thêm mới
                {
                    // Gửi lệnh xuống BUS để thêm nhân viên vào CSDL
                    bus.Them(roleId, txtEmailForm.Text.Trim(), txtMatKhau.Password, txtTen.Text.Trim(), txtHo.Text.Trim(), officeId, birthdate);
                    
                    // Ghi nhận vào Lịch sử hệ thống
                    lichSuBus.GhiNhanChinhSua(_userId, "Thêm", "Nhân viên", $"Thêm nhân viên {txtHo.Text} {txtTen.Text}");
                    ShowDialogMessage("Thêm nhân viên thành công!", "Thành công");
                }
                else // Chế độ Sửa (Cập nhật)
                {
                    // Cập nhật thông tin nhân viên theo ID, mặc định set trạng thái Active là true (Hoạt động)
                    bus.CapNhat(_editId, roleId, txtEmailForm.Text.Trim(), txtMatKhau.Password, txtTen.Text.Trim(), txtHo.Text.Trim(), officeId, birthdate, true);
                    lichSuBus.GhiNhanChinhSua(_userId, "Sửa", "Nhân viên", $"Sửa nhân viên ID={_editId}");
                    ShowDialogMessage("Cập nhật thành công!", "Thành công");
                }
                
                // Đóng form nhập liệu và tải lại bảng danh sách nhân viên
                pnlForm.Visibility = Visibility.Collapsed;
                LoadData();
            }
            catch (Exception ex) { ShowDialogMessage("Lỗi: " + ex.Message, "Lỗi hệ thống"); }
        }

        private void btnHuyForm_Click(object sender, RoutedEventArgs e)
        {
            pnlForm.Visibility = Visibility.Collapsed;
        }

        private void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is DataRowView row)
            {
                // RÀNG BUỘC BẢO VỆ TÀI KHOẢN ADMIN GỐC: Không cho phép khóa tài khoản Administrator được thêm trực tiếp trong SQL (ID = 1 và ID = 4)
                int targetId = Convert.ToInt32(row["ID"]);
                if (targetId == 1 || targetId == 4)
                {
                    ShowDialogMessage("Không được phép khóa hoặc xóa tài khoản Administrator gốc!", "Bảo vệ hệ thống");
                    return;
                }

                // Kiểm tra tài khoản đã bị khóa từ trước chưa
                if (row["Active"] != DBNull.Value && !Convert.ToBoolean(row["Active"]))
                {
                    ShowDialogMessage("Tài khoản này đã bị khóa từ trước!", "Thông báo");
                    return;
                }

                ShowConfirmDialog($"Khóa tài khoản {row["HoTen"]}?", "Xác nhận", () =>
                {
                    try
                    {
                        bus.Xoa(Convert.ToInt32(row["ID"]));
                        lichSuBus.GhiNhanChinhSua(_userId, "Khóa", "Nhân viên", $"Khóa nhân viên {row["HoTen"]}");
                        LoadData();
                    }
                    catch (Exception ex) { ShowDialogMessage("Lỗi: " + ex.Message, "Lỗi hệ thống"); }
                });
            }
        }

        private void ClearForm()
        {
            txtHo.Text = ""; txtTen.Text = ""; txtEmailForm.Text = ""; txtMatKhau.Password = "";
            if (cboVanPhongForm.Items.Count > 0) cboVanPhongForm.SelectedIndex = 0;
            if (cboVaiTroForm.Items.Count > 0) cboVaiTroForm.SelectedIndex = 0;
            dpNgaySinh.SelectedDate = null;
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
