using System.Globalization;
using Microsoft.AspNetCore.Localization;

using VetKlinik.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// Servis katmanı: IHizmetServisi istenirse HizmetServisi ver, her request'e bir nesne
builder.Services.AddScoped<IHizmetServisi, HizmetServisi>();
var app = builder.Build();
// Kültürü tr-TR'ye sabitliyoruz: virgül ondalık, nokta binlik.
// Sabitlemezsek kültür işletim sisteminden gelir ve her makinede farklı olur.
var trTR = new CultureInfo("tr-TR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(trTR),
    SupportedCultures = [trTR],
    SupportedUICultures = [trTR]
});
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
