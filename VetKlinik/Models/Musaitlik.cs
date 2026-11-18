namespace VetKlinik.Models
{
    public class Musaitlik
    {
        public int MusaitlikId { get; set; }

        public int VeterinerId { get; set; }

        public DayOfWeek Gun { get; set; }          // Monday, Tuesday...

        public TimeOnly Baslangic { get; set; }     // 09:00

        public TimeOnly Bitis { get; set; }         // 17:00
    }
}