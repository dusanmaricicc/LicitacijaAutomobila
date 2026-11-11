using System.ComponentModel.DataAnnotations.Schema;

namespace LicitacijaAutomobila.Models.Entities
{
    public class Ponuda
    {
        public int Id { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Iznos { get; set; }

        public DateTime VremePonude { get; set; } = DateTime.Now;

        public int AutomobilId { get; set; }
        public virtual Automobil? Automobil { get; set; }

        [Column(TypeName = "nvarchar(450)")]
        public string KupacId { get; set; } = string.Empty;
        public virtual ApplicationUser? Kupac { get; set; }
    }
}
