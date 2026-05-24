using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading.Tasks;
using BUS;
using FlightManagement.Services;

namespace FlightManagement.UserControls
{
    public partial class ucCSKH : UserControl
    {
        private readonly CSKHBUS cskhBus = new CSKHBUS();
        private readonly EmailNotificationService emailService = new EmailNotificationService();
        private readonly LichSuBUS lichSuBus = new();
        private int _userId;
        private MainWindow _mainWindow;

        // Pagination for Mail Queue
        private int currentPageMail = 1;
        private int pageSizeMail = 15;
        private int totalRecordsMail = 0;

        // Pagination for Feedback
        private int currentPageFb = 1;
        private int pageSizeFb = 15;
        private int totalRecordsFb = 0;

        public ucCSKH(MainWindow mainWindow, int userId)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _userId = userId;

            LoadMailQueue();
            LoadFeedback();
        }

        // ======================= TAB KHÁCH HÀNG (MAIL QUEUE) =======================

        private void LoadMailQueue()
        {
            if (dgMailQueue == null || cskhBus == null) return;
            
            try
            {
                string statusFilter = null;
                if (cboStatusFilter != null && cboStatusFilter.SelectedItem is ComboBoxItem item && item.Content.ToString() != "Tất cả")
                {
                    statusFilter = item.Content.ToString();
                }

                DateTime? flightDate = dpFlightDateFilter?.SelectedDate;

                DataTable dt = cskhBus.LayMailQueue(currentPageMail, pageSizeMail, statusFilter, flightDate);
                
                // Add a custom column for Status Color bindings
                dt.Columns.Add("StatusColor", typeof(SolidColorBrush));
                foreach (DataRow row in dt.Rows)
                {
                    string status = row["Status"].ToString();
                    if (status == "Đã gửi") row["StatusColor"] = new SolidColorBrush(Colors.Green);
                    else if (status == "Lỗi") row["StatusColor"] = new SolidColorBrush(Colors.Red);
                    else row["StatusColor"] = new SolidColorBrush(Colors.Gray);
                }

                if (dt.Rows.Count > 0)
                {
                    totalRecordsMail = Convert.ToInt32(dt.Rows[0]["TotalRecords"]);
                }
                else
                {
                    totalRecordsMail = 0;
                }

                dgMailQueue.ItemsSource = dt.DefaultView;
                UpdatePaginationMail();
            }
            catch (Exception ex)
            {
                ShowDialogMessage("Lỗi tải danh sách Mail: " + ex.Message, "Lỗi hệ thống");
            }
        }

        private void UpdatePaginationMail()
        {
            int totalPages = (int)Math.Ceiling((double)totalRecordsMail / pageSizeMail);
            if (totalPages == 0) totalPages = 1;
            txtPageInfoMail.Text = $"Trang {currentPageMail} / {totalPages}";
            btnPrevMail.IsEnabled = currentPageMail > 1;
            btnNextMail.IsEnabled = currentPageMail < totalPages;
        }

        private void btnPrevMail_Click(object sender, RoutedEventArgs e) { if (currentPageMail > 1) { currentPageMail--; LoadMailQueue(); } }
        private void btnNextMail_Click(object sender, RoutedEventArgs e) { int totalPages = (int)Math.Ceiling((double)totalRecordsMail / pageSizeMail); if (currentPageMail < totalPages) { currentPageMail++; LoadMailQueue(); } }
        
