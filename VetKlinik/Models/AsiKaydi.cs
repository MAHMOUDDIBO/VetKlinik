namespace VetKlinik.Models
{
    public class AsiKaydi
    {
        public int AsiKaydiId { get; set; }
        public int EvcilHayvanId { get; set; }
        public string AsiAdi { get; set; } = "";
        public DateTime YapildigiTarih { get; set; }
        public DateTime? SonrakiDozTarihi { get; set; }   // tekrar dozu olmayabilir

        public EvcilHayvan EvcilHayvan { get; set; } = null!;
    }
}
