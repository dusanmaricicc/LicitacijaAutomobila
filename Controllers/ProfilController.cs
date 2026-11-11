using LicitacijaAutomobila.Models.Entities;
using LicitacijaAutomobila.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LicitacijaAutomobila.Controllers
{
    [Authorize]
    public class ProfilController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfilController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Edit()
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik is null) return Challenge();

            var model = new ProfilViewModel
            {
                Ime = korisnik.Ime,
                Prezime = korisnik.Prezime,
                Email = korisnik.Email ?? string.Empty,
                Telefon = korisnik.PhoneNumber,
                Adresa = korisnik.Adresa
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfilViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik is null) return Challenge();

            // Ako korisnik menja email, proveravamo da li novi email vec koristi neko drugi
            if (!string.Equals(korisnik.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                var postojeci = await _userManager.FindByEmailAsync(model.Email);
                if (postojeci is not null && postojeci.Id != korisnik.Id)
                {
                    ModelState.AddModelError(nameof(model.Email), "Ova email adresa je vec u upotrebi.");
                    return View(model);
                }

                korisnik.Email = model.Email;
                korisnik.UserName = model.Email;
                korisnik.NormalizedEmail = model.Email.ToUpperInvariant();
                korisnik.NormalizedUserName = model.Email.ToUpperInvariant();
            }

            korisnik.Ime = model.Ime;
            korisnik.Prezime = model.Prezime;
            korisnik.PhoneNumber = model.Telefon;
            korisnik.Adresa = model.Adresa;

            var rezultat = await _userManager.UpdateAsync(korisnik);
            if (rezultat.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(korisnik);
                TempData["Poruka"] = "Profil je uspesno azuriran.";
                return RedirectToAction(nameof(Edit));
            }

            foreach (var greska in rezultat.Errors)
                ModelState.AddModelError(string.Empty, greska.Description);

            return View(model);
        }
    }
}
