using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

// Contenu (58 items) traduit depuis l'IPIP-NEO-60 (Maples-Keller, J. L., Williamson, R. L.,
// Sleep, C. E., Carter, N. T., Campbell, W. K., & Miller, J. D. (2017). Using Item Response
// Theory to Develop a 60-Item Representation of the NEO PI-R Using the International
// Personality Item Pool. Assessment, 26(7)), une version courte (2 items/facette au lieu de
// 4) validee independamment de l'IPIP-NEO-120, construite a partir du meme International
// Personality Item Pool (Goldberg, 1999 ; https://ipip.ori.org) - items et grilles de
// notation dans le domaine public, utilisation libre commerciale ou non, sans permission ni
// redevance. Traduction en francais par Challenges Factory.
//
// L'instrument d'origine compte 30 facettes (6 par domaine x 2 items = 60 items). La
// facette O6 "Liberalism" du domaine Ouverture est volontairement exclue ici : ses 2 items
// portent sur la preference de vote ("Tend to vote for liberal political candidates") ET la
// croyance religieuse ("Believe in one true religion") - deux donnees sensibles au sens RGPD
// (art. 9, opinion politique et conviction religieuse) qu'on ne souhaite pas collecter
// nommement en base. Le domaine Ouverture est donc calcule sur 5 facettes/10 items (au lieu
// de 6/12) - seul domaine dont l'echelle theorique differe des 4 autres, cf.
// ScoreMinDomaine/ScoreMaxDomaine.
//
// Format : echelle de Likert en 5 points par affirmation ("Très inexact" = 1 a "Très
// exact" = 5), jamais un format a choix force comme RIASEC - c'est le format de
// l'instrument d'origine.
//
// Scoring corrige du biais d'acquiescence (tendance a repondre de facon homogene,
// independamment du contenu de l'item - ex. cocher systematiquement "5") selon le principe
// des scores ipsatises decrit en Appendix A de Soto, John, Gosling & Potter (2008), cite et
// applique dans l'article source de l'IPIP-NEO-120 (Johnson, 2014) pour tester la robustesse
// du scoring face a ce biais. Principe : chaque reponse brute est recentree sur la moyenne
// des 58 reponses de la personne (l'Indice d'Acquiescement) avant d'etre recodee et sommee -
// cf. CalculerValeurCorrigee. Avec un indice de 3 (aucune tendance particuliere), la formule
// se reduit exactement au recodage classique (6 - note pour un item inverse) : la correction
// ne modifie le profil QUE si la personne a effectivement une tendance a l'acquiescement.
public sealed class BigFiveService(ApplicationDbContext dbContext, IEmailService emailService) : IBigFiveService
{
    private sealed record Item(string FacetteCode, bool EstInverse, string Texte);

