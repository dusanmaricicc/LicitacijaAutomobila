using LicitacijaAutomobila.Data;
using LicitacijaAutomobila.Models.Entities;
using LicitacijaAutomobila.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LicitacijaAutomobila.Controllers
{
    [Authorize(Roles = Roles.Kupac)]
    public class PonudeController : Controller
    {
        private readonly IPonudaRepository _ponudaRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public PonudeController(IPonudaRepository ponudaRepository, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _ponudaRepository = ponudaRepository;
            _userManager = userManager;
            _configuration = configuration;
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DodajPonudu(int automobilId, decimal iznos)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik is null) return Challenge();

            var minimalniKorak = _configuration.GetValue<int>("Licitacija:MinimalniKorakPonudeDinara", 1000);

            var (uspeh, poruka) = _ponudaRepository.DodajPonudu(automobilId, korisnik.Id, iznos, minimalniKorak);

            TempData[uspeh ? "Poruka" : "Greska"] = poruka;
            return RedirectToAction("Details", "Automobili", new { id = automobilId });
        }

        //pregled statusa svih ponuda prijavljenog kupca
        public async Task<IActionResult> MojePonude()
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik is null) return Challenge();

            var ponude = _ponudaRepository.GetByKupac(korisnik.Id);
            return View(ponude);
        }
    }
}
