using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

// Contenu (116 items) traduit depuis l'IPIP-NEO-120 (Johnson, J. A. (2014). Measuring
// thirty facets of the Five Factor Model with a 120-item public domain inventory:
// Development of the IPIP-NEO-120. Journal of Research in Personality, 51, 78-89),
// lui-meme construit a partir de l'International Personality Item Pool (Goldberg, 1999 ;
// https://ipip.ori.org) - items et grilles de notation dans le domaine public, utilisation
// libre commerciale ou non, sans permission ni redevance. Traduction en francais par
// Challenges Factory.
//
// L'instrument d'origine compte 30 facettes (6 par domaine x 4 items = 120 items). La
// facette O6 "Liberalism" du domaine Ouverture est volontairement exclue ici : ses 4 items
// portent explicitement sur la preference de vote ("Tend to vote for liberal/conservative
// candidates", "Believe that we should be tough on crime") - une donnee sensible au sens
// RGPD (art. 9, opinion politique) qu'on ne souhaite pas collecter nommement en base. Le
// domaine Ouverture est donc calcule sur 5 facettes/20 items (au lieu de 6/24) - seul
// domaine dont l'echelle theorique differe des 4 autres, cf. ScoreMinDomaine/ScoreMaxDomaine.
//
// Format : echelle de Likert en 5 points par affirmation ("Très inexact" = 1 a "Très
// exact" = 5), jamais un format a choix force comme RIASEC - c'est le format de
// l'instrument d'origine. Items positivement gardes (+, note telle quelle) ou
// negativement gardes (-, note recodee en 6 - valeur avant sommation) : cf. Items.
public sealed class BigFiveService(ApplicationDbContext dbContext, IEmailService emailService) : IBigFiveService
{
    private sealed record Item(string FacetteCode, bool EstInverse, string Texte);

