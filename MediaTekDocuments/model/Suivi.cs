namespace MediaTekDocuments.model
{
    public class Suivi
    {
        public int Id { get; set; }
        public string Etape { get; set; }

        public Suivi(int id, string etape)
        {
            Id = id;
            Etape = etape;
        }
    }
}
