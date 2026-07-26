using System.Net;

namespace Web.Services;

// Templates HTML des emails du moteur de Challenges, isoles du code metier
// (ICohorteService) pour rester facilement modifiables par l'equipe contenu/marketing
// sans toucher a la logique d'envoi.
public static class ChallengeEmailTemplates
{
    public static (string Sujet, string CorpsHtml) NouvelleEtape(
        string challengeTitre,
        string etapeTitre,
        List<string> carteTitres,
        string lienMonParcours)
    {
        var sujet = $"{challengeTitre} — Nouvelle étape disponible";

        var listeCartes = carteTitres.Count > 0
            ? "<ul>" + string.Join("", carteTitres.Select(titre => $"<li>{WebUtility.HtmlEncode(titre)}</li>")) + "</ul>"
            : "";

        var corps = $"""
            <p>Bonjour,</p>
            <p>Une nouvelle étape de votre Challenge <strong>{WebUtility.HtmlEncode(challengeTitre)}</strong> vient de s'ouvrir : <strong>{WebUtility.HtmlEncode(etapeTitre)}</strong>.</p>
            {listeCartes}
            <p><a href="{lienMonParcours}">Accéder à mon parcours en cours</a></p>
            """;

        return (sujet, corps);
    }

    public static (string Sujet, string CorpsHtml) Cloture(string challengeTitre, string lienBibliotheque)
    {
        var sujet = $"{challengeTitre} — Challenge terminé, félicitations !";

        var corps = $"""
            <p>Bonjour,</p>
            <p>Félicitations, vous avez terminé le Challenge <strong>{WebUtility.HtmlEncode(challengeTitre)}</strong> !</p>
            <p>Toutes les cartes de compétences que vous avez débloquées restent accessibles dans votre bibliothèque personnelle.</p>
            <p><a href="{lienBibliotheque}">Accéder à ma bibliothèque de cartes</a></p>
            """;

        return (sujet, corps);
    }

    // 3e declencheur email (cf. prompt "Depot de preuves, points et forum", section C) :
    // objectif explicite d'inciter au retour regulier sur la plateforme - appel a l'action
    // direct, pas une simple information passive.
    public static (string Sujet, string CorpsHtml) PreuveValideeParLesPairs(
        string challengeTitre,
        string etapeTitre,
        string lienSuiviPreuve)
    {
        var sujet = $"{challengeTitre} — Ta preuve a été validée par tes pairs !";

        var corps = $"""
            <p>Bonne nouvelle,</p>
            <p>Ta preuve pour l'étape <strong>{WebUtility.HtmlEncode(etapeTitre)}</strong> du Challenge <strong>{WebUtility.HtmlEncode(challengeTitre)}</strong> vient d'être validée par tes pairs !</p>
            <p>Il ne reste plus que la validation finale de ton Coach pour la rendre définitive.</p>
            <p><a href="{lienSuiviPreuve}">Voir le détail sur "Suivi de ma preuve"</a></p>
            """;

        return (sujet, corps);
    }

    public static (string Sujet, string CorpsHtml) InvitationDefinirMotDePasse(string lienActivation)
    {
        const string sujet = "Bienvenue sur Challenges Factory — Définissez votre mot de passe";

        var corps = $"""
            <p>Bonjour,</p>
            <p>Un accès à Challenges Factory vient de vous être créé. Pour l'activer, définissez votre mot de passe :</p>
            <p><a href="{lienActivation}">Définir mon mot de passe</a></p>
            <p>Ce lien est valable 7 jours.</p>
            """;

        return (sujet, corps);
    }
}
