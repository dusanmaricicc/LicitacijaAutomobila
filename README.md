# Licitacija automobila – veb aplikacija (ASP.NET Core MVC .NET 8 + EF Core 9 + MS SQL)

Projekat iz Praktikuma primenjenog programiranja – Veb aplikacija za licitaciju automobila.

## Tehnologije

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core 9 (SQL Server provider)
- MS SQL Server (LocalDB podrazumevano)
- ASP.NET Core Identity (autentifikacija i autorizacija, bez spoljnih provajdera)
- Bootstrap 5 (CDN)
- Repository patern (`Repositories/Interfaces` + `Repositories/EF`)

## Struktura projekta

```
Controllers/            Kontroleri (Home, Account, Automobili, Ponude, Profil, Obavestenja, Admin)
Data/                    ApplicationDbContext, DbInitializer (seed), Roles
Models/Entities/         EF entiteti (ApplicationUser, Kategorija, Automobil, SlikaAutomobila, Ponuda, Notifikacija)
Models/BO/               Business objekti (DTO-ovi koje repozitorijumi vracaju kontrolerima)
Models/ViewModels/       ViewModel-i za forme i validaciju
Repositories/Interfaces/ Interfejsi repozitorijuma (Repository patern)
Repositories/EF/         EF implementacije repozitorijuma
Views/                   Razor Views (Bootstrap)
Database/CreateDatabase.sql   Obavezna SQL skripta za kreiranje i popunjavanje baze
```

## Uloge i funkcionalnosti

- **Administrator** – upravljanje nalozima prodavaca/kupaca (CRUD), upravljanje kategorijama automobila, pregled izveštaja o licitacijama.
- **Prodavac** – registracija/prijava, dodavanje automobila na licitaciju (sa slikama), izmena/uklanjanje oglasa (samo dok nema ponuda), pregled kontakta kupca sa najvišom ponudom nakon isteka licitacije.
- **Kupac** – registracija/prijava, pregled i pretraga automobila, davanje ponuda (svaka nova ponuda mora biti za najmanje 1.000 RSD viša od prethodne), pregled statusa svojih ponuda i obaveštenja kada izgubi status najviše ponude.

Trajanje licitacije se određuje po automobilu (prodavac bira datum/vreme početka i trajanje u satima prilikom dodavanja oglasa).

## Pokretanje projekta (Visual Studio 2022)

1. Otvorite `LicitacijaAutomobila.csproj` (ili `.sln` ako ga generišete) u Visual Studio 2022.
2. Proverite konekcioni string u `appsettings.json` (podrazumevano koristi LocalDB – ako imate pun SQL Server, izmenite `Server=` deo).
3. Restore NuGet paketa će se pokrenuti automatski (ili `dotnet restore` iz terminala).
4. Pokrenite aplikaciju (F5 / `dotnet run`).
   - Baza `LicitacijaAutomobilaDb` će biti automatski kreirana (`EnsureCreated`) i popunjena početnim podacima (`Data/DbInitializer.cs`) pri prvom pokretanju – uloge, administratorski nalog, demo prodavac/kupac, kategorije i par demo automobila.

### Napomena o EF Core migracijama (opciono, preporučeno za odbranu)

Projekat po difoltu koristi `EnsureCreated()` radi jednostavnosti pokretanja. Ako želite da radite sa EF Core migracijama (što je uobičajeno u nastavi):

```
Add-Migration InitialCreate
Update-Database
```

nakon čega u `Program.cs` zamenite `context.Database.EnsureCreated()` (u `DbInitializer.cs`) sa `context.Database.Migrate()`.

### Obavezna SQL skripta

Fajl `Database/CreateDatabase.sql` sadrži skriptu (ne backup) za kreiranje baze, svih tabela (Identity + domenske) i seed podataka (uloge, kategorije). Korisnički nalozi (admin/demo) se, iz tehničkih razloga (Identity heš lozinki), kreiraju kroz kod pri prvom pokretanju aplikacije – detaljno objašnjenje se nalazi u komentaru na vrhu skripte, zajedno sa uputstvom kako u SSMS-u eksportovati i podatke (Tasks → Generate Scripts → Schema and Data) ukoliko je za predaju potrebna potpuno popunjena skripta.

## Pristupni parametri (obavezno po uputstvu projekta)

| Uloga      | Email                     | Lozinka         |
|------------|---------------------------|-----------------|
| Administrator | admin@licitacija.rs    | Admin123!       |
| Prodavac (demo) | prodavac@licitacija.rs | Prodavac123! |
| Kupac (demo)    | kupac@licitacija.rs    | Kupac123!    |

Novi nalozi prodavaca i kupaca mogu se i samostalno registrovati preko `/Account/Register`. Administratorski nalog se ne može kreirati samoregistracijom – kreira ga isključivo postojeći administrator (ili je već zasejan gore navedenim podacima).

## Poslovna pravila koja su implementirana

- Svaka nova ponuda mora biti najmanje 1.000 RSD viša od trenutno najviše ponude (ili od početne cene ako ponuda još nema) – konfigurabilno u `appsettings.json` (`Licitacija:MinimalniKorakPonudeDinara`).
- Automobil se ne može menjati/brisati nakon što dobije prvu ponudu.
- Prodavac ne može licitirati na sopstveni automobil.
- Nakon isteka vremena licitacije, prodavac (i administrator) mogu videti kontakt podatke kupca sa najvišom ponudom.
- Kupac koji izgubi status najviše ponude dobija obaveštenje (vidljivo u meniju „Obaveštenja“, sa brojačem nepročitanih – osvežava se nakon učitavanja stranice).
- Prilikom kreiranja korisničkog naloga (i od strane administratora i samoregistracijom), sistem proverava da li već postoji nalog sa istim emailom i o tome obaveštava korisnika.

## Projektne odluke

Pojedini detalji nisu bili unapred definisani, pa su usvojena sledeća rešenja:

- **Minimalni korak ponude:** 1.000 RSD. Vrednost je podesiva u `appsettings.json`.
- **Trajanje licitacije:** određuje ga prodavac pri objavi oglasa (broj sati, podrazumevano 24h).
- **Slike automobila:** podržano je više slika po oglasu (galerija), a čuvaju se u `wwwroot/images/automobili`.
- **Administratorski nalozi:** ne mogu se kreirati javnom registracijom, već isključivo kroz administratorski panel ili početne (seed) podatke, što je bezbednosno opravdano.


