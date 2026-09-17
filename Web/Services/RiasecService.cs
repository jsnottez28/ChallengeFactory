using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

// Contenu traduit depuis l'O*NET Interest Profiler Short Form (60 items), U.S. Department
// of Labor / Employment and Training Administration, National Center for O*NET
// Development - https://www.onetcenter.org/IP.html - sous licence Creative Commons
// Attribution 4.0 International (CC BY 4.0). Traduction en francais par Challenges
// Factory ; attribution conservee dans le pied de page du test (cf. TestRiasec.cshtml) et
// dans l'email de resultat, conformement aux termes de la licence.
//
// Format a choix force par paires (comparaisons par paires, cf. methode de Thurstone) au
// lieu du format "cocher les activites" d'origine : pour chaque paire, deux items de
// dimensions differentes sont proposes et la personne doit choisir celui qui lui
// correspond le plus. Toujours construit uniquement a partir des 60 items reels traduits -
// jamais de contenu invente, y compris pour le round 2 adaptatif (cf. PreparerRound2).
public sealed class RiasecService(ApplicationDbContext dbContext, IEmailService emailService) : IRiasecService
{
    private static readonly (string Dimension, string Texte)[] Questions =
    [
        ("R", "Construire des meubles de cuisine"),
        ("R", "Poser des briques ou du carrelage"),
        ("R", "Réparer des appareils électroménagers"),
        ("R", "Élever des poissons dans une pisciculture"),
        ("R", "Assembler des pièces électroniques"),
        ("R", "Conduire un camion pour livrer des colis dans des bureaux et des maisons"),
        ("R", "Contrôler la qualité de pièces avant expédition"),
        ("R", "Réparer et installer des serrures"),
        ("R", "Régler et faire fonctionner des machines de production"),
        ("R", "Éteindre des feux de forêt"),

        ("I", "Mettre au point un nouveau médicament"),
        ("I", "Étudier des moyens de réduire la pollution de l'eau"),
        ("I", "Réaliser des expériences chimiques"),
        ("I", "Étudier le mouvement des planètes"),
        ("I", "Examiner des échantillons de sang au microscope"),
        ("I", "Enquêter sur la cause d'un incendie"),
        ("I", "Mettre au point une méthode pour mieux prévoir la météo"),
        ("I", "Travailler dans un laboratoire de biologie"),
        ("I", "Inventer un substitut au sucre"),
        ("I", "Réaliser des analyses de laboratoire pour identifier des maladies"),

        ("A", "Écrire des livres ou des pièces de théâtre"),
        ("A", "Jouer d'un instrument de musique"),
        ("A", "Composer ou arranger de la musique"),
        ("A", "Dessiner"),
        ("A", "Créer des effets spéciaux pour des films"),
        ("A", "Peindre des décors de théâtre"),
        ("A", "Écrire des scénarios pour des films ou des séries télévisées"),
        ("A", "Danser du jazz ou des claquettes"),
        ("A", "Chanter dans un groupe"),
        ("A", "Monter des films"),

        ("S", "Enseigner un programme d'exercices physiques à une personne"),
        ("S", "Aider des personnes à surmonter des difficultés personnelles ou émotionnelles"),
        ("S", "Conseiller des personnes dans leur orientation professionnelle"),
        ("S", "Accompagner une rééducation"),
        ("S", "Faire du bénévolat dans une association"),
        ("S", "Apprendre à des enfants à pratiquer un sport"),
        ("S", "Enseigner la langue des signes à des personnes sourdes ou malentendantes"),
        ("S", "Participer à l'animation d'une séance de thérapie de groupe"),
        ("S", "S'occuper d'enfants dans une crèche"),
        ("S", "Enseigner dans un lycée"),

        ("E", "Acheter et vendre des actions et des obligations"),
        ("E", "Gérer un magasin"),
        ("E", "Diriger un salon de coiffure ou d'esthétique"),
        ("E", "Diriger un service au sein d'une grande entreprise"),
        ("E", "Créer sa propre entreprise"),
        ("E", "Négocier des contrats commerciaux"),
        ("E", "Représenter un client dans un procès"),
        ("E", "Commercialiser une nouvelle ligne de vêtements"),
        ("E", "Vendre des articles dans un grand magasin"),
        ("E", "Gérer une boutique de vêtements"),

        ("C", "Créer un tableur avec un logiciel informatique"),
        ("C", "Relire et corriger des documents ou des formulaires"),
        ("C", "Installer des logiciels sur les ordinateurs d'un grand réseau"),
        ("C", "Utiliser une calculatrice"),
        ("C", "Tenir les registres d'expédition et de réception"),
        ("C", "Calculer les salaires des employés"),
        ("C", "Faire l'inventaire des stocks avec un terminal portable"),
        ("C", "Enregistrer des paiements de loyers"),
        ("C", "Tenir les registres d'inventaire"),
        ("C", "Tamponner, trier et distribuer le courrier d'une organisation"),
    ];

