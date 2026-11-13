using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedCguReelles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Meme mecanisme que SeedConfidentialitePPD : remplace le texte de test
            // ("Version rtgsf...") publie a la main depuis
            // /Administration/DocumentsLegaux par les vraies CGU (Type = 0). Consequence
            // attendue : toute session ouverte apres la DateEffective ci-dessous devra
            // (re)accepter cette version (cf. IDocumentLegalService.GetTypesBloquantsAsync).
            migrationBuilder.Sql("""
                UPDATE [DocumentsLegaux] SET [EstPublie] = 0 WHERE [Type] = 0 AND [EstPublie] = 1;
                """);

            migrationBuilder.InsertData(
                table: "DocumentsLegaux",
                columns: ["Type", "Version", "Contenu", "DateEffective", "EstPublie", "CreeLe"],
                values: new object[] { 0, "1.0", ContenuCgu, new System.DateTime(2026, 11, 13), true, System.DateTime.UtcNow });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DocumentsLegaux",
                keyColumns: ["Type", "Version"],
                keyValues: new object[] { 0, "1.0" });

            // Republie la version CGU la plus recente parmi celles depubliees par Up()
            // (en pratique, l'unique version de test existante avant cette migration).
            migrationBuilder.Sql("""
                UPDATE [DocumentsLegaux]
                SET [EstPublie] = 1
                WHERE [Id] = (
                    SELECT TOP 1 [Id] FROM [DocumentsLegaux]
                    WHERE [Type] = 0 AND [EstPublie] = 0
                    ORDER BY [DateEffective] DESC
                );
                """);
        }

        private const string ContenuCgu = """
            En vigueur au 13 novembre 2026.

            ## 1. Objet

            Les présentes conditions générales d'utilisation (« CGU ») régissent l'accès et l'utilisation du site challenges-factory.com et de la plateforme associée (ci-après « la Plateforme »), édités par AJD-&CO (voir nos [mentions légales](/mentions-legales)). Toute création de compte ou utilisation de la Plateforme implique l'acceptation pleine et entière des présentes CGU.

            ## 2. Description des services

            AJD-&CO, sous la marque Challenges Factory, propose :

            - des parcours de formation fondés sur la pédagogie par le défi (Challenge-Based Learning) : Cartes de Compétences, Défis individuels et collectifs, Cohortes, validation des preuves d'action par les pairs, Tuteurs Master ou Coachs ;
            - des prestations de Bilan Carbone®, de bilan de compétences et d'accompagnement à la transition écologique et professionnelle.

            Challenges Factory est certifié Qualiopi et enregistré comme organisme de formation sous le numéro 24280193828 auprès du préfet de région Centre-Val de Loire (cet enregistrement ne vaut pas agrément de l'État).

            Selon l'offre souscrite (particulier ou entreprise), les fonctionnalités accessibles (catalogue, défis individuels, tableaux de bord) peuvent varier ; le détail est précisé lors de l'inscription ou dans le contrat correspondant.

            ## 3. Accès au site et création de compte

            L'accès à certaines pages (articles, pages de présentation) ne nécessite pas de compte. L'accès à un parcours ou à une Cohorte nécessite la création d'un compte personnel, nominatif et non cessible.

            L'utilisateur garantit l'exactitude des informations fournies lors de l'inscription et s'engage à les maintenir à jour. Il est responsable de la confidentialité de ses identifiants et de toute activité réalisée depuis son compte.

            Pour un compte souscrit dans le cadre d'une entreprise (mode BtoB), l'accès et les droits de l'utilisateur sont déterminés par le gestionnaire/administrateur de l'organisation concernée.

            ## 4. Obligations de l'utilisateur

            En utilisant la Plateforme, l'utilisateur s'engage à :

            - ne déposer, dans le cadre des preuves d'action ou des échanges entre pairs, aucun contenu illicite, diffamatoire, discriminatoire ou portant atteinte aux droits d'un tiers ;
            - utiliser les fonctionnalités de validation par les pairs de bonne foi ;
            - ne pas tenter de contourner les mécanismes de sécurité, d'authentification ou de progression de la Plateforme ;
            - respecter les droits de propriété intellectuelle attachés aux contenus de la Plateforme (voir [mentions légales](/mentions-legales)).

            Un contenu déposé par un utilisateur (preuve d'action, commentaire, message) reste sa propriété ; il concède à AJD-&CO le droit de l'héberger, de l'afficher aux membres autorisés de sa Cohorte et, le cas échéant avec son accord explicite, de le valoriser à titre d'exemple (« Pépite ») dans le cadre de la capitalisation pédagogique.

            ## 5. Disponibilité et évolution du service

            AJD-&CO met en œuvre les moyens raisonnables pour assurer l'accessibilité de la Plateforme, sans garantie de continuité absolue (maintenance, incident technique, cas de force majeure). Les fonctionnalités, l'interface et le contenu pédagogique peuvent évoluer ; l'utilisateur en est informé par les moyens usuels (notification sur la Plateforme, e-mail).

            ## 6. Responsabilité

            AJD-&CO s'engage à mettre en œuvre les moyens nécessaires à la bonne exécution des prestations commandées (formation, Bilan Carbone®, accompagnement), sans garantie de résultat au-delà des engagements contractuels spécifiques éventuellement convenus (ex. : contrat B2B).

            AJD-&CO ne peut être tenu responsable des dommages indirects, ni des conséquences d'une utilisation non conforme de la Plateforme, d'une interruption de service indépendante de sa volonté, ou de contenus déposés par un autre utilisateur.

            ## 7. Protection des données personnelles

            Les données personnelles des utilisateurs sont traitées conformément à notre [politique de confidentialité](/confidentialite).

            ## 8. Durée, suspension et résiliation

            Les présentes CGU s'appliquent pendant toute la durée d'utilisation de la Plateforme. AJD-&CO peut suspendre ou résilier l'accès d'un compte en cas de manquement grave aux présentes CGU, après information de l'utilisateur sauf urgence (contenu illicite, atteinte à la sécurité du service).

            L'utilisateur peut demander la clôture de son compte à tout moment en écrivant à [jsnottez@challenges-factory.com](mailto:jsnottez@challenges-factory.com).

            ## 9. Modification des CGU

            AJD-&CO peut modifier les présentes CGU, notamment pour tenir compte d'évolutions légales ou de la Plateforme. Toute nouvelle version entre en vigueur à sa date de publication ; les utilisateurs concernés sont invités à en prendre connaissance et, le cas échéant, à l'accepter lors de leur prochaine connexion.

            ## 10. Droit applicable et litiges

            Les présentes CGU sont soumises au droit français. En cas de litige, et à défaut de résolution amiable, les tribunaux français compétents seront seuls saisis.

            ## 11. Contact

            Pour toute question relative aux présentes CGU : [jsnottez@challenges-factory.com](mailto:jsnottez@challenges-factory.com).
            """;
    }
}
