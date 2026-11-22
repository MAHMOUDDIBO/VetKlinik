using System.ComponentModel.DataAnnotations;

namespace VetKlinik.ViewModels
{
    // Düzenleme formu
    public class VeterinerFormVM
    {
        [Required(ErrorMessage = "{0} boş bırakılamaz.")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "{0} {2} ile {1} karakter arasında olmalıdır.")]
        [Display(Name = "Ad Soyad")]
        public string VeterinerAdSoyad { get; set; } = "";

        [Required(ErrorMessage = "{0} boş bırakılamaz.")]
        [StringLength(50, ErrorMessage = "{0} en fazla {1} karakter olabilir.")]
        [Display(Name = "Uzmanlık alanı")]
        public string UzmanAlani { get; set; } = "";

        [Display(Name = "Verebildiği hizmetler")]
        public List<int> SecilenHizmetIdleri { get; set; } = new();

        public static readonly string[] UzmanlikAlanlari =
            { "Küçük Hayvan", "Büyük Hayvan", "Egzotik Hayvan", "Cerrahi", "Dermatoloji", "Diş Hekimliği", "Dahiliye" };
    }

    // Ekleme formu = düzenleme formu + giriş hesabı bilgileri
    public class VeterinerEkleVM : VeterinerFormVM
    {
        [Required(ErrorMessage = "{0} boş bırakılamaz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
        [Display(Name = "Giriş e-postası")]
        public string Eposta { get; set; } = "";

        [Required(ErrorMessage = "{0} boş bırakılamaz.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "{0} en az {2} karakter olmalıdır.")]
        [DataType(DataType.Password)]
        [Display(Name = "Geçici şifre")]
        public string Sifre { get; set; } = "";
    }

    public class MusaitlikFormVM : IValidatableObject
    {
        [Required(ErrorMessage = "Gün seçin.")]
        [Display(Name = "Gün")]
        public DayOfWeek? Gun { get; set; }

        [Required(ErrorMessage = "{0} boş bırakılamaz.")]
        [Display(Name = "Başlangıç")]
        public TimeOnly? Baslangic { get; set; }

        [Required(ErrorMessage = "{0} boş bırakılamaz.")]
        [Display(Name = "Bitiş")]
        public TimeOnly? Bitis { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Baslangic is not null && Bitis is not null && Bitis <= Baslangic)
                yield return new ValidationResult("Bitiş saati başlangıçtan sonra olmalıdır.", new[] { nameof(Bitis) });
        }
    }
}
