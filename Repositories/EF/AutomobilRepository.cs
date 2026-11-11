using LicitacijaAutomobila.Data;
using LicitacijaAutomobila.Models.BO;
using LicitacijaAutomobila.Models.Entities;
using LicitacijaAutomobila.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LicitacijaAutomobila.Repositories.EF
{
    public class AutomobilRepository : IAutomobilRepository
    {
        private readonly ApplicationDbContext _context;

        public AutomobilRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<AutomobilBO> GetAll()
        {
            return Upit().OrderByDescending(a => a.DatumPostavljanja).Select(MapToBO).ToList();
        }

        public IEnumerable<AutomobilBO> Pretrazi(AutomobilFilter filter)
        {
            var upit = Upit();

            if (!string.IsNullOrWhiteSpace(filter.Marka))
                upit = upit.Where(a => a.Marka.ToLower().Contains(filter.Marka.ToLower())
                    || a.Model.ToLower().Contains(filter.Marka.ToLower()));

            if (filter.KategorijaId.HasValue)
                upit = upit.Where(a => a.KategorijaId == filter.KategorijaId.Value);

            if (filter.CenaOd.HasValue)
                upit = upit.Where(a => a.PocetnaCena >= filter.CenaOd.Value);

            if (filter.CenaDo.HasValue)
                upit = upit.Where(a => a.PocetnaCena <= filter.CenaDo.Value);

            if (filter.GodinaOd.HasValue)
                upit = upit.Where(a => a.Godina >= filter.GodinaOd.Value);

            var rezultat = upit.OrderByDescending(a => a.DatumPostavljanja).Select(MapToBO).ToList();

            if (filter.SamoAktivne)
                rezultat = rezultat.Where(a => a.Aktivna).ToList();

            return rezultat;
        }

        public IEnumerable<AutomobilBO> GetByProdavac(string prodavacId)
        {
            return Upit()
                .Where(a => a.ProdavacId == prodavacId)
                .OrderByDescending(a => a.DatumPostavljanja)
                .Select(MapToBO)
                .ToList();
        }

        public AutomobilBO? GetById(int id)
        {
            var automobil = Upit().SingleOrDefault(a => a.Id == id);
            return automobil is null ? null : MapToBO(automobil);
        }

        public int Add(AutomobilBO automobil, IEnumerable<string> putanjeSlika)
        {
            var entitet = new Automobil
            {
                Marka = automobil.Marka,
                Model = automobil.Model,
                Godina = automobil.Godina,
                PocetnaCena = automobil.PocetnaCena,
                Opis = automobil.Opis,
                KategorijaId = automobil.KategorijaId,
                ProdavacId = automobil.ProdavacId,
                DatumPostavljanja = DateTime.Now,
                DatumPocetka = automobil.DatumPocetka,
                DatumZavrsetka = automobil.DatumZavrsetka
            };

            foreach (var putanja in putanjeSlika)
            {
                entitet.Slike.Add(new SlikaAutomobila { PutanjaSlike = putanja });
            }

            _context.Automobili.Add(entitet);
            _context.SaveChanges();
            return entitet.Id;
        }

        public bool Update(AutomobilBO automobil)
        {
            var entitet = _context.Automobili
                .Include(a => a.Ponude)
                .Include(a => a.Slike)
                .SingleOrDefault(a => a.Id == automobil.Id);

            if (entitet is null) return false;
            if (entitet.ProdavacId != automobil.ProdavacId) return false;
            if (entitet.Ponude.Any()) return false; // ne moze da se menja kad vec ima ponuda

            entitet.Marka = automobil.Marka;
            entitet.Model = automobil.Model;
            entitet.Godina = automobil.Godina;
            entitet.PocetnaCena = automobil.PocetnaCena;
            entitet.Opis = automobil.Opis;
            entitet.KategorijaId = automobil.KategorijaId;
            entitet.DatumPocetka = automobil.DatumPocetka;
            entitet.DatumZavrsetka = automobil.DatumZavrsetka;

            // Nove slike se dodaju uz postojece
            foreach (var putanja in automobil.PutanjeSlika)
            {
                if (!entitet.Slike.Any(s => s.PutanjaSlike == putanja))
                {
                    entitet.Slike.Add(new SlikaAutomobila { PutanjaSlike = putanja });
                }
            }

            _context.SaveChanges();
            return true;
        }

        public bool Delete(int id, string prodavacId)
        {
            var entitet = _context.Automobili
                .Include(a => a.Ponude)
                .SingleOrDefault(a => a.Id == id);

            if (entitet is null) return false;
            if (entitet.ProdavacId != prodavacId) return false;
            if (entitet.Ponude.Any()) return false; // ne moze da se obrise kad vec ima ponuda

            _context.Automobili.Remove(entitet);
            _context.SaveChanges();
            return true;
        }

        public bool ImaPonude(int automobilId)
        {
            return _context.Ponude.Any(p => p.AutomobilId == automobilId);
        }

        public IEnumerable<(int Id, string Putanja)> GetSlike(int automobilId)
        {
            return _context.SlikeAutomobila
                .Where(s => s.AutomobilId == automobilId)
                .Select(s => new ValueTuple<int, string>(s.Id, s.PutanjaSlike))
                .ToList();
        }

        public bool ObrisiSliku(int slikaId, string prodavacId)
        {
            var slika = _context.SlikeAutomobila
                .Include(s => s.Automobil)
                .SingleOrDefault(s => s.Id == slikaId);

            if (slika is null || slika.Automobil is null) return false;
            if (slika.Automobil.ProdavacId != prodavacId) return false;

            _context.SlikeAutomobila.Remove(slika);
            _context.SaveChanges();
            return true;
        }

        private IQueryable<Automobil> Upit()
        {
            return _context.Automobili
                .Include(a => a.Kategorija)
                .Include(a => a.Prodavac)
                .Include(a => a.Slike)
                .Include(a => a.Ponude)
                    .ThenInclude(p => p.Kupac);
        }

        private static AutomobilBO MapToBO(Automobil a)
        {
            var najvisaPonuda = a.Ponude?
                .OrderByDescending(p => p.Iznos)
                .ThenByDescending(p => p.VremePonude)
                .FirstOrDefault();

            return new AutomobilBO
            {
                Id = a.Id,
                Marka = a.Marka,
                Model = a.Model,
                Godina = a.Godina,
                PocetnaCena = a.PocetnaCena,
                Opis = a.Opis,
                DatumPostavljanja = a.DatumPostavljanja,
                DatumPocetka = a.DatumPocetka,
                DatumZavrsetka = a.DatumZavrsetka,
                KategorijaId = a.KategorijaId,
                KategorijaNaziv = a.Kategorija?.Naziv ?? string.Empty,
                ProdavacId = a.ProdavacId,
                ProdavacIme = a.Prodavac is null ? string.Empty : $"{a.Prodavac.Ime} {a.Prodavac.Prezime}",
                PutanjeSlika = a.Slike?.Select(s => s.PutanjaSlike).ToList() ?? new List<string>(),
                TrenutnaCena = najvisaPonuda?.Iznos ?? a.PocetnaCena,
                BrojPonuda = a.Ponude?.Count ?? 0,
                NajviciPonudjacId = najvisaPonuda?.KupacId,
                NajviciPonudjacIme = najvisaPonuda?.Kupac is null
                    ? null
                    : $"{najvisaPonuda.Kupac.Ime} {najvisaPonuda.Kupac.Prezime}"
            };
        }
    }
}
