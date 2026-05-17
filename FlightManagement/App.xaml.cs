using System.Threading;
using System.Windows;

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
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_mutex != null) { _mutex.ReleaseMutex(); _mutex.Dispose(); }
            base.OnExit(e);
        }
    }
}
