using System.ComponentModel.DataAnnotations;

namespace LicitacijaAutomobila.Models.ViewModels
{
    public class ProfilViewModel
    {
        [Required(ErrorMessage = "Ime je obavezno.")]
        [StringLength(50)]
        public string Ime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime je obavezno.")]
        [StringLength(50)]
        public string Prezime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? Telefon { get; set; }

        public string? Adresa { get; set; }
    }
}
