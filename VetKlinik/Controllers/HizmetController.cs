using Microsoft.AspNetCore.Mvc;
using VetKlinik.Services;

namespace VetKlinik.Controllers
{
    // Controller iş kuralı BİLMEZ: servise sorar, sonucu view'a verir.
    public class HizmetController : Controller
    {
        private readonly IHizmetServisi _servis;

        // Constructor injection: servisi biz oluşturmuyoruz, çatı veriyor
        public HizmetController(IHizmetServisi servis)
        {
            _servis = servis;
        }

        // GET /Hizmet
        public IActionResult Index()
        {
            ViewData["Title"] = "Hizmetler";
            return View(_servis.Listele());
        }

        // GET /Hizmet/Detay/3
        public IActionResult Detay(int id)
        {
            var hizmet = _servis.Getir(id);
            if (hizmet is null)
                return NotFound();          // 404

            ViewData["Title"] = hizmet.HizmetAdi;
            return View(hizmet);
        }
    }
}