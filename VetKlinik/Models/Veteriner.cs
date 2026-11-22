namespace VetKlinik.Models
{
    public class Veteriner
    {
        public int VeterinerId { get; set; }
        public string VeterinerAdSoyad { get; set; } = "";
        public string UzmanAlani { get; set; } = "";     // Küçük hayvan, Cerrahi, Dermatoloji...

        // Çoka-çok: hekimin verebildiği hizmetler
        public List<Hizmet> Hizmetler { get; set; } = new();

        // Bire-çok: haftalık çalışma aralıkları
        public List<Musaitlik> Musaitlikler { get; set; } = new();

        public List<Randevu> Randevular { get; set; } = new();
    }
}