    // 58 items, dans l'ordre des facettes N1..N6, E1..E6, O1..O5 (O6 exclue), A1..A6,
    // C1..C6 - numerotation 1 a 58 par position dans le tableau. EstInverse = true signifie
    // que l'item est negativement garde (cf. CalculerValeurCorrigee pour le recodage).
    private static readonly Item[] Items =
    [
        // N1 Anxiété
        new("N1", false, "Je m'inquiète pour un rien"),
        new("N1", false, "Je me sens vite stressé(e)"),
        // N2 Colère
        new("N2", false, "Je m'énerve facilement"),
        new("N2", false, "Je m'emporte facilement"),
        // N3 Dépression
        new("N3", false, "Je me sens souvent triste"),
        new("N3", false, "Je ne m'aime pas beaucoup"),
        // N4 Timidité sociale
        new("N4", false, "J'ai du mal à aller vers les autres"),
        new("N4", false, "Je suis facilement intimidé(e)"),
        // N5 Impulsivité
        new("N5", true, "Je fais rarement des excès"),
        new("N5", true, "Je sais contrôler mes envies"),
        // N6 Vulnérabilité
        new("N6", true, "Je reste calme sous pression"),
        new("N6", true, "Je suis calme même dans les situations tendues"),

        // E1 Convivialité
        new("E1", false, "Je me fais facilement des amis"),
        new("E1", false, "Je me sens à l'aise avec les gens"),
        // E2 Grégarité
        new("E2", false, "J'adore les grandes fêtes"),
        new("E2", true, "J'évite la foule"),
        // E3 Assurance
        new("E3", false, "J'aime prendre les choses en main"),
        new("E3", false, "J'aime prendre la tête d'un groupe"),
        // E4 Dynamisme
        new("E4", false, "Je suis toujours occupé(e)"),
        new("E4", false, "Je suis toujours en mouvement"),
        // E5 Recherche de sensations
        new("E5", false, "J'adore les sensations fortes"),
        new("E5", false, "Je recherche l'aventure"),
        // E6 Enjouement
        new("E6", false, "Je m'amuse beaucoup dans la vie"),
        new("E6", false, "J'aime la vie"),

        // O1 Imagination
        new("O1", false, "J'ai une imagination débordante"),
        new("O1", false, "J'adore rêvasser"),
        // O2 Sens artistique
        new("O2", false, "Je crois en l'importance de l'art"),
        new("O2", true, "Je n'aime pas l'art"),
        // O3 Émotivité
        new("O3", false, "Je ressens mes émotions intensément"),
        new("O3", true, "Je ne suis pas facilement affecté(e) par mes émotions"),
        // O4 Goût du changement
        new("O4", true, "Je préfère m'en tenir à ce que je connais"),
        new("O4", true, "Je n'aime pas l'idée du changement"),
        // O5 Curiosité intellectuelle
        new("O5", true, "J'évite les discussions philosophiques"),
        new("O5", true, "Les discussions théoriques ne m'intéressent pas"),

        // A1 Confiance
        new("A1", false, "Je fais confiance aux autres"),
        new("A1", false, "Je crois que les autres ont de bonnes intentions"),
        // A2 Droiture
        new("A2", true, "Je triche pour prendre l'avantage"),
        new("A2", true, "Je profite des autres"),
        // A3 Altruisme
        new("A3", false, "J'adore aider les autres"),
        new("A3", false, "Je me soucie des autres"),
        // A4 Coopération
        new("A4", true, "J'insulte facilement les gens"),
        new("A4", true, "Je cherche à me venger des autres"),
        // A5 Modestie
        new("A5", true, "Je pense être meilleur(e) que les autres"),
        new("A5", true, "J'ai une très bonne opinion de moi-même"),
        // A6 Compassion
        new("A6", false, "Je compatis avec les personnes sans-abri"),
        new("A6", false, "Je ressens de la compassion pour les plus démunis"),

        // C1 Efficacité personnelle
        new("C1", false, "Je gère les tâches sans difficulté"),
        new("C1", false, "Je sais comment faire aboutir les choses"),
        // C2 Ordre
        new("C2", false, "J'aime ranger"),
        new("C2", true, "Je laisse du désordre dans ma chambre"),
        // C3 Application
        new("C3", false, "Je dis la vérité"),
        new("C3", true, "Je ne tiens pas mes promesses"),
        // C4 Esprit de réussite
        new("C4", false, "Je travaille dur"),
        new("C4", false, "J'ai des exigences élevées, pour moi comme pour les autres"),
        // C5 Autodiscipline
        new("C5", false, "Je mène mes projets à terme"),
        new("C5", true, "J'ai du mal à me mettre à une tâche"),
        // C6 Prudence
        new("C6", true, "Je prends des décisions hâtives"),
        new("C6", true, "J'agis sans réfléchir"),
    ];

    private static readonly string[] OrdreDomaines = ["N", "E", "O", "A", "C"];

    // Nom + description par niveau (Faible/Modéré/Élevé, cf. Niveau) de chaque domaine -
    // vulgarisation standard du modele OCEAN telle qu'enseignee couramment en psychologie
    // de la personnalite, theorie publique (pas un contenu proprietaire de l'IPIP). Pour le
    // Nevrosisme, un score eleve signale une plus grande reactivite au stress - ni "bon" ni
    // "mauvais" en soi, mais le seul domaine dont la lecture "vigilance" va dans ce sens
    // (cf. ConstruireSynthese).
    private sealed record ProfilDomaine(string Nom, string DescriptionHaute, string DescriptionModeree, string DescriptionBasse);