    private static readonly string[] OrdreDimensions = ["R", "I", "A", "S", "E", "C"];

    // Caracterisation generale des 6 types de Holland (nom, description, traits, exemples
    // de metiers, motivations/taches types, environnement favorable, point de vigilance si
    // la dimension est faible) telle qu'enseignee couramment en psychologie de
    // l'orientation - theorie publique, pas un contenu proprietaire de l'O*NET ni copie
    // d'un rapport tiers. Illustratif, jamais un outil d'orientation professionnelle
    // exhaustif ou predictif a lui seul. La synthese globale (cf. ConstruireSynthese)
    // combine ces elements par dimension dominante plutot que de repeter un paragraphe
    // type identique pour chaque dimension.
    private sealed record ProfilDimension(
        string Nom,
        string Description,
        string[] Metiers,
        string[] Traits,
        string[] Motivations,
        string[] Taches,
        string EnvironnementFavorable,
        string PointVigilance);

    private static readonly Dictionary<string, ProfilDimension> Profils = new()
    {
        ["R"] = new ProfilDimension(
            "Réaliste",
            "Vous aimez les activités concrètes, techniques et manuelles. Vous êtes à l'aise avec les outils, les machines ou le travail en extérieur, et préférez des résultats tangibles à la théorie.",
            ["Technicien(ne) de maintenance", "Électricien(ne)", "Agriculteur / Agricultrice", "Mécanicien(ne)", "Artisan(e) (menuisier, plombier…)", "Sapeur-pompier"],
            ["Sens pratique", "Autonomie dans l'action", "Goût du concret", "Fiabilité technique"],
            ["Voir le résultat concret de son travail", "Travailler avec des outils, des machines ou sur le terrain", "Résoudre des problèmes matériels"],
            ["Manipuler des outils ou des équipements techniques", "Intervenir directement sur le terrain", "Réparer, assembler ou construire quelque chose de concret"],
            "un cadre où l'on peut agir concrètement, avec des outils ou des équipements, plutôt qu'un environnement purement théorique",
            "Un score plus faible en Réaliste peut signaler un moindre confort dans les tâches très manuelles ou techniques : un environnement trop axé sur le concret pourrait vous sembler limité."),
        ["I"] = new ProfilDimension(
            "Investigateur",
            "Vous aimez comprendre, analyser et résoudre des problèmes complexes. Vous êtes attiré par la recherche, l'observation et le raisonnement scientifique.",
            ["Chercheur / Chercheuse", "Ingénieur(e)", "Data analyst / Data scientist", "Développeur(euse) informatique", "Médecin", "Biologiste"],
            ["Curiosité intellectuelle", "Esprit d'analyse", "Rigueur scientifique", "Goût de comprendre"],
            ["Comprendre en profondeur avant d'agir", "Résoudre des problèmes complexes par la logique", "Explorer des sujets nouveaux"],
            ["Analyser des données ou des situations complexes", "Mener une recherche ou une investigation", "Tester des hypothèses de façon méthodique"],
            "des temps de réflexion et d'analyse protégés des interruptions, avec un accès facile à l'information",
            "Un score plus faible en Investigateur peut indiquer une préférence pour l'action directe plutôt que pour une longue phase d'analyse : un environnement trop théorique pourrait vous sembler pesant."),
        ["A"] = new ProfilDimension(
            "Artistique",
            "Vous aimez créer, imaginer et vous exprimer librement. Vous êtes attiré par l'originalité, l'esthétique et les activités qui laissent place à l'interprétation personnelle.",
            ["Designer graphique", "Architecte", "Musicien(ne)", "Rédacteur(rice) / Écrivain(e)", "Décorateur(rice) d'intérieur", "Réalisateur(rice)"],
            ["Créativité", "Sensibilité esthétique", "Liberté d'expression", "Goût de l'originalité"],
            ["Exprimer une vision personnelle", "Créer quelque chose d'original", "Sortir des cadres établis"],
            ["Imaginer ou concevoir une création originale", "Mettre en forme une idée de façon esthétique", "Explorer des solutions non conventionnelles"],
            "un cadre flexible, peu formaté, qui laisse de la place à l'initiative et à l'expression personnelle",
            "Un score plus faible en Artistique peut indiquer un moindre confort dans les environnements peu structurés ou exigeant une créativité totalement libre : vous êtes sans doute plus à l'aise avec des méthodes et des repères établis."),
        ["S"] = new ProfilDimension(
            "Social",
            "Vous aimez aider, enseigner et accompagner les autres. Vous êtes à l'aise dans la relation, l'écoute et le travail en équipe au service d'autrui.",
            ["Enseignant(e)", "Infirmier(ère)", "Travailleur(euse) social(e)", "Responsable RH", "Coach / Formateur(rice)", "Conseiller(ère) en orientation"],
            ["Empathie", "Sens de l'écoute", "Goût de la transmission", "Esprit de coopération"],
            ["Aider ou accompagner les autres", "Transmettre un savoir ou une compétence", "Contribuer au bien-être d'un groupe"],
            ["Écouter, conseiller ou accompagner une personne", "Former, enseigner ou expliquer", "Collaborer étroitement avec une équipe"],
            "des interactions humaines fréquentes et authentiques, dans une ambiance de coopération plutôt que de compétition",
            "Un score plus faible en Social peut indiquer une préférence pour un travail plus autonome, avec moins d'interactions ou d'accompagnement direct des autres."),
        ["E"] = new ProfilDimension(
            "Entreprenant",
            "Vous aimez convaincre, diriger et entreprendre. Vous êtes attiré par la prise de décision, la négociation et l'atteinte d'objectifs concrets.",
            ["Commercial(e)", "Chef(fe) d'entreprise", "Manager", "Responsable marketing", "Avocat(e)", "Business developer"],
            ["Esprit d'initiative", "Confiance en soi", "Goût du challenge", "Capacité à convaincre"],
            ["Porter et mener un projet", "Convaincre, négocier ou influencer", "Prendre des décisions et des responsabilités"],
            ["Piloter un projet ou une équipe", "Négocier ou défendre une idée", "Prendre des initiatives et des décisions rapides"],
            "un cadre dynamique, orienté résultats, qui laisse de l'autonomie de décision et valorise l'initiative",
            "Un score plus faible en Entreprenant peut indiquer une préférence pour des rôles d'expertise ou d'exécution plutôt que pour des responsabilités de pilotage, de négociation ou de management commercial."),
        ["C"] = new ProfilDimension(
            "Conventionnel",
            "Vous aimez l'organisation, la précision et les méthodes établies. Vous êtes à l'aise avec les données, les procédures et le respect des règles.",
            ["Comptable", "Gestionnaire administratif(ve)", "Analyste financier(ère)", "Assistant(e) de direction", "Auditeur(rice)", "Bibliothécaire / Documentaliste"],
            ["Sens de l'organisation", "Rigueur", "Fiabilité", "Attention au détail"],
            ["Travailler de façon structurée et méthodique", "Garantir la fiabilité et la qualité d'un résultat", "Suivre des procédures claires"],
            ["Organiser, classer ou structurer de l'information", "Suivre des procédures avec précision", "Gérer des données ou des documents avec rigueur"],
            "un cadre structuré, avec des procédures claires et des attentes bien définies",
            "Un score plus faible en Conventionnel peut indiquer un moindre confort face aux tâches très administratives ou répétitives : vous préférez sans doute des missions moins encadrées par des procédures fixes."),
    };

