namespace VetKlinik.Models
{
    // Klinikte verilen hizmet: muayene, aşı, kısırlaştırma...
    // Süre, randevu çakışma hesabında kullanılır.
    public class Hizmet
    {
        public int HizmetId { get; set; }
        public string HizmetAdi { get; set; } = "";
        public int SureDakika { get; set; }
        public decimal Ucret { get; set; }

        // Çoka-çok: bu hizmeti verebilen hekimler (ara tablo: VeterinerHizmetleri)
        public List<Veteriner> Veterinerler { get; set; } = new();
    }
}
