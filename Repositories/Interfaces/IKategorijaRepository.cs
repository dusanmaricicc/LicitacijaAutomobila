using LicitacijaAutomobila.Models.BO;

namespace LicitacijaAutomobila.Repositories.Interfaces
{
    public interface IKategorijaRepository
    {
        IEnumerable<KategorijaBO> GetAll();
        KategorijaBO? GetById(int id);
        void Add(KategorijaBO kategorija);
        void Update(KategorijaBO kategorija);

        // Vraca false ukoliko kategorija ne postoji ili je u upotrebi (postoje automobili u toj kategoriji)
        bool Delete(int id);

        bool PostojiSaNazivom(string naziv, int? izuzetiId = null);
    }
}
