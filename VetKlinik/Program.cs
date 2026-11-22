using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Guvenlik;
using VetKlinik.Mcp;
using VetKlinik.Models;
using VetKlinik.Services;

var builder = WebApplication.CreateBuilder(args);

// =====================================================================
// 1) SERVİS KAYDI — hepsi builder.Build()'dan ÖNCE
// =====================================================================

builder.Services.AddControllersWithViews();

// --- Veritabanı: EF Core + SQLite (bağlantı dizesi appsettings.json'da, parola yok) ---
builder.Services.AddDbContext<UygulamaDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("VarsayilanBaglanti")));

// --- Kimlik: ASP.NET Core Identity ---
builder.Services.AddIdentity<Kullanici, IdentityRole>(o =>
    {
        // Ödev şartı: admin şifresi "sau" (3 karakter). Bu yüzden kurallar gevşek.
        o.Password.RequiredLength = 3;
        o.Password.RequireDigit = false;
        o.Password.RequireLowercase = false;
        o.Password.RequireUppercase = false;
        o.Password.RequireNonAlphanumeric = false;
        o.Password.RequiredUniqueChars = 1;
        o.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<UygulamaDbContext>()
    .AddDefaultTokenProviders()
    .AddClaimsPrincipalFactory<KullaniciClaimsFabrikasi>();

builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Hesap/Giris";
    o.AccessDeniedPath = "/Hesap/ErisimEngellendi";

    // API isteklerinde giriş sayfasına yönlendirme (302) değil, doğru durum kodu: 401 / 403
    o.Events.OnRedirectToLogin = ctx =>
    {
        if (ctx.Request.Path.StartsWithSegments("/api")) ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        else ctx.Response.Redirect(ctx.RedirectUri);
        return Task.CompletedTask;
    };
    o.Events.OnRedirectToAccessDenied = ctx =>
    {
        if (ctx.Request.Path.StartsWithSegments("/api")) ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        else ctx.Response.Redirect(ctx.RedirectUri);
        return Task.CompletedTask;
    };
});

// --- İkinci kimlik doğrulama şeması: "Authorization: Bearer vk_..." (MCP ve API istemcileri) ---
builder.Services.AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, KisiselAnahtarDogrulayici>(KisiselAnahtarDogrulayici.SemaAdi, null);

builder.Services.AddAuthorization(o =>
{
    // MCP ucu yalnızca kişisel anahtarla açılır
    o.AddPolicy("Mcp", p => p
        .AddAuthenticationSchemes(KisiselAnahtarDogrulayici.SemaAdi)
        .RequireAuthenticatedUser());
});

// --- Hafta 7: Session (randevu formunda son seçimler) ---
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(20);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

// --- Servis katmanı: iş kuralları TEK yerde. MVC, REST API ve MCP aynı servisleri kullanır. ---
// Scoped: her request'e bir nesne (DbContext de böyle yaşar).
builder.Services.AddScoped<IHizmetServisi, HizmetServisi>();
builder.Services.AddScoped<IVeterinerServisi, VeterinerServisi>();
builder.Services.AddScoped<IEvcilHayvanServisi, EvcilHayvanServisi>();
builder.Services.AddScoped<IAsiServisi, AsiServisi>();
builder.Services.AddScoped<IRandevuServisi, RandevuServisi>();
builder.Services.AddScoped<IRaporServisi, RaporServisi>();

// --- MCP sunucusu (ModelContextProtocol.AspNetCore) ---
builder.Services.AddHttpContextAccessor();   // araçlar oturumdaki kullanıcıyı buradan okur
builder.Services.AddMcpServer()
    .WithHttpTransport(o => o.Stateless = true)
    .WithTools<KlinikAraclari>();

var app = builder.Build();

// =====================================================================
// 2) KÜLTÜR — tr-TR sabit: "1.250,50" her makinede aynı okunsun (Hafta 6)
// =====================================================================
var trTR = new CultureInfo("tr-TR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(trTR),
    SupportedCultures = [trTR],
    SupportedUICultures = [trTR]
});

// =====================================================================
// 3) VERİTABANI — migration'ları uygula, rolleri/admin'i/örnek veriyi oluştur
// =====================================================================
await VeriTohumlayici.TohumlaAsync(app.Services);

// =====================================================================
// 4) MIDDLEWARE PIPELINE — SIRA ÖNEMLİ
// =====================================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// /mcp HTTPS'e yönlendirilmez: MCP Inspector (Node.js) yerel geliştirme sertifikasına
// güvenmediği için http://localhost üzerinden bağlanır. Yalnızca yerel geliştirme içindir.
app.UseWhen(ctx => !ctx.Request.Path.StartsWithSegments("/mcp"),
    dal => dal.UseHttpsRedirection());

app.UseRouting();
app.UseAuthentication();   // "kimsin?"   — mutlaka Authorization'dan ÖNCE
app.UseAuthorization();    // "yetkin var mı?"
app.UseSession();          // session'ı okuyan her şey bundan SONRA

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id:int?}")
    .WithStaticAssets();

// MCP ucu: POST /mcp — yetkisiz istek 401 alır
app.MapMcp("/mcp").RequireAuthorization("Mcp");

app.Run();
