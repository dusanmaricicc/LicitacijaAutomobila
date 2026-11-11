using LicitacijaAutomobila.Data;
using LicitacijaAutomobila.Models.Entities;
using LicitacijaAutomobila.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LicitacijaAutomobila.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterViewModel { Rola = Roles.Kupac });
        }

        // Samostalna registracija je dozvoljena samo za Prodavca i Kupca.
        // Administratorske naloge kreira Administrator kroz AdminController.
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (model.Rola != Roles.Prodavac && model.Rola != Roles.Kupac)
            {
                ModelState.AddModelError(nameof(model.Rola), "Nevazeca uloga.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Provera da li vec postoji nalog sa istim podacima (email)
            var postojeci = await _userManager.FindByEmailAsync(model.Email);
            if (postojeci is not null)
            {
                ModelState.AddModelError(nameof(model.Email),
                    "Nalog sa ovom email adresom vec postoji. Ako je ovo vas nalog, prijavite se ili izmenite profil.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.Telefon,
                Ime = model.Ime,
                Prezime = model.Prezime,
                Adresa = model.Adresa,
                Aktivan = true,
                DatumRegistracije = DateTime.Now
            };

            var rezultat = await _userManager.CreateAsync(user, model.Password);

            if (rezultat.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Rola);
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["Poruka"] = "Uspesno ste se registrovali.";
                return RedirectToAction("Index", "Home");
            }

            foreach (var greska in rezultat.Errors)
            {
                ModelState.AddModelError(string.Empty, greska.Description);
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Pogresan email ili lozinka.");
                return View(model);
            }

            if (!user.Aktivan)
            {
                ModelState.AddModelError(string.Empty, "Vas nalog je deaktiviran. Obratite se administratoru.");
                return View(model);
            }

            var rezultat = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (rezultat.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Pogresan email ili lozinka.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
