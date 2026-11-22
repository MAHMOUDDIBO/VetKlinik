using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Guvenlik;
using VetKlinik.Models;
using VetKlinik.ViewModels;

namespace VetKlinik.Controllers
{
    // Giriş, kayıt, çıkış ve profil (kişisel MCP/API anahtarı)
    public class HesapController : Controller
    {
        private readonly SignInManager<Kullanici> _girisler;
        private readonly UserManager<Kullanici> _kullanicilar;

        public HesapController(SignInManager<Kullanici> girisler, UserManager<Kullanici> kullanicilar)
        {
            _girisler = girisler;
            _kullanicilar = kullanicilar;
        }

        [HttpGet]
        public IActionResult Giris(string? returnUrl = null)
        {
            ViewData["Title"] = "Giriş";
            return View(new GirisVM { DonusAdresi = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Giris(GirisVM girdi)
        {
            ViewData["Title"] = "Giriş";
            if (!ModelState.IsValid)
                return View(girdi);

            // Kullanıcı adı = e-posta (kayıtta öyle oluşturuyoruz)
            var sonuc = await _girisler.PasswordSignInAsync(girdi.Eposta, girdi.Sifre, girdi.BeniHatirla, lockoutOnFailure: false);
            if (!sonuc.Succeeded)
            {
                // Hangisinin yanlış olduğunu SÖYLEMİYORUZ: kayıtlı e-postaları tahmin ettirmemek için
                ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
                return View(girdi);
            }

            // Açık yönlendirme (open redirect) koruması: yalnızca kendi sitemize dön
            if (!string.IsNullOrEmpty(girdi.DonusAdresi) && Url.IsLocalUrl(girdi.DonusAdresi))
                return LocalRedirect(girdi.DonusAdresi);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Kayit()
        {
            ViewData["Title"] = "Üye ol";
            return View(new KayitVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Kayit(KayitVM girdi)
        {
            ViewData["Title"] = "Üye ol";
            if (!ModelState.IsValid)
                return View(girdi);

            var kullanici = new Kullanici
            {
                UserName = girdi.Eposta,
                Email = girdi.Eposta,
                AdSoyad = girdi.AdSoyad.Trim()
            };
            var sonuc = await _kullanicilar.CreateAsync(kullanici, girdi.Sifre);
            if (!sonuc.Succeeded)
            {
                foreach (var hata in sonuc.Errors)
                    ModelState.AddModelError(string.Empty, hata.Description);
                return View(girdi);
            }

            // Kayıt olan HERKES "Uye" olur. Admin ve hekim rolleri yalnızca admin tarafından verilir.
            await _kullanicilar.AddToRoleAsync(kullanici, Roller.Uye);
            await _girisler.SignInAsync(kullanici, isPersistent: false);

            TempData["Mesaj"] = $"Hoş geldiniz, {kullanici.AdSoyad}! Şimdi evcil hayvanınızı ekleyebilirsiniz.";
            return RedirectToAction("Ekle", "EvcilHayvan");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cikis()
        {
            await _girisler.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult ErisimEngellendi()
        {
            ViewData["Title"] = "Erişim engellendi";
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Profil()
        {
            ViewData["Title"] = "Profil";
            var kullanici = await _kullanicilar.GetUserAsync(User);
            if (kullanici is null) return NotFound();

            ViewData["Roller"] = await _kullanicilar.GetRolesAsync(kullanici);
            return View(kullanici);
        }

        // Yeni kişisel anahtar: eskisi geçersiz olur. Anahtar BİR KEZ gösterilir, yalnızca özeti saklanır.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnahtarOlustur()
        {
            var kullanici = await _kullanicilar.GetUserAsync(User);
            if (kullanici is null) return NotFound();

            var anahtar = AnahtarYardimcisi.Olustur();
            kullanici.ApiAnahtariOzeti = AnahtarYardimcisi.Ozetle(anahtar);
            await _kullanicilar.UpdateAsync(kullanici);

            TempData["YeniAnahtar"] = anahtar;
            return RedirectToAction(nameof(Profil));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnahtarSil()
        {
            var kullanici = await _kullanicilar.GetUserAsync(User);
            if (kullanici is null) return NotFound();

            kullanici.ApiAnahtariOzeti = null;
            await _kullanicilar.UpdateAsync(kullanici);

            TempData["Mesaj"] = "Kişisel anahtar iptal edildi.";
            return RedirectToAction(nameof(Profil));
        }
    }
}
