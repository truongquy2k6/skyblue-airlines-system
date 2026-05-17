namespace DTO
{
    public class MayBayDTO
    {
        public int ID { get; set; }
        public string TenMayBay { get; set; } = "";
        public string Model { get; set; } = "";
        public int TongGhe { get; set; }
        public int GheFirstClass { get; set; }
        public int GheEconomy { get; set; }
        public int GheBusiness { get; set; }
        public decimal TyLeBusiness { get; set; }
    }
}