    private static readonly Dictionary<string, ProfilDomaine> Domaines = new()
    {
        ["N"] = new ProfilDomaine(
            "Névrosisme",
            "Vous ressentez les émotions négatives (inquiétude, tristesse, irritation) de façon plus intense et plus fréquente que la moyenne. Vous êtes sans doute plus sensible au stress et aux tensions, ce qui peut être un moteur de vigilance mais aussi une source de fatigue si l'environnement est trop exigeant.",
            "Vous gérez le stress et les émotions négatives ni mieux ni moins bien que la moyenne : certaines situations vous affectent, d'autres vous laissent de marbre, selon le contexte.",
            "Vous restez généralement calme et stable émotionnellement, même sous pression. Les contrariétés vous affectent peu durablement, ce qui peut être un vrai atout dans les situations de tension — à condition de ne pas sous-estimer un stress réel chez les autres."),
        ["E"] = new ProfilDomaine(
            "Extraversion",
            "Vous puisez votre énergie dans l'interaction avec les autres : sociable, dynamique, à l'aise pour prendre la parole ou l'initiative en groupe. Les environnements stimulants et les échanges fréquents vous conviennent bien.",
            "Vous appréciez autant les moments sociaux que les moments plus calmes et solitaires, selon le contexte — ni franchement extraverti(e), ni franchement introverti(e).",
            "Vous puisez votre énergie dans des environnements plus calmes et recherchez moins la stimulation sociale intense. Cela ne signifie pas un manque de compétences relationnelles, mais une préférence pour des interactions plus ciblées et moins nombreuses."),
        ["O"] = new ProfilDomaine(
            "Ouverture",
            "Vous êtes curieux(se), attiré(e) par les idées nouvelles, l'art, l'imagination et les expériences qui sortent de l'ordinaire. Vous êtes à l'aise avec l'abstraction et le changement.",
            "Vous appréciez la nouveauté sans pour autant rechercher systématiquement à sortir des sentiers battus — un équilibre entre goût de l'exploration et attachement à ce qui est éprouvé.",
            "Vous préférez le concret, les méthodes éprouvées et les repères stables aux idées abstraites ou aux changements fréquents. Cela traduit un ancrage dans le réel et le pragmatisme plutôt qu'un manque de curiosité."),
        ["A"] = new ProfilDomaine(
            "Agréabilité",
            "Vous accordez une grande importance à la coopération, à la confiance et au bien-être d'autrui. Vous êtes naturellement enclin(e) à faire confiance et à privilégier l'harmonie dans vos relations.",
            "Vous savez aussi bien coopérer que défendre votre point de vue, selon les situations — ni systématiquement conciliant(e), ni systématiquement compétitif(ve).",
            "Vous êtes plus à l'aise pour défendre vos intérêts et votre point de vue, même au prix d'un désaccord, plutôt que de rechercher le consensus à tout prix. Cela peut être un atout en négociation, à condition de rester attentif(ve) à l'impact sur les autres."),
        ["C"] = new ProfilDomaine(
            "Conscienciosité",
            "Vous êtes organisé(e), discipliné(e) et orienté(e) vers l'atteinte de vos objectifs. Vous respectez vos engagements et menez vos projets à terme avec méthode.",
            "Votre niveau d'organisation et de discipline varie selon les contextes et les enjeux — ni rigoureux(se) à l'excès, ni désordonné(e).",
            "Vous êtes plus spontané(e) et flexible que méthodique, avec une organisation qui s'adapte au moment plutôt qu'à une planification stricte. Cela peut être un atout d'adaptabilité, au prix parfois d'une moindre régularité."),
    };

    private sealed record ProfilFacette(string Nom, string DomaineCode);

