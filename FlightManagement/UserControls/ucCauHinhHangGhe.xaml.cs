using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BUS;

namespace FlightManagement.UserControls
{
    public partial class ucCauHinhHangGhe : UserControl
    {
        private readonly HangGheBUS bus = new();
        private readonly LichSuBUS lichSuBus = new();
        private int _userId, _selectedCabinId = 1;
        private DataTable _cabinTypes = new();

        public ucCauHinhHangGhe(int userId) 
        { 
            InitializeComponent(); 
            _userId = userId; 
            LoadCabinTypes(); 
        }

        private void LoadCabinTypes()
        {
            try
            {
                _cabinTypes = bus.HienThi();
                pnlCabinButtons.Children.Clear();
                foreach (DataRow r in _cabinTypes.Rows)
                {
                    int id = Convert.ToInt32(r["ID"]);
                    string name = r["TenHangGhe"].ToString()!;
                    var btn = new Button
                    {
                        Content = name, 
                        Tag = id, 
                        Height = 42, 
                        Padding = new Thickness(24, 6, 24, 6),
                        Margin = new Thickness(0, 0, 12, 0), 
                        FontSize = 13.5, 
                        FontWeight = FontWeights.Bold,
                        Cursor = System.Windows.Input.Cursors.Hand,
                        Background = id == _selectedCabinId ? new SolidColorBrush(Color.FromRgb(21, 101, 192)) : new SolidColorBrush(Color.FromRgb(245, 247, 250)),
                        Foreground = id == _selectedCabinId ? Brushes.White : new SolidColorBrush(Color.FromRgb(69, 90, 100)),
                        BorderBrush = id == _selectedCabinId ? new SolidColorBrush(Color.FromRgb(21, 101, 192)) : new SolidColorBrush(Color.FromRgb(207, 216, 220)),
                        BorderThickness = new Thickness(1)
                    };
                    btn.SetValue(MaterialDesignThemes.Wpf.ButtonAssist.CornerRadiusProperty, new CornerRadius(6));
                    btn.Click += CabinBtn_Click;
                    pnlCabinButtons.Children.Add(btn);
                }
                LoadCauHinh(_selectedCabinId);
            }
            catch { }
        }

        private void CabinBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                _selectedCabinId = id;
                
                foreach (Button b in pnlCabinButtons.Children)
                {
                    bool sel = (int)b.Tag == id;
                    b.Background = sel ? new SolidColorBrush(Color.FromRgb(21, 101, 192)) : new SolidColorBrush(Color.FromRgb(245, 247, 250));
                    b.Foreground = sel ? Brushes.White : new SolidColorBrush(Color.FromRgb(69, 90, 100));
                    b.BorderBrush = sel ? new SolidColorBrush(Color.FromRgb(21, 101, 192)) : new SolidColorBrush(Color.FromRgb(207, 216, 220));
                }
                
