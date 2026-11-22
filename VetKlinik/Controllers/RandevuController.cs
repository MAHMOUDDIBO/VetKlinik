using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetKlinik.Guvenlik;
using VetKlinik.Models;
using VetKlinik.Services;
using VetKlinik.ViewModels;

namespace VetKlinik.Controllers
{
    // Randevu akışı:  Al (Bekliyor) → Onayla / Reddet → Tamamla (muayene notu)
    // Kural ve yetki kontrolleri RandevuServisi'nde; controller yalnızca HTTP işini yapar.
    [Authorize]
    public class RandevuController : Controller
    {
        private const string TaslakAnahtari = "RandevuTaslagi";   // Hafta 7 — Session

        private readonly IRandevuServisi _servis;
        private readonly IEvcilHayvanServisi _hayvanlar;
        private readonly IHizmetServisi _hizmetler;

        public RandevuController(IRandevuServisi servis, IEvcilHayvanServisi hayvanlar, IHizmetServisi hizmetler)
        {
            _servis = servis;
            _hayvanlar = hayvanlar;
            _hizmetler = hizmetler;
        }

        private AktifKullanici K => User.AktifKullaniciAl();

        // GET /Randevu?durum=Bekliyor
        public async Task<IActionResult> Index(RandevuDurumu? durum)
        {
            ViewData["Title"] = User.IsInRole(Roller.Uye) ? "Randevularım" : "Randevular";
            ViewData["Durum"] = durum;
            return View(await _servis.ListeleAsync(K, durum));
        }

        public async Task<IActionResult> Detay(int id)
        {
            var r = await _servis.GetirAsync(id, K);
            if (r is null) return NotFound();

            ViewData["Title"] = "Randevu #" + r.RandevuId;
            ViewData["Yonetebilir"] = _servis.YonetebilirMi(r, K);
            ViewData["Sahibi"] = _servis.SahibiMi(r, K) || K.AdminMi;
            return View(r);
        }

        // ------------------ Randevu al ------------------

        [HttpGet]
        [Authorize(Roles = Roller.AdminVeyaUye)]
        public async Task<IActionResult> Al(int? hayvanId)
        {
            ViewData["Title"] = "Randevu al";

            // Session'daki son seçim formu önceden doldurur (Hafta 7)
            var taslak = HttpContext.Session.NesneOku<RandevuTaslagi>(TaslakAnahtari) ?? new RandevuTaslagi();
            var vm = new RandevuFormVM
            {
                EvcilHayvanId = hayvanId ?? taslak.EvcilHayvanId,
                HizmetId = taslak.HizmetId,
                Tarih = DateTime.Today.AddDays(1)
            };

            if (!await SecimleriYukleAsync())
            {
                TempData["Hata"] = "Randevu alabilmek için önce bir evcil hayvan ekleyin.";
                return RedirectToAction("Ekle", "EvcilHayvan");
            }
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = Roller.AdminVeyaUye)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Al(RandevuFormVM girdi)
        {
            ViewData["Title"] = "Randevu al";

            if (ModelState.IsValid && girdi.SecimiCoz(out var vetId, out var saat))
            {
                var talep = new RandevuTalebi(girdi.EvcilHayvanId!.Value, girdi.HizmetId!.Value, vetId,
                    girdi.Tarih!.Value.Date + saat.ToTimeSpan());

                var sonuc = await _servis.OlusturAsync(talep, K);
                if (sonuc.Basarili)
                {
                    HttpContext.Session.NesneYaz(TaslakAnahtari,
                        new RandevuTaslagi { EvcilHayvanId = girdi.EvcilHayvanId, HizmetId = girdi.HizmetId });

                    TempData["Mesaj"] = "Randevu talebiniz alındı. Hekim onayından sonra kesinleşecek.";
                    return RedirectToAction(nameof(Detay), new { id = sonuc.Veri!.RandevuId });
                }
                // Çakışma vb. — servisin açıklaması formun üstünde görünür
                ModelState.AddModelError(string.Empty, sonuc.Hata!);
            }

            await SecimleriYukleAsync();
            return View(girdi);
        }

        // ------------------ Düzenle (yalnızca Bekliyor) ------------------

