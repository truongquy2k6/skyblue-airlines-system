using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FlightManagement
{
    public partial class App : Application
    {
        private static Mutex? _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "SkyBlue_FlightManagement_Mutex";
            bool createdNew;
            _mutex = new Mutex(true, appName, out createdNew);
            if (!createdNew)
            {
                MessageBox.Show("Ứng dụng SkyBlue Airlines đang chạy!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                Current.Shutdown();
                return;
            }

            // Đăng ký bộ xử lý sự kiện lăn chuột toàn cục cho toàn bộ các DataGrid trong ứng dụng
            EventManager.RegisterClassHandler(
                typeof(DataGrid), 
                UIElement.PreviewMouseWheelEvent, 
                new MouseWheelEventHandler(DataGrid_PreviewMouseWheel), 
                true);

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_mutex != null) { _mutex.ReleaseMutex(); _mutex.Dispose(); }
            base.OnExit(e);
        }

        // Bộ xử lý sự kiện lăn chuột toàn cục cho DataGrid
        private static void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Handled) return;

            if (sender is DataGrid dg)
            {
                // Tìm ScrollViewer bên trong cấu trúc trực quan (Visual Tree) của DataGrid
                var scrollViewer = FindVisualChild<ScrollViewer>(dg);
                
                // Nếu DataGrid không có thanh cuộn hoặc không cần cuộn dọc (tất cả dòng đều hiển thị vừa vặn)
                if (scrollViewer == null || scrollViewer.ScrollableHeight == 0)
                {
                    e.Handled = true;
                    
                    // Tìm ScrollViewer cha bên ngoài DataGrid
                    var parentScrollViewer = FindParent<ScrollViewer>(dg);
                    if (parentScrollViewer != null)
                    {
                        // Cuộn trực tiếp bằng cách dịch chuyển offset (120 delta tương đương cuộn 48 pixel)
                        double newOffset = parentScrollViewer.VerticalOffset - (e.Delta * 0.4);
                        parentScrollViewer.ScrollToVerticalOffset(newOffset);
                    }
                }
                else
                {
                    // Nếu DataGrid có thanh cuộn riêng đang hoạt động:
                    // Chỉ cho phép cuộn trang cha khi người dùng đã cuộn kịch khung của DataGrid (lên đỉnh hoặc xuống đáy)
                    double offset = scrollViewer.VerticalOffset;
                    if ((e.Delta > 0 && offset == 0) || (e.Delta < 0 && offset >= scrollViewer.ScrollableHeight))
                    {
                        e.Handled = true;
                        
                        var parentScrollViewer = FindParent<ScrollViewer>(dg);
                        if (parentScrollViewer != null)
                        {
                            double newOffset = parentScrollViewer.VerticalOffset - (e.Delta * 0.4);
                            parentScrollViewer.ScrollToVerticalOffset(newOffset);
                        }
                    }
                }
            }
        }

        // Hàm hỗ trợ duyệt Visual Tree ngược lên trên để tìm đối tượng cha phù hợp
        private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            
            if (parentObject is T parent)
                return parent;
                
            return FindParent<T>(parentObject);
        }

        // Hàm hỗ trợ duyệt Visual Tree để tìm đối tượng con phù hợp
        private static T? FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T t)
                    return t;
                
                T? childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }
    }
}