    // Synergies entre paires de dimensions (theorie publique de l'hexagone de Holland) -
    // cle canonique via ClePaire (ordre fixe par OrdreDimensions, independant de l'ordre
    // d'appel).
    private static readonly Dictionary<string, string> Synergies = new()
    {
        ["RI"] = "Pragmatisme technique et rigueur scientifique : vous aimez comprendre en profondeur avant d'agir, puis mettre la solution en pratique.",
        ["RA"] = "Sens pratique et créativité : vous aimez concevoir des solutions concrètes qui sortent des sentiers battus, à la croisée du faire et de l'imaginer.",
        ["RS"] = "Compétence technique au service des autres : votre expertise concrète prend tout son sens quand elle aide ou accompagne quelqu'un.",
        ["RE"] = "Sens de l'action et goût d'entreprendre : vous aimez transformer une idée en résultat concret, avec l'envie de porter et piloter le projet.",
        ["RC"] = "Rigueur pratique et sens de l'organisation : vous savez structurer votre travail manuel ou technique avec méthode et fiabilité.",
        ["IA"] = "Curiosité intellectuelle et créativité : vous aimez explorer des idées nouvelles et les traduire en solutions originales.",
        ["IS"] = "Analyse au service de l'humain : votre goût pour comprendre en profondeur nourrit une réelle volonté d'aider ou de faire progresser les autres.",
        ["IE"] = "Analyse et esprit d'initiative : vous savez creuser un sujet en profondeur puis en tirer des décisions et des opportunités concrètes.",
        ["IC"] = "Rigueur analytique et méthode : vous aimez structurer une analyse complexe avec précision et exactitude.",
        ["AS"] = "Créativité et relation à l'autre : vous aimez utiliser votre sensibilité pour transmettre, exprimer ou accompagner.",
        ["AE"] = "Créativité et esprit d'initiative : vous aimez porter des idées originales et convaincre les autres de les suivre.",
        ["AC"] = "Créativité et sens du cadre : vous savez donner une forme structurée et soignée à vos idées.",
        ["SE"] = "Relation humaine et leadership : vous aimez mobiliser, motiver et emmener un groupe vers un objectif commun.",
        ["SC"] = "Sens de l'autre et fiabilité : vous accompagnez les autres avec constance, méthode et un vrai souci du détail.",
        ["EC"] = "Esprit d'initiative et rigueur : vous savez allier ambition et sens de l'organisation pour mener un projet à bien.",
    };

