using System.Security.Cryptography;
using System.Text;

namespace VetKlinik.Guvenlik
{
    // Kişisel erişim anahtarı: üretilir, kullanıcıya BİR KEZ gösterilir,
    // veritabanına yalnızca SHA-256 özeti yazılır.
    public static class AnahtarYardimcisi
    {
        public static string Olustur() =>
            "vk_" + Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();

        public static string Ozetle(string anahtar) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(anahtar)));
    }
}
