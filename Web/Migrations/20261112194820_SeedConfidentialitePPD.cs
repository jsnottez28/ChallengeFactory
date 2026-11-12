using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedConfidentialitePPD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remplace le texte de test ("Version rtgsf...") publie a la main depuis
            // /Administration/DocumentsLegaux par le vrai contenu de la politique de
            // confidentialite. Meme mecanisme transactionnel que
            // DocumentLegalService.CreerEtPublierAsync : on depublie l'ancienne version
            // PPD (Type = 1) avant d'inserer la nouvelle, publiee. Consequence attendue :
            // toute session ouverte apres la DateEffective ci-dessous devra (re)accepter
            // cette version (cf. IDocumentLegalService.GetTypesBloquantsAsync).
            migrationBuilder.Sql("""
                UPDATE [DocumentsLegaux] SET [EstPublie] = 0 WHERE [Type] = 1 AND [EstPublie] = 1;
                """);

            migrationBuilder.InsertData(
                table: "DocumentsLegaux",
                columns: ["Type", "Version", "Contenu", "DateEffective", "EstPublie", "CreeLe"],
                values: new object[] { 1, "1.0", ContenuPolitiqueConfidentialite, new System.DateTime(2026, 11, 12), true, System.DateTime.UtcNow });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DocumentsLegaux",
                keyColumns: ["Type", "Version"],
                keyValues: new object[] { 1, "1.0" });

            // Republie la version PPD la plus recente parmi celles depubliees par Up()
            // (en pratique, l'unique version de test existante avant cette migration).
            migrationBuilder.Sql("""
                UPDATE [DocumentsLegaux]
                SET [EstPublie] = 1
                WHERE [Id] = (
                    SELECT TOP 1 [Id] FROM [DocumentsLegaux]
                    WHERE [Type] = 1 AND [EstPublie] = 0
                    ORDER BY [DateEffective] DESC
                );
                """);
        }

        private const string ContenuPolitiqueConfidentialite = """
            En vigueur au 12 novembre 2026.

            ## Qui traite vos données

            Le responsable de traitement est AJD-&CO, SAS au capital de 4 500 €, SIRET 839 480 316 00018, 20 rue Georges Fessard, 28190 Courville-sur-Eure, qui exploite la marque Challenges Factory. Pour toute question sur vos données : [jsnottez@challenges-factory.com](mailto:jsnottez@challenges-factory.com).

            ## Quelles données, pourquoi, sur quelle base

            <div class="table-responsive">
            <table class="table table-bordered table-sm">
            <thead>
            <tr>
            <th>Traitement</th>
            <th>Données</th>
            <th>Finalité</th>
            <th>Base légale</th>
            <th>Durée de conservation</th>
            </tr>
            </thead>
            <tbody>
            <tr>
            <td>Newsletter</td>
            <td>Nom, prénom, e-mail professionnel, entreprise, fonction</td>
            <td>Envoyer tous les 15 jours nos actualités et articles</td>
            <td>Intérêt légitime (communication B2B liée à votre activité professionnelle)</td>
            <td>Jusqu'à votre désinscription, puis 3 ans sans échange de votre part. Après désinscription, seule votre adresse est gardée 3 ans pour respecter votre opposition</td>
            </tr>
            <tr>
            <td>Prospection et relation commerciale</td>
            <td>Nom, prénom, fonction, entreprise, profil et publications LinkedIn publics, échanges avec nous</td>
            <td>Vous proposer nos services (Bilan Carbone®, accompagnement, formations) et suivre nos échanges</td>
            <td>Intérêt légitime (prospection B2B)</td>
            <td>3 ans après le dernier contact émanant de vous</td>
            </tr>
            <tr>
            <td>Formulaire de contact</td>
            <td>Nom, e-mail, message</td>
            <td>Répondre à votre demande</td>
            <td>Intérêt légitime (réponse à une sollicitation)</td>
            <td>3 ans après le dernier échange</td>
            </tr>
            <tr>
            <td>Clients et stagiaires</td>
            <td>Identité, coordonnées, données de facturation et de formation</td>
            <td>Exécuter la prestation, facturer, respecter nos obligations (Qualiopi, comptabilité)</td>
            <td>Exécution du contrat et obligation légale</td>
            <td>Durée de la relation, puis 10 ans pour les pièces comptables</td>
            </tr>
            </tbody>
            </table>
            </div>

            Nous ne vendons ni ne louons vos données. Nous n'utilisons pas de pixel de suivi d'ouverture dans nos e-mails.

            ## D'où viennent les données de prospection

            Quand nous ne les tenons pas de vous, elles proviennent de vos profils et publications professionnels publics sur LinkedIn (recherches LinkedIn Sales Navigator). Un outil d'intelligence artificielle nous aide à évaluer l'intérêt d'un contact et à rédiger un premier message. Aucune décision ayant un effet juridique sur vous n'est prise de façon automatisée : chaque prise de contact reste une invitation que vous êtes libre d'ignorer.

            ## Qui y a accès

            Uniquement l'équipe Challenges Factory et nos prestataires techniques, qui agissent sur nos instructions :

            - Google (Gmail, Google Sheets, Apps Script, API Gemini) : envoi des e-mails, tableau de suivi, aide à la rédaction ;
            - PhantomBuster et Apify : automatisation de LinkedIn et lecture des publications publiques ;
            - LinkedIn (Sales Navigator) : recherche et messagerie ;
            - Cloudflare : accès sécurisé au site ;
            - notre CRM, hébergé sur nos propres serveurs en France.

            ## Transferts hors de l'Union européenne

            Certains prestataires (notamment Google et Cloudflare) peuvent traiter des données aux États-Unis. Ces transferts reposent sur le cadre de protection des données UE–États-Unis (Data Privacy Framework) ou sur les clauses contractuelles types de la Commission européenne.

            ## Vos droits

            Vous pouvez accéder à vos données, les faire rectifier ou effacer, limiter leur traitement, vous opposer à leur traitement, et notamment à toute prospection, à tout moment et sans justification. Pour la newsletter, le lien « Se désinscrire » présent dans chaque envoi suffit. Pour le reste, écrivez à [jsnottez@challenges-factory.com](mailto:jsnottez@challenges-factory.com) : nous répondons sous un mois.

            Si vous estimez que vos droits ne sont pas respectés, vous pouvez adresser une réclamation à la CNIL (lien : [https://www.cnil.fr/fr/plaintes](https://www.cnil.fr/fr/plaintes)).

            ## Sécurité

            Nos outils sont protégés par des accès individuels et des connexions chiffrées. Seules les personnes qui en ont besoin accèdent aux données.
            """;
    }
}
