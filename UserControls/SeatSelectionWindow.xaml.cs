using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FlightManagement.UserControls
{
    public partial class SeatSelectionWindow : Window
    {
        public string SelectedSeat { get; private set; } = "";
        private Button? _currentBtn;
        
        public SeatSelectionWindow(int ecoSeats, int bizSeats, List<string> bookedSeats)
        {
            InitializeComponent();
            GenerateSeats(wpBusiness, bizSeats, "Business", bookedSeats);
            GenerateSeats(wpEconomy, ecoSeats, "Economy", bookedSeats);
        }

        private void GenerateSeats(WrapPanel wp, int total, string type, List<string> booked)
        {
            int cols = type == "Business" ? 4 : 6;
            int rows = total / cols + (total % cols > 0 ? 1 : 0);
            
            char[] rowLetters = { 'A', 'B', 'C', 'D', 'E', 'F' };
            int currentSeat = 1;
            
            for (int r = 1; r <= rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (currentSeat > total) break;
                    
                    string seatName = $"{r:D2}{rowLetters[c]}";
                    if (type == "Economy")
                    {
                        // To avoid overlapping row numbers with Business, add offset
                        int offset = 10; // example offset
                        seatName = $"{r + offset:D2}{rowLetters[c]}";
                    }
                    
                    Button btn = new Button
                    {
                        Content = seatName,
                        Width = 45,
                        Height = 45,
                        Margin = new Thickness(5),
                        Tag = seatName
                    };

                    if (type == "Business") 
                    {
                        btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8E1"));
                        btn.Foreground = Brushes.DarkOrange;
                        if (c == 2) btn.Margin = new Thickness(25, 5, 5, 5); // Aisle
                    }
                    else
                    {
                        btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
                        btn.Foreground = Brushes.DodgerBlue;
                        if (c == 3) btn.Margin = new Thickness(25, 5, 5, 5); // Aisle
                    }

                    if (booked.Contains(seatName))
                    {
                        btn.Background = Brushes.LightGray;
                        btn.Foreground = Brushes.Gray;
                        btn.IsEnabled = false;
                    }
                    else
                    {
                        btn.Click += Seat_Click;
                    }

                    wp.Children.Add(btn);
                    currentSeat++;
                }
            }
        }

        private void Seat_Click(object sender, RoutedEventArgs e)
        {
            if (_currentBtn != null)
            {
                _currentBtn.BorderBrush = null;
                _currentBtn.BorderThickness = new Thickness(0);
            }
            
            _currentBtn = sender as Button;
            if (_currentBtn != null)
            {
                _currentBtn.BorderBrush = Brushes.Red;
                _currentBtn.BorderThickness = new Thickness(3);
                SelectedSeat = _currentBtn.Tag.ToString()!;
            }
        }

        private void btnXacNhan_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedSeat))
            {
                MessageBox.Show("Vui lòng chọn một ghế!");
                return;
            }
            DialogResult = true;
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
