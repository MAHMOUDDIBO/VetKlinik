using System.ComponentModel.DataAnnotations;

namespace VetKlinik.ViewModels
{
    public class AsiFormVM : IValidatableObject
    {
        [Required(ErrorMessage = "{0} boş bırakılamaz.")]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "{0} {2} ile {1} karakter arasında olmalıdır.")]
        [Display(Name = "Aşı adı")]
        public string AsiAdi { get; set; } = "";

        [Required(ErrorMessage = "{0} boş bırakılamaz.")]
        [DataType(DataType.Date)]
        [Display(Name = "Yapıldığı tarih")]
        public DateTime? YapildigiTarih { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Sonraki doz tarihi")]
        public DateTime? SonrakiDozTarihi { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (YapildigiTarih is not null && YapildigiTarih.Value.Date > DateTime.Today)
                yield return new ValidationResult("Aşı gelecekte yapılmış olamaz.", new[] { nameof(YapildigiTarih) });

            if (YapildigiTarih is not null && SonrakiDozTarihi is not null && SonrakiDozTarihi <= YapildigiTarih)
                yield return new ValidationResult("Sonraki doz, yapıldığı tarihten sonra olmalıdır.", new[] { nameof(SonrakiDozTarihi) });
        }
    }
}
