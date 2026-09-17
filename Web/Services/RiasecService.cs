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

    // Ordre fixe utilise pour l'affichage et pour departager les egalites de score lors du
    // calcul du code Holland (cf. RepondreAsync).
    private static readonly string[] OrdreDimensions = ["R", "I", "A", "S", "E", "C"];

    private static readonly Dictionary<string, (string Nom, string Description)> Profils = new()
    {
        ["R"] = ("Réaliste", "Vous aimez les activités concrètes, techniques et manuelles. Vous êtes à l'aise avec les outils, les machines ou le travail en extérieur, et préférez des résultats tangibles à la théorie."),
        ["I"] = ("Investigateur", "Vous aimez comprendre, analyser et résoudre des problèmes complexes. Vous êtes attiré par la recherche, l'observation et le raisonnement scientifique."),
        ["A"] = ("Artistique", "Vous aimez créer, imaginer et vous exprimer librement. Vous êtes attiré par l'originalité, l'esthétique et les activités qui laissent place à l'interprétation personnelle."),
        ["S"] = ("Social", "Vous aimez aider, enseigner et accompagner les autres. Vous êtes à l'aise dans la relation, l'écoute et le travail en équipe au service d'autrui."),
        ["E"] = ("Entreprenant", "Vous aimez convaincre, diriger et entreprendre. Vous êtes attiré par la prise de décision, la négociation et l'atteinte d'objectifs concrets."),
        ["C"] = ("Conventionnel", "Vous aimez l'organisation, la précision et les méthodes établies. Vous êtes à l'aise avec les données, les procédures et le respect des règles."),
    };

    public List<RiasecQuestionInfo> GetQuestions() =>
        Questions.Select((q, i) => new RiasecQuestionInfo
        {
            NumeroQuestion = i + 1,
            Dimension = q.Dimension,
            Texte = q.Texte,
        }).ToList();

    public async Task<RiasecResultatInfo?> GetDernierResultatAsync(string utilisateurId)
    {
        var resultat = await dbContext.RiasecResultats
            .Where(r => r.UtilisateurId == utilisateurId)
            .OrderByDescending(r => r.CompleteLe)
            .FirstOrDefaultAsync();

        return resultat is null ? null : VersInfo(resultat);
    }

    public async Task<(bool Success, string? ErrorMessage, RiasecResultatInfo? Resultat)> RepondreAsync(string utilisateurId, HashSet<int> reponses)
    {
        var utilisateur = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == utilisateurId);
        if (utilisateur is null)
        {
            return (false, "Utilisateur introuvable.", null);
        }

        if (reponses.Count == 0)
        {
            return (false, "Merci de cocher au moins une activité qui vous plairait.", null);
        }

        var scores = OrdreDimensions.ToDictionary(d => d, _ => 0);
        for (var i = 0; i < Questions.Length; i++)
        {
            var numero = i + 1;
            if (reponses.Contains(numero))
            {
                scores[Questions[i].Dimension]++;
            }
        }

        var codeHolland = string.Concat(
            OrdreDimensions
                .OrderByDescending(d => scores[d])
                .ThenBy(d => Array.IndexOf(OrdreDimensions, d))
                .Take(3));

        var resultat = new RiasecResultat
        {
            UtilisateurId = utilisateurId,
            ScoreR = scores["R"],
            ScoreI = scores["I"],
            ScoreA = scores["A"],
            ScoreS = scores["S"],
            ScoreE = scores["E"],
            ScoreC = scores["C"],
            CodeHolland = codeHolland,
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
            CompleteLe = resultat.CompleteLe,
            Dimensions = OrdreDimensions.Select(d => new RiasecDimensionInfo
            {
                Code = d,
                Nom = Profils[d].Nom,
                Description = Profils[d].Description,
                Score = scores[d],
            }).ToList(),
        };
    }
}
