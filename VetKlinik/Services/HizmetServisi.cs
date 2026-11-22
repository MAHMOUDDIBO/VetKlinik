using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Models;

namespace VetKlinik.Services
{
    public class HizmetServisi : IHizmetServisi
    {
        private readonly UygulamaDbContext _db;

        public HizmetServisi(UygulamaDbContext db)
        {
            _db = db;
        }

        public Task<List<Hizmet>> ListeleAsync() =>
            _db.Hizmetler.AsNoTracking().OrderBy(h => h.HizmetAdi).ToListAsync();

        public Task<Hizmet?> GetirAsync(int id) =>
            _db.Hizmetler.Include(h => h.Veterinerler).AsNoTracking()
                .FirstOrDefaultAsync(h => h.HizmetId == id);

        // İş kuralı: aynı adda iki hizmet olamaz (büyük/küçük harf ve Türkçe İ/ı duyarsız).
        // SQLite'ın lower() fonksiyonu Türkçe harfleri bilmediği için karşılaştırma bellekte.
        public async Task<bool> AyniAdVarMiAsync(string hizmetAdi, int haricId = 0)
        {
            var adlar = await _db.Hizmetler.Where(h => h.HizmetId != haricId)
                .Select(h => h.HizmetAdi).ToListAsync();
            return adlar.Any(a => string.Equals(a.Trim(), hizmetAdi.Trim(),
                StringComparison.CurrentCultureIgnoreCase));
        }

        public async Task EkleAsync(Hizmet hizmet)
        {
            _db.Hizmetler.Add(hizmet);
            await _db.SaveChangesAsync();   // HizmetId'yi veritabanı verir
        }

        public async Task<bool> GuncelleAsync(Hizmet hizmet)
        {
            var mevcut = await _db.Hizmetler.FindAsync(hizmet.HizmetId);
            if (mevcut is null) return false;

            mevcut.HizmetAdi = hizmet.HizmetAdi;
            mevcut.SureDakika = hizmet.SureDakika;
            mevcut.Ucret = hizmet.Ucret;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<IslemSonucu> SilAsync(int id)
        {
            var hizmet = await _db.Hizmetler.FindAsync(id);
            if (hizmet is null) return IslemSonucu.Yok();

            if (await _db.Randevular.AnyAsync(r => r.HizmetId == id))
                return IslemSonucu.Hatali("Bu hizmete ait randevular olduğu için silinemez.");

            _db.Hizmetler.Remove(hizmet);
            await _db.SaveChangesAsync();
            return IslemSonucu.Tamam();
        }
    }
}
