using System.Security.Claims;

namespace VetKlinik.Guvenlik
{
    // Servis katmanına giden "kim istiyor?" bilgisi.
    // Servisler HttpContext bilmez; MVC, API ve MCP aynı nesneyi verir.
    public record AktifKullanici(string Id, bool AdminMi, int? VeterinerId)
    {
        public bool VeterinerMi => VeterinerId.HasValue;
    }

    public static class ClaimsUzantilari
    {
        public const string VeterinerIdClaim = "VeterinerId";
        public const string AdSoyadClaim = "AdSoyad";

        public static AktifKullanici AktifKullaniciAl(this ClaimsPrincipal user)
        {
            var id = user.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new InvalidOperationException("Oturum açılmamış.");
            int? vetId = int.TryParse(user.FindFirstValue(VeterinerIdClaim), out var v) ? v : null;
            return new AktifKullanici(id, user.IsInRole(Roller.Admin), vetId);
        }

        public static string AdSoyad(this ClaimsPrincipal user) =>
            user.FindFirstValue(AdSoyadClaim) ?? user.Identity?.Name ?? "";
    }
}