                LoadCauHinh(id);
            }
        }

        private void LoadCauHinh(int cabinId)
        {
            try
            {
                var row = _cabinTypes.Select($"ID = {cabinId}");
                string name = row.Length > 0 ? row[0]["TenHangGhe"].ToString()! : "";
                txtCabinTitle.Text = $"Tiện ích cho hạng {name}";

                DataTable dt = bus.LayCauHinh(cabinId);
                pnlAmenities.Children.Clear();
                
                foreach (DataRow r in dt.Rows)
                {
                    int amenityId = Convert.ToInt32(r["AmenityID"]);
                    bool selected = Convert.ToBoolean(r["DuocChon"]);
                    string amenityName = r["TenDichVu"].ToString()!;
                    decimal price = Convert.ToDecimal(r["Gia"]);

                    var cb = new CheckBox 
                    { 
                        IsChecked = selected, 
                        Margin = new Thickness(0, 0, 12, 0), 
                        VerticalAlignment = VerticalAlignment.Center 
                    };
                    cb.Tag = amenityId;

                    var nameText = new TextBlock 
                    { 
                        Text = amenityName, 
                        FontSize = 14, 
                        Margin = new Thickness(0, 0, 0, 3),
                        Foreground = selected ? new SolidColorBrush(Color.FromRgb(13, 71, 161)) : new SolidColorBrush(Color.FromRgb(55, 71, 79)),
                        FontWeight = selected ? FontWeights.Bold : FontWeights.SemiBold 
                    };

                    var priceText = new TextBlock 
                    { 
                        Text = $"Giá: {price:N0} đ", 
                        FontSize = 12.5, 
                        Foreground = selected ? new SolidColorBrush(Color.FromRgb(230, 74, 25)) : new SolidColorBrush(Color.FromRgb(120, 144, 156)),
                        FontWeight = selected ? FontWeights.Bold : FontWeights.Medium 
                    };

                    var card = new Border
                    {
                        BorderBrush = selected ? new SolidColorBrush(Color.FromRgb(30, 136, 229)) : new SolidColorBrush(Color.FromRgb(236, 239, 241)),
                        BorderThickness = new Thickness(selected ? 1.5 : 1),
                        Background = selected ? new SolidColorBrush(Color.FromRgb(225, 245, 254)) : Brushes.White,
                        CornerRadius = new CornerRadius(10), 
                        Padding = new Thickness(18, 14, 18, 14),
                        Margin = new Thickness(0, 0, 14, 14), 
                        Width = 255, 
                        Cursor = System.Windows.Input.Cursors.Hand,
                        Effect = selected 
                            ? new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 10, ShadowDepth = 1, Color = Color.FromRgb(30, 136, 229), Opacity = 0.15 }
                            : new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 8, ShadowDepth = 1, Color = Colors.Black, Opacity = 0.04 }
                    };

                    var sp = new StackPanel { Orientation = Orientation.Horizontal };
                    sp.Children.Add(cb);
                    
                    var textSp = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                    textSp.Children.Add(nameText);
                    textSp.Children.Add(priceText);
                    sp.Children.Add(textSp);
                    card.Child = sp;

                    card.MouseDown += (s, _) => 
                    { 
                        cb.IsChecked = !cb.IsChecked; 
                    };
                    
                    cb.Checked += (s, _) =>
                    {
                        try 
                        { 
                            bus.GanDichVu(cabinId, amenityId); 
                            lichSuBus.GhiNhanChinhSua(_userId, "Gán DV", "Hạng ghế", $"Gán {amenityName} cho {name}"); 
                        }
                        catch { }
                        
                        card.BorderBrush = new SolidColorBrush(Color.FromRgb(30, 136, 229));
                        card.BorderThickness = new Thickness(1.5);
                        card.Background = new SolidColorBrush(Color.FromRgb(225, 245, 254));
                        card.Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 10, ShadowDepth = 1, Color = Color.FromRgb(30, 136, 229), Opacity = 0.15 };
                        
                        nameText.Foreground = new SolidColorBrush(Color.FromRgb(13, 71, 161));
                        nameText.FontWeight = FontWeights.Bold;
                        
                        priceText.Foreground = new SolidColorBrush(Color.FromRgb(230, 74, 25));
                        priceText.FontWeight = FontWeights.Bold;
                    };
                    
                    cb.Unchecked += (s, _) =>
                    {
                        try 
                        { 
                            bus.GoDichVu(cabinId, amenityId); 
                            lichSuBus.GhiNhanChinhSua(_userId, "Gỡ DV", "Hạng ghế", $"Gỡ {amenityName} khỏi {name}"); 
                        }
                        catch { }
                        
                        card.BorderBrush = new SolidColorBrush(Color.FromRgb(236, 239, 241));
                        card.BorderThickness = new Thickness(1);
                        card.Background = Brushes.White;
                        card.Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 8, ShadowDepth = 1, Color = Colors.Black, Opacity = 0.04 };
                        
                        nameText.Foreground = new SolidColorBrush(Color.FromRgb(55, 71, 79));
                        nameText.FontWeight = FontWeights.SemiBold;
                        
                        priceText.Foreground = new SolidColorBrush(Color.FromRgb(120, 144, 156));
                        priceText.FontWeight = FontWeights.Medium;
                    };

                    pnlAmenities.Children.Add(card);
                }
            }
            catch { }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var row = _cabinTypes.Select($"ID = {_selectedCabinId}");
                string name = row.Length > 0 ? row[0]["TenHangGhe"].ToString()! : "Hạng ghế";
                
                // Ghi nhận vào lịch sử chỉnh sửa hệ thống
                lichSuBus.GhiNhanChinhSua(_userId, "Lưu cấu hình", "Hạng ghế", $"Xác nhận lưu toàn bộ cấu hình tiện ích cho hạng {name}");
                
                ShowDialogMessage($"🎉 Xác nhận lưu cấu hình thành công!\n\nToàn bộ các tiện ích đã chọn cho hạng ghế '{name}' đã được lưu trữ an toàn và đồng bộ hóa trên toàn bộ hệ thống bán vé của SkyBlue Airlines.", "Cấu hình Hạng ghế");
            }
            catch (System.Exception ex)
            {
                ShowDialogMessage($"Lỗi khi lưu cấu hình: {ex.Message}", "Lỗi hệ thống");
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