    private static string ClePaire(string a, string b)
    {
        var ia = Array.IndexOf(OrdreDimensions, a);
        var ib = Array.IndexOf(OrdreDimensions, b);
        return ia < ib ? a + b : b + a;
    }

    // Distance sur l'hexagone de Holland (R-I-A-S-E-C-R…) : 1 = voisines, 2 = alternees,
    // 3 = opposees. Fonde sur la disposition hexagonale standard du modele (position
    // relative des 6 dimensions), pas une mesure inventee.
    private static int DistanceHexagone(string a, string b)
    {
        var ia = Array.IndexOf(OrdreDimensions, a);
        var ib = Array.IndexOf(OrdreDimensions, b);
        var diff = Math.Abs(ia - ib);
        return Math.Min(diff, OrdreDimensions.Length - diff);
    }

    private static (string Type, string Description) AnalyserCoherence(string[] lettres)
    {
        var distances = new[]
        {
            DistanceHexagone(lettres[0], lettres[1]),
            DistanceHexagone(lettres[1], lettres[2]),
            DistanceHexagone(lettres[0], lettres[2]),
        };

        if (distances.Any(d => d == 3))
        {
            return ("Contrasté",
                "Votre profil combine des dimensions opposées sur l'hexagone de Holland : des intérêts a priori éloignés cohabitent chez vous. C'est souvent le signe d'un profil polyvalent, capable de faire le pont entre des univers différents - à condition de trouver un environnement qui laisse une vraie place à chacune de ces facettes plutôt que d'en sacrifier une.");
        }
        if (distances.All(d => d == 2))
        {
            return ("Complémentaire",
                "Vos trois dimensions dominantes sont réparties de façon équilibrée sur l'hexagone de Holland, sans être ni immédiatement voisines ni opposées. Cette configuration traduit des facettes complémentaires plutôt que redondantes - une combinaison de forces qui peut être un vrai atout dans des rôles transverses.");
        }
        return ("Cohérent",
            "Vos trois dimensions dominantes se suivent sur l'hexagone de Holland : elles s'articulent naturellement entre elles et dessinent une orientation professionnelle claire et cohérente.");
    }