    // 116 items, dans l'ordre des facettes N1..N6, E1..E6, O1..O5 (O6 exclue), A1..A6,
    // C1..C6 - numerotation 1 a 116 par position dans le tableau. EstInverse = true signifie
    // que la note brute (1-5) est recodee en (6 - note) avant sommation dans le score de
    // facette (cf. RepondreAsync).
    private static readonly Item[] Items =
    [
        // N1 Anxiété
        new("N1", false, "Je m'inquiète pour un rien"),
        new("N1", false, "Je m'attends toujours au pire"),
        new("N1", false, "J'ai peur de beaucoup de choses"),
        new("N1", false, "Je me sens vite stressé(e)"),
        // N2 Colère
        new("N2", false, "Je m'énerve facilement"),
        new("N2", false, "Je m'irrite facilement"),
        new("N2", false, "Je m'emporte facilement"),
        new("N2", true, "Rien ne m'agace facilement"),
        // N3 Dépression
        new("N3", false, "Je me sens souvent triste"),
        new("N3", false, "Je ne m'aime pas beaucoup"),
        new("N3", false, "J'ai souvent le moral à zéro"),
        new("N3", true, "Je me sens bien dans ma peau"),
        // N4 Timidité sociale
        new("N4", false, "J'ai du mal à aller vers les autres"),
        new("N4", false, "J'évite d'attirer l'attention sur moi"),
        new("N4", false, "Je ne suis à l'aise qu'avec mes amis"),
        new("N4", true, "Les situations sociales difficiles ne me dérangent pas"),
        // N5 Impulsivité
        new("N5", false, "Il m'arrive de me laisser aller à l'excès"),
        new("N5", true, "Je fais rarement des excès"),
        new("N5", true, "Je résiste facilement aux tentations"),
        new("N5", true, "Je sais contrôler mes envies"),
        // N6 Vulnérabilité
        new("N6", false, "Je panique facilement"),
        new("N6", false, "Je me sens vite dépassé(e) par les événements"),
        new("N6", false, "J'ai l'impression de ne pas savoir gérer les choses"),
        new("N6", true, "Je reste calme sous pression"),

        // E1 Convivialité
        new("E1", false, "Je me fais facilement des amis"),
        new("E1", false, "Je me sens à l'aise avec les gens"),
        new("E1", true, "J'évite les contacts avec les autres"),
        new("E1", true, "Je garde mes distances avec les autres"),
        // E2 Grégarité
        new("E2", false, "J'adore les grandes fêtes"),
        new("E2", false, "J'aime parler à beaucoup de monde en soirée"),
        new("E2", true, "Je préfère être seul(e)"),
        new("E2", true, "J'évite la foule"),
        // E3 Assurance
        new("E3", false, "J'aime prendre les choses en main"),
        new("E3", false, "J'aime prendre la tête d'un groupe"),
        new("E3", false, "J'aime avoir le contrôle des choses"),
        new("E3", true, "J'attends que les autres montrent la voie"),
        // E4 Dynamisme
        new("E4", false, "Je suis toujours occupé(e)"),
        new("E4", false, "Je suis toujours en mouvement"),
        new("E4", false, "Je fais beaucoup de choses pendant mon temps libre"),
        new("E4", true, "J'aime prendre les choses tranquillement"),
        // E5 Recherche de sensations
        new("E5", false, "J'adore les sensations fortes"),
        new("E5", false, "Je recherche l'aventure"),
        new("E5", false, "J'aime prendre des risques"),
        new("E5", false, "J'aime agir de façon impulsive et extravagante"),
        // E6 Enjouement
        new("E6", false, "Je rayonne de joie"),
        new("E6", false, "Je m'amuse beaucoup dans la vie"),
        new("E6", false, "J'aime la vie"),
        new("E6", false, "Je vois toujours le bon côté des choses"),

        // O1 Imagination
        new("O1", false, "J'ai une imagination débordante"),
        new("O1", false, "J'aime m'évader dans des fantaisies"),
        new("O1", false, "J'adore rêvasser"),
        new("O1", false, "J'aime me perdre dans mes pensées"),
        // O2 Sens artistique
        new("O2", false, "Je crois en l'importance de l'art"),
        new("O2", false, "Je remarque la beauté là où d'autres ne la voient pas"),
        new("O2", true, "Je n'aime pas la poésie"),
        new("O2", true, "Je n'aime pas visiter les musées d'art"),
        // O3 Émotivité
        new("O3", false, "Je ressens mes émotions intensément"),
        new("O3", false, "Je ressens les émotions des autres"),
        new("O3", true, "Je remarque rarement mes propres réactions émotionnelles"),
        new("O3", true, "Je ne comprends pas les gens qui s'émeuvent facilement"),
        // O4 Goût du changement
        new("O4", false, "Je préfère la variété à la routine"),
        new("O4", true, "Je préfère m'en tenir à ce que je connais"),
        new("O4", true, "Je n'aime pas les changements"),
        new("O4", true, "Je suis attaché(e) aux façons de faire habituelles"),
        // O5 Curiosité intellectuelle
        new("O5", false, "J'aime lire des textes intellectuellement exigeants"),
        new("O5", true, "J'évite les discussions philosophiques"),
        new("O5", true, "J'ai du mal à comprendre les idées abstraites"),
        new("O5", true, "Les discussions théoriques ne m'intéressent pas"),

        // A1 Confiance
        new("A1", false, "Je fais confiance aux autres"),
        new("A1", false, "Je crois que les autres ont de bonnes intentions"),
        new("A1", false, "Je crois ce que les gens me disent"),
        new("A1", true, "Je me méfie des gens"),
        // A2 Droiture
        new("A2", true, "J'utilise les autres pour arriver à mes fins"),
        new("A2", true, "Je triche pour prendre l'avantage"),
        new("A2", true, "Je profite des autres"),
        new("A2", true, "Je mets des bâtons dans les roues des autres"),
        // A3 Altruisme
        new("A3", false, "J'adore aider les autres"),
        new("A3", false, "Je me soucie des autres"),
        new("A3", true, "Je suis indifférent(e) aux sentiments des autres"),
        new("A3", true, "Je ne prends pas le temps de m'occuper des autres"),
        // A4 Coopération
        new("A4", true, "J'aime en découdre"),
        new("A4", true, "Je crie facilement sur les gens"),
        new("A4", true, "J'insulte facilement les gens"),
        new("A4", true, "Je cherche à me venger des autres"),
        // A5 Modestie
        new("A5", true, "Je pense être meilleur(e) que les autres"),
        new("A5", true, "J'ai une très bonne opinion de moi-même"),
        new("A5", true, "Je me fais une haute idée de moi-même"),
        new("A5", true, "Je me vante de mes qualités"),
        // A6 Compassion
        new("A6", false, "Je compatis avec les personnes sans-abri"),
        new("A6", false, "Je ressens de la compassion pour les plus démunis"),
        new("A6", true, "Les problèmes des autres ne m'intéressent pas"),
        new("A6", true, "J'évite de penser aux personnes dans le besoin"),

        // C1 Efficacité personnelle
        new("C1", false, "Je mène mes tâches à bien"),
        new("C1", false, "J'excelle dans ce que je fais"),
        new("C1", false, "Je gère les tâches sans difficulté"),
        new("C1", false, "Je sais comment faire aboutir les choses"),
        // C2 Ordre
        new("C2", false, "J'aime ranger"),
        new("C2", true, "J'oublie souvent de remettre les choses à leur place"),
        new("C2", true, "Je laisse du désordre dans ma chambre"),
        new("C2", true, "Je laisse traîner mes affaires"),
        // C3 Application
        new("C3", false, "Je tiens mes promesses"),
        new("C3", false, "Je dis la vérité"),
        new("C3", true, "Je transgresse les règles"),
        new("C3", true, "Je ne tiens pas mes promesses"),
        // C4 Esprit de réussite
        new("C4", false, "Je travaille dur"),
        new("C4", false, "J'en fais plus que ce qu'on attend de moi"),
        new("C4", true, "J'en fais juste assez pour m'en sortir"),
        new("C4", true, "Je consacre peu de temps et d'efforts à mon travail"),
        // C5 Autodiscipline
        new("C5", false, "Je suis toujours prêt(e)"),
        new("C5", false, "Je mène mes projets à terme"),
        new("C5", true, "Je perds mon temps"),
        new("C5", true, "J'ai du mal à me mettre à une tâche"),
        // C6 Prudence
        new("C6", true, "Je me lance dans les choses sans réfléchir"),
        new("C6", true, "Je prends des décisions hâtives"),
        new("C6", true, "Je me précipite dans les choses"),
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

    // Noms des 29 facettes (traduits des labels de Johnson, 2014, eux-memes repris de la
    // structure du NEO PI-R de Costa & McCrae) - vocabulaire academique standard, pas un
    // contenu proprietaire.
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

    private const int ScoreMinFacette = 4; // 4 items x 1 point minimum
    private const int ScoreMaxFacette = 20; // 4 items x 5 points maximum

    private static int ScoreMinDomaine(string domaine) => FacettesParDomaine[domaine].Length * ScoreMinFacette;
    private static int ScoreMaxDomaine(string domaine) => FacettesParDomaine[domaine].Length * ScoreMaxFacette;

    public List<BigFiveQuestionInfo> GetQuestions() =>
        Items.Select((item, i) => new BigFiveQuestionInfo { NumeroQuestion = i + 1, Texte = item.Texte }).ToList();

    public async Task<BigFiveResultatInfo?> GetDernierResultatAsync(string utilisateurId)
    {
        var resultat = await dbContext.BigFiveResultats
            .Include(r => r.Facettes)
            .Where(r => r.UtilisateurId == utilisateurId)
            .OrderByDescending(r => r.CompleteLe)
            .FirstOrDefaultAsync();

        return resultat is null ? null : VersInfo(resultat);
    }

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

        var scoresFacettes = Facettes.Keys.ToDictionary(code => code, _ => 0);
        for (var i = 0; i < Items.Length; i++)
        {
            var item = Items[i];
            var note = reponses[i + 1];
            var noteRecodee = item.EstInverse ? 6 - note : note;
            scoresFacettes[item.FacetteCode] += noteRecodee;
        }

        var resultat = new BigFiveResultat
        {
            UtilisateurId = utilisateurId,
            ScoreNevrosisme = FacettesParDomaine["N"].Sum(f => scoresFacettes[f]),
            ScoreExtraversion = FacettesParDomaine["E"].Sum(f => scoresFacettes[f]),
            ScoreOuverture = FacettesParDomaine["O"].Sum(f => scoresFacettes[f]),
            ScoreAgreabilite = FacettesParDomaine["A"].Sum(f => scoresFacettes[f]),
            ScoreConsciencieusite = FacettesParDomaine["C"].Sum(f => scoresFacettes[f]),
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

    // Niveau qualitatif base sur la position du score dans l'intervalle THEORIQUE
    // [ScoreMin, ScoreMax] de l'instrument (tiers egaux), jamais une norme statistique
    // construite sur un echantillon de reference qu'on ne possede pas.
    private static string CalculerNiveau(int score, int min, int max)
    {
        var position = (double)(score - min) / (max - min);
        return position switch
        {
            < 1.0 / 3 => "Faible",
            < 2.0 / 3 => "Modéré",
            _ => "Élevé",
        };
    }

    private static string DescriptionSelonNiveau(ProfilDomaine profil, string niveau) => niveau switch
    {
        "Élevé" => profil.DescriptionHaute,
        "Faible" => profil.DescriptionBasse,
        _ => profil.DescriptionModeree,
    };

    // Construit la synthese interpretative a partir des 5 scores de domaine - dimensionnelle
    // (jamais un "type" ferme), fondee uniquement sur les scores reels de ce resultat.
    private static BigFiveSyntheseInfo ConstruireSynthese(Dictionary<string, (int Score, string Niveau)> domaines)
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
        var scoresDomaines = new Dictionary<string, int>
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
            CompleteLe = resultat.CompleteLe,
        };
    }
}
