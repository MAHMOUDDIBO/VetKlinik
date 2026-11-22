using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Guvenlik;
using VetKlinik.Models;
using VetKlinik.Services;
using VetKlinik.ViewModels;

namespace VetKlinik.Controllers
{
    // Üye kendi hayvanlarını, admin ve hekim tüm hastaları görür.
    // Yetki kontrolü serviste: başkasının hayvanının id'si yazılırsa 404 döner (varlığı bile belli olmaz).
    [Authorize]
    public class EvcilHayvanController : Controller
    {
        private readonly IEvcilHayvanServisi _servis;

        public EvcilHayvanController(IEvcilHayvanServisi servis)
        {
            _servis = servis;
        }

        private AktifKullanici K => User.AktifKullaniciAl();

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = User.IsInRole(Roller.Uye) ? "Evcil Hayvanlarım" : "Hastalar";
            return View(await _servis.ListeleAsync(K));
        }

        public async Task<IActionResult> Detay(int id)
        {
            var hayvan = await _servis.GetirAsync(id, K);
            if (hayvan is null) return NotFound();

            ViewData["Title"] = hayvan.EvcilHayvanAdi;
            ViewData["Duzenleyebilir"] = _servis.DuzenleyebilirMi(hayvan, K);
            return View(hayvan);
        }

        [HttpGet]
        [Authorize(Roles = Roller.Uye)]
        public IActionResult Ekle()
        {
            ViewData["Title"] = "Evcil hayvan ekle";
            return View(new EvcilHayvanFormVM());
        }

        [HttpPost]
        [Authorize(Roles = Roller.Uye)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(EvcilHayvanFormVM girdi)
        {
            ViewData["Title"] = "Evcil hayvan ekle";
            if (!ModelState.IsValid)
                return View(girdi);

            var hayvan = VmdenEntity(girdi);
            await _servis.EkleAsync(hayvan, K);

            TempData["Mesaj"] = $"{hayvan.EvcilHayvanAdi} eklendi.";
            return RedirectToAction(nameof(Detay), new { id = hayvan.EvcilHayvanId });
        }

        [HttpGet]
        public async Task<IActionResult> Duzenle(int id)
        {
            var hayvan = await _servis.GetirAsync(id, K);
            if (hayvan is null || !_servis.DuzenleyebilirMi(hayvan, K)) return NotFound();

            ViewData["Title"] = "Düzenle: " + hayvan.EvcilHayvanAdi;
            return View(new EvcilHayvanFormVM
            {
                EvcilHayvanAdi = hayvan.EvcilHayvanAdi,
                Tur = hayvan.Tur,
                Irk = hayvan.Irk,
                DogumTarihi = hayvan.DogumTarihi,
                Kilo = hayvan.Kilo
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(int id, EvcilHayvanFormVM girdi)
        {
            ViewData["Title"] = "Düzenle";
            if (!ModelState.IsValid)
                return View(girdi);

            var degerler = VmdenEntity(girdi);
            degerler.EvcilHayvanId = id;
            var sonuc = await _servis.GuncelleAsync(degerler, K);
            if (sonuc.Bulunamadi) return NotFound();

            TempData["Mesaj"] = "Bilgiler güncellendi.";
            return RedirectToAction(nameof(Detay), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Sil(int id)
        {
            var hayvan = await _servis.GetirAsync(id, K);
            if (hayvan is null || !_servis.DuzenleyebilirMi(hayvan, K)) return NotFound();

            ViewData["Title"] = "Sil: " + hayvan.EvcilHayvanAdi;
            return View(hayvan);
        }

        [HttpPost, ActionName("Sil")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SilOnay(int id)
        {
            var sonuc = await _servis.SilAsync(id, K);
            if (sonuc.Bulunamadi) return NotFound();

            if (!sonuc.Basarili)
            {
                TempData["Hata"] = sonuc.Hata;
                return RedirectToAction(nameof(Detay), new { id });
            }
            TempData["Mesaj"] = "Kayıt silindi.";
            return RedirectToAction(nameof(Index));
        }

        private static EvcilHayvan VmdenEntity(EvcilHayvanFormVM g) => new()
        {
            EvcilHayvanAdi = g.EvcilHayvanAdi.Trim(),
            Tur = g.Tur,
            Irk = g.Irk?.Trim() ?? "",
            DogumTarihi = g.DogumTarihi!.Value.Date,
            Kilo = g.Kilo!.Value
        };
    }
}
