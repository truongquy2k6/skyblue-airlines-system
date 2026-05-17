using System.Net;
using System.Net.Mail;
using System.Text;

namespace FlightManagement.Services
{
    public class EmailNotificationService
    {
        private readonly string? _host = Environment.GetEnvironmentVariable("FM_SMTP_HOST");
        private readonly string? _port = Environment.GetEnvironmentVariable("FM_SMTP_PORT");
        private readonly string? _username = Environment.GetEnvironmentVariable("FM_SMTP_USER");
        private readonly string? _password = Environment.GetEnvironmentVariable("FM_SMTP_PASS");
        private readonly string? _from = Environment.GetEnvironmentVariable("FM_SMTP_FROM");
        private readonly string? _fromName = Environment.GetEnvironmentVariable("FM_SMTP_FROM_NAME");
        private readonly string? _replyTo = Environment.GetEnvironmentVariable("FM_SMTP_REPLY_TO");
        private readonly string? _enableSsl = Environment.GetEnvironmentVariable("FM_SMTP_SSL");

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(_host) &&
            !string.IsNullOrWhiteSpace(_port) &&
            !string.IsNullOrWhiteSpace(_username) &&
            !string.IsNullOrWhiteSpace(_password) &&
            !string.IsNullOrWhiteSpace(_from);

        public void SendBookingConfirmation(
            string toEmail,
            string fullName,
            string bookingReference,
            string flightNumber,
            string route,
            string date,
            string time,
            string seat)
        {
            if (!IsConfigured) return;

            if (!int.TryParse(_port, out int smtpPort))
            {
                smtpPort = 587;
            }

            bool enableSsl = !string.Equals(_enableSsl, "false", StringComparison.OrdinalIgnoreCase);
            string seatValue = string.IsNullOrWhiteSpace(seat) ? "Chua chon" : seat;

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f7f9; margin: 0; padding: 20px; }}
        .ticket {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 10px 30px rgba(0,0,0,0.1); border: 1px solid #e1e8ed; }}
        .header {{ background: linear-gradient(135deg, #3f51b5 0%, #1a237e 100%); color: white; padding: 30px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 24px; letter-spacing: 2px; }}
        .header p {{ margin: 5px 0 0; opacity: 0.8; font-size: 14px; }}
        .content {{ padding: 35px; }}
        .greeting {{ font-size: 18px; color: #263238; margin-bottom: 25px; }}
        .info-table {{ width: 100%; border-collapse: collapse; margin-bottom: 30px; }}
        .info-cell {{ padding: 15px 0; border-bottom: 1px solid #f0f4f8; }}
        .label {{ font-size: 11px; color: #90a4ae; font-weight: bold; text-transform: uppercase; margin-bottom: 5px; }}
        .value {{ font-size: 16px; color: #263238; font-weight: 600; }}
        .highlight {{ color: #3f51b5; font-size: 18px; font-weight: 800; }}
        .seat-badge {{ background-color: #e8eaf6; padding: 8px 15px; border-radius: 6px; display: inline-block; color: #d32f2f; font-weight: 900; font-size: 20px; }}
        .footer {{ background-color: #fafbfc; padding: 20px; text-align: center; color: #78909c; font-size: 13px; border-top: 1px solid #f0f4f8; }}
        .barcode {{ font-family: 'Courier New', Courier, monospace; letter-spacing: 3px; font-size: 20px; color: #263238; margin-top: 15px; opacity: 0.5; }}
    </style>
</head>
<body>
    <div class='ticket'>
        <div class='header'>
            <h1>SKYBLUE AIRLINES</h1>
            <p>ELECTRONIC TICKET CONFIRMATION</p>
        </div>
        <div class='content'>
            <div class='greeting'>Xin chào <strong>{fullName}</strong>,</div>
            <p style='color: #546e7a; line-height: 1.6;'>Cảm ơn bạn đã lựa chọn SkyBlue Airlines. Đơn đặt vé của bạn đã được xác nhận thành công. Dưới đây là thông tin chi tiết về hành trình sắp tới:</p>
            
            <table class='info-table'>
                <tr>
                    <td class='info-cell' width='50%'>
                        <div class='label'>Mã đặt chỗ (PNR)</div>
                        <div class='value highlight'>{bookingReference}</div>
                    </td>
                    <td class='info-cell'>
                        <div class='label'>Tuyến bay</div>
                        <div class='value' style='color: #3f51b5;'>{route}</div>
                    </td>
                </tr>
                <tr>
                    <td class='info-cell'>
                        <div class='label'>Số hiệu chuyến bay</div>
                        <div class='value'>{flightNumber}</div>
                    </td>
                    <td class='info-cell'>
                        <div class='label'>Ngày khởi hành</div>
                        <div class='value'>{date}</div>
                    </td>
                </tr>
                <tr>
                    <td class='info-cell'>
                        <div class='label'>Giờ khởi hành</div>
                        <div class='value' style='font-size: 20px;'>{time}</div>
                    </td>
                    <td class='info-cell'>
                        <div class='label'>Số ghế</div>
                        <div class='seat-badge'>{seatValue}</div>
                    </td>
                </tr>
            </table>

            <div style='text-align: center;'>
                <div class='barcode'>|||||||||||||||||||||||</div>
                <div style='font-size: 10px; color: #90a4ae;'>{bookingReference}</div>
            </div>
        </div>
        <div class='footer'>
            <p>Chúc bạn có một chuyến bay tốt đẹp cùng SkyBlue Airlines!</p>
            <p style='font-size: 11px;'>Đây là email tự động, vui lòng không trả lời email này.</p>
        </div>
    </div>
</body>
</html>";

            string senderName = string.IsNullOrWhiteSpace(_fromName) ? "SkyBlue Airlines" : _fromName.Trim();
            using var message = new MailMessage
            {
                From = new MailAddress(_from!, senderName),
                Subject = "Xác nhận đặt vé - " + bookingReference,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(toEmail.Trim());
            if (!string.IsNullOrWhiteSpace(_replyTo))
            {
                message.ReplyToList.Add(new MailAddress(_replyTo.Trim()));
            }

            using var client = new SmtpClient(_host!, smtpPort)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(_username!, _password!)
            };
            client.Send(message);
        }

    }
}
