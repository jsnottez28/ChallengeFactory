namespace Domain.Entities;

// Mode d'un compte ou d'un Challenge - cf. CLAUDE.md, "Point d'implementation transverse" :
// attribut de premier niveau, jamais deduit indirectement d'une autre donnee.
public enum ModePlateforme
{
    // BtoB en premier (valeur 0) : c'est la valeur de repli correcte pour les comptes
    // deja existants lors du backfill de la migration ajoutant ce champ sur
    // ApplicationUser (les comptes crees avant ce champ etaient de fait des comptes
    // admin/BtoB, cf. ApplicationUser.Mode).
    BtoB,
    BtoC
}
