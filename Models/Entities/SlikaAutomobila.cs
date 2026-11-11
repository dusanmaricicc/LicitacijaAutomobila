namespace LicitacijaAutomobila.Models.Entities
{
    public class SlikaAutomobila
    {
        public int Id { get; set; }
        public string PutanjaSlike { get; set; } = string.Empty;

        public int AutomobilId { get; set; }
        public virtual Automobil? Automobil { get; set; }
    } 
}
