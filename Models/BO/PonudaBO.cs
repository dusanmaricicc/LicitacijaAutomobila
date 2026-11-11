namespace LicitacijaAutomobila.Models.BO
{
    public class PonudaBO
    {
        public int Id { get; set; }
        public decimal Iznos { get; set; }
        public DateTime VremePonude { get; set; }

        public int AutomobilId { get; set; }
        public string AutomobilNaslov { get; set; } = string.Empty;
        public bool AutomobilAktivna { get; set; }
        public bool AutomobilZavrsena { get; set; }

        public string KupacId { get; set; } = string.Empty;
        public string KupacIme { get; set; } = string.Empty;

        
        public bool JeNajvisa { get; set; }
    }
}
