using LicitacijaAutomobila.Models.BO;

namespace LicitacijaAutomobila.Repositories.Interfaces
{
    public class AutomobilFilter
    {
        public string? Marka { get; set; }
        public int? KategorijaId { get; set; }
        public decimal? CenaOd { get; set; }
        public decimal? CenaDo { get; set; }
        public int? GodinaOd { get; set; }
        public bool SamoAktivne { get; set; } = true;
    }

    public interface IAutomobilRepository
    {
        IEnumerable<AutomobilBO> GetAll();
        IEnumerable<AutomobilBO> Pretrazi(AutomobilFilter filter);
        IEnumerable<AutomobilBO> GetByProdavac(string prodavacId);
        AutomobilBO? GetById(int id);

        int Add(AutomobilBO automobil, IEnumerable<string> putanjeSlika);

        // Vraca false ako automobil ne postoji, ne pripada prodavcu ili vec ima ponude (ne moze da se menja)
        bool Update(AutomobilBO automobil);

        // Vraca false ako automobil ne postoji, ne pripada prodavcu ili vec ima ponude (ne moze da se obrise)
        bool Delete(int id, string prodavacId);

        bool ImaPonude(int automobilId);

        

        IEnumerable<(int Id, string Putanja)> GetSlike(int automobilId);
        bool ObrisiSliku(int slikaId, string prodavacId);
    }
}
