using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public sealed class ScheinService(ApplicationDbContext dbContext, IEmailService emailService) : IScheinService
{
    private sealed record Item(string AncreCode, string Texte);

    // 45 items, dans l'ordre et le libelle exacts des deux sources transmises (identiques sur
    // les deux). Chaque colonne de la grille de notation d'origine correspond a une ancre :
    // l'item n est affecte a l'ancre OrdreAncres[(n-1) % 9], soit les items 1,10,19,28,37 pour
    // la 1re ancre (TECH), 2,11,20,29,38 pour la 2e (MG), etc. - cf. OrdreAncres ci-dessous.
    private static readonly Item[] Items =
    [
        new("TECH", "Mon rêve est d'être tellement bon(ne) dans ce que je fais que mes conseils d'expert seront recherchés en permanence."),
        new("MG", "Je suis pleinement satisfait(e) dans mon travail quand j'ai réussi à intégrer et à gérer les efforts des autres."),
        new("AUT", "Je rêve d'avoir une carrière qui me donne la liberté de faire mon travail à ma façon et selon mon propre programme."),
        new("SEC", "J'attache plus d'importance à la sécurité et à la stabilité qu'à la liberté et l'autonomie."),
        new("CRE", "Je suis toujours à l'affût d'idées qui me permettraient de démarrer ma propre entreprise…"),
        new("CAU", "J'estimerai avoir réussi ma carrière seulement si j'ai le sentiment de contribuer réellement au bien-être de la société."),
        new("DEF", "Je rêve d'une carrière dans laquelle je puisse résoudre ou venir à bout de situations particulièrement difficiles."),
        new("VIE", "Je préférerais quitter mon entreprise plutôt que d'être placé(e) sur un poste qui compromet ma capacité à poursuivre mes intérêts personnels et familiaux."),
        new("INTER", "Je rêve d'avoir une carrière internationale qui me permette de voyager et de travailler avec des personnes de diverses cultures."),

        new("TECH", "J'estimerai avoir réussi ma carrière seulement si je peux développer mes capacités techniques ou fonctionnelles à un très haut niveau de compétence."),
        new("MG", "Je rêve d'être responsable d'une organisation complexe et de prendre des décisions qui touchent nombre de personnes."),
        new("AUT", "Je suis pleinement satisfait(e) dans mon travail quand je suis complètement libre de définir mes propres tâches, programmes et procédures."),
        new("SEC", "Je préférerais quitter définitivement mon entreprise plutôt que d'accepter une mission qui compromettrait ma sécurité dans cette entreprise."),
        new("CRE", "Monter ma propre affaire est plus important pour moi que d'atteindre un haut niveau de management dans l'organisation d'autrui."),
        new("CAU", "Je suis pleinement satisfait(e) dans ma carrière lorsque je peux mettre mes talents au service des autres."),
        new("DEF", "J'ai le sentiment de réussir dans ma carrière seulement si je peux faire face et surmonter des défis particulièrement retors."),
        new("VIE", "Je rêve d'une carrière qui me permette d'intégrer mes besoins personnels, familiaux et professionnels."),
        new("INTER", "Travailler à l'étranger m'attire."),

        new("TECH", "Devenir directeur de la fonction correspondant à mon domaine d'expertise m'attire plus que d'atteindre un poste de direction générale."),
        new("MG", "J'estimerai avoir réussi dans ma carrière seulement si je deviens directeur général d'une organisation."),
        new("AUT", "J'estimerai avoir réussi dans ma carrière seulement si j'atteins une autonomie et une liberté totale."),
        new("SEC", "Je recherche des emplois dans des organisations qui me procureront un sentiment de sécurité et de stabilité."),
        new("CRE", "Je suis pleinement satisfait(e) dans ma carrière quand j'ai pu construire quelque chose qui est entièrement le fruit de mes idées et efforts."),
        new("CAU", "Utiliser mes compétences pour que le monde devienne un endroit plus agréable pour vivre et travailler est plus important pour moi que d'atteindre une position managériale élevée."),
        new("DEF", "J'ai été pleinement satisfait(e) dans ma carrière quand j'ai résolu des problèmes apparemment insolubles ou quand je suis venu(e) à bout de situations apparemment impossibles."),
        new("VIE", "J'estimerai avoir réussi dans la vie seulement si j'ai pu trouver un équilibre entre mes besoins personnels, ceux liés à ma famille et ma carrière."),
        new("INTER", "J'estimerai avoir réussi ma carrière seulement si je parviens à travailler dans un environnement international."),

        new("TECH", "Je préférerais quitter mon entreprise plutôt que d'accepter une mission qui me ferait sortir de mon champ d'expertise."),
        new("MG", "Atteindre un poste de direction générale m'attire plus que de devenir directeur de la fonction correspondant à mon domaine d'expertise."),
        new("AUT", "L'opportunité de faire mon travail à ma façon, libre de règles et de contraintes, est plus importante pour moi que la sécurité."),
        new("SEC", "Je suis pleinement satisfait(e) dans mon travail quand j'éprouve le sentiment d'une sécurité totale sur le plan financier et sur celui de l'emploi."),
        new("CRE", "J'estimerai avoir réussi ma carrière seulement si j'arrive à créer ou à élaborer quelque chose qui est ma propre idée ou mon propre produit."),
        new("CAU", "Je rêve d'avoir une carrière qui apporte une réelle contribution à l'humanité et à la société."),
        new("DEF", "Je recherche des opportunités de travail qui défient fortement mes capacités à résoudre des problèmes et/ou mon goût de la compétition."),
        new("VIE", "Équilibrer les exigences de la vie personnelle et professionnelle est plus important pour moi que d'atteindre une position managériale élevée."),
        new("INTER", "Je préférerais quitter mon entreprise plutôt que d'accepter une mission qui n'impliquerait pas la possibilité d'une mobilité internationale."),

        new("TECH", "Je suis pleinement satisfait(e) de mon travail quand j'ai été capable d'utiliser les compétences et talents rattachés à ma spécialisation."),
        new("MG", "Je préférerais quitter mon entreprise plutôt que d'accepter un travail qui m'empêcherait d'atteindre une position de management général."),
        new("AUT", "Je préférerais quitter mon entreprise plutôt que d'accepter un travail qui réduirait mon autonomie et ma liberté."),
        new("SEC", "Je rêve d'avoir une carrière qui me permette d'éprouver un sentiment de sécurité et de stabilité."),
        new("CRE", "Je rêve de démarrer et de développer ma propre affaire."),
        new("CAU", "Je préférerais quitter mon entreprise plutôt que d'accepter une mission qui amoindrirait mes capacités d'être au service des autres."),
        new("DEF", "Travailler sur des problèmes quasiment insolubles est plus important pour moi que d'atteindre une position managériale élevée."),
        new("VIE", "J'ai toujours cherché des opportunités de travail qui minimisent les interférences avec les préoccupations personnelles ou familiales."),
        new("INTER", "Je rêve d'avoir une carrière qui me permette d'avoir des responsabilités internationales."),
    ];

    private static readonly string[] OrdreAncres = ["TECH", "MG", "AUT", "SEC", "CRE", "CAU", "DEF", "VIE", "INTER"];

    // Noms et descriptions vulgarises a partir de Schein (1990) et Cerdin (2007) - theorie
    // publiee, pas un contenu proprietaire au sens de l'enonce des 9 ancres elles-memes
    // (contrairement au questionnaire precis, cf. IScheinService pour la question de licence
    // sur les 45 AFFIRMATIONS).
    private static readonly Dictionary<string, (string Nom, string Description)> Ancres = new()
    {
        ["TECH"] = ("Ancre technique",
            "Vous organisez votre carrière autour d'une spécialisation ou d'une expertise reconnue. Votre identité professionnelle se construit autour de ce domaine : vous cherchez à vous perfectionner et à rester une référence dans votre spécialité. Vous êtes à l'aise pour diriger d'autres personnes dans votre domaine technique, mais le management en tant que tel ne vous attire pas — vous préférez éviter les postes de direction générale qui vous éloigneraient de votre expertise."),
        ["MG"] = ("Ancre managériale",
            "Vous êtes orienté(e) vers les sommets de l'organisation, là où s'exercent le pouvoir et l'influence. Vous voulez endosser la responsabilité d'un ensemble de résultats et identifiez votre réussite personnelle à celle de l'entreprise. Un poste technique ou opérationnel n'est pour vous qu'une étape d'apprentissage : votre ambition est d'accéder à un poste généraliste de direction, pas de devenir l'expert le plus pointu d'un domaine spécialisé."),
        ["AUT"] = ("Ancre autonomie",
            "Vous recherchez avant tout la liberté d'organiser votre travail à votre façon, sans contrainte excessive. Les réglementations restrictives vous pèsent, et vous pouvez refuser une promotion si elle doit réduire votre autonomie. Posséder votre propre entreprise peut être une façon naturelle de réaliser pleinement ce besoin d'indépendance."),
        ["SEC"] = ("Ancre sécurité/stabilité",
            "Votre priorité est la sécurité financière et la stabilité de l'emploi. Vous accordez moins d'importance au contenu du poste ou au rang hiérarchique qu'à la certitude de pouvoir vous projeter sereinement. Cette stabilité peut s'accompagner d'une grande loyauté envers votre employeur, en échange d'une forme de garantie."),
        ["CRE"] = ("Ancre créativité",
            "Vous organisez vos choix de carrière autour du besoin de créer quelque chose de nouveau — une entreprise, un produit, un service. Visionnaire et entreprenant(e), vous aimez innover, planifier et faire aboutir vos idées. Les réseaux ont pour vous une grande importance, et vous êtes prêt(e) à vous lancer seul(e) dès que vous vous en sentirez capable."),
        ["CAU"] = ("Ancre dévouement à une cause",
            "Vos choix de carrière sont guidés par un fort désir de service : vous voulez réaliser quelque chose qui a de la valeur à vos yeux, que ce soit améliorer le monde, aider les autres ou contribuer à une cause qui vous dépasse. Cette orientation peut s'exprimer dans n'importe quel métier, pas seulement dans l'humanitaire."),
        ["DEF"] = ("Ancre défi pur",
            "Vous définissez votre carrière en termes essentiellement compétitifs. Les obstacles difficiles, voire réputés insurmontables, vous attirent — qu'il s'agisse de problèmes intellectuels complexes, de situations à forts enjeux ou de concurrence interpersonnelle directe. La routine et la facilité vous lassent rapidement : la nouveauté et la difficulté sont des fins en soi."),
        ["VIE"] = ("Ancre qualité de vie",
            "Vous cherchez avant tout à équilibrer vos besoins personnels, familiaux et les exigences de votre carrière. La qualité de vie — telle que vous la définissez — est au centre de vos choix professionnels, quitte à sacrifier certains aspects de votre carrière (une promotion impliquant un déménagement, par exemple) pour préserver cet équilibre."),
        ["INTER"] = ("Ancre internationale",
            "Vous êtes particulièrement attiré(e) par la découverte de nouveaux environnements, pays et cultures. La mobilité internationale est pour vous une fin en soi : vous préférez développer vos compétences et votre carrière dans des contextes internationaux, perçus comme plus porteurs de défis et de développement qu'une expérience strictement nationale."),
    };

    private const int ScoreMinAncre = 5; // 5 items x 1 point minimum
    private const int ScoreMaxAncre = 25; // 5 items x 5 points maximum (hors points bonus des choix prioritaires)
    private const int PointsChoixPrioritaire = 4; // cf. Cerdin (2007), etape 2 du scoring
    private const int NombreChoixPrioritairesAttendu = 3;

    public List<ScheinQuestionInfo> GetQuestions() =>
        Items.Select((item, i) => new ScheinQuestionInfo { NumeroQuestion = i + 1, Texte = item.Texte }).ToList();

    public async Task<(bool Success, string? ErrorMessage, ScheinResultatInfo? Resultat)> RepondreAsync(
        string utilisateurId, Dictionary<int, int> reponses, List<int> choixPrioritaires)
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

        var choixDistincts = choixPrioritaires.Distinct().ToList();
        if (choixDistincts.Count != NombreChoixPrioritairesAttendu
            || choixDistincts.Any(numero => numero < 1 || numero > Items.Length))
        {
            return (false, $"Merci de choisir exactement {NombreChoixPrioritairesAttendu} affirmations à l'étape finale.", null);
        }

        var scoresAncres = OrdreAncres.ToDictionary(code => code, _ => 0);
        for (var i = 0; i < Items.Length; i++)
        {
            scoresAncres[Items[i].AncreCode] += reponses[i + 1];
        }
        foreach (var numero in choixDistincts)
        {
            scoresAncres[Items[numero - 1].AncreCode] += PointsChoixPrioritaire;
        }

        var resultat = new ScheinResultat
        {
            UtilisateurId = utilisateurId,
            ScoreTechnique = scoresAncres["TECH"],
            ScoreManageriale = scoresAncres["MG"],
            ScoreAutonomie = scoresAncres["AUT"],
            ScoreSecurite = scoresAncres["SEC"],
            ScoreCreativite = scoresAncres["CRE"],
            ScoreCause = scoresAncres["CAU"],
            ScoreDefiPur = scoresAncres["DEF"],
            ScoreQualiteDeVie = scoresAncres["VIE"],
            ScoreInternationale = scoresAncres["INTER"],
            CompleteLe = DateTime.UtcNow,
        };
        dbContext.ScheinResultats.Add(resultat);
        await dbContext.SaveChangesAsync();

        var info = VersInfo(resultat);

        if (!string.IsNullOrWhiteSpace(utilisateur.Email))
        {
            var (sujet, corps) = ChallengeEmailTemplates.ResultatSchein(info);
            await emailService.EnvoyerAsync(utilisateur.Email, sujet, corps);
        }

        return (true, null, info);
    }

    public async Task<ScheinResultatInfo?> GetDernierResultatAsync(string utilisateurId)
    {
        var resultat = await dbContext.ScheinResultats
            .Where(r => r.UtilisateurId == utilisateurId)
            .OrderByDescending(r => r.CompleteLe)
            .FirstOrDefaultAsync();

        return resultat is null ? null : VersInfo(resultat);
    }

    // Construit le resume a partir de la ou des ancre(s) dominante(s) reelles (egalites
    // possibles, cf. EstDominante) - jamais un texte invariant. "Votre ou vos plus hauts
    // scores indiquent vos orientations de carrière" (grille source) : au pluriel des que
    // plusieurs ancres sont a egalite au sommet.
    private static string ConstruireResume(List<ScheinAncreInfo> ancresTriees)
    {
        var dominantes = ancresTriees.Where(a => a.EstDominante).ToList();
        if (dominantes.Count == 1)
        {
            return $"Votre score le plus élevé est votre {dominantes[0].Nom.ToLowerInvariant()} ({dominantes[0].Score} points) : " +
                "c'est l'ancre qui semble le plus guider vos choix de carrière aujourd'hui. Une ancre de carrière n'est pas figée : " +
                "vous pouvez en changer au fil de votre parcours professionnel.";
        }

        var noms = string.Join(", ", dominantes.Select(a => a.Nom.ToLowerInvariant()));
        return $"Plusieurs ancres ressortent à égalité en tête de votre profil ({noms}, {dominantes[0].Score} points chacune) : " +
            "ce sont elles qui semblent le plus guider vos choix de carrière aujourd'hui. Une ancre de carrière n'est pas figée : " +
            "vous pouvez en changer au fil de votre parcours professionnel.";
    }

    private static ScheinResultatInfo VersInfo(ScheinResultat resultat)
    {
        var scores = new Dictionary<string, int>
        {
            ["TECH"] = resultat.ScoreTechnique,
            ["MG"] = resultat.ScoreManageriale,
            ["AUT"] = resultat.ScoreAutonomie,
            ["SEC"] = resultat.ScoreSecurite,
            ["CRE"] = resultat.ScoreCreativite,
            ["CAU"] = resultat.ScoreCause,
            ["DEF"] = resultat.ScoreDefiPur,
            ["VIE"] = resultat.ScoreQualiteDeVie,
            ["INTER"] = resultat.ScoreInternationale,
        };
        var scoreMax = scores.Values.Max();

        var ancresTriees = OrdreAncres
            .Select(code => (code, score: scores[code]))
            .OrderByDescending(x => x.score)
            .Select(x => new ScheinAncreInfo
            {
                Code = x.code,
                Nom = Ancres[x.code].Nom,
                Score = x.score,
                ScoreMin = ScoreMinAncre,
                ScoreMax = ScoreMaxAncre,
                PourcentagePosition = (int)Math.Round((double)(x.score - ScoreMinAncre) / (ScoreMaxAncre - ScoreMinAncre) * 100),
                Description = Ancres[x.code].Description,
                EstDominante = x.score == scoreMax,
            })
            .ToList();

        return new ScheinResultatInfo
        {
            Ancres = ancresTriees,
            Resume = ConstruireResume(ancresTriees),
            CompleteLe = resultat.CompleteLe,
        };
    }
}
