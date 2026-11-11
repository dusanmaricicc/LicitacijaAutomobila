using LicitacijaAutomobila.Models.Entities;
using LicitacijaAutomobila.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LicitacijaAutomobila.Controllers
{
    [Authorize]
    public class ObavestenjaController : Controller
    {
        private readonly INotifikacijaRepository _notifikacijaRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ObavestenjaController(INotifikacijaRepository notifikacijaRepository, UserManager<ApplicationUser> userManager)
        {
            _notifikacijaRepository = notifikacijaRepository;
            _userManager = userManager;
        }

        // lista svih obavestenja ulogovanog korisnika; ulaskom na stranicu se oznacavaju kao procitana
        public async Task<IActionResult> Index()
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik is null) return Challenge();

            var obavestenja = _notifikacijaRepository.GetAll(korisnik.Id);
            _notifikacijaRepository.OznaciSveProcitano(korisnik.Id);

            return View(obavestenja);
        }
    }
}
