using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicitacijaAutomobila.Models.Entities
{
    public class Automobil
    {
        public int Id { get; set; }

        [Required, StringLength(30)]
        public string Marka { get; set; } = string.Empty;

        [Required, StringLength(30)]
        public string Model { get; set; } = string.Empty;

        [Range(1950, 2100)]
        public int Godina { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Pocetna cena mora biti pozitivan broj.")]
        public decimal PocetnaCena { get; set; }

        [Required, StringLength(1000)]
        public string Opis { get; set; } = string.Empty;

        public DateTime DatumPostavljanja { get; set; } = DateTime.Now;
        public DateTime DatumPocetka { get; set; }
        public DateTime DatumZavrsetka { get; set; }

        public int KategorijaId { get; set; }
        public virtual Kategorija? Kategorija { get; set; }

        
        [Required]
        public string ProdavacId { get; set; } = string.Empty;
        public virtual ApplicationUser? Prodavac { get; set; }

        public virtual ICollection<SlikaAutomobila> Slike { get; set; } = new List<SlikaAutomobila>();
        public virtual ICollection<Ponuda> Ponude { get; set; } = new List<Ponuda>();

        [NotMapped]
        public bool Aktivna => DateTime.Now >= DatumPocetka && DateTime.Now < DatumZavrsetka;

        [NotMapped]
        public bool Zavrsena => DateTime.Now >= DatumZavrsetka;

        [NotMapped]
        public bool JosNijePocela => DateTime.Now < DatumPocetka;
    }
}
