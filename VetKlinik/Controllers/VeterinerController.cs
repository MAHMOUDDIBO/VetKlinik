using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Guvenlik;
using VetKlinik.Models;
using VetKlinik.Services;
using VetKlinik.ViewModels;

namespace VetKlinik.Controllers
{
    [Authorize(Roles = Roller.Admin)]
    public class VeterinerController : Controller
    {
        private readonly IVeterinerServisi _servis;
        private readonly IHizmetServisi _hizmetler;

        public VeterinerController(IVeterinerServisi servis, IHizmetServisi hizmetler)
        {
            _servis = servis;
            _hizmetler = hizmetler;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Veteriner Hekimler";
            return View(await _servis.ListeleAsync());
        }

        [AllowAnonymous]
        public async Task<IActionResult> Detay(int id)
        {
            var vet = await _servis.GetirAsync(id);
            if (vet is null) return NotFound();

            ViewData["Title"] = vet.VeterinerAdSoyad;
            return View(vet);
        }

        [HttpGet]
        public async Task<IActionResult> Ekle()
        {
            ViewData["Title"] = "Hekim ekle";
            await HizmetleriYukleAsync();
            return View(new VeterinerEkleVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(VeterinerEkleVM girdi)
        {
            ViewData["Title"] = "Hekim ekle";
            if (!ModelState.IsValid)
            {
                await HizmetleriYukleAsync();
                return View(girdi);
            }

            var vet = new Veteriner
            {
                VeterinerAdSoyad = girdi.VeterinerAdSoyad.Trim(),
                UzmanAlani = girdi.UzmanAlani.Trim()
            };
            var sonuc = await _servis.EkleAsync(vet, girdi.SecilenHizmetIdleri, girdi.Eposta.Trim(), girdi.Sifre);
            if (!sonuc.Basarili)
            {
                ModelState.AddModelError(string.Empty, sonuc.Hata!);
                await HizmetleriYukleAsync();
                return View(girdi);
            }

            TempData["Mesaj"] = $"{vet.VeterinerAdSoyad} eklendi. Şimdi çalışma saatlerini tanımlayın.";
            return RedirectToAction(nameof(Detay), new { id = sonuc.Veri!.VeterinerId });
        }

        [HttpGet]
        public async Task<IActionResult> Duzenle(int id)
        {
            var vet = await _servis.GetirAsync(id);
            if (vet is null) return NotFound();

            ViewData["Title"] = "Hekim düzenle";
            await HizmetleriYukleAsync();
            return View(new VeterinerFormVM
            {
                VeterinerAdSoyad = vet.VeterinerAdSoyad,
                UzmanAlani = vet.UzmanAlani,
                SecilenHizmetIdleri = vet.Hizmetler.Select(h => h.HizmetId).ToList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(int id, VeterinerFormVM girdi)
        {
            ViewData["Title"] = "Hekim düzenle";
            if (!ModelState.IsValid)
            {
                await HizmetleriYukleAsync();
                return View(girdi);
            }

            var sonuc = await _servis.GuncelleAsync(id, girdi.VeterinerAdSoyad.Trim(), girdi.UzmanAlani.Trim(), girdi.SecilenHizmetIdleri);
            if (sonuc.Bulunamadi) return NotFound();

            TempData["Mesaj"] = "Hekim bilgileri güncellendi.";
            return RedirectToAction(nameof(Detay), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Sil(int id)
        {
            var vet = await _servis.GetirAsync(id);
            if (vet is null) return NotFound();

            ViewData["Title"] = "Hekim sil";
            return View(vet);
        }

        [HttpPost, ActionName("Sil")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SilOnay(int id)
        {
            var sonuc = await _servis.SilAsync(id);
            if (sonuc.Bulunamadi) return NotFound();

            if (sonuc.Basarili)
            {
                TempData["Mesaj"] = "Hekim ve giriş hesabı silindi.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Hata"] = sonuc.Hata;
            return RedirectToAction(nameof(Detay), new { id });
        }

        // ------------------ Çalışma saatleri ------------------

        // POST /Veteriner/MusaitlikEkle/3  (form Detay sayfasında)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MusaitlikEkle(int id, MusaitlikFormVM girdi)
        {
            if (!ModelState.IsValid)
            {
                TempData["Hata"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction(nameof(Detay), new { id });
            }

            var sonuc = await _servis.MusaitlikEkleAsync(new Musaitlik
            {
                VeterinerId = id,
                Gun = girdi.Gun!.Value,
                Baslangic = girdi.Baslangic!.Value,
                Bitis = girdi.Bitis!.Value
            });
            if (sonuc.Bulunamadi) return NotFound();

            if (sonuc.Basarili) TempData["Mesaj"] = "Çalışma aralığı eklendi.";
            else TempData["Hata"] = sonuc.Hata;
            return RedirectToAction(nameof(Detay), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MusaitlikSil(int id)
        {
            var sonuc = await _servis.MusaitlikSilAsync(id);
            if (sonuc.Bulunamadi) return NotFound();

            TempData["Mesaj"] = "Çalışma aralığı silindi.";
            return RedirectToAction(nameof(Detay), new { id = sonuc.Veri });
        }

        private async Task HizmetleriYukleAsync() =>
            ViewData["Hizmetler"] = await _hizmetler.ListeleAsync();
    }
}
