using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Dtos;
using VetKlinik.Services;

namespace VetKlinik.Controllers.Api
{
    [ApiController]
    [Route("api/veterinerler")]
    public class VeterinerlerApiController : ControllerBase
    {
        private readonly IVeterinerServisi _veterinerler;
        private readonly IHizmetServisi _hizmetler;
        private readonly IRandevuServisi _randevular;

        public VeterinerlerApiController(IVeterinerServisi veterinerler, IHizmetServisi hizmetler, IRandevuServisi randevular)
        {
            _veterinerler = veterinerler;
            _hizmetler = hizmetler;
            _randevular = randevular;
        }

        // GET /api/veterinerler              → tüm hekimler
        // GET /api/veterinerler?hizmetId=3   → LINQ filtresi: o hizmeti verebilenler
        [HttpGet]
        public async Task<ActionResult<List<VeterinerDto>>> Listele([FromQuery] int? hizmetId) =>
            (await _veterinerler.ListeleAsync(hizmetId)).Select(VeterinerDto.Den).ToList();

        // GET /api/veterinerler/2 → 200 | 404
        [HttpGet("{id:int}")]
        public async Task<ActionResult<VeterinerDto>> Getir(int id)
        {
            var v = await _veterinerler.GetirAsync(id);
            return v is null ? NotFound() : VeterinerDto.Den(v);
        }

        // GET /api/veterinerler/uygun?hizmetId=1&tarih=2026-11-23
        // Belirli bir tarihte, seçilen hizmet için uygun hekimler ve boş saatleri.
        // Randevu Al sayfası bu ucu jQuery ile çağırır.  → 200 | 400 | 404
        [HttpGet("uygun")]
        public async Task<ActionResult<UygunlukDto>> Uygun([FromQuery] int? hizmetId, [FromQuery] string? tarih)
        {
            if (hizmetId is null)
                return BadRequest(new { hata = "hizmetId zorunludur." });
            if (!DateOnly.TryParseExact(tarih, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var gun))
                return BadRequest(new { hata = "tarih yyyy-MM-dd biçiminde olmalıdır." });

            var hizmet = await _hizmetler.GetirAsync(hizmetId.Value);
            if (hizmet is null)
                return NotFound(new { hata = "Hizmet bulunamadı." });

            var hekimler = await _randevular.UygunSaatlerAsync(hizmet.HizmetId, gun);
            return new UygunlukDto(hizmet.HizmetId, hizmet.HizmetAdi, hizmet.SureDakika,
                gun.ToString("yyyy-MM-dd"), hekimler.Select(UygunHekimDto.Den).ToList());
        }
    }
}