    // Construit la synthese interpretative du profil a partir des 3 dimensions dominantes
    // (CodeHolland, deja ordonnees par score decroissant) et des scores complets - jamais
    // dimension par dimension, pour ne pas repeter un paragraphe type identique 6 fois.
    private static RiasecSyntheseInfo ConstruireSynthese(string codeHolland, Dictionary<string, int> scores)
    {
        var lettres = codeHolland.Select(c => c.ToString()).ToArray();
        var (type, descriptionType) = AnalyserCoherence(lettres);

        var synergies = new List<string>();
        foreach (var (a, b) in new[] { (lettres[0], lettres[1]), (lettres[1], lettres[2]), (lettres[0], lettres[2]) })
        {
            if (Synergies.TryGetValue(ClePaire(a, b), out var texte) && !synergies.Contains(texte))
            {
                synergies.Add(texte);
            }
        }

        var motivations = lettres.SelectMany(l => Profils[l].Motivations.Take(2)).Distinct().ToList();
        var taches = lettres.SelectMany(l => Profils[l].Taches.Take(2)).Distinct().ToList();

        var dimensionsFaibles = OrdreDimensions
            .Where(d => !lettres.Contains(d))
            .OrderBy(d => scores[d])
            .Take(2)
            .ToList();
        var pointsVigilance = dimensionsFaibles.Select(d => Profils[d].PointVigilance).ToList();

        var environnementIdeal = lettres.Select(l => Profils[l].EnvironnementFavorable).ToList();

        var noms = lettres.Select(l => Profils[l].Nom).ToList();
        var resume = $"Avec un profil {noms[0]} / {noms[1]} / {noms[2]} (code {codeHolland}), vous êtes sans doute plus à l'aise dans un poste qui combine ces trois dimensions plutôt que dans un poste qui n'en mobilise qu'une seule. Utilisez le code Holland et les exemples de métiers ci-dessus comme point de départ pour explorer des pistes - pas comme une liste fermée : de nombreux métiers combinent ces dimensions autrement.";

        return new RiasecSyntheseInfo
        {
            TypeProfil = type,
            TypeProfilDescription = descriptionType,
            Synergies = synergies,
            MotivationsCles = motivations,
            TachesPreferees = taches,
            PointsVigilance = pointsVigilance,
            EnvironnementIdeal = environnementIdeal,
            Resume = resume,
        };
    }

    // 30 paires de base couvrant les 60 items une fois chacun (jamais deux items de la
    // meme dimension dans une paire), construites par un tirage a graine fixe -
    // reproductible, jamais un tirage aleatoire par utilisateur.
    private static readonly (int NumeroA, int NumeroB)[] PairesBase = ConstruirePairesBase();

    // 6 des 30 paires de base (une tous les 5) sont reposees a l'identique plus loin dans
    // le round 1, pour l'echelle de fiabilite - jamais reformulees.
    private static readonly int[] IndicesPairesControle = [0, 5, 10, 15, 20, 25];

    // PaireId 1-30 = paires de base (dans l'ordre de ConstruirePairesBase), 31-36 = leurs
    // doublons de controle (31 duplique la paire d'indice 0, etc.).
    private const int NombrePairesBase = 30;

    // Ordre de presentation fixe et "en aveugle" des 36 paires du round 1 - melange une
    // bonne fois pour toutes, jamais groupe, jamais de dimension affichee.
    private static readonly int[] OrdrePresentationRound1 = ConstruireOrdrePresentation(NombrePairesBase + IndicesPairesControle.Length, 20260917 + 1);

    // Le round 2 ne se declenche que si les 2 dimensions les plus proches apres le round 1
    // ont un ecart de score inferieur ou egal a ce seuil - inutile de redemander si le
    // resultat est deja tranche.
    private const int SeuilDepartage = 3;

    // Nombre maximal de paires de departage au round 2 (zip des items gagnants des 2
    // dimensions les plus proches).
    private const int MaxPairesRound2 = 5;

