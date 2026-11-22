using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Guvenlik;
using VetKlinik.Models;

namespace VetKlinik.Services
{
    public class AsiServisi : IAsiServisi
    {
        private readonly UygulamaDbContext _db;

        public AsiServisi(UygulamaDbContext db)
        {
            _db = db;
        }

        // Aşı kaydını yalnızca klinik personeli (admin, hekim) girer; sahip yalnızca görür.
        private static bool YonetebilirMi(AktifKullanici k) => k.AdminMi || k.VeterinerMi;

        private static bool GorebilirMi(EvcilHayvan h, AktifKullanici k) =>
            k.AdminMi || k.VeterinerMi || h.SahipId == k.Id;

        // LINQ: geçmiş (tümü, yeniden eskiye) ve yaklaşan (sonraki dozu bugün ya da sonra olanlar)
        public async Task<AsiTakvimi?> TakvimAsync(int hayvanId, AktifKullanici k)
        {
            var hayvan = await _db.EvcilHayvanlar.Include(h => h.AsiKayitlari).AsNoTracking()
                .FirstOrDefaultAsync(h => h.EvcilHayvanId == hayvanId);
            if (hayvan is null || !GorebilirMi(hayvan, k))
                return null;

            var bugun = DateTime.Today;
            var gecmis = hayvan.AsiKayitlari.OrderByDescending(a => a.YapildigiTarih).ToList();
            var yaklasan = hayvan.AsiKayitlari
                .Where(a => a.SonrakiDozTarihi.HasValue && a.SonrakiDozTarihi.Value.Date >= bugun)
                .OrderBy(a => a.SonrakiDozTarihi)
                .ToList();

            return new AsiTakvimi(hayvan, gecmis, yaklasan);
        }

        public async Task<AsiKaydi?> GetirAsync(int asiKaydiId, AktifKullanici k)
        {
            var asi = await _db.AsiKayitlari.Include(a => a.EvcilHayvan).AsNoTracking()
                .FirstOrDefaultAsync(a => a.AsiKaydiId == asiKaydiId);
            return asi is not null && GorebilirMi(asi.EvcilHayvan, k) ? asi : null;
        }

        public async Task<IslemSonucu> EkleAsync(AsiKaydi asi, AktifKullanici k)
        {
            if (!YonetebilirMi(k)) return IslemSonucu.Yok();
            if (!await _db.EvcilHayvanlar.AnyAsync(h => h.EvcilHayvanId == asi.EvcilHayvanId))
                return IslemSonucu.Yok("Evcil hayvan bulunamadı.");

            _db.AsiKayitlari.Add(asi);
            await _db.SaveChangesAsync();
            return IslemSonucu.Tamam();
        }

        public async Task<IslemSonucu> GuncelleAsync(AsiKaydi degerler, AktifKullanici k)
        {
            if (!YonetebilirMi(k)) return IslemSonucu.Yok();
            var asi = await _db.AsiKayitlari.FindAsync(degerler.AsiKaydiId);
            if (asi is null) return IslemSonucu.Yok();

            asi.AsiAdi = degerler.AsiAdi;
            asi.YapildigiTarih = degerler.YapildigiTarih;
            asi.SonrakiDozTarihi = degerler.SonrakiDozTarihi;
            await _db.SaveChangesAsync();
            return IslemSonucu.Tamam();
        }

        public async Task<IslemSonucu<int>> SilAsync(int asiKaydiId, AktifKullanici k)
        {
            if (!YonetebilirMi(k)) return IslemSonucu<int>.Yok();
            var asi = await _db.AsiKayitlari.FindAsync(asiKaydiId);
            if (asi is null) return IslemSonucu<int>.Yok();

            _db.AsiKayitlari.Remove(asi);
            await _db.SaveChangesAsync();
            return IslemSonucu<int>.Tamam(asi.EvcilHayvanId);
        }
    }
}