    // Noms des 29 facettes (traduits des labels de Maples-Keller et al., 2017, eux-memes
    // repris de la structure du NEO PI-R de Costa & McCrae) - vocabulaire academique
    // standard, pas un contenu proprietaire.
    private static readonly Dictionary<string, ProfilFacette> Facettes = new()
    {
        ["N1"] = new("Anxiété", "N"),
        ["N2"] = new("Colère", "N"),
        ["N3"] = new("Dépression", "N"),
        ["N4"] = new("Timidité sociale", "N"),
        ["N5"] = new("Impulsivité", "N"),
        ["N6"] = new("Vulnérabilité", "N"),
        ["E1"] = new("Convivialité", "E"),
        ["E2"] = new("Grégarité", "E"),
        ["E3"] = new("Assurance", "E"),
        ["E4"] = new("Dynamisme", "E"),
        ["E5"] = new("Recherche de sensations", "E"),
        ["E6"] = new("Enjouement", "E"),
        ["O1"] = new("Imagination", "O"),
        ["O2"] = new("Sens artistique", "O"),
        ["O3"] = new("Émotivité", "O"),
        ["O4"] = new("Goût du changement", "O"),
        ["O5"] = new("Curiosité intellectuelle", "O"),
        ["A1"] = new("Confiance", "A"),
        ["A2"] = new("Droiture", "A"),
        ["A3"] = new("Altruisme", "A"),
        ["A4"] = new("Coopération", "A"),
        ["A5"] = new("Modestie", "A"),
        ["A6"] = new("Compassion", "A"),
        ["C1"] = new("Efficacité personnelle", "C"),
        ["C2"] = new("Ordre", "C"),
        ["C3"] = new("Application", "C"),
        ["C4"] = new("Esprit de réussite", "C"),
        ["C5"] = new("Autodiscipline", "C"),
        ["C6"] = new("Prudence", "C"),
    };

    // Ordre d'affichage des facettes par domaine (O n'en compte que 5, O6 exclue).
    private static readonly Dictionary<string, string[]> FacettesParDomaine = new()
    {
        ["N"] = ["N1", "N2", "N3", "N4", "N5", "N6"],
        ["E"] = ["E1", "E2", "E3", "E4", "E5", "E6"],
        ["O"] = ["O1", "O2", "O3", "O4", "O5"],
        ["A"] = ["A1", "A2", "A3", "A4", "A5", "A6"],
        ["C"] = ["C1", "C2", "C3", "C4", "C5", "C6"],
    };

    private const int ScoreMinFacette = 2; // 2 items x 1 point minimum (echelle nominale)
    private const int ScoreMaxFacette = 10; // 2 items x 5 points maximum (echelle nominale)

    private static int ScoreMinDomaine(string domaine) => FacettesParDomaine[domaine].Length * ScoreMinFacette;
    private static int ScoreMaxDomaine(string domaine) => FacettesParDomaine[domaine].Length * ScoreMaxFacette;

    public List<BigFiveQuestionInfo> GetQuestions() =>
        Items.Select((item, i) => new BigFiveQuestionInfo { NumeroQuestion = i + 1, Texte = item.Texte }).ToList();

    // Score corrige du biais d'acquiescence (Soto, John, Gosling & Potter, 2008, Appendix A) :
    // on recentre la reponse brute sur l'Indice d'Acquiescement (IA, moyenne des 58 reponses
    // de la personne) avant de la recoder, puis on recentre le resultat sur le milieu
    // theorique de l'echelle (3) pour rester directement comparable au recodage classique.
    // Avec IA = 3 (aucune tendance a l'acquiescement), la formule se reduit exactement a
    // "note" pour un item normal et "6 - note" pour un item inverse - le recodage classique
    // d'origine. La correction n'a d'effet que si la personne a reellement une tendance a
    // repondre de facon homogene, independamment du contenu de chaque item.
    private static decimal CalculerValeurCorrigee(int note, bool estInverse, decimal indiceAcquiescement) =>
        estInverse ? 3 - (note - indiceAcquiescement) : 3 + (note - indiceAcquiescement);

    public async Task<(bool Success, string? ErrorMessage, BigFiveResultatInfo? Resultat)> RepondreAsync(string utilisateurId, Dictionary<int, int> reponses)
    {
        var utilisateur = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == utilisateurId);
        if (utilisateur is null)
        {
            return (false, "Utilisateur introuvable.", null);
        }

        for (var numero = 1; numero <= Items.Length; numero++)
        {
            if (!reponses.TryGetValue(numero, out var note) || note is < 1 or > 5)
            {
                return (false, "Merci de répondre à toutes les affirmations du test.", null);
            }
        }

