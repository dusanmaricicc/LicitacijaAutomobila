using LicitacijaAutomobila.Data;
using LicitacijaAutomobila.Models.BO;
using LicitacijaAutomobila.Models.Entities;
using LicitacijaAutomobila.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LicitacijaAutomobila.Repositories.EF
{
    public class KategorijaRepository : IKategorijaRepository
    {
        private readonly ApplicationDbContext _context;

        public KategorijaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<KategorijaBO> GetAll()
        {
            return _context.Kategorije
                .Include(k => k.Automobili)
                .OrderBy(k => k.Naziv)
                .Select(k => MapToBO(k))
                .ToList();
        }

        public KategorijaBO? GetById(int id)
        {
            var kategorija = _context.Kategorije
                .Include(k => k.Automobili)
                .SingleOrDefault(k => k.Id == id);

            return kategorija is null ? null : MapToBO(kategorija);
        }

        public void Add(KategorijaBO kategorija)
        {
            var entitet = new Kategorija { Naziv = kategorija.Naziv };
            _context.Kategorije.Add(entitet);
            _context.SaveChanges();
        }

        public void Update(KategorijaBO kategorija)
        {
            var entitet = _context.Kategorije.SingleOrDefault(k => k.Id == kategorija.Id);
            if (entitet is null) return;

            entitet.Naziv = kategorija.Naziv;
            _context.SaveChanges();
        }

        public bool Delete(int id)
        {
            var entitet = _context.Kategorije
                .Include(k => k.Automobili)
                .SingleOrDefault(k => k.Id == id);

            if (entitet is null) return false;
            if (entitet.Automobili.Any()) return false; // kategorija je u upotrebi

            _context.Kategorije.Remove(entitet);
            _context.SaveChanges();
            return true;
        }

        public bool PostojiSaNazivom(string naziv, int? izuzetiId = null)
        {
            return _context.Kategorije.Any(k => k.Naziv.ToLower() == naziv.ToLower()
                && (izuzetiId == null || k.Id != izuzetiId));
        }

        private static KategorijaBO MapToBO(Kategorija k) => new()
        {
            Id = k.Id,
            Naziv = k.Naziv,
            BrojAutomobila = k.Automobili?.Count ?? 0
        };
    }
}
