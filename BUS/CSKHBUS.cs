using System;
using System.Data;
using DAL;

namespace BUS
{
    public class CSKHBUS
    {
        private readonly CSKHDAL dal = new CSKHDAL();

        // Lấy danh sách hàng đợi gửi Mail (Có phân trang)
        public DataTable LayMailQueue(int pageNumber, int pageSize, string statusFilter = null, DateTime? flightDate = null)
        {
            return dal.LayMailQueue(pageNumber, pageSize, statusFilter, flightDate);
        }

        // Thêm mới một vé vào hàng đợi Mail Queue
        public bool ThemMailQueue(int ticketId)
        {
            // Có thể thêm validation nghiệp vụ ở đây (Ví dụ: Kiểm tra ID hợp lệ)
            if (ticketId <= 0) return false;
            return dal.ThemMailQueue(ticketId);
        }

        // Cập nhật trạng thái của Mail Queue sau khi gửi (hoặc lỗi)
        public bool CapNhatTrangThaiMail(int queueId, string status, string errorMessage = null)
        {
            if (queueId <= 0 || string.IsNullOrEmpty(status)) return false;
            return dal.CapNhatTrangThaiMail(queueId, status, errorMessage);
        }

        // Lấy danh sách Feedback (Có phân trang)
        public DataTable LayFeedback(int pageNumber, int pageSize)
        {
            return dal.LayFeedback(pageNumber, pageSize);
        }

        // Thêm mới một Feedback
        public bool ThemFeedback(string passengerName, string passengerPhone, string passengerEmail, int rating, string category, string content, int operatorId)
        {
            // Kiểm tra tính hợp lệ của dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(passengerName) || rating < 1 || rating > 5 || string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(content) || operatorId <= 0)
            {
                throw new ArgumentException("Dữ liệu Feedback không hợp lệ (Kiểm tra lại Tên, Điểm đánh giá, Thể loại và Nội dung).");
            }

            return dal.ThemFeedback(passengerName.Trim(), passengerPhone?.Trim(), passengerEmail?.Trim(), rating, category.Trim(), content.Trim(), operatorId);
        }
    }
}
