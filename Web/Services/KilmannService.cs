using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public sealed class KilmannService(ApplicationDbContext dbContext, IEmailService emailService) : IKilmannService
{
    // Contenu (30 enonces a choix force, grille de scoring, descriptions des 5 modes)
    // transcrit tel quel depuis la ressource "Methode Thomas-Kilmann" fournie par
    // Challenges Factory. Pour chaque question, Questions[i] donne l'enonce A puis
    // l'enonce B ; Grille[i] donne - dans le meme ordre - le mode de gestion de conflit
    // auquel chaque enonce correspond (cf. RepondreAsync/Calculer).
    private static readonly (string TexteA, string TexteB)[] Questions =
    [
        ("Je laisse parfois à d'autres la responsabilité de résoudre le problème.", "Plutôt que de discuter de nos désaccords, j'essaie de faire ressortir les points sur lesquels nous nous entendons."),
        ("J'essaie de trouver une solution de compromis.", "Je tâche de répondre à toutes nos préoccupations, aux siennes comme aux miennes."),
        ("Je poursuis généralement mes objectifs avec détermination.", "Je chercherais à rassurer l'autre et à préserver notre relation."),
        ("J'essaie de trouver une solution de compromis.", "Je sacrifie parfois mes propres désirs à ceux de l'autre."),
        ("Je cherche systématiquement l'aide de l'autre pour trouver une solution.", "J'essaie de faire le nécessaire pour éviter les tensions inutiles."),
        ("J'essaie d'éviter de me créer des désagréments.", "J'essaie de faire adopter ma position."),
        ("J'essaie de repousser le problème pour m'accorder un délai de réflexion.", "Je cède sur certains points en échange d'autres concessions."),
        ("Je poursuis généralement mes objectifs avec détermination.", "J'essaie de soulever immédiatement toutes les questions qui peuvent se poser."),
        ("Je trouve qu'il ne vaut pas toujours la peine de se préoccuper des divergences.", "Je fais des efforts pour arriver à mon but."),
        ("Je poursuis mes objectifs avec détermination.", "J'essaie de trouver une solution de compromis."),
        ("J'essaie de soulever immédiatement toutes les questions qui peuvent se poser.", "Je chercherais à rassurer l'autre et à préserver notre relation."),
        ("J'évite parfois les positions sujettes à controverse.", "Je fais des concessions si l'autre en fait."),
        ("Je propose un terrain d'entente.", "J'insiste pour faire passer mes idées."),
        ("Je fais part de mes idées à l'autre et je lui demande les siennes.", "J'essaie de lui démontrer la logique et les avantages de ma position."),
        ("Je chercherais à rassurer l'autre et à préserver notre relation.", "J'essaie de faire le nécessaire pour éviter des tensions."),
        ("J'essaie de ne pas blesser l'autre.", "J'essaie de convaincre l'autre personne des mérites de ma position."),
        ("Je poursuis généralement mes objectifs avec détermination.", "J'essaie de faire le nécessaire pour éviter les tensions inutiles."),
        ("Je peux laisser l'autre maintenir ses positions si cela lui fait plaisir.", "Je fais des concessions si l'autre en fait."),
        ("J'essaie de soulever immédiatement toutes les questions qui peuvent se poser.", "J'essaie de repousser le problème pour m'accorder un délai de réflexion."),
        ("Je cherche à régler immédiatement nos divergences.", "J'essaie de trouver un juste milieu pour nos concessions mutuelles."),
        ("En abordant les négociations, j'essaie de prendre en considération les aspirations de l'autre.", "Je penche toujours en faveur d'une discussion directe du problème."),
        ("J'essaie de trouver une position intermédiaire entre la sienne et la mienne.", "Je fais valoir mes désirs."),
        ("Je m'attache très souvent à satisfaire toutes nos aspirations.", "Je laisse parfois à d'autres la responsabilité de résoudre le problème."),
        ("Si l'autre prend sa position très à cœur, j'essaierais de satisfaire ses aspirations.", "J'essaie de l'amener à un compromis."),
        ("J'essaie de lui démontrer la logique et les avantages de ma position.", "En abordant les négociations, j'essaie de prendre en considération les aspirations de l'autre."),
        ("Je propose un terrain d'entente.", "Je cherche presque toujours à satisfaire toutes nos aspirations."),
        ("J'évite parfois les positions sujettes à controverse.", "Si cela fait plaisir à l'autre, je peux le laisser maintenir ses positions."),
        ("Je poursuis généralement mes objectifs avec détermination.", "Je cherche généralement l'aide de l'autre pour trouver une solution."),
        ("Je propose un terrain d'entente.", "Je trouve qu'il ne vaut pas toujours la peine de se préoccuper des divergences."),
        ("J'essaie de ne pas blesser l'autre.", "Je partage toujours ma vision du problème avec l'autre pour que nous puissions le régler."),
    ];

    private static readonly (string ModeA, string ModeB)[] Grille =
    [
        ("Eviter", "Ceder"),
        ("Compromis", "Collaborer"),
        ("Rivaliser", "Ceder"),
        ("Compromis", "Ceder"),
        ("Collaborer", "Eviter"),
        ("Eviter", "Rivaliser"),
        ("Eviter", "Compromis"),
        ("Rivaliser", "Collaborer"),
        ("Eviter", "Rivaliser"),
        ("Rivaliser", "Compromis"),
        ("Collaborer", "Ceder"),
        ("Eviter", "Compromis"),
        ("Compromis", "Rivaliser"),
        ("Collaborer", "Rivaliser"),
        ("Ceder", "Eviter"),
        ("Ceder", "Rivaliser"),
        ("Rivaliser", "Eviter"),
        ("Ceder", "Compromis"),
        ("Collaborer", "Eviter"),
        ("Collaborer", "Compromis"),
        ("Ceder", "Collaborer"),
        ("Compromis", "Rivaliser"),
        ("Collaborer", "Eviter"),
        ("Ceder", "Compromis"),
        ("Rivaliser", "Ceder"),
        ("Compromis", "Collaborer"),
        ("Eviter", "Ceder"),
        ("Rivaliser", "Collaborer"),
        ("Compromis", "Eviter"),
        ("Ceder", "Collaborer"),
    ];

    private static readonly string[] Modes = ["Rivaliser", "Collaborer", "Compromis", "Eviter", "Ceder"];

    private static readonly Dictionary<string, (string Nom, string Description, string[] Positif, string[] Negatif)> Profils = new()
    {
        ["Rivaliser"] = (
            "Rivaliser / Contrôle",
            "On peut l'appeler « agression, compétition, imposition… ». La préoccupation principale de celui qui adopte une telle attitude est de tenter de gagner, de défaire l'autre partie. Les intérêts et les besoins de l'autre ne comptent pas. Un tel comportement est inadéquat parce qu'il est basé sur de fausses perceptions de la réalité que l'individu tend à renforcer.",
            ["La situation exige une solution rapide.", "Les 2 parties sont en accord.", "Les autres alternatives ont échoué."],
            ["Un des individus n'a pas de place pour s'exprimer."]
        ),
        ["Collaborer"] = (
            "Collaborer",
            "Ce type de réaction est marqué par la volonté des parties de parvenir à une solution qui répond aux besoins et aux préoccupations de chacun. Il allie de manière positive et productive l'affirmation de soi et la coopération. Si, malgré tout, le conflit persiste, c'est que souvent une seule des parties impliquées accepte de collaborer.",
            ["Le temps permet de bien résoudre la situation.", "Les parties sont disponibles et possèdent les habiletés pour franchir le processus."],
            ["Le temps, la disponibilité ou les habiletés nécessaires ne sont pas présents."]
        ),
        ["Compromis"] = (
            "Compromis",
            "En adoptant cette attitude, l'une des parties impliquées dans le conflit cède quelque chose à l'autre dans l'esprit de le satisfaire suffisamment pour qu'il cesse son comportement ou pour qu'il cède lui aussi quelque chose en retour. Le compromis constitue une sorte de transaction où chacun s'échange un avantage quelconque dans le but de mettre fin au conflit. C'est une solution qui est limitée dans son efficacité.",
            ["Les 2 parties ont l'impression de pouvoir avoir réponse à leurs besoins par une entente.", "Des intérêts communs sont présents."],
            []
        ),
        ["Eviter"] = (
            "Éviter",
            "La personne qui adopte cette attitude le fait parce qu'elle craint la confrontation. Elle tente alors de fuir l'autre et de l'éviter. Elle s'affirme donc peu et ne coopère pas du tout. Cette attitude est inadéquate parce qu'elle favorise le retrait social.",
            ["La situation est plutôt sans importance.", "Le moment n'est pas opportun.", "Un temps pour décompresser est nécessaire."],
            ["La situation est importante.", "La situation ne s'estompera pas, mais risque plutôt de s'aggraver."]
        ),
        ["Ceder"] = (
            "Céder",
            "Il s'agit aussi d'accommodement, de céder… celui qui adopte une telle attitude tente d'accommoder l'autre en cédant devant son comportement agressif. Il met de côté ses propres intérêts pour satisfaire ceux de l'autre. La personne qui accommode l'autre s'affirme très peu tout en coopérant beaucoup. Ce comportement est inadéquat parce qu'il peut mener à des attitudes de retrait et encourager la domination.",
            ["La situation est plutôt sans importance.", "Le moment n'est pas opportun.", "Un temps pour décompresser est nécessaire.", "La relation est actuellement plus importante à préserver que résoudre le problème."],
            ["L'aversion à résoudre des conflits laisse en suspens des conflits importants.", "L'autre individu est prêt à résoudre la situation."]
        ),
    };

    public List<KilmannQuestionInfo> GetQuestions() =>
        Questions.Select((q, i) => new KilmannQuestionInfo
        {
            NumeroQuestion = i + 1,
            Options =
            [
                new KilmannQuestionOptionInfo { Lettre = "A", Texte = q.TexteA },
                new KilmannQuestionOptionInfo { Lettre = "B", Texte = q.TexteB },
            ],
        }).ToList();

    public (bool Success, string? ErrorMessage, KilmannResultatInfo? Resultat) Calculer(Dictionary<int, string> reponses)
    {
        for (var numero = 1; numero <= Questions.Length; numero++)
        {
            if (!reponses.TryGetValue(numero, out var lettre) || (lettre != "A" && lettre != "B"))
            {
                return (false, "Merci de répondre à toutes les questions du test.", null);
            }
        }

        var scores = Modes.ToDictionary(m => m, _ => 0);
        for (var i = 0; i < Questions.Length; i++)
        {
            var choisie = reponses[i + 1];
            var ligne = Grille[i];
            var mode = choisie == "A" ? ligne.ModeA : ligne.ModeB;
            scores[mode]++;
        }

        var dominant = scores.OrderByDescending(s => s.Value).First().Key;

        var resultat = new KilmannResultatInfo
        {
            ProfilDominant = dominant,
            CompleteLe = DateTime.UtcNow,
            Modes = Modes.Select(code =>
            {
                var profil = Profils[code];
                return new KilmannModeInfo
                {
                    Code = code,
                    Nom = profil.Nom,
                    Score = scores[code],
                    Description = profil.Description,
                    UtilisationPositive = [.. profil.Positif],
                    UtilisationNegative = [.. profil.Negatif],
                };
            }).ToList(),
        };

        return (true, null, resultat);
    }

    public async Task<KilmannResultatInfo?> GetDernierResultatAsync(string utilisateurId)
    {
        var resultat = await dbContext.KilmannResultats
            .Where(r => r.UtilisateurId == utilisateurId)
            .OrderByDescending(r => r.CompleteLe)
            .FirstOrDefaultAsync();

        return resultat is null ? null : VersInfo(resultat);
    }

    public async Task<(bool Success, string? ErrorMessage, KilmannResultatInfo? Resultat)> RepondreAsync(string utilisateurId, Dictionary<int, string> reponses)
    {
        var utilisateur = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == utilisateurId);
        if (utilisateur is null)
        {
            return (false, "Utilisateur introuvable.", null);
        }

        var (success, errorMessage, resultatCalcule) = Calculer(reponses);
        if (!success || resultatCalcule is null)
        {
            return (false, errorMessage, null);
        }

        var scoreParMode = resultatCalcule.Modes.ToDictionary(m => m.Code, m => m.Score);

        var resultat = new KilmannResultat
        {
            UtilisateurId = utilisateurId,
            ScoreRivaliser = scoreParMode["Rivaliser"],
            ScoreCollaborer = scoreParMode["Collaborer"],
            ScoreCompromis = scoreParMode["Compromis"],
            ScoreEviter = scoreParMode["Eviter"],
            ScoreCeder = scoreParMode["Ceder"],
            ProfilDominant = resultatCalcule.ProfilDominant,
            CompleteLe = DateTime.UtcNow,
        };
        dbContext.KilmannResultats.Add(resultat);
        await dbContext.SaveChangesAsync();

        var info = VersInfo(resultat);

        if (!string.IsNullOrWhiteSpace(utilisateur.Email))
        {
            var (sujet, corps) = ChallengeEmailTemplates.ResultatKilmann(info);
            await emailService.EnvoyerAsync(utilisateur.Email, sujet, corps);
        }

        return (true, null, info);
    }

    private static KilmannResultatInfo VersInfo(KilmannResultat resultat)
    {
        var scores = new Dictionary<string, int>
        {
            ["Rivaliser"] = resultat.ScoreRivaliser,
            ["Collaborer"] = resultat.ScoreCollaborer,
            ["Compromis"] = resultat.ScoreCompromis,
            ["Eviter"] = resultat.ScoreEviter,
            ["Ceder"] = resultat.ScoreCeder,
        };

        return new KilmannResultatInfo
        {
            ProfilDominant = resultat.ProfilDominant,
            CompleteLe = resultat.CompleteLe,
            Modes = Modes.Select(code =>
            {
                var profil = Profils[code];
                return new KilmannModeInfo
                {
                    Code = code,
                    Nom = profil.Nom,
                    Score = scores[code],
                    Description = profil.Description,
                    UtilisationPositive = [.. profil.Positif],
                    UtilisationNegative = [.. profil.Negatif],
                };
            }).ToList(),
        };
    }
}
