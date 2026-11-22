using VetKlinik.Guvenlik;
using VetKlinik.Models;

namespace VetKlinik.Services
{
    // Her metot "kim istiyor?" bilgisini (AktifKullanici) alır.
    // Sahiplik kontrolü burada, TEK yerde yapılır: üye yalnızca kendi hayvanını görür.
    public interface IEvcilHayvanServisi
    {
        Task<List<EvcilHayvan>> ListeleAsync(AktifKullanici k);
        Task<EvcilHayvan?> GetirAsync(int id, AktifKullanici k);
        Task<List<EvcilHayvan>> AdIleBulAsync(string ad, AktifKullanici k);
        Task EkleAsync(EvcilHayvan hayvan, AktifKullanici k);
        Task<IslemSonucu> GuncelleAsync(EvcilHayvan degerler, AktifKullanici k);
        Task<IslemSonucu> SilAsync(int id, AktifKullanici k);
        bool DuzenleyebilirMi(EvcilHayvan hayvan, AktifKullanici k);
    }
}
