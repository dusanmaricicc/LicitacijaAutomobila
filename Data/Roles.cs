namespace LicitacijaAutomobila.Data
{
    // Konstante za nazive uloga koje koristimo kroz celu aplikaciju
    public static class Roles
    {
        public const string Administrator = "Administrator";
        public const string Prodavac = "Prodavac";
        public const string Kupac = "Kupac";

        public static readonly string[] Sve = { Administrator, Prodavac, Kupac };
    }
}
