using System.ComponentModel.DataAnnotations;

namespace LicitacijaAutomobila.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ime je obavezno.")]
        [StringLength(50)]
        [RegularExpression(@"^[A-ZČĆŽŠĐ][a-zA-ZčćžšđČĆŽŠĐ]*$",
        ErrorMessage = "Ime mora poceti velikim slovom i sadrzati samo slova.")]
        [Display(Name = "Ime")]
        public string Ime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime je obavezno.")]
        [StringLength(50)]
        [RegularExpression(@"^[A-ZČĆŽŠĐ][a-zA-ZčćžšđČĆŽŠĐ]*$",
        ErrorMessage = "Prezime mora poceti velikim slovom i sadrzati samo slova.")]
        [Display(Name = "Prezime")]
        public string Prezime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Unesite ispravnu email adresu.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Unesite ispravan broj telefona.")]
        [Display(Name = "Telefon")]
        public string? Telefon { get; set; }

        [Display(Name = "Adresa")]
        public string? Adresa { get; set; }

        [Required(ErrorMessage = "Lozinka je obavezna.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Lozinka mora imati najmanje {2} karaktera.")]
        [DataType(DataType.Password)]
        [Display(Name = "Lozinka")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Potvrda lozinke je obavezna.")]
        [DataType(DataType.Password)]
        [Display(Name = "Potvrda lozinke")]
        [Compare("Password", ErrorMessage = "Lozinka i potvrda lozinke se ne poklapaju.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Izaberite ulogu.")]
        [Display(Name = "Registrujem se kao")]
        public string Rola { get; set; } = string.Empty; // "Prodavac" ili "Kupac"
    }
}
