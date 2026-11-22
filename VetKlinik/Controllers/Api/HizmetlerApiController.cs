using Microsoft.AspNetCore.Mvc;
using VetKlinik.Dtos;
using VetKlinik.Services;

namespace VetKlinik.Controllers.Api
{
    // REST API — herkese açık hizmet listesi
    [ApiController]
    [Route("api/hizmetler")]
    public class HizmetlerApiController : ControllerBase
    {
        private readonly IHizmetServisi _servis;

        public HizmetlerApiController(IHizmetServisi servis)
        {
            _servis = servis;
        }

        // GET /api/hizmetler → 200
        [HttpGet]
        public async Task<ActionResult<List<HizmetDto>>> Listele() =>
            (await _servis.ListeleAsync()).Select(HizmetDto.Den).ToList();

        // GET /api/hizmetler/3 → 200 | 404
        [HttpGet("{id:int}")]
        public async Task<ActionResult<HizmetDto>> Getir(int id)
        {
            var h = await _servis.GetirAsync(id);
            return h is null ? NotFound() : HizmetDto.Den(h);
        }
    }
}