        [HttpGet]
        public async Task<IActionResult> Duzenle(int id)
        {
            var r = await _servis.GetirAsync(id, K);
            if (r is null || !(_servis.SahibiMi(r, K) || K.AdminMi)) return NotFound();
            if (r.Durum != RandevuDurumu.Bekliyor)
            {
                TempData["Hata"] = "Yalnızca onay bekleyen randevular değiştirilebilir.";
                return RedirectToAction(nameof(Detay), new { id });
            }

            ViewData["Title"] = "Randevuyu değiştir";
            ViewData["RandevuId"] = id;
            await SecimleriYukleAsync();
            return View("Al", new RandevuFormVM
            {
                EvcilHayvanId = r.EvcilHayvanId,
                HizmetId = r.HizmetId,
                Tarih = r.Baslangic.Date,
                Secim = $"{r.VeterinerId}|{r.Baslangic:HH\\:mm}"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(int id, RandevuFormVM girdi)
        {
            ViewData["Title"] = "Randevuyu değiştir";
            ViewData["RandevuId"] = id;

            if (ModelState.IsValid && girdi.SecimiCoz(out var vetId, out var saat))
            {
                var talep = new RandevuTalebi(girdi.EvcilHayvanId!.Value, girdi.HizmetId!.Value, vetId,
                    girdi.Tarih!.Value.Date + saat.ToTimeSpan());

                var sonuc = await _servis.GuncelleAsync(id, talep, K);
                if (sonuc.Bulunamadi) return NotFound();
                if (sonuc.Basarili)
                {
                    TempData["Mesaj"] = "Randevu güncellendi.";
                    return RedirectToAction(nameof(Detay), new { id });
                }
                ModelState.AddModelError(string.Empty, sonuc.Hata!);
            }

            await SecimleriYukleAsync();
            return View("Al", girdi);
        }

        // ------------------ Durum değişiklikleri ------------------

        [HttpPost]
        [Authorize(Roles = Roller.AdminVeyaVeteriner)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Onayla(int id, string? donus) =>
            SonucaGoreDon(await _servis.OnaylaAsync(id, K), "Randevu onaylandı.", id, donus);

        [HttpPost]
        [Authorize(Roles = Roller.AdminVeyaVeteriner)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reddet(int id, string? donus) =>
            SonucaGoreDon(await _servis.ReddetAsync(id, K), "Randevu reddedildi.", id, donus);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Iptal(int id) =>
            SonucaGoreDon(await _servis.IptalAsync(id, K), "Randevu iptal edildi.", id, null);

        [HttpPost]
        [Authorize(Roles = Roller.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var sonuc = await _servis.SilAsync(id, K);
            if (sonuc.Bulunamadi) return NotFound();
            TempData["Mesaj"] = "Randevu silindi.";
            return RedirectToAction(nameof(Index));
        }

        // ------------------ Muayene notu ------------------

        [HttpGet]
        [Authorize(Roles = Roller.AdminVeyaVeteriner)]
        public async Task<IActionResult> Tamamla(int id)
        {
            var r = await _servis.GetirAsync(id, K);
            if (r is null || !_servis.YonetebilirMi(r, K)) return NotFound();

            ViewData["Title"] = "Muayene notu";
            ViewData["Randevu"] = r;
            return View(new RandevuTamamlaVM { MuayeneNotu = r.MuayeneNotu ?? "" });
        }

        [HttpPost]
        [Authorize(Roles = Roller.AdminVeyaVeteriner)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Tamamla(int id, RandevuTamamlaVM girdi)
        {
            var r = await _servis.GetirAsync(id, K);
            if (r is null || !_servis.YonetebilirMi(r, K)) return NotFound();

            if (ModelState.IsValid)
            {
                var sonuc = await _servis.TamamlaAsync(id, girdi.MuayeneNotu, K);
                if (sonuc.Basarili)
                {
                    TempData["Mesaj"] = "Muayene notu kaydedildi, randevu tamamlandı.";
                    return RedirectToAction(nameof(Detay), new { id });
                }
                ModelState.AddModelError(string.Empty, sonuc.Hata!);
            }

            ViewData["Title"] = "Muayene notu";
            ViewData["Randevu"] = r;
            return View(girdi);
        }

        // ------------------ Yardımcılar ------------------

        private IActionResult SonucaGoreDon(IslemSonucu sonuc, string basariMesaji, int id, string? donus)
        {
            if (sonuc.Bulunamadi) return NotFound();

            if (sonuc.Basarili) TempData["Mesaj"] = basariMesaji;
            else TempData["Hata"] = sonuc.Hata;

            if (!string.IsNullOrEmpty(donus) && Url.IsLocalUrl(donus))
                return LocalRedirect(donus);
            return RedirectToAction(nameof(Detay), new { id });
        }

        // Seçim listeleri: yalnızca kullanıcının GÖREBİLDİĞİ hayvanlar. Hayvan yoksa false.
        private async Task<bool> SecimleriYukleAsync()
        {
            var hayvanlar = await _hayvanlar.ListeleAsync(K);
            var hizmetler = await _hizmetler.ListeleAsync();

            ViewData["Hayvanlar"] = new SelectList(
                hayvanlar.Select(h => new
                {
                    h.EvcilHayvanId,
                    Ad = K.AdminMi ? $"{h.EvcilHayvanAdi} ({h.Sahip.AdSoyad})" : $"{h.EvcilHayvanAdi} — {h.Tur}"
                }),
                "EvcilHayvanId", "Ad");
            ViewData["Hizmetler"] = new SelectList(
                hizmetler.Select(h => new { h.HizmetId, Ad = $"{h.HizmetAdi} — {h.SureDakika} dk, {h.Ucret:N2} ₺" }),
                "HizmetId", "Ad");

            return hayvanlar.Count > 0;
        }
    }
}
