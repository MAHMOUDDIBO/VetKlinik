using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Guvenlik;
using VetKlinik.Models;

namespace VetKlinik.Services
{
    // Projenin en önemli iş kuralları burada: müsaitlik, çakışma, onay akışı, yetki.
    public class RandevuServisi : IRandevuServisi
    {
        private const int SlotAraligiDakika = 15;   // uygun saatler 15 dakikada bir önerilir

        private readonly UygulamaDbContext _db;

        public RandevuServisi(UygulamaDbContext db)
        {
            _db = db;
        }

        // ---------------------------------------------------------------
        // YETKİ
        // ---------------------------------------------------------------

        // Onay/ret/muayene notu: admin ya da randevunun KENDİ hekimi
        public bool YonetebilirMi(Randevu r, AktifKullanici k) =>
            k.AdminMi || (k.VeterinerMi && r.VeterinerId == k.VeterinerId);

        public bool SahibiMi(Randevu r, AktifKullanici k) => r.EvcilHayvan.SahipId == k.Id;

        private IQueryable<Randevu> Gorunur(AktifKullanici k)
        {
            var sorgu = _db.Randevular
                .Include(r => r.EvcilHayvan).ThenInclude(h => h.Sahip)
                .Include(r => r.Hizmet)
                .Include(r => r.Veteriner)
                .AsQueryable();

            if (k.AdminMi) return sorgu;
            if (k.VeterinerMi) return sorgu.Where(r => r.VeterinerId == k.VeterinerId);
            return sorgu.Where(r => r.EvcilHayvan.SahipId == k.Id);
        }

        // ---------------------------------------------------------------
        // MÜSAİTLİK VE ÇAKIŞMA
        // İki aralık [a1, a2) ve [b1, b2) şu durumda çakışır:  a1 < b2  VE  b1 < a2
        // ---------------------------------------------------------------

        private static bool Cakisir(DateTime a1, DateTime a2, DateTime b1, DateTime b2) => a1 < b2 && b1 < a2;

        public async Task<List<UygunHekim>> UygunSaatlerAsync(int hizmetId, DateOnly tarih, int? veterinerId = null)
        {
            var hizmet = await _db.Hizmetler.FindAsync(hizmetId);
            if (hizmet is null) return new();

            // LINQ: o gün çalışan VE bu hizmeti verebilen hekimler
            var hekimSorgusu = _db.Veterinerler
                .Include(v => v.Musaitlikler)
                .Where(v => v.Hizmetler.Any(h => h.HizmetId == hizmetId))
                .Where(v => v.Musaitlikler.Any(m => m.Gun == tarih.DayOfWeek));
            if (veterinerId is not null)
                hekimSorgusu = hekimSorgusu.Where(v => v.VeterinerId == veterinerId);

            var hekimler = await hekimSorgusu.AsNoTracking().OrderBy(v => v.VeterinerAdSoyad).ToListAsync();
            if (hekimler.Count == 0) return new();

            var gunBasi = tarih.ToDateTime(TimeOnly.MinValue);
            var gunSonu = gunBasi.AddDays(1);
            var hekimIdleri = hekimler.Select(v => v.VeterinerId).ToList();

            // O gün yer tutan randevular — TEK sorgu, sonra bellekte karşılaştırma
            var dolu = await _db.Randevular.AsNoTracking()
                .Where(r => hekimIdleri.Contains(r.VeterinerId)
                            && r.Baslangic >= gunBasi && r.Baslangic < gunSonu
                            && r.Durum != RandevuDurumu.Reddedildi && r.Durum != RandevuDurumu.IptalEdildi)
                .ToListAsync();

            var simdi = DateTime.Now;
            var sonuc = new List<UygunHekim>();

            foreach (var v in hekimler)
            {
                var saatler = new List<TimeOnly>();
                foreach (var m in v.Musaitlikler.Where(m => m.Gun == tarih.DayOfWeek).OrderBy(m => m.Baslangic))
                {
                    var basDk = (int)m.Baslangic.ToTimeSpan().TotalMinutes;
                    var bitDk = (int)m.Bitis.ToTimeSpan().TotalMinutes;

                    // Hizmet süresi aralığın içine sığmalı: başlangıç + süre <= bitiş
                    for (var dk = basDk; dk + hizmet.SureDakika <= bitDk; dk += SlotAraligiDakika)
                    {
                        var bas = gunBasi.AddMinutes(dk);
                        var bit = bas.AddMinutes(hizmet.SureDakika);
                        if (bas <= simdi) continue;
                        if (dolu.Any(r => r.VeterinerId == v.VeterinerId && Cakisir(bas, bit, r.Baslangic, r.Bitis))) continue;
                        saatler.Add(TimeOnly.FromDateTime(bas));
                    }
                }
                if (saatler.Count > 0)
                    sonuc.Add(new UygunHekim(v.VeterinerId, v.VeterinerAdSoyad, v.UzmanAlani, saatler));
            }
            return sonuc;
        }

