using System.ComponentModel.DataAnnotations;

namespace VetKlinik.ViewModels
{
    public class GirisVM
    {
        [Required(ErrorMessage = "{0} boş bırakılamaz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
        [Display(Name = "E-posta")]
        public string Eposta { get; set; } = "";

        [Required(ErrorMessage = "{0} boş bırakılamaz.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; } = "";

        [Display(Name = "Beni hatırla")]
        public bool BeniHatirla { get; set; }

        public string? DonusAdresi { get; set; }
    }

    public class KayitVM
    {
        [Required(ErrorMessage = "{0} boş bırakılamaz.")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "{0} {2} ile {1} karakter arasında olmalıdır.")]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; } = "";

        [Required(ErrorMessage = "{0} boş bırakılamaz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
        [Display(Name = "E-posta")]
        public string Eposta { get; set; } = "";

        [Required(ErrorMessage = "{0} boş bırakılamaz.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "{0} en az {2} karakter olmalıdır.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; } = "";

        [Compare(nameof(Sifre), ErrorMessage = "Şifreler eşleşmiyor.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre (tekrar)")]
        public string SifreTekrar { get; set; } = "";
    }
}
