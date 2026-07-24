namespace Infrastructure.ExternalServices.Email;

public static class EmailTemplates
{
    private static string BaseTemplate(string titre, string contenu) => $"""
        <!DOCTYPE html>
        <html lang="fr">
        <body style="margin:0;padding:0;background-color:#f4f4f4;font-family:Arial,sans-serif;">
            <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f4f4f4;padding:40px 0;">
                <tr>
                    <td align="center">
                        <table width="600" cellpadding="0" cellspacing="0" style="background-color:#ffffff;border-radius:8px;overflow:hidden;">
                            
                            <!-- Header -->
                            <tr>
                                <td style="background-color:#003189;padding:32px 40px;text-align:center;">
                                    <h1 style="color:#ffffff;margin:0;font-size:22px;">Challenge Factory</h1>
                                    <p style="color:#a8bce8;margin:6px 0 0;font-size:13px;">
                                        Plateforme d'apprentissage en ligne
                                    </p>
                                </td>
                            </tr>

                            <!-- Bande accent -->
                            <tr><td style="background-color:#00a8e0;height:4px;"></td></tr>

                            <!-- Contenu -->
                            <tr>
                                <td style="padding:40px;">
                                    {contenu}
                                </td>
                            </tr>

                            <!-- Footer -->
                            <tr>
                                <td style="background-color:#f8f8f8;padding:24px 40px;border-top:1px solid #eeeeee;">
                                    <p style="color:#999999;font-size:12px;margin:0;text-align:center;">
                                        Cet email a été envoyé automatiquement — merci de ne pas y répondre.<br>
                                        © {DateTime.Now.Year} Challenge Factory
                                    </p>
                                </td>
                            </tr>

                        </table>
                    </td>
                </tr>
            </table>
        </body>
        </html>
        """;

    public static string ConfirmationEmail(string nomUtilisateur, string lienConfirmation) =>
        BaseTemplate("Confirmez votre email", $"""
            <h2 style="color:#003189;font-size:20px;margin:0 0 16px;">
                Bienvenue sur Challenge Factory 👋
            </h2>
            <p style="color:#444444;font-size:15px;line-height:1.6;margin:0 0 24px;">
                Bonjour <strong>{nomUtilisateur}</strong>,<br><br>
                Merci de vous être inscrit sur Challenge Factory.
                Veuillez confirmer votre adresse email en cliquant sur le bouton ci-dessous.
            </p>
            <table cellpadding="0" cellspacing="0" style="margin:0 0 24px;">
                <tr>
                    <td style="background-color:#003189;border-radius:6px;padding:14px 32px;">
                        <a href="{lienConfirmation}" 
                           style="color:#ffffff;text-decoration:none;font-size:15px;font-weight:600;">
                            Confirmer mon adresse email
                        </a>
                    </td>
                </tr>
            </table>
            <p style="color:#888888;font-size:13px;line-height:1.6;margin:0;">
                Si le bouton ne fonctionne pas, copiez ce lien :<br>
                <a href="{lienConfirmation}" style="color:#00a8e0;word-break:break-all;">{lienConfirmation}</a>
            </p>
            <p style="color:#888888;font-size:13px;margin:16px 0 0;">
                Si vous n'êtes pas à l'origine de cette demande, ignorez cet email.
            </p>
            """);

    public static string ResetMotDePasse(string nomUtilisateur, string lienReset) =>
        BaseTemplate("Réinitialisation de mot de passe", $"""
            <h2 style="color:#003189;font-size:20px;margin:0 0 16px;">
                Réinitialisation de votre mot de passe
            </h2>
            <p style="color:#444444;font-size:15px;line-height:1.6;margin:0 0 24px;">
                Bonjour <strong>{nomUtilisateur}</strong>,<br><br>
                Nous avons reçu une demande de réinitialisation de mot de passe.
                Cliquez sur le bouton ci-dessous pour choisir un nouveau mot de passe.
            </p>
            <table cellpadding="0" cellspacing="0" style="margin:0 0 24px;">
                <tr>
                    <td style="background-color:#003189;border-radius:6px;padding:14px 32px;">
                        <a href="{lienReset}" 
                           style="color:#ffffff;text-decoration:none;font-size:15px;font-weight:600;">
                            Réinitialiser mon mot de passe
                        </a>
                    </td>
                </tr>
            </table>
            <p style="color:#888888;font-size:13px;line-height:1.6;margin:0;">
                Si le bouton ne fonctionne pas, copiez ce lien :<br>
                <a href="{lienReset}" style="color:#00a8e0;word-break:break-all;">{lienReset}</a>
            </p>
            <p style="color:#888888;font-size:13px;margin:16px 0 0;">
                Ce lien expirera dans <strong>24 heures</strong>.<br>
                Si vous n'avez pas demandé cette réinitialisation, ignorez cet email.
            </p>
            """);
}