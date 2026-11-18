namespace VetKlinik.Models
{
    public class AsiKaydi
    {
        public int AsiKaydiId { get; set; }

        public int EvcilHayvanId { get; set; }      // hangi hayvana yapıldı

        public string AsiAdi { get; set; } = "";

        public DateTime YapildigiTarih { get; set; }

        public DateTime? SonrakiDozTarihi { get; set; }   // ? → tekrar dozu olmayabilir
    }
}