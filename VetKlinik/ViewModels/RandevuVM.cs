using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace VetKlinik.ViewModels
{
    // Randevu alma / düzenleme formu.
    // Secim = "hekimId|SS:dd" — kullanıcı uygun saatler listesinden birine tıklar (API'den gelir).
    public class RandevuFormVM : IValidatableObject
    {
        [Required(ErrorMessage = "Evcil hayvan seçin.")]
        [Display(Name = "Evcil hayvan")]
        public int? EvcilHayvanId { get; set; }

        [Required(ErrorMessage = "Hizmet seçin.")]
        [Display(Name = "Hizmet")]
        public int? HizmetId { get; set; }

        [Required(ErrorMessage = "Tarih seçin.")]
        [DataType(DataType.Date)]
        [Display(Name = "Tarih")]
        public DateTime? Tarih { get; set; }

        [Required(ErrorMessage = "Listeden bir hekim ve saat seçin.")]
        [Display(Name = "Hekim ve saat")]
        public string? Secim { get; set; }

        // "3|10:30" → (3, 10:30). Biçim bozuksa false.
        public bool SecimiCoz(out int veterinerId, out TimeOnly saat)
        {
            veterinerId = 0;
            saat = default;
            var parcalar = (Secim ?? "").Split('|');
            return parcalar.Length == 2
                   && int.TryParse(parcalar[0], out veterinerId)
                   && TimeOnly.TryParseExact(parcalar[1], "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out saat);
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Tarih is not null && Tarih.Value.Date < DateTime.Today)
                yield return new ValidationResult("Geçmiş bir tarih seçilemez.", new[] { nameof(Tarih) });

            if (!string.IsNullOrEmpty(Secim) && !SecimiCoz(out _, out _))
                yield return new ValidationResult("Geçersiz saat seçimi.", new[] { nameof(Secim) });
        }
    }

    public class RandevuTamamlaVM
    {
        [Required(ErrorMessage = "{0} boş bırakılamaz.")]
        [StringLength(2000, MinimumLength = 5, ErrorMessage = "{0} {2} ile {1} karakter arasında olmalıdır.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Muayene notu")]
        public string MuayeneNotu { get; set; } = "";
    }

    // Hafta 7 — Session: kullanıcının son seçimleri (form bir sonraki açılışta hazır gelsin)
    public class RandevuTaslagi
    {
        public int? EvcilHayvanId { get; set; }
        public int? HizmetId { get; set; }
    }
}
