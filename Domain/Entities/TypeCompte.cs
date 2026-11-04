namespace Domain.Entities;

// Nature du compte - attribut de premier niveau de ApplicationUser (cf. CLAUDE.md, "Point
// d'implementation transverse" : jamais deduit indirectement d'un rattachement ou d'une
// autre donnee).
//
// - Participant : compte qui suit des parcours (Challenges, Cohortes, cartes, preuves).
// - ClientCarbone : compte "entreprise cliente" ouvert par un Gestionnaire a partir d'un
//   Pre-diagnostic carbone. Il ne suit aucun parcours : il remplit le questionnaire de
//   collecte du Bilan Carbone(R), consulte sa check-list et portera plus tard les plans
//   d'action de decarbonation de son entreprise. Volontairement distinct d'un compte
//   Participant, meme pour une meme personne (deux usages, deux espaces).
//
// Participant en premier (valeur 0) : valeur de repli correcte pour tous les comptes
// existants lors du backfill de la migration ajoutant ce champ.
public enum TypeCompte
{
    Participant,
    ClientCarbone,
}
