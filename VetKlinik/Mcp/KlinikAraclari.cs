using System.ComponentModel;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using ModelContextProtocol.Server;
using VetKlinik.Dtos;
using VetKlinik.Guvenlik;
using VetKlinik.Services;

namespace VetKlinik.Mcp
{
    // MCP araçları: bir yapay zekâ asistanı (Claude Desktop, VS Code, MCP Inspector) bu metotları çağırır.
    //
    // ÖNEMLİ: Burada TEK bir iş kuralı yok. Her araç, MVC sayfalarının ve REST API'nin kullandığı
    // AYNI servisleri çağırır. Çakışma kontrolü, sahiplik kontrolü... hepsi serviste (ödev şartı).
    //
    // Kimlik: /mcp ucu "Authorization: Bearer vk_..." ister (Program.cs). Kullanıcıyı HttpContext'ten
    // alıp servise veriyoruz; servis yalnızca o kullanıcının verisini döndürür.
    [McpServerToolType]
    public class KlinikAraclari
    {
        private static readonly JsonSerializerOptions JsonAyar = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping   // Türkçe harfler okunur kalsın
        };

        private readonly IHizmetServisi _hizmetler;
        private readonly IEvcilHayvanServisi _hayvanlar;
        private readonly IAsiServisi _asilar;
        private readonly IRandevuServisi _randevular;
        private readonly IHttpContextAccessor _http;

        public KlinikAraclari(IHizmetServisi hizmetler, IEvcilHayvanServisi hayvanlar, IAsiServisi asilar,
            IRandevuServisi randevular, IHttpContextAccessor http)
        {
            _hizmetler = hizmetler;
            _hayvanlar = hayvanlar;
            _asilar = asilar;
            _randevular = randevular;
            _http = http;
        }

        private AktifKullanici K => _http.HttpContext!.User.AktifKullaniciAl();

        private static string Json(object o) => JsonSerializer.Serialize(o, JsonAyar);

        // ---------------------------------------------------------------
        // OKUYAN ARAÇLAR
        // ---------------------------------------------------------------

        [McpServerTool(Name = "hizmetler")]
        [Description("Klinikte verilen hizmetleri Id, süre (dakika) ve ücretleriyle listeler. " +
                     "Diğer araçlardaki hizmetId değerleri buradan alınır.")]
        public async Task<string> Hizmetler() =>
            Json((await _hizmetler.ListeleAsync()).Select(HizmetDto.Den));

        [McpServerTool(Name = "hayvanlarim")]
        [Description("Oturum sahibinin kayıtlı evcil hayvanlarını listeler.")]
        public async Task<string> Hayvanlarim() =>
            Json((await _hayvanlar.ListeleAsync(K)).Select(EvcilHayvanDto.Den));

        [McpServerTool(Name = "uygun_saatler")]
        [Description("Belirtilen gün ve hizmet için boş olan hekimleri ve başlangıç saatlerini listeler.")]
        public async Task<string> UygunSaatler(
            [Description("Hizmet Id'si (hizmetler aracından)")] int hizmetId,
            [Description("Tarih, yyyy-MM-dd biçiminde. Örn: 2026-11-23")] string tarih)
        {
            if (!DateOnly.TryParseExact(tarih, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var gun))
                return "Tarih yyyy-MM-dd biçiminde olmalıdır.";

            var hizmet = await _hizmetler.GetirAsync(hizmetId);
            if (hizmet is null)
                return $"{hizmetId} numaralı hizmet bulunamadı. Geçerli Id'ler için 'hizmetler' aracını kullanın.";

            var hekimler = await _randevular.UygunSaatlerAsync(hizmetId, gun);
            if (hekimler.Count == 0)
                return $"{gun:dd.MM.yyyy} ({GunAdlari.Ad(gun.DayOfWeek)}) günü \"{hizmet.HizmetAdi}\" için boş saat yok.";

            return Json(new UygunlukDto(hizmet.HizmetId, hizmet.HizmetAdi, hizmet.SureDakika,
                gun.ToString("yyyy-MM-dd"), hekimler.Select(UygunHekimDto.Den).ToList()));
        }

