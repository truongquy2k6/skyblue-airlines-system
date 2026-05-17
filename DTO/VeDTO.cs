namespace DTO
{
    public class VeDTO
    {
        public int ID { get; set; }
        public string MaDatCho { get; set; } = "";
        public string TenKhach { get; set; } = "";
        public string SoDT { get; set; } = "";
        public string SoHieu { get; set; } = "";
        public string TuyenBay { get; set; } = "";
        public string NgayGio { get; set; } = "";
        public string HangGhe { get; set; } = "";
        public string TrangThai { get; set; } = "";
        public string Email { get; set; } = "";
        public string SoHoChieu { get; set; } = "";
        public string Firstname { get; set; } = "";
        public string Lastname { get; set; } = "";
        public int ScheduleID { get; set; }
        public int CabinTypeID { get; set; }
        public int UserID { get; set; }
        public string QuocTich { get; set; } = "";
        public int PassportCountryID { get; set; }
        public bool Confirmed { get; set; }
        public string SeatNumber { get; set; } = "";
    }

    public class DichVuDTO
    {
        public int ID { get; set; }
        public string TenDichVu { get; set; } = "";
        public decimal Gia { get; set; }
    }

    public class HangGheDTO
    {
        public int ID { get; set; }
        public string TenHangGhe { get; set; } = "";
        public double HeSoGia { get; set; }
    }

    public class QuocGiaDTO
    {
        public int ID { get; set; }
        public string TenQuocGia { get; set; } = "";
    }

    public class VanPhongDTO
    {
        public int ID { get; set; }
        public string TenVanPhong { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Contact { get; set; } = "";
        public string QuocGia { get; set; } = "";
    }

    public class VaiTroDTO
    {
        public int ID { get; set; }
        public string TenVaiTro { get; set; } = "";
    }
}
