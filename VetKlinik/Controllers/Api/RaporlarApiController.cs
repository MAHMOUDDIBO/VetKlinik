using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Guvenlik;
using VetKlinik.Services;

namespace VetKlinik.Controllers.Api
{
    [ApiController]
    [Route("api/raporlar")]
    [Authorize(Roles = Roller.Admin, AuthenticationSchemes = KisiselAnahtarDogrulayici.CerezVeyaAnahtar)]
    public class RaporlarApiController : ControllerBase
    {
        private readonly IRaporServisi _rapor;

        public RaporlarApiController(IRaporServisi rapor)
        {
            _rapor = rapor;
        }

        // GET /api/raporlar/hizmet?baslangic=2026-11-01&bitis=2026-11-30
        // Hizmet türüne göre randevu sayısı ve ciro → 200 | 400 | 401 | 403
        [HttpGet("hizmet")]
        public async Task<ActionResult<List<HizmetRaporSatiri>>> HizmetRaporu([FromQuery] DateOnly? baslangic, [FromQuery] DateOnly? bitis)
        {
            if (baslangic is not null && bitis is not null && bitis < baslangic)
                return BadRequest(new { hata = "bitis, baslangic'tan önce olamaz." });

            return await _rapor.HizmetRaporuAsync(baslangic, bitis);
        }
    }
}