        private void cboStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentPageMail = 1;
            LoadMailQueue();
        }

        private void dpFlightDateFilter_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            currentPageMail = 1;
            LoadMailQueue();
        }

        private void btnRefreshMail_Click(object sender, RoutedEventArgs e)
        {
            // Remove event handlers temporarily to avoid multiple DB calls
            cboStatusFilter.SelectionChanged -= cboStatusFilter_SelectionChanged;
            dpFlightDateFilter.SelectedDateChanged -= dpFlightDateFilter_SelectedDateChanged;

            cboStatusFilter.SelectedIndex = 0;
            dpFlightDateFilter.SelectedDate = null;
            currentPageMail = 1;

            LoadMailQueue();

            cboStatusFilter.SelectionChanged += cboStatusFilter_SelectionChanged;
            dpFlightDateFilter.SelectedDateChanged += dpFlightDateFilter_SelectedDateChanged;
        }

        private void ShowDialogMessage(string message, string title = "Thông báo")
        {
            var view = new StackPanel { Margin = new Thickness(25), MinWidth = 350 };
            
            var titleBlock = new TextBlock 
            { 
                Text = title, 
                FontWeight = FontWeights.Bold, 
                FontSize = 18, 
                Margin = new Thickness(0, 0, 0, 15),
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51))
            };
            view.Children.Add(titleBlock);

            var contentBlock = new TextBlock 
            { 
                Text = message,
                FontSize = 15,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 25),
                Foreground = new SolidColorBrush(Color.FromRgb(85, 85, 85))
            };
            view.Children.Add(contentBlock);

            var btnOk = new Button 
            { 
                Content = "XÁC NHẬN", 
                HorizontalAlignment = HorizontalAlignment.Right,
                Style = (Style)FindResource("MaterialDesignFlatButton"),
                Foreground = new SolidColorBrush(Color.FromRgb(57, 73, 171))
            };
            btnOk.Click += (s, ev) => cskhDialogHost.IsOpen = false;
            view.Children.Add(btnOk);

            cskhDialogHost.DialogContent = view;
            cskhDialogHost.IsOpen = true;
        }

        private async void btnSendSelectedEmail_Click(object sender, RoutedEventArgs e)
        {
            System.Collections.Generic.List<DataRowView> selectedRows = new System.Collections.Generic.List<DataRowView>();
            
            // Xử lý khi người dùng chỉ chọn một vài ô (Cell Selection)
            foreach (var cellInfo in dgMailQueue.SelectedCells)
            {
                if (cellInfo.Item is DataRowView rowView && !selectedRows.Contains(rowView))
                {
                    selectedRows.Add(rowView);
                }
            }

            // Xử lý fallback cho trường hợp chọn nguyên dòng
            if (selectedRows.Count == 0)
            {
                foreach (var item in dgMailQueue.SelectedItems)
                {
                    if (item is DataRowView rowView && !selectedRows.Contains(rowView))
                    {
                        selectedRows.Add(rowView);
                    }
                }
            }

            if (selectedRows.Count == 0)
            {
                ShowDialogMessage("Vui lòng chọn ít nhất một vé trong danh sách để gửi mail!", "Thông báo");
                return;
            }

            if (!emailService.IsConfigured)
            {
                ShowDialogMessage("Dịch vụ SMTP chưa được cấu hình. Vui lòng kiểm tra lại!", "Lỗi");
                return;
            }

            btnSendSelectedEmail.IsEnabled = false;
            
            // Hiện Loading Dialog
            var loadingView = new StackPanel { Margin = new Thickness(40), HorizontalAlignment = HorizontalAlignment.Center };
            loadingView.Children.Add(new ProgressBar 
            { 
                Style = (Style)FindResource("MaterialDesignCircularProgressBar"), 
                Value = 0, 
                IsIndeterminate = true, 
                Width = 50, Height = 50, 
                Margin = new Thickness(0, 0, 0, 20),
                Foreground = new SolidColorBrush(Color.FromRgb(57, 73, 171))
            });
            loadingView.Children.Add(new TextBlock 
            { 
                Text = "Đang gửi mail xác nhận...", 
                FontSize = 16, 
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(85, 85, 85)),
                HorizontalAlignment = HorizontalAlignment.Center
            });

            cskhDialogHost.DialogContent = loadingView;
            cskhDialogHost.IsOpen = true;

            // Chờ một chút để UI render mượt mà
            await Task.Delay(400);

            int successCount = 0;
            int errorCount = 0;

            foreach (DataRowView rowView in selectedRows)
            {
                DataRow row = rowView.Row;
                int queueId = Convert.ToInt32(row["ID"]);
                string status = row["Status"].ToString();
                
                if (status == "Đã gửi") continue; // Bỏ qua nếu đã gửi

                string email = row["Email"].ToString();
                string hoTen = row["TenKhach"].ToString();
                string bookingRef = row["BookingReference"].ToString();
                string soHieu = row["FlightNumber"].ToString();
                string tuyenBay = row["TuyenBay"].ToString();
                string ngayBay = Convert.ToDateTime(row["NgayBay"]).ToString("dd/MM/yyyy");
                string gioBay = row["GioBay"].ToString();
                string seatNumber = row["SeatNumber"].ToString();

                if (string.IsNullOrWhiteSpace(email))
                {
                    cskhBus.CapNhatTrangThaiMail(queueId, "Lỗi", "Khách hàng không có Email");
                    errorCount++;
                    continue;
                }

                // Gửi mail ngầm
                bool isSuccess = false;
                string errMsg = "";
                await Task.Run(() =>
                {
                    try
                    {
                        emailService.SendBookingConfirmation(email, hoTen.Trim(), bookingRef, soHieu, tuyenBay, ngayBay, gioBay, seatNumber);
                        isSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        errMsg = ex.Message;
                    }
                });

                if (isSuccess)
                {
                    cskhBus.CapNhatTrangThaiMail(queueId, "Đã gửi");
                    successCount++;
                }
                else
                {
                    cskhBus.CapNhatTrangThaiMail(queueId, "Lỗi", errMsg);
                    errorCount++;
                }
            }

            cskhDialogHost.IsOpen = false;
            btnSendSelectedEmail.IsEnabled = true;

            // Ghi nhận lịch sử chỉnh sửa hệ thống
            if (successCount > 0)
            {
                lichSuBus.GhiNhanChinhSua(_userId, "Gửi Email", "CSKH", $"Gửi thành công {successCount} email xác nhận đặt vé");
            }

            ShowDialogMessage($"Đã xử lý xong.\nThành công: {successCount}\nThất bại: {errorCount}", "Kết quả gửi mail");
            LoadMailQueue();
        }

        private void btnSendCustomEmail_Click(object sender, RoutedEventArgs e)
        {
            ShowDialogMessage("Tính năng Gửi Mail Tùy Chỉnh đang được phát triển.\nSắp tới nhân viên có thể tự do soạn Nội dung, Khuyến mãi và đính kèm file cho tệp khách hàng đã chọn!", "Tính năng mở rộng");
        }


        // ======================= TAB FEEDBACK =======================

        private void LoadFeedback()
        {
            try
            {
                DataTable dt = cskhBus.LayFeedback(currentPageFb, pageSizeFb);
                if (dt.Rows.Count > 0)
                {
                    totalRecordsFb = Convert.ToInt32(dt.Rows[0]["TotalRecords"]);
                }
                else
                {
                    totalRecordsFb = 0;
                }

                dgFeedback.ItemsSource = dt.DefaultView;
                UpdatePaginationFb();
            }
            catch (Exception ex)
            {
                ShowDialogMessage("Lỗi tải danh sách Feedback: " + ex.Message, "Lỗi");
            }
        }

        private void UpdatePaginationFb()
        {
            int totalPages = (int)Math.Ceiling((double)totalRecordsFb / pageSizeFb);
            if (totalPages == 0) totalPages = 1;
            txtPageInfoFb.Text = $"Trang {currentPageFb} / {totalPages}";
            btnPrevFb.IsEnabled = currentPageFb > 1;
            btnNextFb.IsEnabled = currentPageFb < totalPages;
        }

        private void btnPrevFb_Click(object sender, RoutedEventArgs e) { if (currentPageFb > 1) { currentPageFb--; LoadFeedback(); } }
        private void btnNextFb_Click(object sender, RoutedEventArgs e) { int totalPages = (int)Math.Ceiling((double)totalRecordsFb / pageSizeFb); if (currentPageFb < totalPages) { currentPageFb++; LoadFeedback(); } }

        private void cboFbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pnlCustomCategory == null) return;
            if (cboFbCategory.SelectedItem is ComboBoxItem item && item.Content.ToString() == "Khác")
            {
                pnlCustomCategory.Visibility = Visibility.Visible;
            }
            else
            {
                pnlCustomCategory.Visibility = Visibility.Collapsed;
            }
        }

        private void btnSaveFeedback_Click(object sender, RoutedEventArgs e)
        {
            string name = txtFbName.Text.Trim();
            string phone = txtFbPhone.Text.Trim();
            string email = txtFbEmail.Text.Trim();
            string content = txtFbContent.Text.Trim();
            
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(content))
            {
                ShowDialogMessage("Vui lòng điền đủ Tên hành khách và Nội dung đánh giá!", "Thiếu thông tin");
                return;
            }

            int rating = 5;
            if (cboFbRating.SelectedIndex >= 0)
            {
                // Index 0: 5 Sao, Index 1: 4 Sao...
                rating = 5 - cboFbRating.SelectedIndex;
            }

            string category = "Khác";
            if (cboFbCategory.SelectedItem is ComboBoxItem item)
            {
                category = item.Content.ToString();
            }

            if (category == "Khác")
            {
                string customCat = txtCustomCategory.Text.Trim();
                if (string.IsNullOrEmpty(customCat))
                {
                    ShowDialogMessage("Vui lòng nhập tên Hạng mục tùy chỉnh!", "Thiếu thông tin");
                    return;
                }
                category = customCat;
            }

            try
            {
                bool success = cskhBus.ThemFeedback(name, phone, email, rating, category, content, _userId);
                if (success)
                {
                    ShowDialogMessage("Đã ghi nhận Feedback thành công!", "Thành công");
                    
                    // Ghi nhận lịch sử chỉnh sửa hệ thống
                    lichSuBus.GhiNhanChinhSua(_userId, "Thêm Feedback", "CSKH", $"Ghi nhận đánh giá từ hành khách {name} - Hạng mục: {category}");
                    
                    // Reset form
                    txtFbName.Text = "";
                    txtFbPhone.Text = "";
                    txtFbEmail.Text = "";
                    txtFbContent.Text = "";
                    txtCustomCategory.Text = "";
                    cboFbRating.SelectedIndex = 0;
                    cboFbCategory.SelectedIndex = 0;

                    // Reload
                    currentPageFb = 1;
                    LoadFeedback();
                }
            }
            catch (Exception ex)
            {
                ShowDialogMessage("Lỗi lưu Feedback: " + ex.Message, "Lỗi");
            }
        }
    }
}
