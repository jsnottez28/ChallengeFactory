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

    // Descriptions et exemples de metiers : caracterisation generale des 6 types de
    // Holland telle qu'enseignee couramment en psychologie de l'orientation (theorie
    // publique, pas un contenu proprietaire de l'O*NET) - illustratif, jamais un outil
    // d'orientation professionnelle exhaustif ou predictif a lui seul.
    private static readonly Dictionary<string, (string Nom, string Description, string[] Metiers)> Profils = new()
    {
        ["R"] = ("Réaliste", "Vous aimez les activités concrètes, techniques et manuelles. Vous êtes à l'aise avec les outils, les machines ou le travail en extérieur, et préférez des résultats tangibles à la théorie.",
            ["Technicien(ne) de maintenance", "Électricien(ne)", "Agriculteur / Agricultrice", "Mécanicien(ne)", "Artisan(e) (menuisier, plombier…)", "Sapeur-pompier"]),
        ["I"] = ("Investigateur", "Vous aimez comprendre, analyser et résoudre des problèmes complexes. Vous êtes attiré par la recherche, l'observation et le raisonnement scientifique.",
            ["Chercheur / Chercheuse", "Ingénieur(e)", "Data analyst / Data scientist", "Développeur(euse) informatique", "Médecin", "Biologiste"]),
        ["A"] = ("Artistique", "Vous aimez créer, imaginer et vous exprimer librement. Vous êtes attiré par l'originalité, l'esthétique et les activités qui laissent place à l'interprétation personnelle.",
            ["Designer graphique", "Architecte", "Musicien(ne)", "Rédacteur(rice) / Écrivain(e)", "Décorateur(rice) d'intérieur", "Réalisateur(rice)"]),
        ["S"] = ("Social", "Vous aimez aider, enseigner et accompagner les autres. Vous êtes à l'aise dans la relation, l'écoute et le travail en équipe au service d'autrui.",
            ["Enseignant(e)", "Infirmier(ère)", "Travailleur(euse) social(e)", "Responsable RH", "Coach / Formateur(rice)", "Conseiller(ère) en orientation"]),
        ["E"] = ("Entreprenant", "Vous aimez convaincre, diriger et entreprendre. Vous êtes attiré par la prise de décision, la négociation et l'atteinte d'objectifs concrets.",
            ["Commercial(e)", "Chef(fe) d'entreprise", "Manager", "Responsable marketing", "Avocat(e)", "Business developer"]),
        ["C"] = ("Conventionnel", "Vous aimez l'organisation, la précision et les méthodes établies. Vous êtes à l'aise avec les données, les procédures et le respect des règles.",
            ["Comptable", "Gestionnaire administratif(ve)", "Analyste financier(ère)", "Assistant(e) de direction", "Auditeur(rice)", "Bibliothécaire / Documentaliste"]),
    };

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
                Score = scores[d],
            }).ToList(),
        };
    }
}