        // Randevu alınabilir mi? Alınamıyorsa NEDENİ (kullanıcıya gösterilecek cümle); alınabiliyorsa null.
        public async Task<string?> CakismaNedeniAsync(int veterinerId, int hizmetId, DateTime baslangic,
            int? evcilHayvanId, int? haricRandevuId = null)
        {
            if (baslangic <= DateTime.Now)
                return "Geçmiş bir zamana randevu alınamaz.";

            var hizmet = await _db.Hizmetler.FindAsync(hizmetId);
            if (hizmet is null) return "Hizmet bulunamadı.";

            var vet = await _db.Veterinerler
                .Include(v => v.Hizmetler)
                .Include(v => v.Musaitlikler)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VeterinerId == veterinerId);
            if (vet is null) return "Hekim bulunamadı.";

            if (!vet.Hizmetler.Any(h => h.HizmetId == hizmetId))
                return $"{vet.VeterinerAdSoyad} \"{hizmet.HizmetAdi}\" hizmetini vermiyor.";

            var bitis = baslangic.AddMinutes(hizmet.SureDakika);
            var basSaat = TimeOnly.FromDateTime(baslangic);
            var bitSaat = TimeOnly.FromDateTime(bitis);

            // 1) Hekimin çalışma saatleri içinde mi? (hizmet süresi DAHİL)
            var gunAraliklari = vet.Musaitlikler.Where(m => m.Gun == baslangic.DayOfWeek).ToList();
            var icinde = bitis.Date == baslangic.Date &&
                         gunAraliklari.Any(m => basSaat >= m.Baslangic && bitSaat <= m.Bitis);
            if (!icinde)
            {
                var saatler = gunAraliklari.Count == 0
                    ? "o gün çalışmıyor"
                    : "çalışma saatleri: " + string.Join(", ", gunAraliklari.OrderBy(m => m.Baslangic)
                        .Select(m => $"{m.Baslangic:HH\\:mm}–{m.Bitis:HH\\:mm}"));
                return $"{vet.VeterinerAdSoyad} bu saatte müsait değil ({saatler}). " +
                       $"\"{hizmet.HizmetAdi}\" {hizmet.SureDakika} dakika sürer.";
            }

            // 2) Hekimin başka randevusuyla çakışıyor mu?
            var gunBasi = baslangic.Date;
            var gunSonu = gunBasi.AddDays(1);
            var oGun = await _db.Randevular.AsNoTracking()
                .Where(r => r.Baslangic >= gunBasi && r.Baslangic < gunSonu
                            && r.Durum != RandevuDurumu.Reddedildi && r.Durum != RandevuDurumu.IptalEdildi
                            && (haricRandevuId == null || r.RandevuId != haricRandevuId)
                            && (r.VeterinerId == veterinerId || r.EvcilHayvanId == evcilHayvanId))
                .ToListAsync();

            var hekimCakisma = oGun.FirstOrDefault(r => r.VeterinerId == veterinerId &&
                                                        Cakisir(baslangic, bitis, r.Baslangic, r.Bitis));
            if (hekimCakisma is not null)
                return $"{vet.VeterinerAdSoyad} {hekimCakisma.Baslangic:HH\\:mm}–{hekimCakisma.Bitis:HH\\:mm} " +
                       "arasında başka bir randevuda.";