        var indiceAcquiescement = (decimal)Enumerable.Range(1, Items.Length).Average(numero => reponses[numero]);

        var scoresFacettes = Facettes.Keys.ToDictionary(code => code, _ => 0m);
        for (var i = 0; i < Items.Length; i++)
        {
            var item = Items[i];
            var note = reponses[i + 1];
            scoresFacettes[item.FacetteCode] += CalculerValeurCorrigee(note, item.EstInverse, indiceAcquiescement);
        }

        var resultat = new BigFiveResultat
        {
            UtilisateurId = utilisateurId,
            ScoreNevrosisme = FacettesParDomaine["N"].Sum(f => scoresFacettes[f]),
            ScoreExtraversion = FacettesParDomaine["E"].Sum(f => scoresFacettes[f]),
            ScoreOuverture = FacettesParDomaine["O"].Sum(f => scoresFacettes[f]),
            ScoreAgreabilite = FacettesParDomaine["A"].Sum(f => scoresFacettes[f]),
            ScoreConsciencieusite = FacettesParDomaine["C"].Sum(f => scoresFacettes[f]),
            IndiceAcquiescement = indiceAcquiescement,
            Facettes = [.. scoresFacettes.Select(kv => new BigFiveResultatFacette { Code = kv.Key, Score = kv.Value })],
            CompleteLe = DateTime.UtcNow,
        };
        dbContext.BigFiveResultats.Add(resultat);
        await dbContext.SaveChangesAsync();

        var info = VersInfo(resultat);

        if (!string.IsNullOrWhiteSpace(utilisateur.Email))
        {
            var (sujet, corps) = ChallengeEmailTemplates.ResultatBigFive(info);
            await emailService.EnvoyerAsync(utilisateur.Email, sujet, corps);
        }

