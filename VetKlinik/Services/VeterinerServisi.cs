using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Guvenlik;
using VetKlinik.Models;

namespace VetKlinik.Services
{
    public class VeterinerServisi : IVeterinerServisi
    {
        private readonly UygulamaDbContext _db;
        private readonly UserManager<Kullanici> _kullanicilar;

        public VeterinerServisi(UygulamaDbContext db, UserManager<Kullanici> kullanicilar)
        {
            _db = db;
            _kullanicilar = kullanicilar;
        }

        // LINQ ile filtre: hizmetId verilirse yalnızca o hizmeti verebilen hekimler
        public async Task<List<Veteriner>> ListeleAsync(int? hizmetId = null)
        {
            var sorgu = _db.Veterinerler
                .Include(v => v.Hizmetler)
                .Include(v => v.Musaitlikler)
                .AsNoTracking();

            if (hizmetId is not null)
                sorgu = sorgu.Where(v => v.Hizmetler.Any(h => h.HizmetId == hizmetId));

            return await sorgu.OrderBy(v => v.VeterinerAdSoyad).ToListAsync();
        }

        public Task<Veteriner?> GetirAsync(int id) =>
            _db.Veterinerler
                .Include(v => v.Hizmetler)
                .Include(v => v.Musaitlikler)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VeterinerId == id);

        // Hekim kaydı + giriş hesabı birlikte oluşur. Biri başarısızsa ikisi de geri alınır (transaction).
        public async Task<IslemSonucu<Veteriner>> EkleAsync(Veteriner veteriner, IEnumerable<int> hizmetIdleri,
            string eposta, string sifre)
        {
            if (await _kullanicilar.FindByEmailAsync(eposta) is not null)
                return IslemSonucu<Veteriner>.Hatali("Bu e-posta ile kayıtlı bir kullanıcı var.");

            var idler = hizmetIdleri.ToList();
            veteriner.Hizmetler = await _db.Hizmetler.Where(h => idler.Contains(h.HizmetId)).ToListAsync();

            await using var islem = await _db.Database.BeginTransactionAsync();

            _db.Veterinerler.Add(veteriner);
            await _db.SaveChangesAsync();

            var kullanici = new Kullanici
            {
                UserName = eposta, Email = eposta, EmailConfirmed = true,
                AdSoyad = veteriner.VeterinerAdSoyad, VeterinerId = veteriner.VeterinerId
            };
            var sonuc = await _kullanicilar.CreateAsync(kullanici, sifre);
            if (!sonuc.Succeeded)
            {
                await islem.RollbackAsync();
                return IslemSonucu<Veteriner>.Hatali(string.Join(" ", sonuc.Errors.Select(e => e.Description)));
            }
            await _kullanicilar.AddToRoleAsync(kullanici, Roller.Veteriner);

            await islem.CommitAsync();
            return IslemSonucu<Veteriner>.Tamam(veteriner);
        }

        public async Task<IslemSonucu> GuncelleAsync(int id, string adSoyad, string uzmanAlani, IEnumerable<int> hizmetIdleri)
        {
            var vet = await _db.Veterinerler.Include(v => v.Hizmetler).FirstOrDefaultAsync(v => v.VeterinerId == id);
            if (vet is null) return IslemSonucu.Yok();

            vet.VeterinerAdSoyad = adSoyad;
            vet.UzmanAlani = uzmanAlani;

            // Çoka-çok güncelleme: listeyi temizle, seçilenleri ekle. Ara tabloyu EF Core yönetir.
            var idler = hizmetIdleri.ToList();
            vet.Hizmetler.Clear();
            vet.Hizmetler.AddRange(await _db.Hizmetler.Where(h => idler.Contains(h.HizmetId)).ToListAsync());

            // Hekimin giriş hesabındaki ad da güncel kalsın
            var hesap = await _db.Users.FirstOrDefaultAsync(k => k.VeterinerId == id);
            if (hesap is not null) hesap.AdSoyad = adSoyad;

            await _db.SaveChangesAsync();
            return IslemSonucu.Tamam();
        }

        public async Task<IslemSonucu> SilAsync(int id)
        {
            var vet = await _db.Veterinerler.FindAsync(id);
            if (vet is null) return IslemSonucu.Yok();

            if (await _db.Randevular.AnyAsync(r => r.VeterinerId == id))
                return IslemSonucu.Hatali("Bu hekimin randevu geçmişi olduğu için silinemez.");

            var hesap = await _db.Users.FirstOrDefaultAsync(k => k.VeterinerId == id);
            if (hesap is not null)
                await _kullanicilar.DeleteAsync(hesap);

            _db.Veterinerler.Remove(vet);
            await _db.SaveChangesAsync();
            return IslemSonucu.Tamam();
        }

        // İş kuralı: aynı gün içinde iki çalışma aralığı üst üste binemez
        public async Task<IslemSonucu> MusaitlikEkleAsync(Musaitlik musaitlik)
        {
            if (!await _db.Veterinerler.AnyAsync(v => v.VeterinerId == musaitlik.VeterinerId))
                return IslemSonucu.Yok("Hekim bulunamadı.");

            if (musaitlik.Bitis <= musaitlik.Baslangic)
                return IslemSonucu.Hatali("Bitiş saati başlangıçtan sonra olmalıdır.");

            var ayniGun = await _db.Musaitlikler
                .Where(m => m.VeterinerId == musaitlik.VeterinerId && m.Gun == musaitlik.Gun)
                .ToListAsync();
            if (ayniGun.Any(m => musaitlik.Baslangic < m.Bitis && m.Baslangic < musaitlik.Bitis))
                return IslemSonucu.Hatali("Bu aralık hekimin o günkü başka bir çalışma aralığıyla çakışıyor.");

            _db.Musaitlikler.Add(musaitlik);
            await _db.SaveChangesAsync();
            return IslemSonucu.Tamam();
        }

        public async Task<IslemSonucu<int>> MusaitlikSilAsync(int musaitlikId)
        {
            var m = await _db.Musaitlikler.FindAsync(musaitlikId);
            if (m is null) return IslemSonucu<int>.Yok();

            _db.Musaitlikler.Remove(m);
            await _db.SaveChangesAsync();
            return IslemSonucu<int>.Tamam(m.VeterinerId);
        }
    }
}
