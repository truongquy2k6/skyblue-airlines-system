using System.Windows;
using System.Windows.Controls;

namespace FlightManagement.UserControls
{
    public partial class ucTongQuanVe : UserControl
    {
        public ucTongQuanVe(MainWindow mainWindow, int userId, int scheduleId = 0)
        {
            InitializeComponent();
            
            // Khởi tạo và nạp 2 UserControl con vào trong 2 Tab
            contentDanhSachVe.Content = new ucQuanLyVe(userId);
            contentDatVeMoi.Content = new ucDatVeMayBay(mainWindow, userId, scheduleId, this);
            
            // Nếu có scheduleId (chuyển từ màn hình tìm kiếm sang) thì tự động mở qua Tab Đặt vé mới
            if (scheduleId > 0)
            {
                tabDatVeMoi.IsSelected = true;
            }
        }

        public void ShowLoading(bool show)
        {
            pnlLoading.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                if (tabControl.SelectedIndex == 0) // Tab "Danh sách Vé"
                {
                    if (contentDanhSachVe.Content is ucQuanLyVe qlVe)
                    {
                        qlVe.LoadData();
                    }
                }
            }
        }
    }
}
