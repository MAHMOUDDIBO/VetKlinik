using System.ComponentModel.DataAnnotations;
using VetKlinik.Models;
using VetKlinik.Services;

namespace VetKlinik.Dtos
{
    // API entity DEĞİL DTO döndürür:
    //  - Sahip, parola özeti gibi iç alanlar dışarı sızmaz
    //  - Gezinme özelliklerindeki döngüler (Hekim → Randevu → Hekim...) JSON'u patlatmaz
    //  - Veritabanı değişse de API sözleşmesi aynı kalır

    public record HizmetDto(int Id, string Ad, int SureDakika, decimal Ucret)
    {
        public static HizmetDto Den(Hizmet h) => new(h.HizmetId, h.HizmetAdi, h.SureDakika, h.Ucret);
    }

    public record CalismaAraligiDto(string Gun, string Baslangic, string Bitis);

    public record VeterinerDto(int Id, string AdSoyad, string UzmanAlani, List<string> Hizmetler, List<CalismaAraligiDto> CalismaSaatleri)
    {
        public static VeterinerDto Den(Veteriner v) => new(
            v.VeterinerId, v.VeterinerAdSoyad, v.UzmanAlani,
            v.Hizmetler.OrderBy(h => h.HizmetAdi).Select(h => h.HizmetAdi).ToList(),
            v.Musaitlikler.OrderBy(m => ((int)m.Gun + 6) % 7).ThenBy(m => m.Baslangic)
                .Select(m => new CalismaAraligiDto(GunAdlari.Ad(m.Gun), m.Baslangic.ToString("HH:mm"), m.Bitis.ToString("HH:mm")))
                .ToList());
    }

    public record UygunHekimDto(int VeterinerId, string AdSoyad, string UzmanAlani, List<string> Saatler)
    {
        public static UygunHekimDto Den(UygunHekim u) =>
            new(u.VeterinerId, u.AdSoyad, u.UzmanAlani, u.Saatler.Select(s => s.ToString("HH:mm")).ToList());
    }

    public record UygunlukDto(int HizmetId, string HizmetAdi, int SureDakika, string Tarih, List<UygunHekimDto> Hekimler);

    public record EvcilHayvanDto(int Id, string Ad, string Tur, string Irk, DateOnly DogumTarihi, decimal Kilo)
    {
        public static EvcilHayvanDto Den(EvcilHayvan h) =>
            new(h.EvcilHayvanId, h.EvcilHayvanAdi, h.Tur, h.Irk, DateOnly.FromDateTime(h.DogumTarihi), h.Kilo);
    }

    public record AsiDto(int Id, string AsiAdi, DateOnly YapildigiTarih, DateOnly? SonrakiDozTarihi)
    {
        public static AsiDto Den(AsiKaydi a) => new(a.AsiKaydiId, a.AsiAdi,
            DateOnly.FromDateTime(a.YapildigiTarih),
            a.SonrakiDozTarihi is null ? null : DateOnly.FromDateTime(a.SonrakiDozTarihi.Value));
    }

    public record AsiTakvimiDto(int HayvanId, string HayvanAdi, List<AsiDto> Gecmis, List<AsiDto> Yaklasan)
    {
        public static AsiTakvimiDto Den(AsiTakvimi t) => new(
            t.Hayvan.EvcilHayvanId, t.Hayvan.EvcilHayvanAdi,
            t.Gecmis.Select(AsiDto.Den).ToList(),
            t.Yaklasan.Select(AsiDto.Den).ToList());
    }

    public record RandevuDto(int Id, string EvcilHayvan, string Hizmet, string Veteriner,
        DateTime Baslangic, DateTime Bitis, int SureDakika, decimal Ucret, string Durum, string? MuayeneNotu)
    {
        public static RandevuDto Den(Randevu r) => new(
            r.RandevuId, r.EvcilHayvan.EvcilHayvanAdi, r.Hizmet.HizmetAdi, r.Veteriner.VeterinerAdSoyad,
            r.Baslangic, r.Bitis, r.SureDakika, r.Ucret, r.Durum.ToString(), r.MuayeneNotu);
    }

    // API'ye GELEN veri de doğrulanır: [ApiController] geçersiz modeli otomatik 400 ile reddeder
    public class RandevuOlusturDto
    {
        [Required] public int? EvcilHayvanId { get; set; }
        [Required] public int? HizmetId { get; set; }
        [Required] public int? VeterinerId { get; set; }
        [Required] public DateTime? Baslangic { get; set; }
    }

    public static class GunAdlari
    {
        private static readonly string[] Adlar = { "Pazar", "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi" };
        public static string Ad(DayOfWeek gun) => Adlar[(int)gun];

        // Pazartesi'den başlayan hafta sırası (seçim listeleri için)
        public static readonly DayOfWeek[] HaftaSirasi =
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
            DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
        };
    }
}
