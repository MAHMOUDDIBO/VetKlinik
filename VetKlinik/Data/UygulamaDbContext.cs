using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VetKlinik.Models;

namespace VetKlinik.Data
{
    // IdentityDbContext: Identity tablolarını (AspNetUsers, AspNetRoles...) da içerir.
    public class UygulamaDbContext : IdentityDbContext<Kullanici>
    {
        public UygulamaDbContext(DbContextOptions<UygulamaDbContext> options) : base(options) { }

        public DbSet<Hizmet> Hizmetler => Set<Hizmet>();
        public DbSet<Veteriner> Veterinerler => Set<Veteriner>();
        public DbSet<Musaitlik> Musaitlikler => Set<Musaitlik>();
        public DbSet<EvcilHayvan> EvcilHayvanlar => Set<EvcilHayvan>();
        public DbSet<AsiKaydi> AsiKayitlari => Set<AsiKaydi>();
        public DbSet<Randevu> Randevular => Set<Randevu>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);   // Identity tabloları için ŞART

            mb.Entity<Kullanici>(e =>
            {
                e.Property(k => k.AdSoyad).HasMaxLength(80);
                e.HasIndex(k => k.ApiAnahtariOzeti).IsUnique();

                // Hekim hesabı ↔ Veteriner kaydı (bire-bir). Hekim silinirse bağ boşalır.
                e.HasOne<Veteriner>().WithOne()
                    .HasForeignKey<Kullanici>(k => k.VeterinerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            mb.Entity<Hizmet>(e =>
            {
                e.Property(h => h.HizmetAdi).HasMaxLength(50).IsRequired();
                e.HasIndex(h => h.HizmetAdi).IsUnique();
            });

            mb.Entity<Veteriner>(e =>
            {
                e.Property(v => v.VeterinerAdSoyad).HasMaxLength(80).IsRequired();
                e.Property(v => v.UzmanAlani).HasMaxLength(50);

                // Çoka-çok: EF Core ara tabloyu kendisi oluşturur
                e.HasMany(v => v.Hizmetler).WithMany(h => h.Veterinerler)
                    .UsingEntity(j => j.ToTable("VeterinerHizmetleri"));
            });

            mb.Entity<Musaitlik>(e =>
            {
                e.HasOne(m => m.Veteriner).WithMany(v => v.Musaitlikler)
                    .HasForeignKey(m => m.VeterinerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            mb.Entity<EvcilHayvan>(e =>
            {
                e.Property(h => h.EvcilHayvanAdi).HasMaxLength(40).IsRequired();
                e.Property(h => h.Tur).HasMaxLength(30);
                e.Property(h => h.Irk).HasMaxLength(40);
                e.HasOne(h => h.Sahip).WithMany()
                    .HasForeignKey(h => h.SahipId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            mb.Entity<AsiKaydi>(e =>
            {
                e.Property(a => a.AsiAdi).HasMaxLength(60).IsRequired();
                e.HasOne(a => a.EvcilHayvan).WithMany(h => h.AsiKayitlari)
                    .HasForeignKey(a => a.EvcilHayvanId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            mb.Entity<Randevu>(e =>
            {
                e.HasOne(r => r.EvcilHayvan).WithMany(h => h.Randevular)
                    .HasForeignKey(r => r.EvcilHayvanId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Randevusu olan hekim/hizmet silinemez (Restrict) — servis kibar mesaj verir
                e.HasOne(r => r.Veteriner).WithMany(v => v.Randevular)
                    .HasForeignKey(r => r.VeterinerId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(r => r.Hizmet).WithMany()
                    .HasForeignKey(r => r.HizmetId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Enum veritabanında okunur metin olarak dursun: "Bekliyor", "Onaylandi"...
                e.Property(r => r.Durum).HasConversion<string>().HasMaxLength(20);
                e.Property(r => r.MuayeneNotu).HasMaxLength(2000);

                // Çakışma sorgusu "hekim + gün" ile arar
                e.HasIndex(r => new { r.VeterinerId, r.Baslangic });
            });
        }
    }
}
