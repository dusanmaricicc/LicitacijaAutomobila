using LicitacijaAutomobila.Models.BO;

namespace LicitacijaAutomobila.Repositories.Interfaces
{
    public interface INotifikacijaRepository
    {
        IEnumerable<NotifikacijaBO> GetAll(string korisnikId);
        int BrojNeprocitanih(string korisnikId);
        void Dodaj(string korisnikId, string poruka, int? automobilId);
        void OznaciSveProcitano(string korisnikId);
    }
}
