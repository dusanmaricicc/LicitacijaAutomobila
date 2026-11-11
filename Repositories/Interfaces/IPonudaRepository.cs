using LicitacijaAutomobila.Models.BO;

namespace LicitacijaAutomobila.Repositories.Interfaces
{
    public interface IPonudaRepository
    {
        IEnumerable<PonudaBO> GetByAutomobil(int automobilId);
        PonudaBO? GetNajvisaPonuda(int automobilId);
        IEnumerable<PonudaBO> GetByKupac(string kupacId);

        // Dodaje ponudu uz validaciju poslovnih pravila (licitacija aktivna, minimalni korak 1000 din...)
        // Vraca (true, "") ako je uspelo, ili (false, "poruka o gresci") ako nije.
        (bool uspeh, string poruka) DodajPonudu(int automobilId, string kupacId, decimal iznos, int minimalniKorak);
    }
}
