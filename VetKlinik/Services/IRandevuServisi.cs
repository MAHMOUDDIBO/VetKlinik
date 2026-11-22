using VetKlinik.Guvenlik;
using VetKlinik.Models;

namespace VetKlinik.Services
{
    // Randevu talebi: kim (hayvan), ne (hizmet), kimde (hekim), ne zaman
    public record RandevuTalebi(int EvcilHayvanId, int HizmetId, int VeterinerId, DateTime Baslangic);

    // Bir hekimin o gün boş olan başlangıç saatleri
    public record UygunHekim(int VeterinerId, string AdSoyad, string UzmanAlani, List<TimeOnly> Saatler);

    public interface IRandevuServisi
    {
        Task<List<UygunHekim>> UygunSaatlerAsync(int hizmetId, DateOnly tarih, int? veterinerId = null);
        Task<string?> CakismaNedeniAsync(int veterinerId, int hizmetId, DateTime baslangic, int? evcilHayvanId, int? haricRandevuId = null);

        Task<List<Randevu>> ListeleAsync(AktifKullanici k, RandevuDurumu? durum = null);
        Task<Randevu?> GetirAsync(int id, AktifKullanici k);

        Task<IslemSonucu<Randevu>> OlusturAsync(RandevuTalebi talep, AktifKullanici k);
        Task<IslemSonucu> GuncelleAsync(int id, RandevuTalebi talep, AktifKullanici k);
        Task<IslemSonucu> OnaylaAsync(int id, AktifKullanici k);
        Task<IslemSonucu> ReddetAsync(int id, AktifKullanici k);
        Task<IslemSonucu> TamamlaAsync(int id, string muayeneNotu, AktifKullanici k);
        Task<IslemSonucu> IptalAsync(int id, AktifKullanici k);
        Task<IslemSonucu> SilAsync(int id, AktifKullanici k);

        bool YonetebilirMi(Randevu r, AktifKullanici k);
        bool SahibiMi(Randevu r, AktifKullanici k);
    }
}