        return (true, null, info);
    }

    public async Task<BigFiveResultatInfo?> GetDernierResultatAsync(string utilisateurId)
    {
        var resultat = await dbContext.BigFiveResultats
            .Include(r => r.Facettes)
            .Where(r => r.UtilisateurId == utilisateurId)
            .OrderByDescending(r => r.CompleteLe)
            .FirstOrDefaultAsync();

        return resultat is null ? null : VersInfo(resultat);
    }

    // Niveau qualitatif base sur la position du score dans l'intervalle THEORIQUE nominal
    // [ScoreMin, ScoreMax] de l'instrument (tiers egaux), jamais une norme statistique
    // construite sur un echantillon de reference qu'on ne possede pas. Apres correction du
    // biais d'acquiescence, la position peut legerement deborder de [0, 100] % - non
    // tronquee, pour ne pas masquer un profil atypique.
    private static string CalculerNiveau(decimal score, int min, int max)
    {
        var position = (double)(score - min) / (max - min);
        return position switch
        {
            < 1.0 / 3 => "Faible",
            < 2.0 / 3 => "Modéré",
            _ => "Élevé",
        };
    }

    // Commentaire sur l'Indice d'Acquiescement (IA, moyenne des 58 reponses brutes avant
    // correction) - toujours derive de la vraie valeur, jamais un texte generique invariant.
    // 3 = milieu theorique de l'echelle (aucune tendance particuliere).
    private static string CommenterIndiceAcquiescement(decimal indiceAcquiescement)
    {
        var ecart = Math.Abs(indiceAcquiescement - 3);
        if (ecart < 0.5m)
        {
            return "Vos réponses ne montrent pas de tendance particulière à répondre de façon homogène : les scores ci-dessous reflètent directement votre profil.";
        }
        if (ecart < 1m)
        {
            return "Vos réponses montrent une légère tendance à répondre de façon homogène aux affirmations, indépendamment de leur contenu : les scores ci-dessous ont été corrigés en conséquence.";
        }
        return "Vos réponses montrent une tendance marquée à répondre de façon homogène aux affirmations, indépendamment de leur contenu : les scores ci-dessous ont été fortement corrigés pour neutraliser ce biais — à interpréter avec un peu plus de prudence.";
    }

    private static string DescriptionSelonNiveau(ProfilDomaine profil, string niveau) => niveau switch
    {
        "Élevé" => profil.DescriptionHaute,
        "Faible" => profil.DescriptionBasse,
        _ => profil.DescriptionModeree,
    };

    // Construit la synthese interpretative a partir des 5 scores de domaine - dimensionnelle
    // (jamais un "type" ferme), fondee uniquement sur les scores reels de ce resultat.
    private static BigFiveSyntheseInfo ConstruireSynthese(Dictionary<string, (decimal Score, string Niveau)> domaines)
    {
        var pointsForts = new List<string>();
        foreach (var code in new[] { "E", "O", "A", "C" })
        {
            if (domaines[code].Niveau == "Élevé")
            {
                pointsForts.Add($"{Domaines[code].Nom} : {Domaines[code].DescriptionHaute}");
            }
        }
        if (domaines["N"].Niveau == "Faible")
        {
            pointsForts.Add($"Stabilité émotionnelle : {Domaines["N"].DescriptionBasse}");
        }

        var pointsVigilance = new List<string>();
        if (domaines["N"].Niveau == "Élevé")
        {
            pointsVigilance.Add($"Névrosisme élevé : {Domaines["N"].DescriptionHaute}");
        }

        var dominant = new[] { "E", "O", "A", "C" }.OrderByDescending(c => (double)domaines[c].Score / ScoreMaxDomaine(c)).First();
        var niveauDominant = domaines[dominant].Niveau.ToLowerInvariant();
        var niveauNevrosisme = domaines["N"].Niveau.ToLowerInvariant();
        var resume = $"Votre trait le plus marqué est {Domaines[dominant].Nom} ({niveauDominant}). " +
            $"Côté gestion du stress et des émotions négatives (Névrosisme), vous vous situez à un niveau {niveauNevrosisme}. " +
            "Le modèle Big Five décrit une position sur 5 échelles continues plutôt qu'un type figé : utilisez ce profil comme point de repère, pas comme une case définitive.";

        return new BigFiveSyntheseInfo { PointsForts = pointsForts, PointsVigilance = pointsVigilance, Resume = resume };
    }

    private static BigFiveResultatInfo VersInfo(BigFiveResultat resultat)
    {
        var scoresDomaines = new Dictionary<string, decimal>
        {
            ["N"] = resultat.ScoreNevrosisme,
            ["E"] = resultat.ScoreExtraversion,
            ["O"] = resultat.ScoreOuverture,
            ["A"] = resultat.ScoreAgreabilite,
            ["C"] = resultat.ScoreConsciencieusite,
        };
        var scoresFacettes = resultat.Facettes.ToDictionary(f => f.Code, f => f.Score);

        var niveauxDomaines = OrdreDomaines.ToDictionary(
            d => d,
            d => (Score: scoresDomaines[d], Niveau: CalculerNiveau(scoresDomaines[d], ScoreMinDomaine(d), ScoreMaxDomaine(d))));

        var domaines = OrdreDomaines.Select(d =>
        {
            var (score, niveau) = niveauxDomaines[d];
            var min = ScoreMinDomaine(d);
            var max = ScoreMaxDomaine(d);
            return new BigFiveDomaineInfo
            {
                Code = d,
                Nom = Domaines[d].Nom,
                Score = score,
                ScoreMin = min,
                ScoreMax = max,
                PourcentagePosition = (int)Math.Round((double)(score - min) / (max - min) * 100),
                Niveau = niveau,
                Description = DescriptionSelonNiveau(Domaines[d], niveau),
                Facettes = [.. FacettesParDomaine[d].Select(code => new BigFiveFacetteInfo
                {
                    Code = code,
                    Nom = Facettes[code].Nom,
                    DomaineCode = d,
                    Score = scoresFacettes[code],
                })],
            };
        }).ToList();

        return new BigFiveResultatInfo
        {
            Domaines = domaines,
            Synthese = ConstruireSynthese(niveauxDomaines),
            IndiceAcquiescement = resultat.IndiceAcquiescement,
            IndiceAcquiescementCommentaire = CommenterIndiceAcquiescement(resultat.IndiceAcquiescement),
            CompleteLe = resultat.CompleteLe,
        };
    }
}
