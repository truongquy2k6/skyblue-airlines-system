namespace DTO
{
    public class TuyenBayDTO
    {
        public int ID { get; set; }
        public string DiemDi { get; set; } = "";
        public string DiemDen { get; set; } = "";
        public int KhoangCach { get; set; }
        public int ThoiGianBay { get; set; }
        public int DepartureAirportID { get; set; }
        public int ArrivalAirportID { get; set; }
        public string MaDi { get; set; } = "";
        public string MaDen { get; set; } = "";
    }
}
