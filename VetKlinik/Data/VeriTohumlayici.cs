using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetKlinik.Guvenlik;
using VetKlinik.Models;

namespace VetKlinik.Data
{
    // Uygulama açılırken: bekleyen migration'ları uygular, rolleri ve admin'i oluşturur,
    // veritabanı boşsa örnek veri ekler.
    public static class VeriTohumlayici
    {
        public static async Task TohumlaAsync(IServiceProvider servisler)
        {
            using var kapsam = servisler.CreateScope();
            var sp = kapsam.ServiceProvider;
            var db = sp.GetRequiredService<UygulamaDbContext>();
            var kullanicilar = sp.GetRequiredService<UserManager<Kullanici>>();
            var roller = sp.GetRequiredService<RoleManager<IdentityRole>>();
            var ayarlar = sp.GetRequiredService<IConfiguration>();

            await db.Database.MigrateAsync();

            foreach (var rol in Roller.Hepsi)
                if (!await roller.RoleExistsAsync(rol))
                    await roller.CreateAsync(new IdentityRole(rol));

            // Ödev şartı: Admin = ogrencinumarasi@sakarya.edu.tr / sau
            await KullaniciOlusturAsync(kullanicilar,
                ayarlar["Tohum:AdminEposta"] ?? "admin@sakarya.edu.tr",
                ayarlar["Tohum:AdminSifre"] ?? "sau",
                "Klinik Yöneticisi", Roller.Admin, null);

            if (await db.Hizmetler.AnyAsync())
                return;   // örnek veri yalnızca ilk açılışta

            // ---------- Hizmetler ----------
            var muayene = new Hizmet { HizmetAdi = "Muayene", SureDakika = 30, Ucret = 500 };
            var asilama = new Hizmet { HizmetAdi = "Aşılama", SureDakika = 15, Ucret = 350 };
            var kisir = new Hizmet { HizmetAdi = "Kısırlaştırma", SureDakika = 90, Ucret = 3000 };
            var dis = new Hizmet { HizmetAdi = "Diş Temizliği", SureDakika = 45, Ucret = 1200 };
            var tiras = new Hizmet { HizmetAdi = "Tıraş ve Bakım", SureDakika = 60, Ucret = 700 };
            db.Hizmetler.AddRange(muayene, asilama, kisir, dis, tiras);

            // ---------- Hekimler + çalışma saatleri ----------
            var ayse = new Veteriner
            {
                VeterinerAdSoyad = "Ayşe Kaya", UzmanAlani = "Küçük Hayvan",
                Hizmetler = { muayene, asilama, dis },
                Musaitlikler =
                {
                    Aralik(DayOfWeek.Monday, 9, 17), Aralik(DayOfWeek.Wednesday, 9, 17),
                    Aralik(DayOfWeek.Saturday, 10, 14)
                }
            };
            var mehmet = new Veteriner
            {
                VeterinerAdSoyad = "Mehmet Demir", UzmanAlani = "Cerrahi",
                Hizmetler = { muayene, kisir },
                Musaitlikler = { Aralik(DayOfWeek.Tuesday, 9, 17), Aralik(DayOfWeek.Thursday, 9, 17) }
            };
            var zeynep = new Veteriner
            {
                VeterinerAdSoyad = "Zeynep Arslan", UzmanAlani = "Dermatoloji",
                Hizmetler = { muayene, asilama, tiras },
                Musaitlikler = { Aralik(DayOfWeek.Monday, 13, 18), Aralik(DayOfWeek.Friday, 9, 17) }
            };
            db.Veterinerler.AddRange(ayse, mehmet, zeynep);
            await db.SaveChangesAsync();

            await KullaniciOlusturAsync(kullanicilar, "ayse.kaya@vetklinik.com", "sau", ayse.VeterinerAdSoyad, Roller.Veteriner, ayse.VeterinerId);
            await KullaniciOlusturAsync(kullanicilar, "mehmet.demir@vetklinik.com", "sau", mehmet.VeterinerAdSoyad, Roller.Veteriner, mehmet.VeterinerId);
            await KullaniciOlusturAsync(kullanicilar, "zeynep.arslan@vetklinik.com", "sau", zeynep.VeterinerAdSoyad, Roller.Veteriner, zeynep.VeterinerId);

            // ---------- Üyeler + hayvanlar ----------
            var ali = await KullaniciOlusturAsync(kullanicilar, "ali@ornek.com", "sau", "Ali Yılmaz", Roller.Uye, null);
            var elif = await KullaniciOlusturAsync(kullanicilar, "elif@ornek.com", "sau", "Elif Şahin", Roller.Uye, null);

            var pamuk = new EvcilHayvan { EvcilHayvanAdi = "Pamuk", Tur = "Kedi", Irk = "Van Kedisi", DogumTarihi = new DateTime(2022, 3, 10), Kilo = 4.2m, SahipId = ali.Id };
            var karabas = new EvcilHayvan { EvcilHayvanAdi = "Karabaş", Tur = "Köpek", Irk = "Kangal", DogumTarihi = new DateTime(2020, 7, 1), Kilo = 48.5m, SahipId = ali.Id };
            var mavis = new EvcilHayvan { EvcilHayvanAdi = "Maviş", Tur = "Kuş", Irk = "Muhabbet Kuşu", DogumTarihi = new DateTime(2024, 1, 20), Kilo = 0.04m, SahipId = elif.Id };
            db.EvcilHayvanlar.AddRange(pamuk, karabas, mavis);

            var bugun = DateTime.Today;
            db.AsiKayitlari.AddRange(
                new AsiKaydi { EvcilHayvan = pamuk, AsiAdi = "Karma Aşı", YapildigiTarih = bugun.AddMonths(-11), SonrakiDozTarihi = bugun.AddDays(20) },
                new AsiKaydi { EvcilHayvan = pamuk, AsiAdi = "Kuduz", YapildigiTarih = bugun.AddMonths(-2), SonrakiDozTarihi = bugun.AddMonths(10) },
                new AsiKaydi { EvcilHayvan = pamuk, AsiAdi = "İç Parazit", YapildigiTarih = bugun.AddMonths(-3) },
                new AsiKaydi { EvcilHayvan = karabas, AsiAdi = "Kuduz", YapildigiTarih = bugun.AddMonths(-6), SonrakiDozTarihi = bugun.AddMonths(6) },
                new AsiKaydi { EvcilHayvan = karabas, AsiAdi = "Karma Aşı (DHPP)", YapildigiTarih = bugun.AddMonths(-13), SonrakiDozTarihi = bugun.AddMonths(-1) });

            // ---------- Randevular ----------
            var gecenPazartesi = OncekiGun(DayOfWeek.Monday).AddHours(10);
            var gelecekPazartesi = SonrakiGun(DayOfWeek.Monday).AddHours(10);
            var gelecekSali = SonrakiGun(DayOfWeek.Tuesday).AddHours(11);

            db.Randevular.AddRange(
                Yeni(pamuk, ayse, muayene, gecenPazartesi, RandevuDurumu.Tamamlandi,
                     "Genel durum iyi. Kilo ideal aralıkta. Karma aşı zamanı yaklaşıyor."),
                Yeni(karabas, ayse, asilama, gecenPazartesi.AddHours(2), RandevuDurumu.Tamamlandi,
                     "Kuduz aşısı yapıldı, yan etki gözlenmedi."),
                Yeni(pamuk, ayse, asilama, gelecekPazartesi, RandevuDurumu.Onaylandi, null),
                Yeni(karabas, mehmet, muayene, gelecekSali, RandevuDurumu.Bekliyor, null),
                Yeni(mavis, zeynep, tiras, SonrakiGun(DayOfWeek.Friday).AddHours(14), RandevuDurumu.Bekliyor, null));

            await db.SaveChangesAsync();
        }

