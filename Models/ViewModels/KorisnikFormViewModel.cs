using System.ComponentModel.DataAnnotations;

namespace LicitacijaAutomobila.Models.ViewModels
{
    public class KorisnikFormViewModel
    {
        public string? Id { get; set; } // null = kreiranje novog korisnika

        [Required(ErrorMessage = "Ime je obavezno.")]
        [StringLength(50)]
        [RegularExpression(@"^[A-ZČĆŽŠĐ][a-zA-ZčćžšđČĆŽŠĐ]*$",
        ErrorMessage = "Ime mora poceti velikim slovom i sadrzati samo slova.")]
        public string Ime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime je obavezno.")]
        [StringLength(50)]
        [RegularExpression(@"^[A-ZČĆŽŠĐ][a-zA-ZčćžšđČĆŽŠĐ]*$",
        ErrorMessage = "Prezime mora poceti velikim slovom i sadrzati samo slova.")]
        public string Prezime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? Telefon { get; set; }

        public string? Adresa { get; set; }

        [Required(ErrorMessage = "Izaberite ulogu.")]
        public string Rola { get; set; } = string.Empty;

        public bool Aktivan { get; set; } = true;

        // Obavezno samo prilikom kreiranja novog korisnika
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Lozinka mora imati najmanje {2} karaktera.")]
        public string? Password { get; set; }

        public bool JeIzmena => !string.IsNullOrEmpty(Id);
    }
}
