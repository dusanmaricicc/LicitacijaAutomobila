using LicitacijaAutomobila.Data;
using LicitacijaAutomobila.Models.BO;
using LicitacijaAutomobila.Models.Entities;
using LicitacijaAutomobila.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LicitacijaAutomobila.Repositories.EF
{
    public class PonudaRepository : IPonudaRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly INotifikacijaRepository _notifikacijaRepository;

        public PonudaRepository(ApplicationDbContext context, INotifikacijaRepository notifikacijaRepository)
        {
            _context = context;
            _notifikacijaRepository = notifikacijaRepository;
        }

        public IEnumerable<PonudaBO> GetByAutomobil(int automobilId)
        {
            var ponude = _context.Ponude
                .Include(p => p.Kupac)
                .Include(p => p.Automobil)
                .Where(p => p.AutomobilId == automobilId)
                .OrderByDescending(p => p.Iznos)
                .ThenByDescending(p => p.VremePonude)
                .ToList();

            return ponude.Select((p, index) => MapToBO(p, jeNajvisa: index == 0)).ToList();
        }

        public PonudaBO? GetNajvisaPonuda(int automobilId)
        {
            var ponuda = _context.Ponude
                .Include(p => p.Kupac)
                .Include(p => p.Automobil)
                .Where(p => p.AutomobilId == automobilId)
                .OrderByDescending(p => p.Iznos)
                .ThenByDescending(p => p.VremePonude)
                .FirstOrDefault();

            return ponuda is null ? null : MapToBO(ponuda, jeNajvisa: true);
        }

        public IEnumerable<PonudaBO> GetByKupac(string kupacId)
        {
            var ponude = _context.Ponude
                .Include(p => p.Kupac)
                .Include(p => p.Automobil)
                .Where(p => p.KupacId == kupacId)
                .OrderByDescending(p => p.VremePonude)
                .ToList();

            var rezultat = new List<PonudaBO>();
            foreach (var ponuda in ponude)
            {
                var najvisaZaAuto = _context.Ponude
                    .Where(p => p.AutomobilId == ponuda.AutomobilId)
                    .OrderByDescending(p => p.Iznos)
                    .ThenByDescending(p => p.VremePonude)
                    .First();

                rezultat.Add(MapToBO(ponuda, jeNajvisa: najvisaZaAuto.Id == ponuda.Id));
            }
            return rezultat;
        }

        public (bool uspeh, string poruka) DodajPonudu(int automobilId, string kupacId, decimal iznos, int minimalniKorak)
        {
            var automobil = _context.Automobili
                .Include(a => a.Ponude)
                .SingleOrDefault(a => a.Id == automobilId);

            if (automobil is null)
                return (false, "Automobil ne postoji.");

            if (automobil.ProdavacId == kupacId)
                return (false, "Ne mozete licitirati na sopstveni automobil.");

            if (DateTime.Now < automobil.DatumPocetka)
                return (false, "Licitacija za ovaj automobil jos nije pocela.");

            if (DateTime.Now >= automobil.DatumZavrsetka)
                return (false, "Licitacija za ovaj automobil je zavrsena.");

            var trenutnaNajvisa = automobil.Ponude
                .OrderByDescending(p => p.Iznos)
                .ThenByDescending(p => p.VremePonude)
                .FirstOrDefault();

            var prethodnaCena = trenutnaNajvisa?.Iznos ?? automobil.PocetnaCena;
            var minimalniIznos = trenutnaNajvisa is null ? automobil.PocetnaCena : prethodnaCena + minimalniKorak;

            if (iznos < minimalniIznos)
            {
                return (false, $"Ponuda mora biti najmanje {minimalniIznos:N0} RSD " +
                    $"({(trenutnaNajvisa is null ? "najmanje pocetna cena" : $"trenutna najvisa ponuda + {minimalniKorak:N0} RSD")}).");
            }

            var prethodniNajviciKupacId = trenutnaNajvisa?.KupacId;

            var novaPonuda = new Ponuda
            {
                AutomobilId = automobilId,
                KupacId = kupacId,
                Iznos = iznos,
                VremePonude = DateTime.Now
            };

            _context.Ponude.Add(novaPonuda);
            _context.SaveChanges();

            // Obavestenje prethodnom najvisem ponudjacu da je izgubio status najvise ponude
            if (!string.IsNullOrEmpty(prethodniNajviciKupacId) && prethodniNajviciKupacId != kupacId)
            {
                _notifikacijaRepository.Dodaj(
                    prethodniNajviciKupacId,
                    $"Vasa ponuda za automobil \"{automobil.Marka} {automobil.Model}\" je nadmasena novom ponudom od {iznos:N0} RSD.",
                    automobilId);
            }

            return (true, "Ponuda je uspesno zabelezena.");
        }

        private static PonudaBO MapToBO(Ponuda p, bool jeNajvisa) => new()
        {
            Id = p.Id,
            Iznos = p.Iznos,
            VremePonude = p.VremePonude,
            AutomobilId = p.AutomobilId,
            AutomobilNaslov = p.Automobil is null ? string.Empty : $"{p.Automobil.Marka} {p.Automobil.Model} ({p.Automobil.Godina})",
            AutomobilAktivna = p.Automobil is not null && DateTime.Now >= p.Automobil.DatumPocetka && DateTime.Now < p.Automobil.DatumZavrsetka,
            AutomobilZavrsena = p.Automobil is not null && DateTime.Now >= p.Automobil.DatumZavrsetka,
            KupacId = p.KupacId,
            KupacIme = p.Kupac is null ? string.Empty : $"{p.Kupac.Ime} {p.Kupac.Prezime}",
            JeNajvisa = jeNajvisa
        };
    }
}
