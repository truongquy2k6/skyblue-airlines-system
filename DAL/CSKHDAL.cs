using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DAL
{
    public class CSKHDAL
    {
        // Lấy danh sách hàng đợi gửi Mail (Có phân trang)
        public DataTable LayMailQueue(int pageNumber, int pageSize, string statusFilter = null, DateTime? flightDate = null)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber },
                new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize },
                new SqlParameter("@StatusFilter", SqlDbType.NVarChar) { Value = (object)statusFilter ?? DBNull.Value },
                new SqlParameter("@FlightDate", SqlDbType.Date) { Value = (object)flightDate ?? DBNull.Value }
            };
            return DatabaseHelper.ExecuteQuery("sp_CSKH_LayMailQueue", parameters);
        }

        // Thêm mới một vé vào hàng đợi Mail Queue
        public bool ThemMailQueue(int ticketId)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@TicketID", SqlDbType.Int) { Value = ticketId }
            };
            return DatabaseHelper.ExecuteNonQuery("sp_CSKH_ThemMailQueue", parameters) > 0;
        }

        // Cập nhật trạng thái của Mail Queue sau khi gửi (hoặc lỗi)
        public bool CapNhatTrangThaiMail(int queueId, string status, string errorMessage = null)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@QueueID", SqlDbType.Int) { Value = queueId },
                new SqlParameter("@Status", SqlDbType.NVarChar) { Value = status },
                new SqlParameter("@ErrorMessage", SqlDbType.NVarChar) { Value = (object)errorMessage ?? DBNull.Value }
            };
            return DatabaseHelper.ExecuteNonQuery("sp_CSKH_CapNhatTrangThaiMail", parameters) > 0;
        }

        // Lấy danh sách Feedback (Có phân trang)
        public DataTable LayFeedback(int pageNumber, int pageSize)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber },
                new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize }
            };
            return DatabaseHelper.ExecuteQuery("sp_CSKH_LayFeedback", parameters);
        }

        // Thêm mới một Feedback
        public bool ThemFeedback(string passengerName, string passengerPhone, string passengerEmail, int rating, string category, string content, int operatorId)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PassengerName", SqlDbType.NVarChar) { Value = passengerName },
                new SqlParameter("@PassengerPhone", SqlDbType.VarChar) { Value = (object)passengerPhone ?? DBNull.Value },
                new SqlParameter("@PassengerEmail", SqlDbType.VarChar) { Value = (object)passengerEmail ?? DBNull.Value },
                new SqlParameter("@Rating", SqlDbType.Int) { Value = rating },
                new SqlParameter("@Category", SqlDbType.NVarChar) { Value = category },
                new SqlParameter("@Content", SqlDbType.NVarChar) { Value = content },
                new SqlParameter("@OperatorID", SqlDbType.Int) { Value = operatorId }
            };
            return DatabaseHelper.ExecuteNonQuery("sp_CSKH_ThemFeedback", parameters) > 0;
        }
    }
}
