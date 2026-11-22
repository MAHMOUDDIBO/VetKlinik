using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetKlinik.Migrations
{
    /// <inheritdoc />
    public partial class IlkKurulum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Hizmetler",
                columns: table => new
                {
                    HizmetId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HizmetAdi = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SureDakika = table.Column<int>(type: "INTEGER", nullable: false),
                    Ucret = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hizmetler", x => x.HizmetId);
                });

            migrationBuilder.CreateTable(
                name: "Veterinerler",
                columns: table => new
                {
                    VeterinerId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VeterinerAdSoyad = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    UzmanAlani = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Veterinerler", x => x.VeterinerId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AdSoyad = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    VeterinerId = table.Column<int>(type: "INTEGER", nullable: true),
                    ApiAnahtariOzeti = table.Column<string>(type: "TEXT", nullable: true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Veterinerler_VeterinerId",
                        column: x => x.VeterinerId,
                        principalTable: "Veterinerler",
                        principalColumn: "VeterinerId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Musaitlikler",
                columns: table => new
                {
                    MusaitlikId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VeterinerId = table.Column<int>(type: "INTEGER", nullable: false),
                    Gun = table.Column<int>(type: "INTEGER", nullable: false),
                    Baslangic = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    Bitis = table.Column<TimeOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Musaitlikler", x => x.MusaitlikId);
                    table.ForeignKey(
                        name: "FK_Musaitlikler_Veterinerler_VeterinerId",
                        column: x => x.VeterinerId,
                        principalTable: "Veterinerler",
                        principalColumn: "VeterinerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VeterinerHizmetleri",
                columns: table => new
                {
                    HizmetlerHizmetId = table.Column<int>(type: "INTEGER", nullable: false),
                    VeterinerlerVeterinerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VeterinerHizmetleri", x => new { x.HizmetlerHizmetId, x.VeterinerlerVeterinerId });
                    table.ForeignKey(
                        name: "FK_VeterinerHizmetleri_Hizmetler_HizmetlerHizmetId",
                        column: x => x.HizmetlerHizmetId,
                        principalTable: "Hizmetler",
                        principalColumn: "HizmetId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VeterinerHizmetleri_Veterinerler_VeterinerlerVeterinerId",
                        column: x => x.VeterinerlerVeterinerId,
                        principalTable: "Veterinerler",
                        principalColumn: "VeterinerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvcilHayvanlar",
                columns: table => new
                {
                    EvcilHayvanId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EvcilHayvanAdi = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Tur = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Irk = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    DogumTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Kilo = table.Column<decimal>(type: "TEXT", nullable: false),
                    SahipId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvcilHayvanlar", x => x.EvcilHayvanId);
                    table.ForeignKey(
                        name: "FK_EvcilHayvanlar_AspNetUsers_SahipId",
                        column: x => x.SahipId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AsiKayitlari",
                columns: table => new
                {
                    AsiKaydiId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EvcilHayvanId = table.Column<int>(type: "INTEGER", nullable: false),
                    AsiAdi = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    YapildigiTarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SonrakiDozTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsiKayitlari", x => x.AsiKaydiId);
                    table.ForeignKey(
                        name: "FK_AsiKayitlari_EvcilHayvanlar_EvcilHayvanId",
                        column: x => x.EvcilHayvanId,
                        principalTable: "EvcilHayvanlar",
                        principalColumn: "EvcilHayvanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Randevular",
                columns: table => new
                {
                    RandevuId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EvcilHayvanId = table.Column<int>(type: "INTEGER", nullable: false),
                    VeterinerId = table.Column<int>(type: "INTEGER", nullable: false),
                    HizmetId = table.Column<int>(type: "INTEGER", nullable: false),
                    Baslangic = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SureDakika = table.Column<int>(type: "INTEGER", nullable: false),
                    Ucret = table.Column<decimal>(type: "TEXT", nullable: false),
                    Durum = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    MuayeneNotu = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Randevular", x => x.RandevuId);
                    table.ForeignKey(
                        name: "FK_Randevular_EvcilHayvanlar_EvcilHayvanId",
                        column: x => x.EvcilHayvanId,
                        principalTable: "EvcilHayvanlar",
                        principalColumn: "EvcilHayvanId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Randevular_Hizmetler_HizmetId",
                        column: x => x.HizmetId,
                        principalTable: "Hizmetler",
                        principalColumn: "HizmetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Randevular_Veterinerler_VeterinerId",
                        column: x => x.VeterinerId,
                        principalTable: "Veterinerler",
                        principalColumn: "VeterinerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsiKayitlari_EvcilHayvanId",
                table: "AsiKayitlari",
                column: "EvcilHayvanId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ApiAnahtariOzeti",
                table: "AspNetUsers",
                column: "ApiAnahtariOzeti",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_VeterinerId",
                table: "AspNetUsers",
                column: "VeterinerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvcilHayvanlar_SahipId",
                table: "EvcilHayvanlar",
                column: "SahipId");

            migrationBuilder.CreateIndex(
                name: "IX_Hizmetler_HizmetAdi",
                table: "Hizmetler",
                column: "HizmetAdi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Musaitlikler_VeterinerId",
                table: "Musaitlikler",
                column: "VeterinerId");

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_EvcilHayvanId",
                table: "Randevular",
                column: "EvcilHayvanId");

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_HizmetId",
                table: "Randevular",
                column: "HizmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_VeterinerId_Baslangic",
                table: "Randevular",
                columns: new[] { "VeterinerId", "Baslangic" });

            migrationBuilder.CreateIndex(
                name: "IX_VeterinerHizmetleri_VeterinerlerVeterinerId",
                table: "VeterinerHizmetleri",
                column: "VeterinerlerVeterinerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsiKayitlari");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Musaitlikler");

            migrationBuilder.DropTable(
                name: "Randevular");

            migrationBuilder.DropTable(
                name: "VeterinerHizmetleri");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "EvcilHayvanlar");

            migrationBuilder.DropTable(
                name: "Hizmetler");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Veterinerler");
        }
    }
}
