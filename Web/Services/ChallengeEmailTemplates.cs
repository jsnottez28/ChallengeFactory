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

    // Email de lancement (etape 1), distinct de NouvelleEtape : explique le principe du
    // Challenge-Based Learning et le fonctionnement concret de la plateforme avant de
    // presenter la premiere etape - un stagiaire qui arrive n'a jamais utilise l'outil,
    // contrairement aux etapes suivantes ou il connait deja le fonctionnement.
    public static (string Sujet, string CorpsHtml) LancementParcours(
        string challengeTitre,
        string etapeTitre,
        List<string> carteTitres,
        string lienMonParcours)
    {
        var sujet = $"{challengeTitre} — Votre Challenge démarre, bienvenue !";

        var listeCartes = carteTitres.Count > 0
            ? "<ul>" + string.Join("", carteTitres.Select(titre => $"<li>{WebUtility.HtmlEncode(titre)}</li>")) + "</ul>"
            : "";

        var corps = $"""
            <p>Bonjour,</p>
            <p>Bienvenue sur <strong>{WebUtility.HtmlEncode(challengeTitre)}</strong> ! Voici comment ça va se passer.</p>

            <p><strong>Le principe :</strong> vous n'allez pas suivre une formation passive. Chaque semaine, vous
            recevez un petit nombre de <em>Cartes de Compétences</em> — des ressources courtes et concrètes — et un
            <em>défi individuel</em> à réaliser directement sur le terrain, dans votre quotidien professionnel.</p>

            <p><strong>Comment ça avance :</strong> après avoir relevé votre défi, vous déposez une <em>preuve</em>
            (photo, vidéo, texte ou capture d'écran) sur la plateforme. Cette preuve est ensuite validée par vos
            pairs de la cohorte ou par un Tuteur/Coach — jamais uniquement par une machine. Une fois l'étape
            validée pour l'ensemble de la cohorte, l'étape suivante s'ouvre avec de nouvelles cartes.</p>

            <p><strong>La cohorte :</strong> vous progressez avec un groupe, pas seul. L'entraide entre pairs
            (relire, valider, encourager) fait partie intégrante du parcours.</p>

            <p>Votre première étape est ouverte dès maintenant : <strong>{WebUtility.HtmlEncode(etapeTitre)}</strong>.</p>
            {listeCartes}
            <p><a href="{lienMonParcours}">Accéder à mon parcours</a></p>
            """;

        return (sujet, corps);
    }

    // Test de connaissances amont/aval (Methode Miroir, cf. CLAUDE.md "Mesure d'impact &
    // KPI") : auto-evaluation par etape du Challenge (son objectif pedagogique), jamais un
    // QCM note - cf. TypeTestPositionnement.
    public static (string Sujet, string CorpsHtml) DemandeTestPositionnement(
        string challengeTitre,
        Domain.Entities.TypeTestPositionnement type,
        string lien)
    {
        var estAmont = type == Domain.Entities.TypeTestPositionnement.Amont;

        var sujet = estAmont
            ? $"{challengeTitre} — Test de connaissances avant de démarrer"
            : $"{challengeTitre} — Test de connaissances de fin de parcours";

        var intro = estAmont
            ? "Avant de démarrer votre Challenge, merci de faire le point sur votre niveau actuel de connaissances."
            : "Vous arrivez au terme de votre Challenge : merci de refaire le point sur vos connaissances, pour mesurer votre progression depuis le début.";

        var corps = $"""
            <p>Bonjour,</p>
            <p>{intro}</p>
            <p>Ce test rapide (quelques minutes) évalue votre niveau sur l'objectif pédagogique de chaque étape du
            parcours <strong>{WebUtility.HtmlEncode(challengeTitre)}</strong> — il n'y a pas de bonne ou de mauvaise
            réponse, c'est un point de départ pour mesurer votre progression.</p>
            <p><a href="{lien}">Répondre au test</a></p>
            """;

        return (sujet, corps);
    }

    public static (string Sujet, string CorpsHtml) Cloture(string challengeTitre, string lienBibliotheque, string lienAttestation)
    {
        var sujet = $"{challengeTitre} — Challenge terminé, félicitations !";

        var corps = $"""
            <p>Bonjour,</p>
            <p>Félicitations, vous avez terminé le Challenge <strong>{WebUtility.HtmlEncode(challengeTitre)}</strong> !</p>
            <p>Toutes les cartes de compétences que vous avez débloquées restent accessibles dans votre bibliothèque personnelle.</p>
            <p><a href="{lienBibliotheque}">Accéder à ma bibliothèque de cartes</a></p>
            <p><a href="{lienAttestation}">Voir et imprimer mon attestation de fin de parcours</a></p>
            """;

        return (sujet, corps);
    }

    // 5e declencheur email : enquete de satisfaction a la cloture (critere 7 du
    // Referentiel National Qualite Qualiopi - recueil des appreciations des
    // beneficiaires). Envoyee une seule fois, en meme temps que l'email de cloture.
    public static (string Sujet, string CorpsHtml) DemandeSatisfaction(string challengeTitre, string lienSatisfaction)
    {
        var sujet = $"{challengeTitre} — Votre avis compte";

        var corps = $"""
            <p>Bonjour,</p>
            <p>Vous venez de terminer le Challenge <strong>{WebUtility.HtmlEncode(challengeTitre)}</strong>. Avant de continuer, une dernière chose : votre avis nous aide à améliorer le parcours pour les prochaines Cohortes.</p>
            <p><a href="{lienSatisfaction}">Répondre en 1 minute</a></p>
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

    // 6e declencheur email : Rituel Synchrone (visio) de l'etape (cf. CLAUDE.md, "Boucle CBL
    // hebdomadaire" - Temps 3, Action). Le lien est externe (Zoom/Teams/Meet...), saisi par
    // le Gestionnaire - la plateforme n'heberge jamais elle-meme de visio.
    public static (string Sujet, string CorpsHtml) LienVisio(
        string challengeTitre,
        string etapeTitre,
        DateTime? dateVisio,
        string lienVisio)
    {
        var sujet = $"{challengeTitre} — Lien de connexion pour la visio de l'étape";

        var dateHtml = dateVisio is not null
            ? $"<p>Rendez-vous le <strong>{dateVisio.Value.ToLocalTime():dd/MM/yyyy à HH:mm}</strong>.</p>"
            : "";

        var corps = $"""
            <p>Bonjour,</p>
            <p>Une visio est proposée pour l'étape <strong>{WebUtility.HtmlEncode(etapeTitre)}</strong> du Challenge <strong>{WebUtility.HtmlEncode(challengeTitre)}</strong>.</p>
            {dateHtml}
            <p><a href="{lienVisio}">Rejoindre la visio</a></p>
            """;

        return (sujet, corps);
    }

    // 7e declencheur email : demande d'emargement numerique (suivi de l'execution, exigence
    // Qualiopi) - une carte de l'etape = une ligne a signer, cf. IEmargementService.
    public static (string Sujet, string CorpsHtml) DemandeEmargement(
        string challengeTitre,
        string etapeTitre,
        List<string> carteTitres,
        string lienEmargement)
    {
        var sujet = $"{challengeTitre} — Merci de signer votre émargement";

        var listeCartes = carteTitres.Count > 0
            ? "<ul>" + string.Join("", carteTitres.Select(titre => $"<li>{WebUtility.HtmlEncode(titre)}</li>")) + "</ul>"
            : "";

        var corps = $"""
            <p>Bonjour,</p>
            <p>Merci de confirmer votre participation à la séance <strong>{WebUtility.HtmlEncode(etapeTitre)}</strong> du Challenge <strong>{WebUtility.HtmlEncode(challengeTitre)}</strong> et l'acquisition des cartes suivantes :</p>
            {listeCartes}
            <p><a href="{lienEmargement}">Signer mon émargement</a></p>
            """;

        return (sujet, corps);
    }

    // 8e declencheur email : point d'etape a mi-parcours, envoye automatiquement des que la
    // Cohorte atteint son etape mediane (cf. CohorteService.
    // EnvoyerQuestionnaireMiParcoursSiEtapeMedianeAsync).
    public static (string Sujet, string CorpsHtml) DemandeQuestionnaireMiParcours(string challengeTitre, string lienQuestionnaire)
    {
        var sujet = $"{challengeTitre} — Petit point d'étape à mi-parcours";

        var corps = $"""
            <p>Bonjour,</p>
            <p>Vous êtes à mi-parcours du Challenge <strong>{WebUtility.HtmlEncode(challengeTitre)}</strong>. Un petit point s'impose : quelques minutes pour faire le bilan de cette première moitié et ajuster la suite si besoin.</p>
            <p><a href="{lienQuestionnaire}">Répondre en 2 minutes</a></p>
            """;

        return (sujet, corps);
    }

    // Test psychotechnique DISC : envoye automatiquement des que le stagiaire termine le
    // test (cf. DiscService.RepondreAsync) - jamais de lien de reponse ici, le test est deja
    // termine, seul le resultat est communique.
    public static (string Sujet, string CorpsHtml) ResultatDisc(Application.Common.Interfaces.DiscResultatInfo resultat)
    {
        const string sujet = "Votre profil DISC";

        var traits = string.Join(", ", resultat.ProfilDominantTraits);

        var corps = $"""
            <p>Bonjour,</p>
            <p>Merci d'avoir complété le test DISC. Voici votre résultat :</p>
            <p><strong>Profil dominant : {WebUtility.HtmlEncode(resultat.ProfilDominant)} – {WebUtility.HtmlEncode(resultat.ProfilDominantNom)}</strong></p>
            <p>{WebUtility.HtmlEncode(resultat.ProfilDominantDescription)}</p>
            <p><em>{WebUtility.HtmlEncode(traits)}</em></p>
            <p>Détail de vos scores (sur 25 points chacun) :</p>
            <ul>
                <li>D – Dominant : {resultat.ScoreD}</li>
                <li>I – Influent : {resultat.ScoreI}</li>
                <li>S – Stable : {resultat.ScoreS}</li>
                <li>C – Consciencieux : {resultat.ScoreC}</li>
            </ul>
            """;

        return (sujet, corps);
    }

    // Test psychotechnique RIASEC : envoye automatiquement des que le stagiaire termine le
    // test (cf. RiasecService.RepondreAsync).
    public static (string Sujet, string CorpsHtml) ResultatRiasec(Application.Common.Interfaces.RiasecResultatInfo resultat)
    {
        const string sujet = "Votre profil RIASEC";

        var topTrois = resultat.Dimensions
            .OrderByDescending(d => d.Score)
            .Take(3)
            .ToList();

        var listeDimensions = string.Join("", resultat.Dimensions.Select(d =>
            $"<li>{WebUtility.HtmlEncode(d.Code)} – {WebUtility.HtmlEncode(d.Nom)} : {d.Score}/10</li>"));

        var listeTopTrois = string.Join("", topTrois.Select(d =>
            $"<p><strong>{WebUtility.HtmlEncode(d.Code)} – {WebUtility.HtmlEncode(d.Nom)}</strong><br>{WebUtility.HtmlEncode(d.Description)}</p>"));

        var corps = $"""
            <p>Bonjour,</p>
            <p>Merci d'avoir complété le test RIASEC. Voici votre résultat :</p>
            <p><strong>Code Holland : {WebUtility.HtmlEncode(resultat.CodeHolland)}</strong> (vos 3 dimensions dominantes)</p>
            {listeTopTrois}
            <p>Détail de vos scores (sur 10 points chacun) :</p>
            <ul>
                {listeDimensions}
            </ul>
            <p style="color:#888780; font-size:12px;">Test basé sur l'O*NET Interest Profiler Short Form, U.S. Department of Labor — National Center for O*NET Development, sous licence Creative Commons Attribution 4.0.</p>
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
