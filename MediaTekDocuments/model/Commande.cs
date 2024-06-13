using System;

namespace MediaTekDocuments.model
{
    public class Commande
    {
        public string Id { get; set; }
        public string IdLivreDvd { get; set; }
        public DateTime DateCommande { get; set; }
        public int Montant { get; set; }
        public int NbExemplaire { get; set; }
        public int SuiviId { get; set; }
        public string EtapeSuivi { get; set; }

        public Commande(string id, string idLivreDvd, DateTime dateCommande, int montant, int nbExemplaire, int suiviId, string etapeSuivi)
        {
            Id = id;
            IdLivreDvd = idLivreDvd;
            DateCommande = dateCommande;
            Montant = montant;
            NbExemplaire = nbExemplaire;
            SuiviId = suiviId;
            EtapeSuivi = etapeSuivi;
        }
    }

}

