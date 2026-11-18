namespace VetKlinik.Models
{
    public enum RandevuDurumu
    {
        Bekliyor,
        Onaylandi,
        Reddedildi,
        Tamamlandi
    }

    public class Randevu
    {
        public int RandevuId { get; set; }

        public int EvcilHayvanId { get; set; }

        public int VeterinerId { get; set; }

        public int HizmetId { get; set; }

        public DateTime Baslangic { get; set; }

        // Randevu anında Hizmet'ten kopyalanır
        public int SureDakika { get; set; }

        public decimal Ucret { get; set; }

        // Hesaplanan alan — veritabanına yazılmaz
        public DateTime Bitis => Baslangic.AddMinutes(SureDakika);

        public RandevuDurumu Durum { get; set; } = RandevuDurumu.Bekliyor;

        public string? MuayeneNotu { get; set; }
    }
}