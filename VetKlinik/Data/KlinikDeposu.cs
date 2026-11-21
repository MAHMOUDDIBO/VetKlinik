using VetKlinik.Models;

namespace VetKlinik.Data
{
    // Bellek içi veri deposu — EF Core haftasında yerini veritabanı alacak
    public static class KlinikDeposu
    {
        public static readonly List<Hizmet> Hizmetler = new()
        {
            new Hizmet { HizmetId = 1, HizmetAdi = "Muayene",        SureDakika = 30, Ucret = 500 },
            new Hizmet { HizmetId = 2, HizmetAdi = "Aşılama",        SureDakika = 15, Ucret = 350 },
            new Hizmet { HizmetId = 3, HizmetAdi = "Kısırlaştırma",  SureDakika = 90, Ucret = 3000 },
            new Hizmet { HizmetId = 4, HizmetAdi = "Diş Temizliği",  SureDakika = 45, Ucret = 1200 },
            new Hizmet { HizmetId = 5, HizmetAdi = "Tıraş ve Bakım", SureDakika = 60, Ucret = 700 }
        };
    }
}