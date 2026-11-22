using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Guvenlik;
using VetKlinik.Models;
using VetKlinik.Services;
using VetKlinik.ViewModels;

namespace VetKlinik.Controllers
{
    // Aşı kaydını klinik personeli girer. Sahip, kayıtları hayvanın detay sayfasında görür.
    [Authorize(Roles = Roller.AdminVeyaVeteriner)]
    public class AsiController : Controller
    {
        private readonly IAsiServisi _servis;
        private readonly IEvcilHayvanServisi _hayvanlar;

        public AsiController(IAsiServisi servis, IEvcilHayvanServisi hayvanlar)
        {
            _servis = servis;
            _hayvanlar = hayvanlar;
        }

        private AktifKullanici K => User.AktifKullaniciAl();

        // GET /Asi/Ekle/5  (5 = hayvan id)
        [HttpGet]
        public async Task<IActionResult> Ekle(int id)
        {
            var hayvan = await _hayvanlar.GetirAsync(id, K);
            if (hayvan is null) return NotFound();

            ViewData["Title"] = "Aşı ekle";
            ViewData["Hayvan"] = hayvan;
            return View(new AsiFormVM { YapildigiTarih = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(int id, AsiFormVM girdi)
        {
            var hayvan = await _hayvanlar.GetirAsync(id, K);
            if (hayvan is null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Aşı ekle";
                ViewData["Hayvan"] = hayvan;
                return View(girdi);
            }

            var sonuc = await _servis.EkleAsync(new AsiKaydi
            {
                EvcilHayvanId = id,
                AsiAdi = girdi.AsiAdi.Trim(),
                YapildigiTarih = girdi.YapildigiTarih!.Value.Date,
                SonrakiDozTarihi = girdi.SonrakiDozTarihi?.Date
            }, K);
            if (sonuc.Bulunamadi) return NotFound();

            TempData["Mesaj"] = "Aşı kaydı eklendi.";
            return RedirectToAction("Detay", "EvcilHayvan", new { id });
        }

        // GET /Asi/Duzenle/12  (12 = aşı kaydı id)
        [HttpGet]
        public async Task<IActionResult> Duzenle(int id)
        {
            var asi = await _servis.GetirAsync(id, K);
            if (asi is null) return NotFound();

            ViewData["Title"] = "Aşı düzenle";
            ViewData["Hayvan"] = asi.EvcilHayvan;
            return View(new AsiFormVM
            {
                AsiAdi = asi.AsiAdi,
                YapildigiTarih = asi.YapildigiTarih,
                SonrakiDozTarihi = asi.SonrakiDozTarihi
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(int id, AsiFormVM girdi)
        {
            var asi = await _servis.GetirAsync(id, K);
            if (asi is null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Aşı düzenle";
                ViewData["Hayvan"] = asi.EvcilHayvan;
                return View(girdi);
            }

            await _servis.GuncelleAsync(new AsiKaydi
            {
                AsiKaydiId = id,
                AsiAdi = girdi.AsiAdi.Trim(),
                YapildigiTarih = girdi.YapildigiTarih!.Value.Date,
                SonrakiDozTarihi = girdi.SonrakiDozTarihi?.Date
            }, K);

            TempData["Mesaj"] = "Aşı kaydı güncellendi.";
            return RedirectToAction("Detay", "EvcilHayvan", new { id = asi.EvcilHayvanId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var sonuc = await _servis.SilAsync(id, K);
            if (sonuc.Bulunamadi) return NotFound();

            TempData["Mesaj"] = "Aşı kaydı silindi.";
            return RedirectToAction("Detay", "EvcilHayvan", new { id = sonuc.Veri });
        }
    }
}
