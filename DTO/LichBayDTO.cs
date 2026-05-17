namespace DTO
{
    public class LichBayDTO
    {
        public int ID { get; set; }
        public string SoHieu { get; set; } = "";
        public DateTime NgayBay { get; set; }
        public TimeSpan GioBay { get; set; }
        public string TuyenBay { get; set; } = "";
        public string MaDi { get; set; } = "";
        public string MaDen { get; set; } = "";
        public string SanBayDi { get; set; } = "";
        public string SanBayDen { get; set; } = "";
        public string MayBay { get; set; } = "";
        public string Model { get; set; } = "";
        public decimal GiaEconomy { get; set; }
        public decimal GiaBusiness { get; set; }
        public int ThoiGianBay { get; set; }
        public int KhoangCach { get; set; }
        public string TrangThai { get; set; } = "";
        public bool Confirmed { get; set; }
        public int AircraftID { get; set; }
        public int RouteID { get; set; }
    }
}
