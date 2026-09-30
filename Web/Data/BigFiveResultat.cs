namespace Web.Data;

// Resultat d'un test psychotechnique Big Five (modele OCEAN : Nevrosisme / Extraversion /
// Ouverture / Agreabilite / Conscienciosite) passe par un utilisateur. Contenu (116 items)
// traduit depuis l'IPIP-NEO-120 (Johnson, J. A. (2014). Measuring thirty facets of the Five
// Factor Model with a 120-item public domain inventory. Journal of Research in Personality,
// 51, 78-89), lui-meme construit a partir de l'International Personality Item Pool
// (ipip.ori.org) - domaine public, utilisation libre commerciale ou non. La facette O6
// (Liberalism) de l'instrument d'origine est volontairement exclue : ses items portent sur
// la preference de vote (gauche/droite), une donnee sensible au sens RGPD (art. 9 - opinion
// politique) qu'on ne souhaite pas collecter nommement en base - cf. BigFiveService. Le
// domaine Ouverture est donc calcule sur 5 facettes (20 items) au lieu de 6 (24 items)
// comme les 4 autres domaines - cf. BigFiveService.ScoreMaxDomaine.
//
// Une ligne par passage : les retests sont autorises, GetDernierResultatAsync renvoie le
// plus recent. Le detail par facette est porte par BigFiveResultatFacette (une ligne par
// facette passee, 29 lignes par resultat).
public class BigFiveResultat
{
    public int Id { get; set; }

    public string UtilisateurId { get; set; } = string.Empty;
    public ApplicationUser Utilisateur { get; set; } = null!;

    // Sur 24-120 (6 facettes x 4 items x 1-5) pour N/E/A/C, sur 20-100 (5 facettes) pour O.
    public int ScoreNevrosisme { get; set; }
    public int ScoreExtraversion { get; set; }
    public int ScoreOuverture { get; set; }
    public int ScoreAgreabilite { get; set; }
    public int ScoreConsciencieusite { get; set; }

    public List<BigFiveResultatFacette> Facettes { get; set; } = [];

    public DateTime CompleteLe { get; set; } = DateTime.UtcNow;
}

// Score d'une facette (ex. "N1" Anxiete) pour un resultat donne - sur 4 a 20 points (4
// items x 1 a 5, items inverses recodes avant sommation, cf. BigFiveService).
public class BigFiveResultatFacette
{
    public int Id { get; set; }

    public int BigFiveResultatId { get; set; }
    public BigFiveResultat BigFiveResultat { get; set; } = null!;

    public string Code { get; set; } = string.Empty; // "N1" a "C6" (hors "O6")
    public int Score { get; set; }
}
