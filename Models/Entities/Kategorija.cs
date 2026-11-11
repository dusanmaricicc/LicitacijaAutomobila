using System.ComponentModel.DataAnnotations;

namespace LicitacijaAutomobila.Models.Entities
{
    public class Kategorija
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Naziv kategorije je obavezan.")]
        [StringLength(50, ErrorMessage = "Naziv kategorije ne sme biti duzi od 50 karaktera.")]
        public string Naziv { get; set; } = string.Empty;

        public virtual ICollection<Automobil> Automobili { get; set; } = new List<Automobil>();
    }
}
