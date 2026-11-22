using System.ComponentModel.DataAnnotations;

namespace VetKlinik.ViewModels
{
    // Entity değil ViewModel: kullanıcının gönderebileceği alanlar YALNIZCA bunlar (over-posting koruması).
    // Sayılar nullable: boş gönderilirse [Required] mesajı görünsün.
    public class HizmetFormVM
    {
        [Required(ErrorMessage = "{0} boş bırakılamaz.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "{0} {2} ile {1} karakter arasında olmalıdır.")]
        [Display(Name = "Hizmet adı")]
        public string HizmetAdi { get; set; } = "";

        [Required(ErrorMessage = "{0} boş bırakılamaz.")]
        [Range(5, 480, ErrorMessage = "{0} {1} ile {2} dakika arasında olmalıdır.")]
        [Display(Name = "Süre (dakika)")]
        public int? SureDakika { get; set; }

        [Required(ErrorMessage = "{0} boş bırakılamaz.")]
        [Range(0.0, 100000.0, ErrorMessage = "{0} {1} ile {2} arasında olmalıdır.")]
        [Display(Name = "Ücret (TL)")]
        public decimal? Ucret { get; set; }
    }
}
