using LicitacijaAutomobila.Models.BO;

namespace LicitacijaAutomobila.Models.ViewModels
{
    public class IzvestajViewModel
    {
        public int UkupnoAutomobila { get; set; }
        public int UkupnoAktivnihLicitacija { get; set; }
        public int UkupnoZavrsenihLicitacija { get; set; }
        public int UkupnoPonuda { get; set; }
        public decimal UkupnaVrednostProdatihAutomobila { get; set; }

        public IEnumerable<AutomobilBO> ZavrseneLicitacije { get; set; } = new List<AutomobilBO>();
    }
}
