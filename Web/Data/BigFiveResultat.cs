namespace Web.Data;

// Resultat d'un test psychotechnique Big Five (modele OCEAN : Nevrosisme / Extraversion /
// Ouverture / Agreabilite / Conscienciosite) passe par un utilisateur. Contenu (58 items)
// traduit depuis l'IPIP-NEO-60 (Maples-Keller, J. L., Williamson, R. L., Sleep, C. E.,
// Carter, N. T., Campbell, W. K., & Miller, J. D. (2017). Using Item Response Theory to
// Develop a 60-Item Representation of the NEO PI-R Using the International Personality Item
// Pool. Assessment), lui-meme construit a partir de l'International Personality Item Pool
// (ipip.ori.org) - domaine public, utilisation libre commerciale ou non. La facette O6
// (Liberalism) de l'instrument d'origine est volontairement exclue : ses items portent sur
// la preference de vote ET la croyance religieuse ("une seule vraie religion"), deux donnees
// sensibles au sens RGPD (art. 9) qu'on ne souhaite pas collecter nommement en base - cf.
// BigFiveService. Le domaine Ouverture est donc calcule sur 5 facettes (10 items) au lieu de
// 6 (12 items) comme les 4 autres domaines - cf. BigFiveService.ScoreMaxDomaine.
//
// Scores corriges du biais d'acquiescence (tendance a repondre de facon uniforme, sans lien
// avec le contenu de l'item) selon le principe des scores ipsatises decrit en Appendix A de
// Soto, John, Gosling & Potter (2008), cite et applique dans l'article source de l'IPIP-NEO -
// cf. BigFiveService.IndiceAcquiescement et CalculerValeurCorrigee. D'ou le type decimal
// plutot qu'un simple compte d'items.
//
// Une ligne par passage : les retests sont autorises, GetDernierResultatAsync renvoie le
// plus recent. Le detail par facette est porte par BigFiveResultatFacette (une ligne par
// facette passee, 29 lignes par resultat).
public class BigFiveResultat
{
    public int Id { get; set; }

    public string UtilisateurId { get; set; } = string.Empty;
    public ApplicationUser Utilisateur { get; set; } = null!;

    // Sur 12-60 (6 facettes x 2 items, echelle nominale 1-5 par item) pour N/E/A/C, sur
    // 10-50 (5 facettes) pour O - apres correction du biais d'acquiescement, une valeur peut
    // legerement deborder de cet intervalle nominal (cf. BigFiveService).
    public decimal ScoreNevrosisme { get; set; }
    public decimal ScoreExtraversion { get; set; }
    public decimal ScoreOuverture { get; set; }
    public decimal ScoreAgreabilite { get; set; }
    public decimal ScoreConsciencieusite { get; set; }

    // Moyenne des 58 reponses brutes (1 a 5) avant toute correction - proche de 3 pour une
    // personne sans tendance particuliere a l'acquiescement ; utilisee pour recentrer chaque
    // item avant sommation (cf. BigFiveService.CalculerValeurCorrigee). Conservee en base
    // pour tracabilite/transparence, jamais recalculee a l'affichage.
    public decimal IndiceAcquiescement { get; set; }

    public List<BigFiveResultatFacette> Facettes { get; set; } = [];

    public DateTime CompleteLe { get; set; } = DateTime.UtcNow;
}

// Score d'une facette (ex. "N1" Anxiete) pour un resultat donne - sur 2 a 10 points
// nominalement (2 items, echelle 1-5, items inverses recodes puis corriges du biais
// d'acquiescence avant sommation, cf. BigFiveService).
public class BigFiveResultatFacette
{
    public int Id { get; set; }

    public int BigFiveResultatId { get; set; }
    public BigFiveResultat BigFiveResultat { get; set; } = null!;

    public string Code { get; set; } = string.Empty; // "N1" a "C6" (hors "O6")
    public decimal Score { get; set; }
}
