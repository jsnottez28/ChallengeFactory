using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public sealed class DiscService(ApplicationDbContext dbContext, IEmailService emailService) : IDiscService
{
    // Contenu (questions, options, grille de scoring, descriptions de profil) transcrit tel
    // quel depuis challenges-factory.com/Test/test-disc.html, fourni par Challenges Factory.
    // Pour chaque question, Grille[i] donne - dans l'ordre D, I, S, C - la lettre d'option qui
    // compte pour cette dimension (une seule correspond, cf. RepondreAsync).
    private static readonly string[][] Questions =
    [
        ["Prudent et réfléchi", "Loyal et attentif à autrui", "Influent et démonstratif", "Stratège et entreprenant"],
        ["Sociable et familier", "Honnête et discret", "Energique et orienté vers le résultat", "Méthodique et logique"],
        ["Calme et d'humeur égale", "Déterminé et aimant diriger", "Enjoué et rayonnant", "Formaliste et factuel"],
        ["Sûr de lui et volontaire", "Ordonné et concis", "Familier et stable", "Loquace et de bonne humeur"],
        ["Perspicace et impartial", "Exigeant et direct", "Constant et attaché aux valeurs", "Actif et liant"],
        ["Accommodant et serviable", "Plein d'espoir et expressif", "Puissant et sûr de lui", "Pensif et maître de soi"],
        ["Ouvert et persuasif", "Appliqué et sélectif dans ses relations", "Ferme et entreprenant", "Posé et analytique"],
        ["Déterminé et résolu", "Avenant et jovial", "Sensible et amical", "Logique et correct"],
        ["Compatissant et diplomate", "Précis et mesuré", "Encourageant et ouvert aux idées", "Orienté résultat et rapidité"],
        ["Responsable et ferme", "Réservé et coopératif", "Expansif et imaginatif", "Méticuleux et minutieux"],
        ["Esprit d'équipe et spontané", "Contrôlé et rationnel", "Aimable et prévenant", "Opiniâtre et visant le résultat"],
        ["Analyste et sceptique", "Amical et divertissant", "Exigeant et solide", "Modeste et fidèle"],
        ["Attaché à ses proches et calme", "Affectif et confiant", "Observateur et distant", "Actif et contrôlant"],
        ["Volontaire et tenace", "Conforme et sans parti pris", "Enthousiaste et attachant", "Impliqué et consensuel"],
        ["Formel et à principe", "Jovial et populaire", "Modérateur et apaisant", "Ferme et tranchant"],
        ["Animé et persuasif", "Décideur et pressé", "Analytique et aimant la discipline", "Tolérant et calme"],
        ["Patient et empathique", "Logique et mesuré", "Orienté résultat et prêt au défi", "Ouvert aux idées et arrangeant"],
        ["Influent et décontracté", "Discret et philosophe", "Réfléchi et circonspect", "Opiniâtre et déterminé"],
        ["Axé procédure et bien préparé", "Courageux et autonome", "Extraverti et communicatif", "Bienveillant et de bon conseil"],
        ["Puissant et clair", "Spontané et vif", "Studieux et raisonné", "Paisible et aimant l'harmonie"],
        ["Organisé et prudent", "Patient et serviable", "Argumenté et sûr de lui", "Interactif et ouvert"],
        ["Indépendant et audacieux", "Souple et harmonieux", "Factuel et respectueux des normes", "Aimable et vivant"],
        ["Démonstratif et enthousiaste", "Directif et réaliste", "Compatissant et prévenant", "Attentif et soucieux du détail"],
        ["Stable et altruiste", "Objectif et hardi", "Consciencieux et introspectif", "Sociable et bon vivant"],
        ["Détaillé et précautionneux", "Direct et carré", "Expressif et radieux", "Tolérant et ferme"],
    ];

    private static readonly char[][] Grille =
    [
        ['d', 'c', 'b', 'a'],
        ['c', 'a', 'b', 'd'],
        ['b', 'c', 'a', 'd'],
        ['a', 'd', 'c', 'b'],
        ['b', 'd', 'c', 'a'],
        ['c', 'b', 'a', 'd'],
        ['c', 'a', 'b', 'd'],
        ['a', 'b', 'c', 'd'],
        ['d', 'c', 'a', 'b'],
        ['a', 'c', 'b', 'd'],
        ['d', 'a', 'c', 'b'],
        ['c', 'b', 'd', 'a'],
        ['d', 'b', 'a', 'c'],
        ['a', 'c', 'd', 'b'],
        ['d', 'b', 'c', 'a'],
        ['b', 'a', 'd', 'c'],
        ['c', 'd', 'a', 'b'],
        ['d', 'a', 'b', 'c'],
        ['b', 'c', 'd', 'a'],
        ['a', 'b', 'd', 'c'],
        ['c', 'd', 'b', 'a'],
        ['a', 'd', 'b', 'c'],
        ['b', 'a', 'c', 'd'],
        ['b', 'd', 'a', 'c'],
        ['b', 'c', 'd', 'a'],
    ];

    private static readonly char[] Lettres = ['a', 'b', 'c', 'd'];
    private static readonly char[] Dimensions = ['D', 'I', 'S', 'C'];

    private static readonly Dictionary<char, (string Nom, string Description, string[] Traits)> Profils = new()
    {
        ['D'] = ("Dominant", "Vous êtes orienté résultats, direct et déterminé. Vous aimez relever les défis, prendre des décisions rapidement et exercer un contrôle sur votre environnement. Leader naturel, vous êtes à l'aise dans les situations de compétition et d'urgence.", ["Décideur", "Direct", "Compétitif", "Exigeant", "Autonome", "Ambitieux"]),
        ['I'] = ("Influent", "Vous êtes enthousiaste, communicatif et persuasif. Vous aimez les interactions sociales, inspirez les autres et créez facilement du lien. Optimiste de nature, vous apportez de l'énergie et de la créativité dans les projets collectifs.", ["Enthousiaste", "Sociable", "Optimiste", "Expressif", "Créatif", "Convaincant"]),
        ['S'] = ("Stable", "Vous êtes patient, loyal et coopératif. Vous privilégiez l'harmonie, la constance et le soutien aux autres. Pilier fiable de votre entourage, vous gérez les situations avec calme et bienveillance, favorisant la cohésion d'équipe.", ["Patient", "Loyal", "Calme", "Empathique", "Coopératif", "Fiable"]),
        ['C'] = ("Consciencieux", "Vous êtes analytique, précis et méthodique. Vous attachez une grande importance à la qualité, aux règles et à la rigueur. Votre sens du détail et votre approche structurée vous permettent d'atteindre un haut niveau d'excellence.", ["Précis", "Analytique", "Méthodique", "Rigoureux", "Organisé", "Perfectionniste"]),
    };

    public List<DiscQuestionInfo> GetQuestions() =>
        Questions.Select((options, i) => new DiscQuestionInfo
        {
            NumeroQuestion = i + 1,
            Options = options.Select((texte, j) => new DiscQuestionOptionInfo { Lettre = Lettres[j].ToString(), Texte = texte }).ToList(),
        }).ToList();

    public async Task<DiscResultatInfo?> GetDernierResultatAsync(string utilisateurId)
    {
        var resultat = await dbContext.DiscResultats
            .Where(r => r.UtilisateurId == utilisateurId)
            .OrderByDescending(r => r.CompleteLe)
            .FirstOrDefaultAsync();

        return resultat is null ? null : VersInfo(resultat);
    }

    public async Task<(bool Success, string? ErrorMessage, DiscResultatInfo? Resultat)> RepondreAsync(string utilisateurId, Dictionary<int, string> reponses)
    {
        var utilisateur = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == utilisateurId);
        if (utilisateur is null)
        {
            return (false, "Utilisateur introuvable.", null);
        }

        for (var numero = 1; numero <= Questions.Length; numero++)
        {
            if (!reponses.TryGetValue(numero, out var lettre) || lettre.Length != 1 || !Lettres.Contains(lettre[0]))
            {
                return (false, "Merci de répondre à toutes les questions du test.", null);
            }
        }

        var scores = new Dictionary<char, int> { ['D'] = 0, ['I'] = 0, ['S'] = 0, ['C'] = 0 };
        for (var i = 0; i < Questions.Length; i++)
        {
            var choisie = reponses[i + 1][0];
            var ligne = Grille[i];
            for (var j = 0; j < Dimensions.Length; j++)
            {
                if (ligne[j] == choisie)
                {
                    scores[Dimensions[j]]++;
                }
            }
        }

        var dominant = scores.OrderByDescending(s => s.Value).First().Key;

        var resultat = new DiscResultat
        {
            UtilisateurId = utilisateurId,
            ScoreD = scores['D'],
            ScoreI = scores['I'],
            ScoreS = scores['S'],
            ScoreC = scores['C'],
            ProfilDominant = dominant.ToString(),
            CompleteLe = DateTime.UtcNow,
        };
        dbContext.DiscResultats.Add(resultat);
        await dbContext.SaveChangesAsync();

        var info = VersInfo(resultat);

        if (!string.IsNullOrWhiteSpace(utilisateur.Email))
        {
            var (sujet, corps) = ChallengeEmailTemplates.ResultatDisc(info);
            await emailService.EnvoyerAsync(utilisateur.Email, sujet, corps);
        }

        return (true, null, info);
    }

    private static DiscResultatInfo VersInfo(DiscResultat resultat)
    {
        var dominant = resultat.ProfilDominant.Length > 0 ? resultat.ProfilDominant[0] : 'D';
        var profil = Profils[dominant];

        return new DiscResultatInfo
        {
            ScoreD = resultat.ScoreD,
            ScoreI = resultat.ScoreI,
            ScoreS = resultat.ScoreS,
            ScoreC = resultat.ScoreC,
            ProfilDominant = resultat.ProfilDominant,
            ProfilDominantNom = profil.Nom,
            ProfilDominantDescription = profil.Description,
            ProfilDominantTraits = [.. profil.Traits],
            CompleteLe = resultat.CompleteLe,
        };
    }
}
