namespace VetKlinik.Models
{
    // Hekimin haftanın bir günündeki çalışma aralığı. Örn: Pazartesi 09:00–17:00
    // Randevu yalnızca bu aralıklara alınabilir.
    public class Musaitlik
    {
        public int MusaitlikId { get; set; }
        public int VeterinerId { get; set; }
        public DayOfWeek Gun { get; set; }
        public TimeOnly Baslangic { get; set; }
        public TimeOnly Bitis { get; set; }

        public Veteriner Veteriner { get; set; } = null!;
    }
}
