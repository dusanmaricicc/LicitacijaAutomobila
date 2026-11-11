using LicitacijaAutomobila.Data;
using LicitacijaAutomobila.Models.BO;
using LicitacijaAutomobila.Models.Entities;
using LicitacijaAutomobila.Repositories.Interfaces;

namespace LicitacijaAutomobila.Repositories.EF
{
    public class NotifikacijaRepository : INotifikacijaRepository
    {
        private readonly ApplicationDbContext _context;

        public NotifikacijaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<NotifikacijaBO> GetAll(string korisnikId)
        {
            return _context.Notifikacije
                .Where(n => n.KorisnikId == korisnikId)
                .OrderByDescending(n => n.Datum)
                .Select(n => new NotifikacijaBO
                {
                    Id = n.Id,
                    Poruka = n.Poruka,
                    Datum = n.Datum,
                    Procitano = n.Procitano,
                    AutomobilId = n.AutomobilId
                })
                .ToList();
        }

        public int BrojNeprocitanih(string korisnikId)
        {
            return _context.Notifikacije.Count(n => n.KorisnikId == korisnikId && !n.Procitano);
        }

        public void Dodaj(string korisnikId, string poruka, int? automobilId)
        {
            _context.Notifikacije.Add(new Notifikacija
            {
                KorisnikId = korisnikId,
                Poruka = poruka,
                AutomobilId = automobilId,
                Datum = DateTime.Now,
                Procitano = false
            });
            _context.SaveChanges();
        }

        public void OznaciSveProcitano(string korisnikId)
        {
            var neprocitane = _context.Notifikacije
                .Where(n => n.KorisnikId == korisnikId && !n.Procitano)
                .ToList();

            foreach (var n in neprocitane)
            {
                n.Procitano = true;
            }
            _context.SaveChanges();
        }
    }
}
