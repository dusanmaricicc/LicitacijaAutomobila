namespace LicitacijaAutomobila.Models.BO
{
    public class KorisnikBO
    {
        public string Id { get; set; } = string.Empty;
        public string Ime { get; set; } = string.Empty;
        public string Prezime { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefon { get; set; }
        public string? Adresa { get; set; }
        public bool Aktivan { get; set; }
        public DateTime DatumRegistracije { get; set; }
        public string Rola { get; set; } = string.Empty;

        public string PunoIme => $"{Ime} {Prezime}";
    }
}
