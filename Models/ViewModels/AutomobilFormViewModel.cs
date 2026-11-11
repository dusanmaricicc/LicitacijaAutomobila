using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using LicitacijaAutomobila.Models.BO;

namespace LicitacijaAutomobila.Models.ViewModels
{
    public class AutomobilFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Marka je obavezna.")]
        [StringLength(30)]
        public string Marka { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model je obavezan.")]
        [StringLength(30)]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "Godina proizvodnje je obavezna.")]
        [Range(1950, 2100, ErrorMessage = "Unesite ispravnu godinu proizvodnje.")]
        public int Godina { get; set; }

        [Required(ErrorMessage = "Pocetna cena je obavezna.")]
        [Range(1, 100000000, ErrorMessage = "Pocetna cena mora biti veca od 0.")]
        [Display(Name = "Pocetna cena (RSD)")]
        public decimal PocetnaCena { get; set; }

        [Required(ErrorMessage = "Opis je obavezan.")]
        [StringLength(1000, ErrorMessage = "Opis moze imati najvise {1} karaktera.")]
        public string Opis { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategorija je obavezna.")]
        [Display(Name = "Kategorija")]
        public int KategorijaId { get; set; }

        [Required(ErrorMessage = "Datum i vreme pocetka licitacije su obavezni.")]
        [Display(Name = "Pocetak licitacije")]
        [DataType(DataType.DateTime)]
        public DateTime DatumPocetka { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Trajanje licitacije je obavezno.")]
        [Range(1, 720, ErrorMessage = "Trajanje mora biti izmedju 1 i 720 sati (30 dana).")]
        [Display(Name = "Trajanje licitacije (u satima)")]
        public int TrajanjeSati { get; set; } = 24;

        [Display(Name = "Slike automobila")]
        public List<IFormFile>? Slike { get; set; }

        public List<SlikaZaPrikaz> PostojeceSlike { get; set; } = new();

        public IEnumerable<KategorijaBO> DostupneKategorije { get; set; } = new List<KategorijaBO>();
    }

    public class SlikaZaPrikaz
    {
        public int Id { get; set; }
        public string Putanja { get; set; } = string.Empty;
    }
}
