using DAL;
using System.Data;

namespace BUS
{
    public class SanBayBUS
    {
        private readonly SanBayDAL dal = new();
        public DataTable HienThi() => dal.HienThi();
        public void Them(string iataCode, string name, int countryId) => dal.Them(iataCode, name, countryId);
        public void CapNhat(int id, string iataCode, string name, int countryId) => dal.CapNhat(id, iataCode, name, countryId);
        public void Xoa(int id) => dal.Xoa(id);
    }

    public class TuyenBayBUS
    {
        private readonly TuyenBayDAL dal = new();
        public DataTable HienThi() => dal.HienThi();
        public void Them(int depId, int arrId, int distance, int flightTime) => dal.Them(depId, arrId, distance, flightTime);
        public void CapNhat(int id, int depId, int arrId, int distance, int flightTime) => dal.CapNhat(id, depId, arrId, distance, flightTime);
        public void Xoa(int id) => dal.Xoa(id);
    }
}
