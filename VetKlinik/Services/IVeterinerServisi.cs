using VetKlinik.Models;

namespace VetKlinik.Services
{
    public interface IVeterinerServisi
    {
        Task<List<Veteriner>> ListeleAsync(int? hizmetId = null);
        Task<Veteriner?> GetirAsync(int id);
        Task<IslemSonucu<Veteriner>> EkleAsync(Veteriner veteriner, IEnumerable<int> hizmetIdleri, string eposta, string sifre);
        Task<IslemSonucu> GuncelleAsync(int id, string adSoyad, string uzmanAlani, IEnumerable<int> hizmetIdleri);
        Task<IslemSonucu> SilAsync(int id);
        Task<IslemSonucu> MusaitlikEkleAsync(Musaitlik musaitlik);
        Task<IslemSonucu<int>> MusaitlikSilAsync(int musaitlikId);
    }
}
