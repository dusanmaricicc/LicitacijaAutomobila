namespace LicitacijaAutomobila.Models.BO
{
    public class AutomobilBO
    {
        public int Id { get; set; }
        public string Marka { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Godina { get; set; }
        public decimal PocetnaCena { get; set; }
        public string Opis { get; set; } = string.Empty;
        public DateTime DatumPostavljanja { get; set; }
        public DateTime DatumPocetka { get; set; }
        public DateTime DatumZavrsetka { get; set; }

        public int KategorijaId { get; set; }
        public string KategorijaNaziv { get; set; } = string.Empty;

        public string ProdavacId { get; set; } = string.Empty;
        public string ProdavacIme { get; set; } = string.Empty;

        public List<string> PutanjeSlika { get; set; } = new();

        public decimal TrenutnaCena { get; set; }
        public int BrojPonuda { get; set; }
        public string? NajviciPonudjacId { get; set; }
        public string? NajviciPonudjacIme { get; set; }

        public bool Aktivna => DateTime.Now >= DatumPocetka && DateTime.Now < DatumZavrsetka;
        public bool Zavrsena => DateTime.Now >= DatumZavrsetka;
        public bool JosNijePocela => DateTime.Now < DatumPocetka;

        public string Naslov => $"{Marka} {Model} ({Godina})";
    }
}
