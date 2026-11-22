using System.ComponentModel.DataAnnotations;

namespace VetKlinik.ViewModels
{
    // SahipId YOK: sahibi formdan değil oturumdan alınır (over-posting ile başkası adına kayıt engellenir).
    public class EvcilHayvanFormVM : IValidatableObject
    {
        [Required(ErrorMessage = "{0} boş bırakılamaz.")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "{0} {2} ile {1} karakter arasında olmalıdır.")]
        [Display(Name = "Adı")]
        public string EvcilHayvanAdi { get; set; } = "";

        [Required(ErrorMessage = "Tür seçin.")]
        [Display(Name = "Tür")]
        public string Tur { get; set; } = "";

        [StringLength(40, ErrorMessage = "{0} en fazla {1} karakter olabilir.")]
        [Display(Name = "Irk")]
        public string? Irk { get; set; }

        [Required(ErrorMessage = "{0} boş bırakılamaz.")]
        [DataType(DataType.Date)]
        [Display(Name = "Doğum tarihi")]
        public DateTime? DogumTarihi { get; set; }

        [Required(ErrorMessage = "{0} boş bırakılamaz.")]
        [Range(0.01, 200.0, ErrorMessage = "{0} {1} ile {2} kg arasında olmalıdır.")]
        [Display(Name = "Kilo (kg)")]
        public decimal? Kilo { get; set; }

        public static readonly string[] Turler =
            { "Kedi", "Köpek", "Kuş", "Tavşan", "Hamster", "Sürüngen", "Balık", "Diğer" };

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DogumTarihi is not null && DogumTarihi.Value.Date > DateTime.Today)
                yield return new ValidationResult("Doğum tarihi gelecekte olamaz.", new[] { nameof(DogumTarihi) });

            if (!string.IsNullOrEmpty(Tur) && !Turler.Contains(Tur))
                yield return new ValidationResult("Listeden bir tür seçin.", new[] { nameof(Tur) });
        }
    }
}
