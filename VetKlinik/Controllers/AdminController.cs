using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetKlinik.Guvenlik;
using VetKlinik.Models;
using VetKlinik.Services;

namespace VetKlinik.Controllers
{
    // Yönetim paneli: özet, onay bekleyenler, hizmet raporu, kullanıcılar
    [Authorize(Roles = Roller.Admin)]
    public class AdminController : Controller
    {
        private readonly IRaporServisi _rapor;
        private readonly IRandevuServisi _randevular;
        private readonly UserManager<Kullanici> _kullanicilar;

        public AdminController(IRaporServisi rapor, IRandevuServisi randevular, UserManager<Kullanici> kullanicilar)
        {
            _rapor = rapor;
            _randevular = randevular;
            _kullanicilar = kullanicilar;
        }

        // GET /Admin?baslangic=2026-11-01&bitis=2026-11-30
        public async Task<IActionResult> Index(DateOnly? baslangic, DateOnly? bitis)
        {
            ViewData["Title"] = "Yönetim Paneli";
            ViewData["Ozet"] = await _rapor.PanelOzetiAsync();
            ViewData["Rapor"] = await _rapor.HizmetRaporuAsync(baslangic, bitis);
            ViewData["Bekleyenler"] = await _randevular.ListeleAsync(User.AktifKullaniciAl(), RandevuDurumu.Bekliyor);
            ViewData["Baslangic"] = baslangic?.ToString("yyyy-MM-dd");
            ViewData["Bitis"] = bitis?.ToString("yyyy-MM-dd");
            return View();
        }

        public async Task<IActionResult> Kullanicilar()
        {
            ViewData["Title"] = "Kullanıcılar";
            var liste = await _kullanicilar.Users.AsNoTracking().OrderBy(k => k.AdSoyad).ToListAsync();

            var satirlar = new List<(Kullanici Kullanici, IList<string> Roller)>();
            foreach (var k in liste)
                satirlar.Add((k, await _kullanicilar.GetRolesAsync(k)));

            return View(satirlar);
        }
    }
}
