using MediaTekDocuments.manager;
using MediaTekDocuments.model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace MediaTekDocuments.dal
{
    /// <summary>
    /// Classe d'accès aux données
    /// </summary>
    public class Access
    {
        /// <summary>
        /// adresse de l'API
        /// </summary>
        private static readonly string uriApi = "http://localhost/rest_mediatekdocuments/";
        /// <summary>
        /// instance unique de la classe
        /// </summary>
        private static Access instance = null;
        /// <summary>
        /// instance de ApiRest pour envoyer des demandes vers l'api et recevoir la réponse
        /// </summary>
        private readonly ApiRest api = null;
        /// <summary>
        /// méthode HTTP pour select
        /// </summary>
        private const string GET = "GET";
        /// <summary>
        /// méthode HTTP pour insert
        /// </summary>
        private const string POST = "POST";
        /// <summary>
        /// méthode HTTP pour update
        private const string PUT = "PUT";
        /// <summary>
        /// Méthode privée pour créer un singleton
        /// initialise l'accès à l'API
        /// </summary>
        private Access()
        {
            String authenticationString;
            try
            {
                authenticationString = "admin:adminpwd";
                api = ApiRest.GetInstance(uriApi, authenticationString);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Création et retour de l'instance unique de la classe
        /// </summary>
        /// <returns>instance unique de la classe</returns>
        public static Access GetInstance()
        {
            if (instance == null)
            {
                instance = new Access();
            }
            return instance;
        }

        /// <summary>
        /// Retourne tous les genres à partir de la BDD
        /// </summary>
        /// <returns>Liste d'objets Genre</returns>
        public List<Categorie> GetAllGenres()
        {
            IEnumerable<Genre> lesGenres = TraitementRecup<Genre>(GET, "genre");
            return new List<Categorie>(lesGenres);
        }

        /// <summary>
        /// Retourne tous les rayons à partir de la BDD
        /// </summary>
        /// <returns>Liste d'objets Rayon</returns>
        public List<Categorie> GetAllRayons()
        {
            IEnumerable<Rayon> lesRayons = TraitementRecup<Rayon>(GET, "rayon");
            return new List<Categorie>(lesRayons);
        }

        /// <summary>
        /// Retourne toutes les catégories de public à partir de la BDD
        /// </summary>
        /// <returns>Liste d'objets Public</returns>
        public List<Categorie> GetAllPublics()
        {
            IEnumerable<Public> lesPublics = TraitementRecup<Public>(GET, "public");
            return new List<Categorie>(lesPublics);
        }

        /// <summary>
        /// Retourne toutes les livres à partir de la BDD
        /// </summary>
        /// <returns>Liste d'objets Livre</returns>
        public List<Livre> GetAllLivres()
        {
            List<Livre> lesLivres = TraitementRecup<Livre>(GET, "livre");
            return lesLivres;
        }

        /// <summary>
        /// Retourne toutes les dvd à partir de la BDD
        /// </summary>
        /// <returns>Liste d'objets Dvd</returns>
        public List<Dvd> GetAllDvd()
        {
            List<Dvd> lesDvd = TraitementRecup<Dvd>(GET, "dvd");
            return lesDvd;
        }

        /// <summary>
        /// Retourne toutes les revues à partir de la BDD
        /// </summary>
        /// <returns>Liste d'objets Revue</returns>
        public List<Revue> GetAllRevues()
        {
            List<Revue> lesRevues = TraitementRecup<Revue>(GET, "revue");
            return lesRevues;
        }


        /// <summary>
        /// Retourne les exemplaires d'une revue
        /// </summary>
        /// <param name="idDocument">id de la revue concernée</param>
        /// <returns>Liste d'objets Exemplaire</returns>
        public List<Exemplaire> GetExemplairesRevue(string idDocument)
        {
            String jsonIdDocument = ConvertToJson("id", idDocument);
            List<Exemplaire> lesExemplaires = TraitementRecup<Exemplaire>(GET, "exemplaire/" + jsonIdDocument);
            return lesExemplaires;
        }

        /// <summary>
        /// ecriture d'un exemplaire en base de données
        /// </summary>
        /// <param name="exemplaire">exemplaire à insérer</param>
        /// <returns>true si l'insertion a pu se faire (retour != null)</returns>
        public bool CreerExemplaire(Exemplaire exemplaire)
        {
            String jsonExemplaire = JsonConvert.SerializeObject(exemplaire, new CustomDateTimeConverter());
            try
            {
                // récupération soit d'une liste vide (requête ok) soit de null (erreur)
                List<Exemplaire> liste = TraitementRecup<Exemplaire>(POST, "exemplaire/" + jsonExemplaire);
                return (liste != null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Traitement de la récupération du retour de l'api, avec conversion du json en liste pour les select (GET)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methode">verbe HTTP (GET, POST, PUT, DELETE)</param>
        /// <param name="message">information envoyée</param>
        /// <returns>liste d'objets récupérés (ou liste vide)</returns>
        private List<T> TraitementRecup<T>(String methode, String message)
        {
            List<T> liste = new List<T>();
            try
            {
                JObject retour = api.RecupDistant(methode, message);
                // extraction du code retourné
                String code = (String)retour["code"];
                if (code.Equals("200"))
                {
                    // dans le cas du GET (select), récupération de la liste d'objets
                    if (methode.Equals(GET))
                    {
                        String resultString = JsonConvert.SerializeObject(retour["result"]);
                        // construction de la liste d'objets à partir du retour de l'api
                        liste = JsonConvert.DeserializeObject<List<T>>(resultString, new CustomBooleanJsonConverter());
                    }
                }
                else
                {
                    Console.WriteLine("code erreur = " + code + " message = " + (String)retour["message"]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Erreur lors de l'accès à l'API : " + e.Message);
                Environment.Exit(0);
            }
            return liste;
        }

        /// <summary>
        /// Convertit en json un couple nom/valeur
        /// </summary>
        /// <param name="nom"></param>
        /// <param name="valeur"></param>
        /// <returns>couple au format json</returns>
        private String ConvertToJson(Object nom, Object valeur)
        {
            Dictionary<Object, Object> dictionary = new Dictionary<Object, Object>();
            dictionary.Add(nom, valeur);
            return JsonConvert.SerializeObject(dictionary);
        }

        /// <summary>
        /// Modification du convertisseur Json pour gérer le format de date
        /// </summary>
        private sealed class CustomDateTimeConverter : IsoDateTimeConverter
        {
            public CustomDateTimeConverter()
            {
                base.DateTimeFormat = "yyyy-MM-dd";
            }
        }

        /// <summary>
        /// Modification du convertisseur Json pour prendre en compte les booléens
        /// classe trouvée sur le site :
        /// https://www.thecodebuzz.com/newtonsoft-jsonreaderexception-could-not-convert-string-to-boolean/
        /// </summary>
        private sealed class CustomBooleanJsonConverter : JsonConverter<bool>
        {
            public override bool ReadJson(JsonReader reader, Type objectType, bool existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                return Convert.ToBoolean(reader.ValueType == typeof(string) ? Convert.ToByte(reader.Value) : reader.Value);
            }

            public override void WriteJson(JsonWriter writer, bool value, JsonSerializer serializer)
            {
                serializer.Serialize(writer, value);
            }
        }

        // code ajouter

        /// <summary>
        /// Retourne les commandes des livres
        /// </summary>
        /// <param name="idDocument">id du livre concernée</param>
        /// <returns>Liste d'objets Commande</returns>
        public List<Commande> GetCommandesLivre(string idDocument)
        {
            String jsonIdDocument = ConvertToJson("id", idDocument);
            List<Commande> lesCommandes = TraitementRecup<Commande>(GET, "commande/" + jsonIdDocument);
            return lesCommandes;
        }

        /// <summary>
        /// ecriture d'une commande en base de données
        /// </summary>
        /// <param name="commande">commande à insérer</param>
        /// <returns>true si l'insertion a pu se faire (retour != null)</returns>
        public bool CreerCommande(Commande commande)
        {
            // Sérialisation de l'objet commande en JSON
            String jsonCommande = JsonConvert.SerializeObject(commande, new CustomDateTimeConverter());

            try
            {
                // Envoi de la requête POST avec les données de la commande
                List<Commande> liste = TraitementRecup<Commande>(POST, "commande/" + jsonCommande);

                // Vérification et retour du résultat de l'insertion
                if (liste != null)
                {
                    Console.WriteLine("Insertion de la commande réussie.");
                    return true;
                }
                else
                {
                    Console.WriteLine("Échec de l'insertion de la commande.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Affichage des erreurs pour le débogage
                Console.WriteLine("Erreur lors de l'insertion de la commande: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Modification d'une commande en base de données
        /// </summary>
        /// <param name="commande">commande à modifier</param>
        /// <returns>true si l'insertion a pu se faire (retour != null)</returns>
        public bool ModifieCommande(Commande commande)
        {
            // Convertir l'objet commande en JSON
            String jsonCommande = JsonConvert.SerializeObject(commande, new CustomDateTimeConverter());

            try
            {
                // Créer l'URL avec l'ID de la commande
                string url = "commande/" + commande.Id +"/";

                // Afficher l'URL et les données JSON pour le débogage
                Console.WriteLine($"URL envoyée à l'API : {uriApi}{url}{jsonCommande}");

                // Envoyer la requête PUT avec les données de la commande
                List<Commande> liste = TraitementRecup<Commande>(PUT, url + jsonCommande);

                // Vérifier la réponse
                return (liste != null);
            }
            catch (Exception ex)
            {
                // Afficher l'exception pour le débogage
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Retourne toutes les étapes de suivi à partir de la BDD
        /// </summary>
        /// <returns></returns>
        public List<Suivi> GetAllSuivi()
        {
            IEnumerable<Suivi> lesSuivis = TraitementRecup<Suivi>(GET, "suivi");
            return new List<Suivi>(lesSuivis);
        }

        /// <summary>
        /// Supprime une commande dans la BDD
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool SupprimerCommande(string id)
        {
            string jsonId = ConvertToJson("id", id);
            try
            {
                var jsonResponse = api.RecupDistant("DELETE", "commande/" + jsonId);
                if (jsonResponse != null)
                {
                    var code = (int)jsonResponse["code"];
                    var message = (string)jsonResponse["message"];
                    Console.WriteLine($"Code de réponse: {code}, Message: {message}");

                    if (code == 200)
                    {
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"Erreur lors de la suppression de la commande : {message}", "Erreur");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("Erreur de connexion à l'API");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Retourne les commandes de dvd
        /// </summary>
        /// <param name="idDocument">id du dvd concernée</param>
        /// <returns>Liste d'objets Commande</returns>
        public List<Commande> GetCommandesDvd(string idDocument)
        {
            String jsonIdDocument = ConvertToJson("id", idDocument);
            List<Commande> lesCommandesDvd = TraitementRecup<Commande>(GET, "commande/" + jsonIdDocument);
            return lesCommandesDvd;
        }


    }
}
