using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace FlightManagement.UserControls
{
    public partial class SeatSelectionWindow : Window
    {
        public string SelectedSeat { get; private set; } = "";
        public int SelectedCabinId { get; private set; } = 1;
        private Button? _currentBtn;
        
        public SeatSelectionWindow(int ecoSeats, int bizSeats, int firstSeats, List<string> bookedSeats, string passengerName = "Hành khách", string route = "SGN - HAN")
        {
            InitializeComponent();
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null && mainWindow.IsVisible)
            {
                this.Owner = mainWindow;
            }

            txtPassengerName.Text = passengerName;
            txtRoute.Text = route;

            if (firstSeats > 0)
            {
                sectionFirstClass.Visibility = Visibility.Visible;
                GenerateSeats(wpFirstClass, firstSeats, "First Class", bookedSeats, 1);
            }
            GenerateSeats(wpBusiness, bizSeats, "Business", bookedSeats, firstSeats > 0 ? (firstSeats / 6 + 2) : 1);
            GenerateSeats(wpEconomy, ecoSeats, "Economy", bookedSeats, (bizSeats + firstSeats) / 6 + 4);
        }

        private void GenerateSeats(WrapPanel wp, int total, string type, List<string> booked, int startRow)
        {
            int cols = 6;
            int rows = total / cols + (total % cols > 0 ? 1 : 0);
            char[] rowLetters = { 'A', 'B', 'C', 'D', 'E', 'F' };
            int currentSeatCount = 0;
            
            for (int r = 0; r < rows; r++)
            {
                int rowNum = startRow + r;
                Grid rowGrid = new Grid { Margin = new Thickness(0, 5, 0, 5), Width = 380 };
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                UniformGrid leftGrid = new UniformGrid { Columns = 3 };
                UniformGrid rightGrid = new UniformGrid { Columns = 3 };

                TextBlock txtRow = new TextBlock 
                { 
                    Text = rowNum.ToString(), 
                    VerticalAlignment = VerticalAlignment.Center, 
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = Brushes.Gray,
                    FontSize = 12,
                    FontWeight = FontWeights.Bold
                };
                Grid.SetColumn(txtRow, 1);
                rowGrid.Children.Add(txtRow);

                for (int c = 0; c < 6; c++)
                {
                    if (currentSeatCount >= total) break;

                    string seatName = $"{rowNum:D2}{rowLetters[c]}";
                    Button btn = new Button
                    {
                        Content = seatName,
                        Style = (Style)FindResource("SeatButtonStyle"),
                        Tag = new SeatInfo { Name = seatName, Type = type },
                        FontSize = 10,
                        FontWeight = FontWeights.Bold
                    };

                    if (type == "First Class")
                        btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD700"));
                    else if (type == "Business")
                        btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E53935"));
                    else
                        btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1976D2"));

                    btn.Foreground = Brushes.White;

                    if (booked.Contains(seatName))
                    {
                        btn.IsEnabled = false;
                    }
                    else
                    {
                        btn.Click += Seat_Click;
                    }

                    if (c < 3) leftGrid.Children.Add(btn);
                    else rightGrid.Children.Add(btn);
                    
                    currentSeatCount++;
                }

                Grid.SetColumn(leftGrid, 0);
                Grid.SetColumn(rightGrid, 2);
                rowGrid.Children.Add(leftGrid);
                rowGrid.Children.Add(rightGrid);
                
                wp.Children.Add(rowGrid);
            }
        }

        private void Seat_Click(object sender, RoutedEventArgs e)
        {
            if (_currentBtn != null)
            {
                if (_currentBtn.Tag is SeatInfo oldTag)
                {
                    if (oldTag.Type == "First Class")
                        _currentBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD700"));
                    else if (oldTag.Type == "Business")
                        _currentBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E53935"));
                    else
                        _currentBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1976D2"));
                }
                _currentBtn.BorderThickness = new Thickness(0);
            }
            
            _currentBtn = sender as Button;
            if (_currentBtn != null)
            {
                _currentBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0D2A5C"));
                _currentBtn.BorderBrush = Brushes.White;
                _currentBtn.BorderThickness = new Thickness(2);
                
                if (_currentBtn.Tag is SeatInfo tag)
                {
                    SelectedSeat = tag.Name;
                    if (tag.Type == "First Class") SelectedCabinId = 3;
                    else if (tag.Type == "Business") SelectedCabinId = 2;
                    else SelectedCabinId = 1;
                }
            }
        }

        private class SeatInfo
        {
            public string Name { get; set; } = "";
            public string Type { get; set; } = "";
        }

        private void btnXacNhan_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedSeat))
            {
                ShowDialogMessage("Vui lòng chọn một ghế!", "Chưa chọn ghế");
                return;
            }
            DialogResult = true;
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
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
