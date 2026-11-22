using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Guvenlik;
using VetKlinik.Models;

namespace VetKlinik.Services
{
    public class EvcilHayvanServisi : IEvcilHayvanServisi
    {
        private readonly UygulamaDbContext _db;

        public EvcilHayvanServisi(UygulamaDbContext db)
        {
            _db = db;
        }

        // Yetkinin kalbi: kullanıcının GÖREBİLECEĞİ hayvanlar.
        // Admin ve hekim kliniğin tüm hastalarını görür; üye yalnızca kendininkileri.
        // Adres çubuğunda id değiştirmek işe yaramaz: başkasının hayvanı bu sorguya hiç girmez.
        private IQueryable<EvcilHayvan> Gorunur(AktifKullanici k) =>
            k.AdminMi || k.VeterinerMi
                ? _db.EvcilHayvanlar
                : _db.EvcilHayvanlar.Where(h => h.SahipId == k.Id);

        public bool DuzenleyebilirMi(EvcilHayvan hayvan, AktifKullanici k) =>
            k.AdminMi || hayvan.SahipId == k.Id;

        public Task<List<EvcilHayvan>> ListeleAsync(AktifKullanici k) =>
            Gorunur(k).Include(h => h.Sahip).AsNoTracking()
                .OrderBy(h => h.EvcilHayvanAdi).ToListAsync();

        public Task<EvcilHayvan?> GetirAsync(int id, AktifKullanici k) =>
            Gorunur(k)
                .Include(h => h.Sahip)
                .Include(h => h.AsiKayitlari)
                .Include(h => h.Randevular).ThenInclude(r => r.Hizmet)
                .Include(h => h.Randevular).ThenInclude(r => r.Veteriner)
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.EvcilHayvanId == id);

        // MCP'deki "Pamuk'un aşısı ne zaman?" sorusu için: ada göre, yine yalnızca görünenler arasında
        public async Task<List<EvcilHayvan>> AdIleBulAsync(string ad, AktifKullanici k)
        {
            var liste = await Gorunur(k).AsNoTracking().ToListAsync();
            return liste.Where(h => string.Equals(h.EvcilHayvanAdi.Trim(), ad.Trim(),
                StringComparison.CurrentCultureIgnoreCase)).ToList();
        }

        public async Task EkleAsync(EvcilHayvan hayvan, AktifKullanici k)
        {
            hayvan.SahipId = k.Id;   // sahibi formdan DEĞİL, oturumdan gelir
            _db.EvcilHayvanlar.Add(hayvan);
            await _db.SaveChangesAsync();
        }

        public async Task<IslemSonucu> GuncelleAsync(EvcilHayvan degerler, AktifKullanici k)
        {
            var hayvan = await _db.EvcilHayvanlar.FindAsync(degerler.EvcilHayvanId);
            if (hayvan is null || !DuzenleyebilirMi(hayvan, k))
                return IslemSonucu.Yok();

            hayvan.EvcilHayvanAdi = degerler.EvcilHayvanAdi;
            hayvan.Tur = degerler.Tur;
            hayvan.Irk = degerler.Irk;
            hayvan.DogumTarihi = degerler.DogumTarihi;
            hayvan.Kilo = degerler.Kilo;
            await _db.SaveChangesAsync();
            return IslemSonucu.Tamam();
        }

        public async Task<IslemSonucu> SilAsync(int id, AktifKullanici k)
        {
            var hayvan = await _db.EvcilHayvanlar.FindAsync(id);
            if (hayvan is null || !DuzenleyebilirMi(hayvan, k))
                return IslemSonucu.Yok();

            var acikRandevu = await _db.Randevular.AnyAsync(r => r.EvcilHayvanId == id &&
                (r.Durum == RandevuDurumu.Bekliyor || r.Durum == RandevuDurumu.Onaylandi) &&
                r.Baslangic > DateTime.Now);
            if (acikRandevu)
                return IslemSonucu.Hatali("Bu hayvanın ileri tarihli randevusu var; önce randevuyu iptal edin.");

            _db.EvcilHayvanlar.Remove(hayvan);   // aşı kayıtları ve geçmiş randevular cascade ile silinir
            await _db.SaveChangesAsync();
            return IslemSonucu.Tamam();
        }
    }
}