    private static (int, int)[] ConstruirePairesBase()
    {
        var pool = Enumerable.Range(1, Questions.Length).ToList();
        var rng = new Random(20260917);
        for (var i = pool.Count - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        var paires = new List<(int, int)>();
        while (pool.Count > 0)
        {
            var a = pool[0];
            pool.RemoveAt(0);
            var dimensionA = Questions[a - 1].Dimension;
            var indexB = pool.FindIndex(n => Questions[n - 1].Dimension != dimensionA);
            if (indexB < 0)
            {
                indexB = 0;
            }
            var b = pool[indexB];
            pool.RemoveAt(indexB);
            paires.Add((a, b));
        }
        return [.. paires];
    }

    private static int[] ConstruireOrdrePresentation(int nombreSlots, int graine)
    {
        var ids = Enumerable.Range(1, nombreSlots).ToArray();
        var rng = new Random(graine);
        for (var i = ids.Length - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (ids[i], ids[j]) = (ids[j], ids[i]);
        }
        return ids;
    }

    private static RiasecOptionInfo VersOption(int numeroQuestion) => new()
    {
        NumeroQuestion = numeroQuestion,
        Texte = Questions[numeroQuestion - 1].Texte,
    };

    // Renvoie la paire de base (NumeroA, NumeroB) pour un PaireId de round 1 (1-30 = paire
    // de base directe, 31-36 = doublon pointant vers la paire de base correspondante).
    private static (int NumeroA, int NumeroB) PaireRound1(int paireId) =>
        paireId <= NombrePairesBase
            ? PairesBase[paireId - 1]
            : PairesBase[IndicesPairesControle[paireId - NombrePairesBase - 1]];

    public List<RiasecPaireInfo> GetPairesRound1() =>
        OrdrePresentationRound1.Select(paireId =>
        {
            var (numeroA, numeroB) = PaireRound1(paireId);
            return new RiasecPaireInfo { PaireId = paireId, OptionA = VersOption(numeroA), OptionB = VersOption(numeroB) };
        }).ToList();

    public RiasecRound2Info? PreparerRound2(Dictionary<int, int> reponsesRound1)
    {
        var (valide, scores, _, _, _) = AnalyserRound1(reponsesRound1);
        if (!valide)
        {
            return null;
        }

        var (dimensionA, dimensionB, ecart) = TrouverDimensionsLesPlusProches(scores!);
        if (ecart > SeuilDepartage)
        {
            return new RiasecRound2Info { DimensionA = dimensionA, DimensionB = dimensionB, Paires = [] };
        }

        var gagnantsA = GetNumerosGagnants(reponsesRound1, dimensionA);
        var gagnantsB = GetNumerosGagnants(reponsesRound1, dimensionB);
        var nombrePaires = Math.Min(Math.Min(gagnantsA.Count, gagnantsB.Count), MaxPairesRound2);

        var paires = new List<RiasecPaireInfo>();
        for (var i = 0; i < nombrePaires; i++)
        {
            paires.Add(new RiasecPaireInfo
            {
                PaireId = 1000 + i + 1,
                OptionA = VersOption(gagnantsA[i]),
                OptionB = VersOption(gagnantsB[i]),
            });
        }

        return new RiasecRound2Info { DimensionA = dimensionA, DimensionB = dimensionB, Paires = paires };
    }

    public async Task<RiasecResultatInfo?> GetDernierResultatAsync(string utilisateurId)
    {
        var resultat = await dbContext.RiasecResultats
            .Where(r => r.UtilisateurId == utilisateurId)
            .OrderByDescending(r => r.CompleteLe)
            .FirstOrDefaultAsync();

        return resultat is null ? null : VersInfo(resultat);
    }

    public async Task<(bool Success, string? ErrorMessage, RiasecResultatInfo? Resultat)> RepondreAsync(
        string utilisateurId, Dictionary<int, int> reponsesRound1, Dictionary<int, int> reponsesRound2)
    {
        var utilisateur = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == utilisateurId);
        if (utilisateur is null)
        {
            return (false, "Utilisateur introuvable.", null);
        }

        var (valide, scores, nombrePairesCoherentes, nombrePairesControle, _) = AnalyserRound1(reponsesRound1);
        if (!valide)
        {
            return (false, "Merci de répondre à toutes les paires du questionnaire.", null);
        }

        var round2Attendu = PreparerRound2(reponsesRound1)!;
        string? depDimA = null;
        string? depDimB = null;
        string? depGagnant = null;

        if (round2Attendu.Paires.Count > 0)
        {
            if (reponsesRound2.Count != round2Attendu.Paires.Count ||
                round2Attendu.Paires.Any(p => !reponsesRound2.TryGetValue(p.PaireId, out var choix) ||
                    (choix != p.OptionA.NumeroQuestion && choix != p.OptionB.NumeroQuestion)))
            {
                return (false, "Merci de répondre à toutes les paires de départage.", null);
            }

            var victoiresA = round2Attendu.Paires.Count(p => reponsesRound2[p.PaireId] == p.OptionA.NumeroQuestion);
            depDimA = round2Attendu.DimensionA;
            depDimB = round2Attendu.DimensionB;
            depGagnant = victoiresA > round2Attendu.Paires.Count - victoiresA ? round2Attendu.DimensionA
                : victoiresA < round2Attendu.Paires.Count - victoiresA ? round2Attendu.DimensionB
                : null; // egalite au departage : aucune conclusion supplementaire
        }

        var codeHolland = CalculerCodeHolland(scores!, depDimA, depDimB, depGagnant);

        var resultat = new RiasecResultat
        {
            UtilisateurId = utilisateurId,
            ScoreR = scores!["R"],
            ScoreI = scores["I"],
            ScoreA = scores["A"],
            ScoreS = scores["S"],
            ScoreE = scores["E"],
            ScoreC = scores["C"],
            CodeHolland = codeHolland,
            NombrePairesCoherentes = nombrePairesCoherentes,
            NombrePairesControle = nombrePairesControle,
            DepartageDimensionA = depDimA,
            DepartageDimensionB = depDimB,
            DepartageGagnant = depGagnant,
            CompleteLe = DateTime.UtcNow,
        };
        dbContext.RiasecResultats.Add(resultat);
        await dbContext.SaveChangesAsync();

        var info = VersInfo(resultat);

        if (!string.IsNullOrWhiteSpace(utilisateur.Email))
        {
            var (sujet, corps) = ChallengeEmailTemplates.ResultatRiasec(info);
            await emailService.EnvoyerAsync(utilisateur.Email, sujet, corps);
        }

        return (true, null, info);
    }

