namespace LicitacijaAutomobila.Models.BO
{
    public class NotifikacijaBO
    {
        public int Id { get; set; }
        public string Poruka { get; set; } = string.Empty;
        public DateTime Datum { get; set; }
        public bool Procitano { get; set; }
        public int? AutomobilId { get; set; }
    }
}
