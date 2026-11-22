using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Dtos;
using VetKlinik.Guvenlik;
using VetKlinik.Services;

namespace VetKlinik.Controllers.Api
{
    // Tarayıcı çereziyle VEYA "Authorization: Bearer vk_..." anahtarıyla erişilir.
    [ApiController]
    [Route("api/evcilhayvanlar")]
    [Authorize(AuthenticationSchemes = KisiselAnahtarDogrulayici.CerezVeyaAnahtar)]
    public class EvcilHayvanlarApiController : ControllerBase
    {
        private readonly IEvcilHayvanServisi _hayvanlar;
        private readonly IAsiServisi _asilar;

        public EvcilHayvanlarApiController(IEvcilHayvanServisi hayvanlar, IAsiServisi asilar)
        {
            _hayvanlar = hayvanlar;
            _asilar = asilar;
        }

        // GET /api/evcilhayvanlar → kullanıcının görebildiği hayvanlar
        [HttpGet]
        public async Task<ActionResult<List<EvcilHayvanDto>>> Listele() =>
            (await _hayvanlar.ListeleAsync(User.AktifKullaniciAl())).Select(EvcilHayvanDto.Den).ToList();

        // GET /api/evcilhayvanlar/1/asilar → aşı geçmişi + yaklaşan aşılar. Başkasının hayvanı → 404
        [HttpGet("{id:int}/asilar")]
        public async Task<ActionResult<AsiTakvimiDto>> Asilar(int id)
        {
            var takvim = await _asilar.TakvimAsync(id, User.AktifKullaniciAl());
            return takvim is null ? NotFound() : AsiTakvimiDto.Den(takvim);
        }
    }
}
