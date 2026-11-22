namespace VetKlinik.Services
{
    public record HizmetRaporSatiri(string HizmetAdi, int RandevuSayisi, int TamamlananSayisi, decimal Ciro);

    public record PanelOzeti(int HizmetSayisi, int HekimSayisi, int UyeSayisi, int HayvanSayisi,
        int BekleyenRandevu, int BugunkuRandevu);

    public interface IRaporServisi
    {
        Task<List<HizmetRaporSatiri>> HizmetRaporuAsync(DateOnly? baslangic, DateOnly? bitis);
        Task<PanelOzeti> PanelOzetiAsync();
    }
}
