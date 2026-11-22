using VetKlinik.Models;

namespace VetKlinik.Services
{
    // Hizmet işlemlerinin sözleşmesi. MVC, REST API ve MCP aynı arayüzü kullanır.
    public interface IHizmetServisi
    {
        Task<List<Hizmet>> ListeleAsync();
        Task<Hizmet?> GetirAsync(int id);
        Task<bool> AyniAdVarMiAsync(string hizmetAdi, int haricId = 0);
        Task EkleAsync(Hizmet hizmet);
        Task<bool> GuncelleAsync(Hizmet hizmet);
        Task<IslemSonucu> SilAsync(int id);
    }
}
