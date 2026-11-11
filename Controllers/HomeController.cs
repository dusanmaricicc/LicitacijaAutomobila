using System.Diagnostics;
using LicitacijaAutomobila.Models;
using LicitacijaAutomobila.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LicitacijaAutomobila.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAutomobilRepository _automobilRepository;

        public HomeController(IAutomobilRepository automobilRepository)
        {
            _automobilRepository = automobilRepository;
        }

        public IActionResult Index()
        {
            // Pocetna strana prikazuje nekoliko trenutno aktivnih licitacija
            var aktivni = _automobilRepository
                .Pretrazi(new AutomobilFilter { SamoAktivne = true })
                .OrderByDescending(a => a.DatumPostavljanja)
                .Take(6)
                .ToList();

            return View(aktivni);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
