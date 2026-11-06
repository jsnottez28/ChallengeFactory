namespace Domain.Entities;

// Nature du compte - attribut de premier niveau de ApplicationUser (cf. CLAUDE.md, "Point
// d'implementation transverse" : jamais deduit indirectement d'un rattachement ou d'une
// autre donnee).
//
// - Participant : compte qui suit des parcours (Challenges, Cohortes, cartes, preuves).
// - ClientCarbone : compte "entreprise cliente" ouvert (par un Gestionnaire ou en
//   self-service) a partir d'un Pre-diagnostic carbone - remplit le questionnaire de
//   collecte du Bilan Carbone(R), consulte sa check-list et portera plus tard les plans
//   d'action de decarbonation de son entreprise.
//
// Ce champ ne reflete que la nature du compte A SA CREATION (quel parcours l'a fait
// naitre), pas un cloisonnement des acces : un Participant peut tres bien recevoir en plus
// un Espace Client Carbone (les acces se cumulent) sans que son TypeCompte change. La
// seule source de verite pour "ce compte a acces a l'Espace Client Carbone" est la
// presence d'un Web.Data.DossierCarbone qui lui est rattache (cf. ICollecteCarboneService),
// jamais ce champ.
//
// Participant en premier (valeur 0) : valeur de repli correcte pour tous les comptes
// existants lors du backfill de la migration ajoutant ce champ.
public enum TypeCompte
{
    Participant,
    ClientCarbone,
}
