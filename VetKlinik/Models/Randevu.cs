namespace VetKlinik.Models
{
    public enum RandevuDurumu
    {
        Bekliyor,
        Onaylandi,
        Reddedildi,
        Tamamlandi,
        IptalEdildi
    }

    public class Randevu
    {
        public int RandevuId { get; set; }
        public int EvcilHayvanId { get; set; }
        public int VeterinerId { get; set; }
        public int HizmetId { get; set; }

        public DateTime Baslangic { get; set; }

        // Randevu anında Hizmet'ten KOPYALANIR: hizmetin fiyatı sonradan
        // değişse bile eski randevunun ücreti değişmez.
        public int SureDakika { get; set; }
        public decimal Ucret { get; set; }

        // Hesaplanan alan; setter'ı olmadığı için EF Core tabloya yazmaz.
        public DateTime Bitis => Baslangic.AddMinutes(SureDakika);

        public RandevuDurumu Durum { get; set; } = RandevuDurumu.Bekliyor;
        public string? MuayeneNotu { get; set; }
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public EvcilHayvan EvcilHayvan { get; set; } = null!;
        public Veteriner Veteriner { get; set; } = null!;
        public Hizmet Hizmet { get; set; } = null!;
    }
}
