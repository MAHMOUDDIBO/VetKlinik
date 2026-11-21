using VetKlinik.Models;

namespace VetKlinik.Services
{
    // Hizmet işlemlerinin sözleşmesi: NE yapılabilir?
    // MVC, REST API ve MCP aynı arayüzü kullanacak.
    public interface IHizmetServisi
    {
        List<Hizmet> Listele();
        Hizmet? Getir(int id);
        void Ekle(Hizmet hizmet);
        bool Guncelle(Hizmet hizmet);
        bool Sil(int id);
        bool AyniAdVarMi(string hizmetAdi, int haricId = 0);
    }
}