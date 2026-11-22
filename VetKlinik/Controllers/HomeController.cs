using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Models;
using VetKlinik.Services;

namespace VetKlinik.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHizmetServisi _hizmetler;
        private readonly IVeterinerServisi _veterinerler;

        public HomeController(IHizmetServisi hizmetler, IVeterinerServisi veterinerler)
        {
            _hizmetler = hizmetler;
            _veterinerler = veterinerler;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Ana Sayfa";
            ViewData["Veterinerler"] = await _veterinerler.ListeleAsync();
            return View(await _hizmetler.ListeleAsync());
        }

        public IActionResult Privacy()
        {
            ViewData["Title"] = "Gizlilik";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