    // Valide et depouille le round 1 : les 36 paires doivent toutes etre repondues avec un
    // choix valide (l'une des deux options de la paire). Renvoie les scores par dimension
    // (sur les 30 paires de base uniquement) et la coherence sur les 6 paires de controle.
    private static (bool Valide, Dictionary<string, int>? Scores, int NombrePairesCoherentes, int NombrePairesControle, Dictionary<int, int>? _)
        AnalyserRound1(Dictionary<int, int> reponsesRound1)
    {
        for (var paireId = 1; paireId <= NombrePairesBase + IndicesPairesControle.Length; paireId++)
        {
            var (numeroA, numeroB) = PaireRound1(paireId);
            if (!reponsesRound1.TryGetValue(paireId, out var choix) || (choix != numeroA && choix != numeroB))
            {
                return (false, null, 0, 0, null);
            }
        }

        var scores = OrdreDimensions.ToDictionary(d => d, _ => 0);
        for (var paireId = 1; paireId <= NombrePairesBase; paireId++)
        {
            var choix = reponsesRound1[paireId];
            scores[Questions[choix - 1].Dimension]++;
        }

        var nombrePairesCoherentes = 0;
        for (var i = 0; i < IndicesPairesControle.Length; i++)
        {
            var paireIdBase = IndicesPairesControle[i] + 1;
            var paireIdDoublon = NombrePairesBase + i + 1;
            if (reponsesRound1[paireIdBase] == reponsesRound1[paireIdDoublon])
            {
                nombrePairesCoherentes++;
            }
        }

        return (true, scores, nombrePairesCoherentes, IndicesPairesControle.Length, null);
    }

