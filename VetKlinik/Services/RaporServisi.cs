using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Guvenlik;
using VetKlinik.Models;

namespace VetKlinik.Services
{
    public class RaporServisi : IRaporServisi
    {
        private readonly UygulamaDbContext _db;

        public RaporServisi(UygulamaDbContext db)
        {
            _db = db;
        }

        // Hizmet türüne göre randevu sayısı ve ciro.
        // Filtre veritabanında (Where), gruplama bellekte: SQLite decimal toplamayı (SUM) desteklemez.
        public async Task<List<HizmetRaporSatiri>> HizmetRaporuAsync(DateOnly? baslangic, DateOnly? bitis)
        {
            var sorgu = _db.Randevular.AsNoTracking()
                .Where(r => r.Durum != RandevuDurumu.Reddedildi && r.Durum != RandevuDurumu.IptalEdildi);

            if (baslangic is not null)
            {
                var bas = baslangic.Value.ToDateTime(TimeOnly.MinValue);
                sorgu = sorgu.Where(r => r.Baslangic >= bas);
            }
            if (bitis is not null)
            {
                var bit = bitis.Value.ToDateTime(TimeOnly.MinValue).AddDays(1);
                sorgu = sorgu.Where(r => r.Baslangic < bit);
            }

            var satirlar = await sorgu
                .Select(r => new { r.Hizmet.HizmetAdi, r.Durum, r.Ucret })
                .ToListAsync();

            return satirlar
                .GroupBy(s => s.HizmetAdi)
                .Select(g => new HizmetRaporSatiri(
                    g.Key,
                    g.Count(),
                    g.Count(s => s.Durum == RandevuDurumu.Tamamlandi),
                    g.Where(s => s.Durum == RandevuDurumu.Tamamlandi).Sum(s => s.Ucret)))
                .OrderByDescending(s => s.Ciro)
                .ThenBy(s => s.HizmetAdi)
                .ToList();
        }

        public async Task<PanelOzeti> PanelOzetiAsync()
        {
            var bugun = DateTime.Today;
            var yarin = bugun.AddDays(1);

            var uyeRolId = await _db.Roles.Where(r => r.Name == Roller.Uye).Select(r => r.Id).FirstOrDefaultAsync();

            return new PanelOzeti(
                await _db.Hizmetler.CountAsync(),
                await _db.Veterinerler.CountAsync(),
                await _db.UserRoles.CountAsync(ur => ur.RoleId == uyeRolId),
                await _db.EvcilHayvanlar.CountAsync(),
                await _db.Randevular.CountAsync(r => r.Durum == RandevuDurumu.Bekliyor),
                await _db.Randevular.CountAsync(r => r.Baslangic >= bugun && r.Baslangic < yarin &&
                                                     r.Durum != RandevuDurumu.Reddedildi &&
                                                     r.Durum != RandevuDurumu.IptalEdildi));
        }
    }
}