            // 3) Aynı hayvanın aynı saatte başka randevusu var mı?
            if (evcilHayvanId is not null &&
                oGun.Any(r => r.EvcilHayvanId == evcilHayvanId && Cakisir(baslangic, bitis, r.Baslangic, r.Bitis)))
                return "Bu evcil hayvanın aynı saatte başka bir randevusu var.";

            return null;
        }

        // ---------------------------------------------------------------
        // OKUMA
        // ---------------------------------------------------------------

        public async Task<List<Randevu>> ListeleAsync(AktifKullanici k, RandevuDurumu? durum = null)
        {
            var sorgu = Gorunur(k).AsNoTracking();
            if (durum is not null)
                sorgu = sorgu.Where(r => r.Durum == durum);
            return await sorgu.OrderByDescending(r => r.Baslangic).ToListAsync();
        }

        public Task<Randevu?> GetirAsync(int id, AktifKullanici k) =>
            Gorunur(k).AsNoTracking().FirstOrDefaultAsync(r => r.RandevuId == id);

        // ---------------------------------------------------------------
        // YAZMA
        // ---------------------------------------------------------------

        public async Task<IslemSonucu<Randevu>> OlusturAsync(RandevuTalebi talep, AktifKullanici k)
        {
            if (k.VeterinerMi && !k.AdminMi)
                return IslemSonucu<Randevu>.Hatali("Hekim hesabıyla randevu oluşturulamaz.");

            // Sahiplik: üye yalnızca KENDİ hayvanına randevu alır
            var hayvan = await _db.EvcilHayvanlar.FindAsync(talep.EvcilHayvanId);
            if (hayvan is null || (!k.AdminMi && hayvan.SahipId != k.Id))
                return IslemSonucu<Randevu>.Yok("Evcil hayvan bulunamadı.");

            var hizmet = await _db.Hizmetler.FindAsync(talep.HizmetId);
            if (hizmet is null)
                return IslemSonucu<Randevu>.Yok("Hizmet bulunamadı.");

            var neden = await CakismaNedeniAsync(talep.VeterinerId, talep.HizmetId, talep.Baslangic, talep.EvcilHayvanId);
            if (neden is not null)
                return IslemSonucu<Randevu>.Hatali(neden);

            var randevu = new Randevu
            {
                EvcilHayvanId = hayvan.EvcilHayvanId,
                HizmetId = hizmet.HizmetId,
                VeterinerId = talep.VeterinerId,
                Baslangic = talep.Baslangic,
                SureDakika = hizmet.SureDakika,   // anlık kopya
                Ucret = hizmet.Ucret,             // anlık kopya
                Durum = RandevuDurumu.Bekliyor,   // her randevu onay bekleyerek başlar
                OlusturmaTarihi = DateTime.Now
            };
            _db.Randevular.Add(randevu);
            await _db.SaveChangesAsync();

            var tam = await Gorunur(k).AsNoTracking().FirstAsync(r => r.RandevuId == randevu.RandevuId);
            return IslemSonucu<Randevu>.Tamam(tam);
        }

