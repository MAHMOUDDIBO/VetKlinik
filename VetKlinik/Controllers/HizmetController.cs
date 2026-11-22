using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Guvenlik;
using VetKlinik.Models;
using VetKlinik.Services;
using VetKlinik.ViewModels;

namespace VetKlinik.Controllers
{
    // Liste ve detay herkese açık; ekleme/düzenleme/silme yalnızca Admin.
    [Authorize(Roles = Roller.Admin)]
    public class HizmetController : Controller
    {
        private readonly IHizmetServisi _servis;

        public HizmetController(IHizmetServisi servis)
        {
            _servis = servis;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Hizmetler";
            return View(await _servis.ListeleAsync());
        }

        [AllowAnonymous]
        public async Task<IActionResult> Detay(int id)
        {
            var hizmet = await _servis.GetirAsync(id);
            if (hizmet is null) return NotFound();

            ViewData["Title"] = hizmet.HizmetAdi;
            return View(hizmet);
        }

        [HttpGet]
        public IActionResult Ekle()
        {
            ViewData["Title"] = "Hizmet ekle";
            return View(new HizmetFormVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(HizmetFormVM girdi)
        {
            ViewData["Title"] = "Hizmet ekle";

            if (!string.IsNullOrWhiteSpace(girdi.HizmetAdi) && await _servis.AyniAdVarMiAsync(girdi.HizmetAdi))
                ModelState.AddModelError(nameof(HizmetFormVM.HizmetAdi), "Bu adda bir hizmet zaten var.");

            if (!ModelState.IsValid)
                return View(girdi);                         // 200 — form hatalarla geri

            var hizmet = new Hizmet
            {
                HizmetAdi = girdi.HizmetAdi.Trim(),
                SureDakika = girdi.SureDakika!.Value,       // doğrulama geçtiyse null değil
                Ucret = girdi.Ucret!.Value
            };
            await _servis.EkleAsync(hizmet);

            TempData["Mesaj"] = $"\"{hizmet.HizmetAdi}\" eklendi.";
            return RedirectToAction(nameof(Index));         // 302 — PRG
        }

        [HttpGet]
        public async Task<IActionResult> Duzenle(int id)
        {
            var hizmet = await _servis.GetirAsync(id);
            if (hizmet is null) return NotFound();

            ViewData["Title"] = "Hizmet düzenle";
            return View(new HizmetFormVM
            {
                HizmetAdi = hizmet.HizmetAdi,
                SureDakika = hizmet.SureDakika,
                Ucret = hizmet.Ucret
            });
        }

        // Id ROTADAN gelir, formdan değil: kullanıcı gizli alanla başka kaydı hedefleyemez.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(int id, HizmetFormVM girdi)
        {
            ViewData["Title"] = "Hizmet düzenle";

            if (!string.IsNullOrWhiteSpace(girdi.HizmetAdi) && await _servis.AyniAdVarMiAsync(girdi.HizmetAdi, id))
                ModelState.AddModelError(nameof(HizmetFormVM.HizmetAdi), "Bu adda bir hizmet zaten var.");

            if (!ModelState.IsValid)
                return View(girdi);

            var guncellendi = await _servis.GuncelleAsync(new Hizmet
            {
                HizmetId = id,
                HizmetAdi = girdi.HizmetAdi.Trim(),
                SureDakika = girdi.SureDakika!.Value,
                Ucret = girdi.Ucret!.Value
            });
            if (!guncellendi) return NotFound();

            TempData["Mesaj"] = $"\"{girdi.HizmetAdi.Trim()}\" güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // GET yalnızca onay sayfası gösterir; GET veri SİLMEZ.
        [HttpGet]
        public async Task<IActionResult> Sil(int id)
        {
            var hizmet = await _servis.GetirAsync(id);
            if (hizmet is null) return NotFound();

            ViewData["Title"] = "Hizmet sil";
            return View(hizmet);
        }

        [HttpPost, ActionName("Sil")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SilOnay(int id)
        {
            var sonuc = await _servis.SilAsync(id);
            if (sonuc.Bulunamadi) return NotFound();

            if (sonuc.Basarili) TempData["Mesaj"] = "Hizmet silindi.";
            else TempData["Hata"] = sonuc.Hata;
            return RedirectToAction(nameof(Index));
        }
    }
}
