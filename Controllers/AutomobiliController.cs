using LicitacijaAutomobila.Data;
using LicitacijaAutomobila.Models.BO;
using LicitacijaAutomobila.Models.ViewModels;
using LicitacijaAutomobila.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace LicitacijaAutomobila.Controllers
{
    public class AutomobiliController : Controller
    {
        private readonly IAutomobilRepository _automobilRepository;
        private readonly IKategorijaRepository _kategorijaRepository;
        private readonly IPonudaRepository _ponudaRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<Models.Entities.ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AutomobiliController(
            IAutomobilRepository automobilRepository,
            IKategorijaRepository kategorijaRepository,
            IPonudaRepository ponudaRepository,
            IWebHostEnvironment webHostEnvironment,
            UserManager<Models.Entities.ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _automobilRepository = automobilRepository;
            _kategorijaRepository = kategorijaRepository;
            _ponudaRepository = ponudaRepository;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _configuration = configuration;
        }

        //javni pregled, dostupan svima (i neprijavljenim posetiocima)
        [AllowAnonymous]
        public IActionResult Index(string? marka, int? kategorijaId, decimal? cenaOd, decimal? cenaDo, int? godinaOd, bool prikaziZavrsene = false)
        {
            var filter = new AutomobilFilter
            {
                Marka = marka,
                KategorijaId = kategorijaId,
                CenaOd = cenaOd,
                CenaDo = cenaDo,
                GodinaOd = godinaOd,
                SamoAktivne = !prikaziZavrsene
            };

            var automobili = _automobilRepository.Pretrazi(filter);

            ViewBag.Kategorije = _kategorijaRepository.GetAll();
            ViewBag.Marka = marka;
            ViewBag.KategorijaId = kategorijaId;
            ViewBag.CenaOd = cenaOd;
            ViewBag.CenaDo = cenaDo;
            ViewBag.GodinaOd = godinaOd;
            ViewBag.PrikaziZavrsene = prikaziZavrsene;

            return View(automobili);
        }

        
        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            var automobil = _automobilRepository.GetById(id);
            if (automobil is null) return NotFound();

            ViewBag.Ponude = _ponudaRepository.GetByAutomobil(id);
            ViewBag.MinimalniKorak = _configuration.GetValue<int>("Licitacija:MinimalniKorakPonudeDinara", 1000);

            return View(automobil);
        }

        //oglasi prijavljenog prodavca
        [Authorize(Roles = Roles.Prodavac)]
        public async Task<IActionResult> MojiAutomobili()
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik is null) return Challenge();

            var automobili = _automobilRepository.GetByProdavac(korisnik.Id);
            return View(automobili);
        }

        
        [Authorize(Roles = Roles.Prodavac)]
        public IActionResult Create()
        {
            var model = new AutomobilFormViewModel
            {
                DostupneKategorije = _kategorijaRepository.GetAll(),
                DatumPocetka = DateTime.Now,
                TrajanjeSati = _configuration.GetValue<int>("Licitacija:PodrazumevanoTrajanjeLicitacijeSati", 24)
            };
            return View(model);
        }

        
        [HttpPost]
        [Authorize(Roles = Roles.Prodavac)]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost()
        {
            var unos = CitajFormuAutomobila();

            if (unos.DatumPocetka < DateTime.Now.AddMinutes(-5))
            {
                ModelState.AddModelError(nameof(unos.DatumPocetka), "Pocetak licitacije ne moze biti u proslosti.");
            }

            TryValidateModel(unos);

            if (!ModelState.IsValid)
            {
                unos.DostupneKategorije = _kategorijaRepository.GetAll();
                return View(unos);
            }

            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik is null) return Challenge();

            var putanjeSlika = await SacuvajSlikeAsync(unos.Slike);

            var automobilBO = new AutomobilBO
            {
                Marka = unos.Marka,
                Model = unos.Model,
                Godina = unos.Godina,
                PocetnaCena = unos.PocetnaCena,
                Opis = unos.Opis,
                KategorijaId = unos.KategorijaId,
                ProdavacId = korisnik.Id,
                DatumPocetka = unos.DatumPocetka,
                DatumZavrsetka = unos.DatumPocetka.AddHours(unos.TrajanjeSati)
            };

            var noviId = _automobilRepository.Add(automobilBO, putanjeSlika);

            TempData["Poruka"] = "Automobil je uspesno postavljen na licitaciju.";
            return RedirectToAction(nameof(Details), new { id = noviId });
        }

        // Rucno cita polja iz Request.Form - zaobilazi automatski model binding
        
        private AutomobilFormViewModel CitajFormuAutomobila()
        {
            var forma = Request.Form;

            int.TryParse(forma["Godina"], NumberStyles.Integer, CultureInfo.InvariantCulture, out var godina);
            decimal.TryParse(forma["PocetnaCena"], NumberStyles.Number, CultureInfo.InvariantCulture, out var pocetnaCena);
            int.TryParse(forma["KategorijaId"], NumberStyles.Integer, CultureInfo.InvariantCulture, out var kategorijaId);
            int.TryParse(forma["TrajanjeSati"], NumberStyles.Integer, CultureInfo.InvariantCulture, out var trajanjeSati);
            DateTime.TryParse(forma["DatumPocetka"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var datumPocetka);

            return new AutomobilFormViewModel
            {
                Marka = forma["Marka"].ToString().Trim(),
                Model = forma["Model"].ToString().Trim(),
                Godina = godina,
                PocetnaCena = pocetnaCena,
                Opis = forma["Opis"].ToString().Trim(),
                KategorijaId = kategorijaId,
                DatumPocetka = datumPocetka,
                TrajanjeSati = trajanjeSati == 0 ? 24 : trajanjeSati,
                Slike = forma.Files.Where(f => f.Name == "Slike").ToList()
            };
        }

        
        [Authorize(Roles = Roles.Prodavac)]
        public async Task<IActionResult> Edit(int id)
        {
            var automobil = _automobilRepository.GetById(id);
            if (automobil is null) return NotFound();

            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik is null || automobil.ProdavacId != korisnik.Id) return Forbid();

            if (_automobilRepository.ImaPonude(id))
            {
                TempData["Greska"] = "Automobil vec ima ponude i ne moze se vise menjati.";
                return RedirectToAction(nameof(MojiAutomobili));
            }

            var model = new AutomobilFormViewModel
            {
                Id = automobil.Id,
                Marka = automobil.Marka,
                Model = automobil.Model,
                Godina = automobil.Godina,
                PocetnaCena = automobil.PocetnaCena,
                Opis = automobil.Opis,
                KategorijaId = automobil.KategorijaId,
                DatumPocetka = automobil.DatumPocetka,
                TrajanjeSati = (int)Math.Round((automobil.DatumZavrsetka - automobil.DatumPocetka).TotalHours),
                PostojeceSlike = _automobilRepository.GetSlike(id)
                .Select(s => new SlikaZaPrikaz { Id = s.Id, Putanja = s.Putanja })
                .ToList(),
                DostupneKategorije = _kategorijaRepository.GetAll()
            };

            return View(model);
        }

        
        [HttpPost]
        [Authorize(Roles = Roles.Prodavac)]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public async Task<IActionResult> EditPost(int id)
        {
            var unos = CitajFormuAutomobila();
            unos.Id = id;

            TryValidateModel(unos);

            if (!ModelState.IsValid)
            {
                unos.DostupneKategorije = _kategorijaRepository.GetAll();
                return View(unos);
            }

            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik is null) return Challenge();

            var noveSlike = await SacuvajSlikeAsync(unos.Slike);

            var automobilBO = new AutomobilBO
            {
                Id = unos.Id,
                Marka = unos.Marka,
                Model = unos.Model,
                Godina = unos.Godina,
                PocetnaCena = unos.PocetnaCena,
                Opis = unos.Opis,
                KategorijaId = unos.KategorijaId,
                ProdavacId = korisnik.Id,
                DatumPocetka = unos.DatumPocetka,
                DatumZavrsetka = unos.DatumPocetka.AddHours(unos.TrajanjeSati),
                PutanjeSlika = noveSlike
            };

            var uspeh = _automobilRepository.Update(automobilBO);
            if (!uspeh)
            {
                TempData["Greska"] = "Izmena nije moguca (automobil vec ima ponude ili ne postoji).";
                return RedirectToAction(nameof(MojiAutomobili));
            }

            TempData["Poruka"] = "Oglas je uspesno izmenjen.";
            return RedirectToAction(nameof(Details), new { id = unos.Id });
        }

        
        [HttpPost]
        [Authorize(Roles = Roles.Prodavac)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik is null) return Challenge();

            var uspeh = _automobilRepository.Delete(id, korisnik.Id);
            TempData[uspeh ? "Poruka" : "Greska"] = uspeh
                ? "Oglas je uspesno uklonjen."
                : "Oglas nije moguce ukloniti (vec ima ponude ili ne pripada vama).";

            return RedirectToAction(nameof(MojiAutomobili));
        }

        //prodavac vidi kontakt kupca koji je ponudio najvisu cenu (nakon isteka licitacije)
        [Authorize(Roles = Roles.Prodavac + "," + Roles.Administrator)]
        public async Task<IActionResult> Pobednik(int id)
        {
            var automobil = _automobilRepository.GetById(id);
            if (automobil is null) return NotFound();

            var korisnik = await _userManager.GetUserAsync(User);
            var jeAdmin = User.IsInRole(Roles.Administrator);
            if (!jeAdmin && (korisnik is null || automobil.ProdavacId != korisnik.Id)) return Forbid();

            if (!automobil.Zavrsena)
            {
                TempData["Greska"] = "Licitacija za ovaj automobil jos nije zavrsena.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var najvisaPonuda = _ponudaRepository.GetNajvisaPonuda(id);
            ViewBag.Automobil = automobil;

            if (najvisaPonuda is null)
            {
                ViewBag.Poruka = "Za ovaj automobil nije bilo nijedne ponude.";
                return View((KupacKontaktBO?)null);
            }

            var kupac = await _userManager.FindByIdAsync(najvisaPonuda.KupacId);
            if (kupac is null) return NotFound();

            var kontakt = new KupacKontaktBO
            {
                ImePrezime = $"{kupac.Ime} {kupac.Prezime}",
                Email = kupac.Email ?? string.Empty,
                Telefon = kupac.PhoneNumber ?? "Nije unet",
                IznosPonude = najvisaPonuda.Iznos
            };

            return View(kontakt);
        }

        private async Task<List<string>> SacuvajSlikeAsync(List<IFormFile>? slike)
        {
            var putanje = new List<string>();
            if (slike is null || slike.Count == 0) return putanje;

            var webRoot = string.IsNullOrEmpty(_webHostEnvironment.WebRootPath)
                ? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot")
                : _webHostEnvironment.WebRootPath;
            var direktorijum = Path.Combine(webRoot, "images", "automobili");
            Directory.CreateDirectory(direktorijum);

            foreach (var slika in slike)
            {
                if (slika.Length == 0) continue;

                var ekstenzija = Path.GetExtension(slika.FileName);
                var jedinstvenoIme = $"{Guid.NewGuid()}{ekstenzija}";
                var punaPutanja = Path.Combine(direktorijum, jedinstvenoIme);

                using (var stream = new FileStream(punaPutanja, FileMode.Create))
                {
                    await slika.CopyToAsync(stream);
                }

                putanje.Add($"/images/automobili/{jedinstvenoIme}");
            }

            return putanje;
        }

        [HttpPost]
        [Authorize(Roles = Roles.Prodavac)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ObrisiSliku(int slikaId, int automobilId)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik is null) return Challenge();

            _automobilRepository.ObrisiSliku(slikaId, korisnik.Id);
            return RedirectToAction(nameof(Edit), new { id = automobilId });
        }
    }

    //pomocna klasa - samo za prikaz kontakt podataka pobednika licitacije prodavcu
    public class KupacKontaktBO
    {
        public string ImePrezime { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public decimal IznosPonude { get; set; }
    }
}