        [McpServerTool(Name = "asi_takvimi")]
        [Description("Evcil hayvanın geçmiş aşılarını ve yaklaşan (sonraki doz tarihi gelmemiş) aşılarını döner. " +
                     "Örn: \"Pamuk'un bir sonraki aşısı ne zaman?\"")]
        public async Task<string> AsiTakvimi(
            [Description("Evcil hayvanın adı. Örn: Pamuk")] string hayvanAdi)
        {
            var k = K;
            var bulunanlar = await _hayvanlar.AdIleBulAsync(hayvanAdi, k);
            if (bulunanlar.Count == 0)
                return $"\"{hayvanAdi}\" adında bir evcil hayvanınız bulunamadı.";

            var sonuc = new List<AsiTakvimiDto>();
            foreach (var h in bulunanlar)
            {
                var takvim = await _asilar.TakvimAsync(h.EvcilHayvanId, k);
                if (takvim is not null) sonuc.Add(AsiTakvimiDto.Den(takvim));
            }
            return Json(sonuc);
        }

        [McpServerTool(Name = "randevularim")]
        [Description("Oturum sahibinin randevularını (en yeni önce) durumlarıyla listeler.")]
        public async Task<string> Randevularim() =>
            Json((await _randevular.ListeleAsync(K)).Select(RandevuDto.Den));

        // ---------------------------------------------------------------
        // VERİ YAZAN ARAÇ
        // ---------------------------------------------------------------

        [McpServerTool(Name = "randevu_olustur")]
        [Description("\"Bekliyor\" durumunda yeni bir randevu oluşturur. Saat uygun değilse (hekim çalışmıyor, " +
                     "başka randevu var, hizmet süresi sığmıyor) randevu oluşturulmaz ve NEDENİ açıklanır. " +
                     "Önce uygun_saatler aracıyla boş saatleri kontrol etmek önerilir.")]
        public async Task<string> RandevuOlustur(
            [Description("Evcil hayvanın adı. Örn: Pamuk")] string hayvanAdi,
            [Description("Hizmet Id'si (hizmetler aracından)")] int hizmetId,
            [Description("Hekim Id'si (uygun_saatler aracından)")] int veterinerId,
            [Description("Tarih, yyyy-MM-dd. Örn: 2026-11-23")] string tarih,
            [Description("Başlangıç saati, SS:dd. Örn: 10:30")] string saat)
        {
            if (!DateOnly.TryParseExact(tarih, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var gun))
                return "Tarih yyyy-MM-dd biçiminde olmalıdır.";
            if (!TimeOnly.TryParseExact(saat, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var baslangicSaati))
                return "Saat SS:dd biçiminde olmalıdır (örn: 10:30).";

            var k = K;
            var hayvanlar = await _hayvanlar.AdIleBulAsync(hayvanAdi, k);
            // Admin/hekim tüm hastaları görür; randevuyu yalnızca sahibin hayvanına açıyoruz
            var hayvan = hayvanlar.FirstOrDefault(h => h.SahipId == k.Id) ?? (k.AdminMi ? hayvanlar.FirstOrDefault() : null);
            if (hayvan is null)
                return $"\"{hayvanAdi}\" adında bir evcil hayvanınız bulunamadı.";

            var talep = new RandevuTalebi(hayvan.EvcilHayvanId, hizmetId, veterinerId, gun.ToDateTime(baslangicSaati));
            var sonuc = await _randevular.OlusturAsync(talep, k);

            if (!sonuc.Basarili)
                return "Randevu oluşturulamadı: " + sonuc.Hata;

            return "Randevu talebi oluşturuldu (durum: Bekliyor — hekim onayı gerekiyor).\n" +
                   Json(RandevuDto.Den(sonuc.Veri!));
        }
    }
}