        // Yalnızca "Bekliyor" durumundaki randevu değiştirilebilir; değişiklik yeniden onay bekler.
        public async Task<IslemSonucu> GuncelleAsync(int id, RandevuTalebi talep, AktifKullanici k)
        {
            var r = await _db.Randevular.Include(x => x.EvcilHayvan).FirstOrDefaultAsync(x => x.RandevuId == id);
            if (r is null || !(k.AdminMi || SahibiMi(r, k)))
                return IslemSonucu.Yok();
            if (r.Durum != RandevuDurumu.Bekliyor)
                return IslemSonucu.Hatali("Yalnızca onay bekleyen randevular değiştirilebilir.");

            var hayvan = await _db.EvcilHayvanlar.FindAsync(talep.EvcilHayvanId);
            if (hayvan is null || (!k.AdminMi && hayvan.SahipId != k.Id))
                return IslemSonucu.Yok("Evcil hayvan bulunamadı.");

            var hizmet = await _db.Hizmetler.FindAsync(talep.HizmetId);
            if (hizmet is null) return IslemSonucu.Yok("Hizmet bulunamadı.");

            var neden = await CakismaNedeniAsync(talep.VeterinerId, talep.HizmetId, talep.Baslangic,
                talep.EvcilHayvanId, haricRandevuId: id);   // kendisiyle çakışmasın
            if (neden is not null) return IslemSonucu.Hatali(neden);

            r.EvcilHayvanId = talep.EvcilHayvanId;
            r.HizmetId = talep.HizmetId;
            r.VeterinerId = talep.VeterinerId;
            r.Baslangic = talep.Baslangic;
            r.SureDakika = hizmet.SureDakika;
            r.Ucret = hizmet.Ucret;
            await _db.SaveChangesAsync();
            return IslemSonucu.Tamam();
        }

        public Task<IslemSonucu> OnaylaAsync(int id, AktifKullanici k) =>
            DurumDegistirAsync(id, k, RandevuDurumu.Bekliyor, RandevuDurumu.Onaylandi);

        public Task<IslemSonucu> ReddetAsync(int id, AktifKullanici k) =>
            DurumDegistirAsync(id, k, RandevuDurumu.Bekliyor, RandevuDurumu.Reddedildi);

        private async Task<IslemSonucu> DurumDegistirAsync(int id, AktifKullanici k,
            RandevuDurumu beklenen, RandevuDurumu yeni)
        {
            var r = await _db.Randevular.FindAsync(id);
            if (r is null || !YonetebilirMi(r, k)) return IslemSonucu.Yok();
            if (r.Durum != beklenen)
                return IslemSonucu.Hatali($"Bu işlem yalnızca \"{beklenen}\" durumundaki randevuya uygulanır.");

            r.Durum = yeni;
            await _db.SaveChangesAsync();
            return IslemSonucu.Tamam();
        }

        public async Task<IslemSonucu> TamamlaAsync(int id, string muayeneNotu, AktifKullanici k)
        {
            var r = await _db.Randevular.FindAsync(id);
            if (r is null || !YonetebilirMi(r, k)) return IslemSonucu.Yok();
            if (r.Durum != RandevuDurumu.Onaylandi && r.Durum != RandevuDurumu.Tamamlandi)
                return IslemSonucu.Hatali("Yalnızca onaylanmış randevu tamamlanabilir.");
            if (r.Baslangic > DateTime.Now)
                return IslemSonucu.Hatali("Randevu saati gelmeden muayene notu girilemez.");

            r.MuayeneNotu = muayeneNotu.Trim();
            r.Durum = RandevuDurumu.Tamamlandi;
            await _db.SaveChangesAsync();
            return IslemSonucu.Tamam();
        }

        public async Task<IslemSonucu> IptalAsync(int id, AktifKullanici k)
        {
            var r = await _db.Randevular.Include(x => x.EvcilHayvan).FirstOrDefaultAsync(x => x.RandevuId == id);
            if (r is null || !(k.AdminMi || SahibiMi(r, k))) return IslemSonucu.Yok();
            if (r.Durum != RandevuDurumu.Bekliyor && r.Durum != RandevuDurumu.Onaylandi)
                return IslemSonucu.Hatali("Bu randevu iptal edilemez.");
            if (r.Baslangic <= DateTime.Now)
                return IslemSonucu.Hatali("Saati geçmiş randevu iptal edilemez.");

            r.Durum = RandevuDurumu.IptalEdildi;
            await _db.SaveChangesAsync();
            return IslemSonucu.Tamam();
        }

        public async Task<IslemSonucu> SilAsync(int id, AktifKullanici k)
        {
            if (!k.AdminMi) return IslemSonucu.Yok();
            var r = await _db.Randevular.FindAsync(id);
            if (r is null) return IslemSonucu.Yok();

            _db.Randevular.Remove(r);
            await _db.SaveChangesAsync();
            return IslemSonucu.Tamam();
        }
    }
}
