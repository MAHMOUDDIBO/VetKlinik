using Microsoft.AspNetCore.Identity;

namespace VetKlinik.Models
{
    // Identity'nin kullanıcı tablosunu genişletiyoruz (AspNetUsers).
    // Admin, Veteriner ve Üye aynı tabloda; farkı ROLLER belirler.
    public class Kullanici : IdentityUser
    {
        public string AdSoyad { get; set; } = "";

        // Bu kullanıcı bir hekimse, Veterinerler tablosundaki kaydı
        public int? VeterinerId { get; set; }

        // MCP / API için kişisel erişim anahtarının SHA-256 özeti.
        // Anahtarın kendisi veritabanında DURMAZ (parola gibi).
        public string? ApiAnahtariOzeti { get; set; }
    }
}
