using LicitacijaAutomobila.Data;
using LicitacijaAutomobila.Models.BO;
using LicitacijaAutomobila.Models.Entities;
using LicitacijaAutomobila.Models.ViewModels;
using LicitacijaAutomobila.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LicitacijaAutomobila.Controllers
{
    [Authorize(Roles = Roles.Administrator)]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IKategorijaRepository _kategorijaRepository;
        private readonly IAutomobilRepository _automobilRepository;
        private readonly IPonudaRepository _ponudaRepository;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IKategorijaRepository kategorijaRepository,
            IAutomobilRepository automobilRepository,
            IPonudaRepository ponudaRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _kategorijaRepository = kategorijaRepository;
            _automobilRepository = automobilRepository;
            _ponudaRepository = ponudaRepository;
        }

        public IActionResult Index() => RedirectToAction(nameof(Korisnici));

        
        //  UPRAVLJANJE KORISNICKIM NALOZIMA (Prodavac / Kupac)
        

        public async Task<IActionResult> Korisnici()
        {
            var korisnici = new List<KorisnikBO>();

            foreach (var user in _userManager.Users.ToList())
            {
                var role = await _userManager.GetRolesAsync(user);
                if (role.Contains(Roles.Administrator)) continue; // admin nalozi se ne prikazuju/menjaju ovde

                korisnici.Add(new KorisnikBO
                {
                    Id = user.Id,
                    Ime = user.Ime,
                    Prezime = user.Prezime,
                    Email = user.Email ?? string.Empty,
                    Telefon = user.PhoneNumber,
                    Adresa = user.Adresa,
                    Aktivan = user.Aktivan,
                    DatumRegistracije = user.DatumRegistracije,
                    Rola = role.FirstOrDefault() ?? string.Empty
                });
            }

            return View(korisnici.OrderBy(k => k.Rola).ThenBy(k => k.Prezime).ToList());
        }

        public IActionResult KreirajKorisnika()
        {
            return View("KorisnikForma", new KorisnikFormViewModel { Rola = Roles.Kupac });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KreirajKorisnika(KorisnikFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "Lozinka je obavezna za novi nalog.");
            }

            if (!ModelState.IsValid)
            {
                return View("KorisnikForma", model);
            }

            // Provera duplikata - ako vec postoji nalog sa istim emailom, obavestavamo administratora
            // i nudimo mogucnost da ga izmeni umesto da kreira novi.
            var postojeci = await _userManager.FindByEmailAsync(model.Email);
            if (postojeci is not null)
            {
                ModelState.AddModelError(nameof(model.Email),
                    "Korisnik sa ovom email adresom vec postoji. Umesto kreiranja, mozete izmeniti postojeci nalog.");
                ViewBag.PostojeciKorisnikId = postojeci.Id;
                return View("KorisnikForma", model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.Telefon,
                Ime = model.Ime,
                Prezime = model.Prezime,
                Adresa = model.Adresa,
                Aktivan = model.Aktivan,
                DatumRegistracije = DateTime.Now,
                EmailConfirmed = true
            };

            var rezultat = await _userManager.CreateAsync(user, model.Password!);
            if (!rezultat.Succeeded)
            {
                foreach (var greska in rezultat.Errors)
                    ModelState.AddModelError(string.Empty, greska.Description);
                return View("KorisnikForma", model);
            }

            await _userManager.AddToRoleAsync(user, model.Rola);

            TempData["Poruka"] = "Korisnicki nalog je uspesno kreiran.";
            return RedirectToAction(nameof(Korisnici));
        }

        public async Task<IActionResult> IzmeniKorisnika(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            var uloge = await _userManager.GetRolesAsync(user);

            var model = new KorisnikFormViewModel
            {
                Id = user.Id,
                Ime = user.Ime,
                Prezime = user.Prezime,
                Email = user.Email ?? string.Empty,
                Telefon = user.PhoneNumber,
                Adresa = user.Adresa,
                Aktivan = user.Aktivan,
                Rola = uloge.FirstOrDefault() ?? Roles.Kupac
            };

            return View("KorisnikForma", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IzmeniKorisnika(string id, KorisnikFormViewModel model)
        {
            if (id != model.Id) return BadRequest();

            // Lozinka nije obavezna prilikom izmene
            ModelState.Remove(nameof(model.Password));

            if (!ModelState.IsValid)
            {
                return View("KorisnikForma", model);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                var postojeci = await _userManager.FindByEmailAsync(model.Email);
                if (postojeci is not null && postojeci.Id != user.Id)
                {
                    ModelState.AddModelError(nameof(model.Email), "Ova email adresa je vec u upotrebi kod drugog korisnika.");
                    return View("KorisnikForma", model);
                }
                user.Email = model.Email;
                user.UserName = model.Email;
                user.NormalizedEmail = model.Email.ToUpperInvariant();
                user.NormalizedUserName = model.Email.ToUpperInvariant();
            }

            user.Ime = model.Ime;
            user.Prezime = model.Prezime;
            user.PhoneNumber = model.Telefon;
            user.Adresa = model.Adresa;
            user.Aktivan = model.Aktivan;

            var rezultat = await _userManager.UpdateAsync(user);
            if (!rezultat.Succeeded)
            {
                foreach (var greska in rezultat.Errors)
                    ModelState.AddModelError(string.Empty, greska.Description);
                return View("KorisnikForma", model);
            }

            // Promena uloge, ukoliko je administrator izabrao drugu
            var trenutneUloge = await _userManager.GetRolesAsync(user);
            if (!trenutneUloge.Contains(model.Rola))
            {
                await _userManager.RemoveFromRolesAsync(user, trenutneUloge);
                await _userManager.AddToRoleAsync(user, model.Rola);
            }

            // Ako je uneta nova lozinka, resetujemo je
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, token, model.Password);
            }

            TempData["Poruka"] = "Korisnik je uspesno azuriran.";
            return RedirectToAction(nameof(Korisnici));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ObrisiKorisnika(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            var rezultat = await _userManager.DeleteAsync(user);
            TempData[rezultat.Succeeded ? "Poruka" : "Greska"] = rezultat.Succeeded
                ? "Korisnik je uspesno obrisan."
                : "Korisnika nije moguce obrisati (verovatno ima povezane automobile ili ponude).";

            return RedirectToAction(nameof(Korisnici));
        }

        
        //  UPRAVLJANJE KATEGORIJAMA AUTOMOBILA
        

        public IActionResult Kategorije()
        {
            return View(_kategorijaRepository.GetAll());
        }

        public IActionResult KreirajKategoriju()
        {
            return View("KategorijaForma", new KategorijaFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult KreirajKategoriju(KategorijaFormViewModel model)
        {
            if (_kategorijaRepository.PostojiSaNazivom(model.Naziv))
            {
                ModelState.AddModelError(nameof(model.Naziv), "Kategorija sa ovim nazivom vec postoji.");
            }

            if (!ModelState.IsValid) return View("KategorijaForma", model);

            _kategorijaRepository.Add(new KategorijaBO { Naziv = model.Naziv });
            TempData["Poruka"] = "Kategorija je uspesno kreirana.";
            return RedirectToAction(nameof(Kategorije));
        }

        public IActionResult IzmeniKategoriju(int id)
        {
            var kategorija = _kategorijaRepository.GetById(id);
            if (kategorija is null) return NotFound();

            return View("KategorijaForma", new KategorijaFormViewModel { Id = kategorija.Id, Naziv = kategorija.Naziv });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult IzmeniKategoriju(int id, KategorijaFormViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (_kategorijaRepository.PostojiSaNazivom(model.Naziv, id))
            {
                ModelState.AddModelError(nameof(model.Naziv), "Kategorija sa ovim nazivom vec postoji.");
            }

            if (!ModelState.IsValid) return View("KategorijaForma", model);

            _kategorijaRepository.Update(new KategorijaBO { Id = model.Id, Naziv = model.Naziv });
            TempData["Poruka"] = "Kategorija je uspesno izmenjena.";
            return RedirectToAction(nameof(Kategorije));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ObrisiKategoriju(int id)
        {
            var uspeh = _kategorijaRepository.Delete(id);
            TempData[uspeh ? "Poruka" : "Greska"] = uspeh
                ? "Kategorija je uspesno obrisana."
                : "Kategoriju nije moguce obrisati jer se koristi kod postojecih automobila.";

            return RedirectToAction(nameof(Kategorije));
        }

        
        //  IZVESTAJI O LICITACIJAMA
        

        public IActionResult Izvestaji()
        {
            var svi = _automobilRepository.GetAll().ToList();
            var zavrseni = svi.Where(a => a.Zavrsena).OrderByDescending(a => a.DatumZavrsetka).ToList();

            var model = new IzvestajViewModel
            {
                UkupnoAutomobila = svi.Count,
                UkupnoAktivnihLicitacija = svi.Count(a => a.Aktivna),
                UkupnoZavrsenihLicitacija = zavrseni.Count,
                UkupnoPonuda = svi.Sum(a => a.BrojPonuda),
                UkupnaVrednostProdatihAutomobila = zavrseni.Where(a => a.BrojPonuda > 0).Sum(a => a.TrenutnaCena),
                ZavrseneLicitacije = zavrseni
            };

            return View(model);
        }
    }
}
