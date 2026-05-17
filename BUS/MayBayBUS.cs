using DAL;
using System.Data;

namespace BUS
{
    public class MayBayBUS
    {
        private readonly MayBayDAL dal = new();
        public DataTable HienThi() => dal.HienThi();
        public void Them(string name, string makeModel, int totalSeats, int economySeats, int businessSeats, int firstClassSeats = 0)
            => dal.Them(name, makeModel, totalSeats, economySeats, businessSeats, firstClassSeats);
        public void CapNhat(int id, string name, string makeModel, int totalSeats, int economySeats, int businessSeats, int firstClassSeats = 0)
            => dal.CapNhat(id, name, makeModel, totalSeats, economySeats, businessSeats, firstClassSeats);
        public void Xoa(int id) => dal.Xoa(id);
    }
}
