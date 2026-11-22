namespace VetKlinik.Guvenlik
{
    // Rol adları tek yerde — [Authorize(Roles = Roller.Admin)] yazımında yazım hatasını önler.
    public static class Roller
    {
        public const string Admin = "Admin";
        public const string Veteriner = "Veteriner";
        public const string Uye = "Uye";

        public const string AdminVeyaVeteriner = Admin + "," + Veteriner;
        public const string AdminVeyaUye = Admin + "," + Uye;

        public static readonly string[] Hepsi = { Admin, Veteriner, Uye };
    }
}
