using System.Text.Json;

namespace VetKlinik.Services
{
    // Hafta 7 — ISession yalnızca string/int/byte[] bilir; nesneyi JSON'a çevirip saklıyoruz.
    public static class SessionUzantilari
    {
        public static void NesneYaz<T>(this ISession session, string anahtar, T deger) =>
            session.SetString(anahtar, JsonSerializer.Serialize(deger));

        public static T? NesneOku<T>(this ISession session, string anahtar)
        {
            var json = session.GetString(anahtar);
            return json is null ? default : JsonSerializer.Deserialize<T>(json);
        }
    }
}
