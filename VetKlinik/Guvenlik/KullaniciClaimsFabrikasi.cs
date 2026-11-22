using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using VetKlinik.Models;

namespace VetKlinik.Guvenlik
{
    // Giriş anında çereze yazılan claim'lere AdSoyad ve VeterinerId'yi ekler.
    // Böylece her request'te veritabanına "bu kullanıcı hangi hekim?" diye sormayız.
    public class KullaniciClaimsFabrikasi : UserClaimsPrincipalFactory<Kullanici, IdentityRole>
    {
        public KullaniciClaimsFabrikasi(UserManager<Kullanici> userManager,
            RoleManager<IdentityRole> roleManager, IOptions<IdentityOptions> options)
            : base(userManager, roleManager, options) { }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(Kullanici user)
        {
            var kimlik = await base.GenerateClaimsAsync(user);   // Id, e-posta, ROLLER
            kimlik.AddClaim(new Claim(ClaimsUzantilari.AdSoyadClaim, user.AdSoyad));
            if (user.VeterinerId is int vetId)
                kimlik.AddClaim(new Claim(ClaimsUzantilari.VeterinerIdClaim, vetId.ToString()));
            return kimlik;
        }
    }
}