    private static (string DimensionA, string DimensionB, int Ecart) TrouverDimensionsLesPlusProches(Dictionary<string, int> scores)
    {
        var meilleur = (DimensionA: OrdreDimensions[0], DimensionB: OrdreDimensions[1], Ecart: int.MaxValue, Somme: -1);
        for (var i = 0; i < OrdreDimensions.Length; i++)
        {
            for (var j = i + 1; j < OrdreDimensions.Length; j++)
            {
                var dimA = OrdreDimensions[i];
                var dimB = OrdreDimensions[j];
                var ecart = Math.Abs(scores[dimA] - scores[dimB]);
                var somme = scores[dimA] + scores[dimB];
                if (ecart < meilleur.Ecart || (ecart == meilleur.Ecart && somme > meilleur.Somme))
                {
                    meilleur = (dimA, dimB, ecart, somme);
                }
            }
        }
        return (meilleur.DimensionA, meilleur.DimensionB, meilleur.Ecart);
    }

    // Items de la dimension donnee choisis par l'utilisateur parmi les 30 paires de base -
    // ce sont les "gagnants" de cette dimension au round 1, dans lesquels le round 2 puise
    // (jamais de contenu hors des 60 items reels).
    private static List<int> GetNumerosGagnants(Dictionary<int, int> reponsesRound1, string dimension)
    {
        var gagnants = new List<int>();
        for (var paireId = 1; paireId <= NombrePairesBase; paireId++)
        {
            var choix = reponsesRound1[paireId];
            if (Questions[choix - 1].Dimension == dimension)
            {
                gagnants.Add(choix);
            }
        }
        return gagnants;
    }

    private static string CalculerCodeHolland(Dictionary<string, int> scores, string? depDimA, string? depDimB, string? depGagnant)
    {
        var scoresEffectifs = OrdreDimensions.ToDictionary(d => d, d => (double)scores[d]);

        // Le departage (round 2) fait "permuter" les deux dimensions comparees a
        // l'interieur de l'intervalle [min, max] de leurs propres scores d'origine : la
        // gagnante du departage prend la position du plus haut des deux scores, la
        // perdante celle du plus bas (a peine en-dessous). Ca renverse leur ordre relatif
        // meme si l'ecart de round 1 n'etait pas une egalite stricte, sans jamais faire
        // sauter l'une des deux devant une troisieme dimension qui n'a pas ete comparee.
        if (depGagnant is not null && depDimA is not null && depDimB is not null)
        {
            var perdant = depGagnant == depDimA ? depDimB : depDimA;
            var scoreMax = Math.Max(scoresEffectifs[depDimA], scoresEffectifs[depDimB]);
            var scoreMin = Math.Min(scoresEffectifs[depDimA], scoresEffectifs[depDimB]);
            scoresEffectifs[depGagnant] = scoreMax;
            scoresEffectifs[perdant] = scoreMin - 0.01;
        }

        var classement = OrdreDimensions
            .OrderByDescending(d => scoresEffectifs[d])
            .ThenBy(d => Array.IndexOf(OrdreDimensions, d))
            .Take(3);

        return string.Concat(classement);
    }

    private static RiasecResultatInfo VersInfo(RiasecResultat resultat)
    {
        var scores = new Dictionary<string, int>
        {
            ["R"] = resultat.ScoreR,
            ["I"] = resultat.ScoreI,
            ["A"] = resultat.ScoreA,
            ["S"] = resultat.ScoreS,
            ["E"] = resultat.ScoreE,
            ["C"] = resultat.ScoreC,
        };

        return new RiasecResultatInfo
        {
            CodeHolland = resultat.CodeHolland,
            NombrePairesCoherentes = resultat.NombrePairesCoherentes,
            NombrePairesControle = resultat.NombrePairesControle,
            DepartageDimensionA = resultat.DepartageDimensionA,
            DepartageDimensionB = resultat.DepartageDimensionB,
            DepartageGagnant = resultat.DepartageGagnant,
            CompleteLe = resultat.CompleteLe,
            Dimensions = OrdreDimensions.Select(d => new RiasecDimensionInfo
            {
                Code = d,
                Nom = Profils[d].Nom,
                Description = Profils[d].Description,
                Metiers = [.. Profils[d].Metiers],
                Traits = [.. Profils[d].Traits],
                Score = scores[d],
            }).ToList(),
            Synthese = ConstruireSynthese(resultat.CodeHolland, scores),
        };
    }
}
