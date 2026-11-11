namespace LicitacijaAutomobila.Models.Entities
{
    public class Notifikacija
    {
        public int Id { get; set; }
        public string Poruka { get; set; } = string.Empty;
        public DateTime Datum { get; set; } = DateTime.Now;
        public bool Procitano { get; set; } = false;

        public string KorisnikId { get; set; } = string.Empty;
        public virtual ApplicationUser? Korisnik { get; set; }

        public int? AutomobilId { get; set; }
        public virtual Automobil? Automobil { get; set; }
    }
}
