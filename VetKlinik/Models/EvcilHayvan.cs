namespace VetKlinik.Models
{
    public class EvcilHayvan
    {
        public int EvcilHayvanId { get; set; }
        public string EvcilHayvanAdi { get; set; } = "";
        public string Tur { get; set; } = "";
        public string Irk { get; set; } = "";
        public DateTime DogumTarihi { get; set; }
        public decimal Kilo { get; set; }

        // Sahibin Identity kullanıcı Id'si (string).
        // "Bir üye başkasının hayvanını göremez" kuralı bu alana dayanır.
        public string SahipId { get; set; } = "";
        public Kullanici Sahip { get; set; } = null!;

        public List<AsiKaydi> AsiKayitlari { get; set; } = new();
        public List<Randevu> Randevular { get; set; } = new();
    }
}
