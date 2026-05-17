using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BUS;

namespace FlightManagement.UserControls
{
    public partial class ucLichSuHeThong : UserControl
    {
        private readonly LichSuBUS bus = new();

        private int currentPageTC = 1;
        private int currentPageCS = 1;
        private int totalPagesTC = 1;
        private int totalPagesCS = 1;
        private const int pageSize = 15;

        public ucLichSuHeThong() 
        { 
            InitializeComponent(); 
            LoadData(); 
        }

        private void LoadData()
        {
            currentPageTC = 1;
            currentPageCS = 1;
            UpdatePaginationTC();
            UpdatePaginationCS();
        }

        private void UpdatePaginationTC()
        {
            try
            {
                DataTable pageDt = bus.LayTruyCap(currentPageTC, pageSize);

                if (pageDt == null || pageDt.Rows.Count == 0)
                {
                    dgTruyCap.ItemsSource = null;
                    txtPaginationInfoTC.Text = "Trang 0/0";
                    txtTruyCapCount.Text = "📋 Lịch sử truy cập (0 bản ghi)";
                    btnPrevTC.IsEnabled = false;
                    btnPrevTC.Opacity = 0.45;
                    btnNextTC.IsEnabled = false;
                    btnNextTC.Opacity = 0.45;
                    totalPagesTC = 1;
                    return;
                }

                int totalRows = Convert.ToInt32(pageDt.Rows[0]["TotalRecords"]);
                totalPagesTC = (int)Math.Ceiling((double)totalRows / pageSize);

                if (currentPageTC < 1) currentPageTC = 1;
                if (currentPageTC > totalPagesTC) currentPageTC = totalPagesTC;

                txtTruyCapCount.Text = $"📋 Lịch sử truy cập ({totalRows} bản ghi)";
                dgTruyCap.ItemsSource = pageDt.DefaultView;
                txtPaginationInfoTC.Text = $"Trang {currentPageTC}/{totalPagesTC}";

                // Cập nhật nút chuyển trang
                btnPrevTC.IsEnabled = (currentPageTC > 1);
                btnPrevTC.Opacity = (currentPageTC > 1) ? 1.0 : 0.45;

                btnNextTC.IsEnabled = (currentPageTC < totalPagesTC);
                btnNextTC.Opacity = (currentPageTC < totalPagesTC) ? 1.0 : 0.45;
            }
            catch { }
        }

        private void UpdatePaginationCS()
        {
            try
            {
                DataTable pageDt = bus.LayChinhSua(currentPageCS, pageSize);

                if (pageDt == null || pageDt.Rows.Count == 0)
                {
                    dgChinhSua.ItemsSource = null;
                    txtPaginationInfoCS.Text = "Trang 0/0";
                    txtChinhSuaCount.Text = "📝 Lịch sử chỉnh sửa (0 bản ghi)";
                    btnPrevCS.IsEnabled = false;
                    btnPrevCS.Opacity = 0.45;
                    btnNextCS.IsEnabled = false;
                    btnNextCS.Opacity = 0.45;
                    totalPagesCS = 1;
                    return;
                }

                int totalRows = Convert.ToInt32(pageDt.Rows[0]["TotalRecords"]);
                totalPagesCS = (int)Math.Ceiling((double)totalRows / pageSize);

                if (currentPageCS < 1) currentPageCS = 1;
                if (currentPageCS > totalPagesCS) currentPageCS = totalPagesCS;

                txtChinhSuaCount.Text = $"📝 Lịch sử chỉnh sửa ({totalRows} bản ghi)";
                dgChinhSua.ItemsSource = pageDt.DefaultView;
                txtPaginationInfoCS.Text = $"Trang {currentPageCS}/{totalPagesCS}";

                // Cập nhật nút chuyển trang
                btnPrevCS.IsEnabled = (currentPageCS > 1);
                btnPrevCS.Opacity = (currentPageCS > 1) ? 1.0 : 0.45;

                btnNextCS.IsEnabled = (currentPageCS < totalPagesCS);
                btnNextCS.Opacity = (currentPageCS < totalPagesCS) ? 1.0 : 0.45;
            }
            catch { }
        }

        private void btnPrevTC_Click(object sender, RoutedEventArgs e)
        {
            if (currentPageTC > 1)
            {
                currentPageTC--;
                UpdatePaginationTC();
            }
        }

        private void btnNextTC_Click(object sender, RoutedEventArgs e)
        {
            if (currentPageTC < totalPagesTC)
            {
                currentPageTC++;
                UpdatePaginationTC();
            }
        }

        private void btnPrevCS_Click(object sender, RoutedEventArgs e)
        {
            if (currentPageCS > 1)
            {
                currentPageCS--;
                UpdatePaginationCS();
            }
        }

        private void btnNextCS_Click(object sender, RoutedEventArgs e)
        {
            if (currentPageCS < totalPagesCS)
            {
                currentPageCS++;
                UpdatePaginationCS();
            }
        }

        private void btnXoaTruyCap_Click(object sender, RoutedEventArgs e)
        {
            ShowConfirmDialog("Xóa toàn bộ lịch sử truy cập?", "Xác nhận", () =>
            { 
                try { bus.XoaTruyCap(); LoadData(); } 
                catch (Exception ex) { ShowDialogMessage(ex.Message, "Lỗi hệ thống"); } 
            });
        }

        private void btnXoaChinhSua_Click(object sender, RoutedEventArgs e)
        {
            ShowConfirmDialog("Xóa toàn bộ lịch sử chỉnh sửa?", "Xác nhận", () =>
            { 
                try { bus.XoaChinhSua(); LoadData(); } 
                catch (Exception ex) { ShowDialogMessage(ex.Message, "Lỗi hệ thống"); } 
            });
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
