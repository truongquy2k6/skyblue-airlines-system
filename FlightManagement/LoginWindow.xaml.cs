using System.Data;
using System.Windows;
using System.Windows.Input;
using BUS;

namespace FlightManagement
{
    public partial class LoginWindow : Window
    {
        NhanVienBUS bus = new();

        public LoginWindow()
        {
            InitializeComponent();
            AutoUpdateDatabase();
        }

        private void AutoUpdateDatabase()
        {
            try
            {
                // SP đã được định nghĩa sẵn trong SkyBlue_StoredProcedures.sql
                // Chỉ cần gọi thực thi trực tiếp, không nhúng script SQL vào đây nữa
                using (var conn = DAL.DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new Microsoft.Data.SqlClient.SqlCommand("sp_LichBay_TaoTuDong7Ngay", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Nếu có lỗi (do rớt mạng hoặc sai quyền) thì ghi chú lại nhưng KHÔNG làm sập ứng dụng
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e) => XuLyDangNhap();

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) XuLyDangNhap();
        }

        private void XuLyDangNhap()
        {
            // Lấy email và mật khẩu từ giao diện, dùng Trim() để cắt bỏ khoảng trắng dư thừa ở hai đầu
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password;
            
            // Kiểm tra rỗng sơ bộ trước khi gửi yêu cầu lên mạng để đỡ tốn thời gian
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                txtError.Text = "Vui lòng nhập đầy đủ email và mật khẩu.";
                return;
            }

            try
            {
                // Gọi lớp BUS truyền dữ liệu xuống DAL để nhờ SQL Server kiểm tra
                // Nếu đúng nó sẽ trả về 1 bảng dữ liệu chứa thông tin người đó
                DataTable dt = bus.DangNhap(email, password);
                
                if (dt != null && dt.Rows.Count > 0)
                {
                    // Kiểm tra phòng ngừa lỗi nếu Stored Procedure dưới DB chưa được cập nhật cột Active
                    bool isActive = true;
                    if (dt.Columns.Contains("Active") && dt.Rows[0]["Active"] != DBNull.Value)
                    {
                        isActive = Convert.ToBoolean(dt.Rows[0]["Active"]);
                    }

                    if (!isActive)
                    {
                        txtError.Text = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ Admin!";
                        return;
                    }

                    // Lấy các thông tin cần thiết của người đăng nhập thành công từ bảng kết quả
                    int userId = Convert.ToInt32(dt.Rows[0]["ID"]);
                    string hoTen = dt.Rows[0]["LastName"].ToString()! + " " + dt.Rows[0]["FirstName"].ToString()!;
                    string vaiTro = dt.Rows[0]["VaiTro"].ToString()!;
                    int roleId = Convert.ToInt32(dt.Rows[0]["RoleID"]);
                    string vanPhong = dt.Rows[0]["VanPhong"].ToString()!;

                    // Bắn một tín hiệu ghi lại lịch sử truy cập (log) của người này với IP động nội bộ của họ
                    try 
                    { 
                        string dynamicIp = GetLocalIPAddress();
                        new LichSuBUS().GhiNhanTruyCap(userId, dynamicIp); 
                    } 
                    catch { }

                    // Mở màn hình chính của ứng dụng và truyền toàn bộ thông tin người dùng vào đó
                    string userEmail = dt.Columns.Contains("Email") && dt.Rows[0]["Email"] != DBNull.Value ? dt.Rows[0]["Email"].ToString()! : email;
                    MainWindow main = new MainWindow(userId, hoTen, vaiTro, roleId, vanPhong, userEmail);
                    main.Show();
                    
                    // Tắt cửa sổ Đăng nhập hiện tại đi
                    this.Close();
                }
                else
                {
                    // Tài khoản không tồn tại hoặc sai mật khẩu
                    txtError.Text = "Email hoặc mật khẩu không chính xác.";
                }
            }
            catch (Exception ex)
            {
                // Hiển thị lỗi nếu mạng yếu, rớt kết nối database...
                txtError.Text = "Lỗi kết nối CSDL: " + ex.Message;
            }
        }

        private static string GetLocalIPAddress()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch { }
            return "127.0.0.1";
        }



        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
