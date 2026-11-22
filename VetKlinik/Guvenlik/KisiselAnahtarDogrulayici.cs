using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VetKlinik.Models;

namespace VetKlinik.Guvenlik
{
    // "Authorization: Bearer vk_..." başlığıyla gelen isteği doğrular.
    // MCP istemcileri (Claude Desktop, MCP Inspector) ve API istemcileri çerez taşımaz;
    // kendilerini bu anahtarla tanıtırlar.
    public class KisiselAnahtarDogrulayici : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SemaAdi = "KisiselAnahtar";

        // [Authorize(AuthenticationSchemes = ...)] için: tarayıcı çerezi VEYA anahtar
        public const string CerezVeyaAnahtar = "Identity.Application," + SemaAdi;

        private readonly UserManager<Kullanici> _kullanicilar;
        private readonly IUserClaimsPrincipalFactory<Kullanici> _fabrika;

        public KisiselAnahtarDogrulayici(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder,
            UserManager<Kullanici> kullanicilar, IUserClaimsPrincipalFactory<Kullanici> fabrika)
            : base(options, logger, encoder)
        {
            _kullanicilar = kullanicilar;
            _fabrika = fabrika;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string? baslik = Request.Headers.Authorization;
            if (string.IsNullOrEmpty(baslik) || !baslik.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return AuthenticateResult.NoResult();   // bu şemaya ait değil

            var anahtar = baslik["Bearer ".Length..].Trim();
            if (anahtar.Length == 0)
                return AuthenticateResult.Fail("Anahtar boş.");

            var ozet = AnahtarYardimcisi.Ozetle(anahtar);
            var kullanici = await _kullanicilar.Users.FirstOrDefaultAsync(k => k.ApiAnahtariOzeti == ozet);
            if (kullanici is null)
                return AuthenticateResult.Fail("Geçersiz anahtar.");

            // Çerezle girişteki claim'lerin AYNISI (roller, VeterinerId...)
            var asil = await _fabrika.CreateAsync(kullanici);
            var kimlik = new ClaimsIdentity(asil.Claims, Scheme.Name);
            return AuthenticateResult.Success(
                new AuthenticationTicket(new ClaimsPrincipal(kimlik), Scheme.Name));
        }
    }
}
