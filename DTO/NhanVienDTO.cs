namespace DTO
{
    public class NhanVienDTO
    {
        public int ID { get; set; }
        public string HoTen { get; set; } = "";
        public string Email { get; set; } = "";
        public string VaiTro { get; set; } = "";
        public string VanPhong { get; set; } = "";
        public DateTime? NgaySinh { get; set; }
        public string TrangThai { get; set; } = "";
        public int RoleID { get; set; }
        public int OfficeID { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public bool Active { get; set; }
        public string Password { get; set; } = "";
    }
}
