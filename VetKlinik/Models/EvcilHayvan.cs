namespace VetKlinik.Models
{
    public class EvcilHayvan
    {
        public int EvcilHayvanId { get; set; }

        public string EvcilHayvanAdi { get; set; } = "";

        public string Tur { get; set; } = "";   // Kedi, Köpek, Kuş...

        public string Irk { get; set; } = "";

        public DateTime DogumTarihi { get; set; }

        public decimal Kilo { get; set; }       // 4.5 kg gibi ondalıklı olabilir

        // Hayvanın sahibi. Identity'de kullanıcı Id'si string olduğu için string.
        public string SahipId { get; set; } = "";
    }
}