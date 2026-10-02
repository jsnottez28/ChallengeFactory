namespace Web.Data;

// Resultat d'un test des ancres de carriere (Schein, 1990 / Cerdin, 2007) passe par un
// utilisateur - cf. ScheinService et IScheinService pour le detail de l'instrument et de la
// question de licence encore ouverte sur la source du questionnaire.
//
// Chaque score est sur 5-25 nominalement (5 items x 1-5 points par ancre), augmente de 4
// points par choix prioritaire de l'etape 2 tombant sur cette ancre (jusqu'a 3 choix au
// total, repartis sur 1 a 3 ancres differentes) - contrairement a BigFive, ce n'est pas un
// signal de fiabilite annexe : le choix prioritaire modifie reellement le score final, d'ou
// le type int simple (pas de correction statistique comme l'Indice d'Acquiescement de
// BigFive, cf. ScheinService).
//
// Une ligne par passage : les retests sont autorises, GetDernierResultatAsync renvoie le
// plus recent. Pas de table de detail par facette (contrairement a BigFive) : les 9 ancres
// sont le niveau le plus fin de l'instrument, pas de sous-dimensions.
public class ScheinResultat
{
    public int Id { get; set; }

    public string UtilisateurId { get; set; } = string.Empty;
    public ApplicationUser Utilisateur { get; set; } = null!;

    public int ScoreTechnique { get; set; }
    public int ScoreManageriale { get; set; }
    public int ScoreAutonomie { get; set; }
    public int ScoreSecurite { get; set; }
    public int ScoreCreativite { get; set; }
    public int ScoreCause { get; set; }
    public int ScoreDefiPur { get; set; }
    public int ScoreQualiteDeVie { get; set; }
    public int ScoreInternationale { get; set; }

    public DateTime CompleteLe { get; set; } = DateTime.UtcNow;
}