        private static Musaitlik Aralik(DayOfWeek gun, int basSaat, int bitSaat) =>
            new() { Gun = gun, Baslangic = new TimeOnly(basSaat, 0), Bitis = new TimeOnly(bitSaat, 0) };

        private static Randevu Yeni(EvcilHayvan h, Veteriner v, Hizmet s, DateTime bas, RandevuDurumu durum, string? not) =>
            new()
            {
                EvcilHayvan = h, Veteriner = v, Hizmet = s, Baslangic = bas,
                SureDakika = s.SureDakika, Ucret = s.Ucret, Durum = durum, MuayeneNotu = not,
                OlusturmaTarihi = DateTime.Now
            };

        // Bugünden sonraki ilk "gun" (bugün hariç)
        private static DateTime SonrakiGun(DayOfWeek gun)
        {
            var t = DateTime.Today.AddDays(1);
            while (t.DayOfWeek != gun) t = t.AddDays(1);
            return t;
        }

        private static DateTime OncekiGun(DayOfWeek gun)
        {
            var t = DateTime.Today.AddDays(-1);
            while (t.DayOfWeek != gun) t = t.AddDays(-1);
            return t;
        }

        private static async Task<Kullanici> KullaniciOlusturAsync(UserManager<Kullanici> um,
            string eposta, string sifre, string adSoyad, string rol, int? veterinerId)
        {
            var kullanici = await um.FindByEmailAsync(eposta);
            if (kullanici is not null)
                return kullanici;

            kullanici = new Kullanici
            {
                UserName = eposta, Email = eposta, EmailConfirmed = true,
                AdSoyad = adSoyad, VeterinerId = veterinerId
            };
            var sonuc = await um.CreateAsync(kullanici, sifre);
            if (!sonuc.Succeeded)
                throw new InvalidOperationException("Tohum kullanıcısı oluşturulamadı: " +
                    string.Join(", ", sonuc.Errors.Select(e => e.Description)));

            await um.AddToRoleAsync(kullanici, rol);
            return kullanici;
        }
    }
}
