using System.Net;

namespace Web.Services;

// Templates HTML des emails du moteur de Challenges, isoles du code metier
// (ICohorteService) pour rester facilement modifiables par l'equipe contenu/marketing
// sans toucher a la logique d'envoi.
public static class ChallengeEmailTemplates
{
    // dateHeureVisio/lienConnexionVisio : la visio de l'etape est desormais planifiee
    // exactement au meme moment que le declenchement de cet email (prompt "Visio planifiee
    // par etape", section 1.5) - nullable uniquement en garde-fou defensif, la visio est en
    // pratique toujours deja creee a ce stade (planification rendue obligatoire cote
    // ICohorteService.LancerAsync/ValiderEtapeAsync).
    public static (string Sujet, string CorpsHtml) NouvelleEtape(
        string challengeTitre,
        string etapeTitre,
        List<string> carteTitres,
        string lienMonParcours,
        DateTime? dateHeureVisio,
        string? lienConnexionVisio)
    {
        var sujet = $"{challengeTitre} — Nouvelle étape disponible";

        var listeCartes = carteTitres.Count > 0
            ? "<ul>" + string.Join("", carteTitres.Select(titre => $"<li>{WebUtility.HtmlEncode(titre)}</li>")) + "</ul>"
            : "";

        var blocVisio = dateHeureVisio is not null && !string.IsNullOrWhiteSpace(lienConnexionVisio)
            ? $"""<p>Rendez-vous en visio le <strong>{dateHeureVisio:dd/MM/yyyy à HH:mm}</strong> : <a href="{lienConnexionVisio}">Rejoindre la visio</a></p>"""
            : "";

        var corps = $"""
            <p>Bonjour,</p>
            <p>Une nouvelle étape de votre Challenge <strong>{WebUtility.HtmlEncode(challengeTitre)}</strong> vient de s'ouvrir : <strong>{WebUtility.HtmlEncode(etapeTitre)}</strong>.</p>
            {listeCartes}
            {blocVisio}
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

    // 4e declencheur email (demande d'embarquement acceptee, prompt section H) : meme
    // esprit qu'ailleurs - annonce directe et appel a l'action, pas une simple information.
    public static (string Sujet, string CorpsHtml) EmbarquementValide(
        string challengeTitre,
        string cohorteNom,
        DateTime dateLancement,
        string lienFormations)
    {
        var sujet = $"{challengeTitre} — Ta session est confirmée !";

        var corps = $"""
            <p>Bonne nouvelle,</p>
            <p>Ta demande d'embarquement pour le Challenge <strong>{WebUtility.HtmlEncode(challengeTitre)}</strong> a été validée : la session <strong>{WebUtility.HtmlEncode(cohorteNom)}</strong> est confirmée, avec un lancement prévu le <strong>{dateLancement:dd/MM/yyyy}</strong>.</p>
            <p>Tu es déjà inscrit·e : tu seras prévenu·e dès le lancement de la première étape.</p>
            <p><a href="{lienFormations}">Voir le Challenge</a></p>
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
