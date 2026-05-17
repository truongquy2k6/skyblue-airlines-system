using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FlightManagement.UserControls
{
    public partial class TicketWindow : Window
    {
        public TicketWindow(string tenKhach, string tuyenBay, string soHieu, string ngayBay, string gioBay, string ghe, string bookingRef, string hangGhe)
        {
            InitializeComponent();
            
            txtTenKhach.Text = tenKhach.ToUpper();
            txtTuyenBay.Text = tuyenBay.ToUpper();
            txtChuyenBay.Text = soHieu.ToUpper();
            txtNgayBay.Text = ngayBay.ToUpper();
            txtGioBay.Text = gioBay;
            txtGhe.Text = string.IsNullOrEmpty(ghe) ? "CHƯA CHỌN" : ghe;
            txtBookingRef.Text = bookingRef.ToUpper();

            txtTenKhachStub.Text = tenKhach.ToUpper();
            txtChuyenBayStub.Text = soHieu.ToUpper();
            txtGheStub.Text = string.IsNullOrEmpty(ghe) ? "CHƯA CHỌN" : ghe;
            txtHangGhe.Text = "HẠNG: " + hangGhe.ToUpper();
            
            // Tách lộ trình (nếu có dấu mũi tên Unicode, gạch ngang, hoặc ký hiệu mũi tên)
            string[] separators = new string[] { "→", "->", "-" };
            string foundSeparator = "";
            foreach (var sep in separators)
            {
                if (tuyenBay.Contains(sep))
                {
                    foundSeparator = sep;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(foundSeparator))
            {
                var parts = tuyenBay.Split(new string[] { foundSeparator }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    txtFrom.Text = parts[0].Trim().ToUpper();
                    txtTo.Text = parts[1].Trim().ToUpper();
                    txtFromStub.Text = parts[0].Trim().ToUpper();
                    txtToStub.Text = parts[1].Trim().ToUpper();
                }
                else
                {
                    txtFrom.Text = tuyenBay.ToUpper();
                    txtTo.Text = "-";
                    txtFromStub.Text = tuyenBay.ToUpper();
                    txtToStub.Text = "-";
                }
            }
            else
            {
                txtFrom.Text = tuyenBay.ToUpper();
                txtTo.Text = "-";
                txtFromStub.Text = tuyenBay.ToUpper();
                txtToStub.Text = "-";
            }
            
            txtNgayBayStub.Text = ngayBay.ToUpper();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Tự động lấy đường dẫn thư mục "Documents" của người dùng để lưu vé
                string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
                string folderPath = System.IO.Path.Combine(documentsPath, "SkyBlueTickets");
                
                // Kiểm tra và tạo thư mục nếu chưa tồn tại
                if (!System.IO.Directory.Exists(folderPath))
                {
                    System.IO.Directory.CreateDirectory(folderPath);
                }

                // Tạo tên file tự động: Mã vé + Tên khách hàng (Bỏ dấu cách)
                string safeName = txtTenKhach.Text.Replace(" ", "_");
                string fileName = $"{txtBookingRef.Text}_{safeName}.png";
                string fullPath = System.IO.Path.Combine(folderPath, fileName);

                // Chụp ảnh giao diện vé (High Quality - 300 DPI)
                double dpi = 300;
                double scale = dpi / 96;
                
                // Lấy kích thước thực tế của vùng in vé
                Size size = new Size(printArea.ActualWidth, printArea.ActualHeight);
                printArea.Measure(size);
                printArea.Arrange(new Rect(size));

                RenderTargetBitmap rtb = new RenderTargetBitmap(
                    (int)(size.Width * scale), 
                    (int)(size.Height * scale), 
                    dpi, dpi, System.Windows.Media.PixelFormats.Pbgra32);

                rtb.Render(printArea);

                // Lưu ảnh dưới định dạng PNG (Vì xuất PDF tự động không cần hộp thoại yêu cầu thư viện bên thứ 3)
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));

                using (var stream = System.IO.File.Create(fullPath))
                {
                    encoder.Save(stream);
                }

                ShowDialogMessage($"Vé đã được xuất tự động vào thư mục:\n{fullPath}", "Xuất vé thành công");
                
                // Sau khi xuất xong thì tự động đóng cửa sổ vé
                this.Close();
            }
            catch (System.Exception ex)
            {
                ShowDialogMessage("Có lỗi khi xuất vé: " + ex.Message, "Lỗi");
            }
        }

        private void btnExportPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Sử dụng VisualBrush để "chụp" giao diện vé và in ra bản tạm
                    // Cách này giúp không làm hỏng giao diện đang hiển thị và căn chỉnh trang in tốt hơn
                    VisualBrush visualBrush = new VisualBrush(printArea);
                    System.Windows.Shapes.Rectangle printRect = new System.Windows.Shapes.Rectangle
                    {
                        Width = printArea.ActualWidth,
                        Height = printArea.ActualHeight,
                        Fill = visualBrush
                    };

                    // Tính toán tỷ lệ co dãn để vừa khít trang giấy
                    double margin = 20;
                    double printableWidth = printDialog.PrintableAreaWidth - margin;
                    double printableHeight = printDialog.PrintableAreaHeight - margin;

                    double scale = Math.Min(printableWidth / printRect.Width, printableHeight / printRect.Height);
                    printRect.LayoutTransform = new ScaleTransform(scale, scale);

                    // Đo đạc và sắp xếp lại để in
                    Size printSize = new Size(printableWidth, printableHeight);
                    printRect.Measure(printSize);
                    printRect.Arrange(new Rect(margin / 2, margin / 2, printableWidth, printableHeight));

                    // Thực hiện in - Khử sạch dấu tiếng Việt để Windows Print Spooler không bị lỗi đặt tên file PDF tự động
                    string cleanName = RemoveSign4VietnameseString(txtTenKhach.Text);
                    string safeName = System.Text.RegularExpressions.Regex.Replace(cleanName, @"[^a-zA-Z0-9_]", "_").Replace("___", "_").Replace("__", "_").Trim('_');
                    string jobName = $"{txtBookingRef.Text}_{safeName}";
                    
                    printDialog.PrintVisual(printRect, jobName);

                    ShowDialogMessage("Yêu cầu in vé PDF đã được gửi. Lưu ý: Hãy chọn 'Landscape' trong hộp thoại in nếu vé bị dọc.", "Thông báo");
                    this.Close();
                }
            }
            catch (System.Exception ex)
            {
                ShowDialogMessage("Có lỗi khi in vé: " + ex.Message, "Lỗi");
            }
        }

        private string RemoveSign4VietnameseString(string str)
        {
            string[] signs = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };
            for (int i = 1; i < signs.Length; i++)
            {
                for (int j = 0; j < signs[i].Length; j++)
                {
                    str = str.Replace(signs[i][j].ToString(), signs[0][i - 1].ToString());
                }
            }
            return str;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
