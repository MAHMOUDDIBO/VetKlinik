using VetKlinik.Guvenlik;
using VetKlinik.Models;

namespace VetKlinik.Services
{
    public record AsiTakvimi(EvcilHayvan Hayvan, List<AsiKaydi> Gecmis, List<AsiKaydi> Yaklasan);

    public interface IAsiServisi
    {
        Task<AsiTakvimi?> TakvimAsync(int hayvanId, AktifKullanici k);
        Task<AsiKaydi?> GetirAsync(int asiKaydiId, AktifKullanici k);
        Task<IslemSonucu> EkleAsync(AsiKaydi asi, AktifKullanici k);
        Task<IslemSonucu> GuncelleAsync(AsiKaydi degerler, AktifKullanici k);
        Task<IslemSonucu<int>> SilAsync(int asiKaydiId, AktifKullanici k);
    }
}
