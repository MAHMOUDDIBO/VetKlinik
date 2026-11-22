namespace VetKlinik.Services
{
    // Servisin cevabı: başarılı mı, değilse NEDEN?
    // Bulunamadi ayrı tutulur: MVC'de 404, API'de 404, iş kuralı ihlalinde 400.
    public class IslemSonucu
    {
        public bool Basarili { get; init; }
        public bool Bulunamadi { get; init; }
        public string? Hata { get; init; }

        public static IslemSonucu Tamam() => new() { Basarili = true };
        public static IslemSonucu Hatali(string hata) => new() { Hata = hata };
        public static IslemSonucu Yok(string hata = "Kayıt bulunamadı.") => new() { Bulunamadi = true, Hata = hata };
    }

    public class IslemSonucu<T> : IslemSonucu
    {
        public T? Veri { get; init; }

        public static IslemSonucu<T> Tamam(T veri) => new() { Basarili = true, Veri = veri };
        public new static IslemSonucu<T> Hatali(string hata) => new() { Hata = hata };
        public new static IslemSonucu<T> Yok(string hata = "Kayıt bulunamadı.") => new() { Bulunamadi = true, Hata = hata };
    }
}
