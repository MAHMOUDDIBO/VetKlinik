using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Dtos;
using VetKlinik.Guvenlik;
using VetKlinik.Services;

namespace VetKlinik.Controllers.Api
{
    [ApiController]
    [Route("api/randevular")]
    [Authorize(AuthenticationSchemes = KisiselAnahtarDogrulayici.CerezVeyaAnahtar)]
    public class RandevularApiController : ControllerBase
    {
        private readonly IRandevuServisi _servis;

        public RandevularApiController(IRandevuServisi servis)
        {
            _servis = servis;
        }

        // GET /api/randevular → 200
        [HttpGet]
        public async Task<ActionResult<List<RandevuDto>>> Listele() =>
            (await _servis.ListeleAsync(User.AktifKullaniciAl())).Select(RandevuDto.Den).ToList();

        // GET /api/randevular/5 → 200 | 404
        [HttpGet("{id:int}")]
        public async Task<ActionResult<RandevuDto>> Getir(int id)
        {
            var r = await _servis.GetirAsync(id, User.AktifKullaniciAl());
            return r is null ? NotFound() : RandevuDto.Den(r);
        }

        // POST /api/randevular → 201 Created | 400 (çakışma, doğrulama) | 404 (hayvan/hizmet yok)
        //
        // Veri YAZAN bu uç YALNIZCA Bearer anahtarı kabul eder: çerez tarayıcı tarafından
        // otomatik gönderildiği için çerezle yazmaya izin vermek CSRF açığı olurdu.
        [HttpPost]
        [Authorize(AuthenticationSchemes = KisiselAnahtarDogrulayici.SemaAdi)]
        public async Task<ActionResult<RandevuDto>> Olustur(RandevuOlusturDto girdi)
        {
            // [ApiController] sayesinde [Required] ihlali buraya gelmeden 400 döner
            var talep = new RandevuTalebi(girdi.EvcilHayvanId!.Value, girdi.HizmetId!.Value,
                girdi.VeterinerId!.Value, girdi.Baslangic!.Value);

            var sonuc = await _servis.OlusturAsync(talep, User.AktifKullaniciAl());
            if (sonuc.Bulunamadi) return NotFound(new { hata = sonuc.Hata });
            if (!sonuc.Basarili) return BadRequest(new { hata = sonuc.Hata });

            var dto = RandevuDto.Den(sonuc.Veri!);
            return CreatedAtAction(nameof(Getir), new { id = dto.Id }, dto);
        }
    }
}
