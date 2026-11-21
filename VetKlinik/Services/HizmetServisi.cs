using VetKlinik.Data;
using VetKlinik.Models;

namespace VetKlinik.Services
{
    // Sözleşmenin bellek içi uygulaması: NASIL yapılır?
    // EF Core haftasında yalnızca bu sınıfın içi değişecek.
    public class HizmetServisi : IHizmetServisi
    {
        public List<Hizmet> Listele()
        {
            return KlinikDeposu.Hizmetler.OrderBy(h => h.HizmetAdi).ToList();
        }

        public Hizmet? Getir(int id)
        {
            return KlinikDeposu.Hizmetler.FirstOrDefault(h => h.HizmetId == id);
        }

        public void Ekle(Hizmet hizmet)
        {
            // Id'yi kullanıcı değil, sunucu belirler
            hizmet.HizmetId = KlinikDeposu.Hizmetler.Count == 0
                ? 1
                : KlinikDeposu.Hizmetler.Max(h => h.HizmetId) + 1;

            KlinikDeposu.Hizmetler.Add(hizmet);
        }

        public bool Guncelle(Hizmet hizmet)
        {
            var mevcut = Getir(hizmet.HizmetId);
            if (mevcut is null)
                return false;

            mevcut.HizmetAdi = hizmet.HizmetAdi;
            mevcut.SureDakika = hizmet.SureDakika;
            mevcut.Ucret = hizmet.Ucret;
            return true;
        }

        public bool Sil(int id)
        {
            var hizmet = Getir(id);
            if (hizmet is null)
                return false;

            KlinikDeposu.Hizmetler.Remove(hizmet);
            return true;
        }

        // İş kuralı: aynı adda iki hizmet olamaz.
        // haricId: düzenlemede kaydın kendi adı "tekrar" sayılmasın diye.
        public bool AyniAdVarMi(string hizmetAdi, int haricId = 0)
        {
            return KlinikDeposu.Hizmetler.Any(h =>
                h.HizmetId != haricId &&
                string.Equals(h.HizmetAdi.Trim(), hizmetAdi.Trim(),
                    StringComparison.CurrentCultureIgnoreCase));
        }
    }
}