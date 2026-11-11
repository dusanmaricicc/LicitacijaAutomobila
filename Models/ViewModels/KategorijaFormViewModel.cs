using System.ComponentModel.DataAnnotations;

namespace LicitacijaAutomobila.Models.ViewModels
{
    public class KategorijaFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Naziv kategorije je obavezan.")]
        [StringLength(50)]
        public string Naziv { get; set; } = string.Empty;
    }
}
