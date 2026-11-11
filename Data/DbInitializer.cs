using LicitacijaAutomobila.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace LicitacijaAutomobila.Data
{
    // Pokrece se jednom pri startu aplikacije i:
    // 1. Kreira baze
    // 2. Seeduje uloge (Administrator, Prodavac, Kupac)
    // 3. Kreira podrazumevanog administratora
    // 4. Ubacuje par pocetnih kategorija i oglasa (radi lakseg testiranja)
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            context.Database.EnsureCreated();

            //Uloge
            foreach (var rola in Roles.Sve)
            {
                if (!await roleManager.RoleExistsAsync(rola))
                {
                    await roleManager.CreateAsync(new IdentityRole(rola));
                }
            }

            // Administrator nalog (obavezni parametri)
            const string adminEmail = "admin@licitacija.rs";
            const string adminPassword = "Admin123!";

            if (await userManager.FindByEmailAsync(adminEmail) is null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    Ime = "Glavni",
                    Prezime = "Administrator",
                    Aktivan = true,
                    DatumRegistracije = DateTime.Now
                };

                var rezultat = await userManager.CreateAsync(admin, adminPassword);
                if (rezultat.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, Roles.Administrator);
                }
            }

            // prodavac i kupac (olaksava testiranje aplikacije)
            var prodavac = await CreateDemoUserAsync(userManager, "prodavac@licitacija.rs", "Prodavac123!",
                "Dusan", "Maricic", Roles.Prodavac);

            var kupac = await CreateDemoUserAsync(userManager, "kupac@licitacija.rs", "Kupac123!",
                "Sinisa", "Borovic", Roles.Kupac);

            // Kategorije
            if (!context.Kategorije.Any())
            {
                context.Kategorije.AddRange(
                    new Kategorija { Naziv = "Limuzina" },
                    new Kategorija { Naziv = "SUV" },
                    new Kategorija { Naziv = "Hečbek" },
                    new Kategorija { Naziv = "Karavan" },
                    new Kategorija { Naziv = "Kabriolet" },
                    new Kategorija { Naziv = "Terensko vozilo" }
                );
                context.SaveChanges();
            }

            // Nekoliko automobila na licitaciji (samo ako baza jos nema nijedan)
            if (!context.Automobili.Any() && prodavac is not null)
            {
                var limuzina = context.Kategorije.First(k => k.Naziv == "Limuzina");
                var suv = context.Kategorije.First(k => k.Naziv == "SUV");
                var hecbek = context.Kategorije.First(k => k.Naziv == "Hečbek");

                context.Automobili.AddRange(
                    new Automobil
                    {
                        Marka = "Volkswagen",
                        Model = "Passat",
                        Godina = 2018,
                        PocetnaCena = 800000,
                        Opis = "Volkswagen Passat 2.0 TDI, redovno servisiran, jedan vlasnik.",
                        KategorijaId = limuzina.Id,
                        ProdavacId = prodavac.Id,
                        DatumPostavljanja = DateTime.Now,
                        DatumPocetka = DateTime.Now.AddMinutes(-30),
                        DatumZavrsetka = DateTime.Now.AddDays(2)
                    },
                    new Automobil
                    {
                        Marka = "BMW",
                        Model = "X3",
                        Godina = 2020,
                        PocetnaCena = 2500000,
                        Opis = "BMW X3 xDrive20d, M paket, uvoz iz Nemacke, ocuvan.",
                        KategorijaId = suv.Id,
                        ProdavacId = prodavac.Id,
                        DatumPostavljanja = DateTime.Now,
                        DatumPocetka = DateTime.Now.AddMinutes(-10),
                        DatumZavrsetka = DateTime.Now.AddHours(6)
                    },
                    new Automobil
                    {
                        Marka = "Golf",
                        Model = "7",
                        Godina = 2016,
                        PocetnaCena = 650000,
                        Opis = "Golf 7 1.6 TDI, klima, servisna knjiga.",
                        KategorijaId = hecbek.Id,
                        ProdavacId = prodavac.Id,
                        DatumPostavljanja = DateTime.Now.AddDays(-3),
                        DatumPocetka = DateTime.Now.AddDays(-3),
                        DatumZavrsetka = DateTime.Now.AddDays(-1)
                    }
                );
                context.SaveChanges();
            }
        }

        private static async Task<ApplicationUser?> CreateDemoUserAsync(
            UserManager<ApplicationUser> userManager, string email, string password,
            string ime, string prezime, string rola)
        {
            var postojeci = await userManager.FindByEmailAsync(email);
            if (postojeci is not null) return postojeci;

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Ime = ime,
                Prezime = prezime,
                Adresa = "Bulevar Oslobođenja 1, Novi Sad",
                Aktivan = true,
                DatumRegistracije = DateTime.Now
            };

            var rezultat = await userManager.CreateAsync(user, password);
            if (rezultat.Succeeded)
            {
                await userManager.AddToRoleAsync(user, rola);
                return user;
            }
            return null;
        }
    }
}
