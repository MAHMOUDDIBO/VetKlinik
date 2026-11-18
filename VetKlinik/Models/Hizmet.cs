namespace VetKlinik.Models
{
    public class Hizmet
    {
        public int HizmetId { get; set; }
        public string HizmetAdi {  get; set; }

        public int SureDakika {get; set; }

        public decimal Ucret {  get; set; }
    }
}
