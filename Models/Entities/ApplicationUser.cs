using Microsoft.AspNetCore.Identity;

namespace LicitacijaAutomobila.Models.Entities
{
    // Prosireni Identity korisnik - dodajemo poslovna polja koja nam trebaju
    // za sve tri uloge (Administrator, Prodavac, Kupac).
    public class ApplicationUser : IdentityUser
    {
        public string Ime { get; set; } = string.Empty;
        public string Prezime { get; set; } = string.Empty;
        public string? Adresa { get; set; }
        public DateTime DatumRegistracije { get; set; } = DateTime.Now;
        public bool Aktivan { get; set; } = true;

        public virtual ICollection<Automobil> Automobili { get; set; } = new List<Automobil>();
        public virtual ICollection<Ponuda> Ponude { get; set; } = new List<Ponuda>();
        public virtual ICollection<Notifikacija> Notifikacije { get; set; } = new List<Notifikacija>();
    }
}
