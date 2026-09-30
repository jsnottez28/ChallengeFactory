using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedFacteursEmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ratios ADEME Base Carbone (donnee publique, licence ouverte), extraits du
            // fichier Bilan Carbone(R) V9.0 de l'Association Bilan Carbone (onglets FE
            // Intrants / FE Immobilisations / FE Energie / FE Deplacements). Les valeurs
            // sources en kgCO2e/k-euro sont divisees par 1000 ici (stockees en
            // kgCO2e/euro) pour que le formulaire ne demande jamais qu'un montant en euros
            // - cf. Domain.Entities.TypeDonneeActivite. Insertion idempotente par Code (pas
            // de HasData), meme principe que les autres migrations de seed de ce projet.
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'ACHAT_MARCHANDISES')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'ACHAT_MARCHANDISES', 0, 0, N'Achats de matières premières et marchandises', N'€', 0.146, N'Commerce de gros, à l''exclusion des automobiles et des motocycles - 2023, France continentale, Base Carbone', 1, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'ACHAT_SOUSTRAITANCE')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'ACHAT_SOUSTRAITANCE', 0, 0, N'Sous-traitance et prestations spécialisées', N'€', 0.110, N'Autres services spécialisés, scientifiques et techniques - 2023, France continentale, Base Carbone', 2, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'ACHAT_IT')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'ACHAT_IT', 0, 0, N'Informatique, logiciels et hébergement', N'€', 0.075, N'Programmation, conseil IT / Services d''information - 2023, France continentale, Base Carbone', 3, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'ACHAT_TELECOM')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'ACHAT_TELECOM', 0, 0, N'Télécommunications', N'€', 0.136, N'Services de télécommunications - 2023, France continentale, Base Carbone', 4, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'ACHAT_MARKETING')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'ACHAT_MARKETING', 0, 0, N'Publicité, marketing et études', N'€', 0.113, N'Services de publicité et d''études de marché - 2023, France continentale, Base Carbone', 5, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'ACHAT_JURIDIQUE')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'ACHAT_JURIDIQUE', 0, 0, N'Conseil juridique, comptable et gestion', N'€', 0.067, N'Services juridiques et comptables/ services des sièges sociaux/ conseil de gestion - 2023, France continentale, Base Carbone', 6, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'ACHAT_ASSURANCE')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'ACHAT_ASSURANCE', 0, 0, N'Assurances', N'€', 0.077, N'Assurance, réassurance, retraites (hors sécurité sociale) - 2023, France continentale, Base Carbone', 7, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'ACHAT_TRANSPORT_MARCHANDISES')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'ACHAT_TRANSPORT_MARCHANDISES', 0, 0, N'Transport et logistique de marchandises', N'€', 0.319, N'Transports terrestres et transports par conduites - 2023, France continentale, Base Carbone', 8, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'ACHAT_VOYAGES')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'ACHAT_VOYAGES', 0, 0, N'Voyages d''affaires (agences, réservations)', N'€', 0.136, N'Agences de voyage, voyagistes, réservations - 2023, France continentale, Base Carbone', 9, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'ACHAT_HEBERGEMENT')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'ACHAT_HEBERGEMENT', 0, 0, N'Restauration et hébergement', N'€', 0.250, N'Services d''hébergement et de restauration - 2023, France continentale, Base Carbone', 10, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'ACHAT_IMMOBILIER')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'ACHAT_IMMOBILIER', 0, 0, N'Location et services immobiliers', N'€', 0.021, N'Services immobiliers - 2023, France continentale, Base Carbone', 11, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'ACHAT_TRAVAUX')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'ACHAT_TRAVAUX', 0, 0, N'Travaux et construction', N'€', 0.245, N'Constructions et travaux de construction - 2023, France continentale, Base Carbone', 12, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'ACHAT_FOURNITURES')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'ACHAT_FOURNITURES', 0, 0, N'Fournitures et consommables de bureau', N'€', 0.917, N'Consommables bureautiques, France continentale, Base Carbone', 13, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'ACHAT_RH')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'ACHAT_RH', 0, 0, N'Recrutement, intérim et RH', N'€', 0.041, N'Services liés à l''emploi - 2023, France continentale, Base Carbone', 14, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'ACHAT_FORMATION')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'ACHAT_FORMATION', 0, 0, N'Formation', N'€', 0.066, N'Services de l''enseignement - 2023, France continentale, Base Carbone', 15, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'ACHAT_PRODUITS_DIVERS')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'ACHAT_PRODUITS_DIVERS', 0, 0, N'Achats de produits manufacturés divers', N'€', 0.231, N'Autres produits manufacturés - 2023, France continentale, Base Carbone', 16, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'ACHAT_AUTRES')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'ACHAT_AUTRES', 0, 0, N'Autres achats et services non classés', N'€', 0.157, N'Autres services personnels - 2023, France continentale, Base Carbone', 17, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'IMMO_MACHINES')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'IMMO_MACHINES', 1, 0, N'Machines et équipements', N'€', 0.273, N'Machines et équipements - 2023, France continentale, Base Carbone', 1, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'IMMO_INFORMATIQUE')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'IMMO_INFORMATIQUE', 1, 0, N'Matériel informatique et autres équipements', N'€', 0.231, N'Autres produits manufacturés - 2023, France continentale, Base Carbone', 2, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'IMMO_MOBILIER')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'IMMO_MOBILIER', 1, 0, N'Mobilier', N'€', 0.231, N'Meubles - 2023, France continentale, Base Carbone', 3, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'IMMO_BATIMENTS')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'IMMO_BATIMENTS', 1, 0, N'Bâtiments et travaux de construction', N'€', 0.245, N'Constructions et travaux de construction - 2023, France continentale, Base Carbone', 4, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'IMMO_VEHICULES')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'IMMO_VEHICULES', 1, 0, N'Véhicules', N'€', 0.239, N'Autres matériels de transport - 2023, France continentale, Base Carbone', 5, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'ENERGIE_ELEC')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'ENERGIE_ELEC', 2, 1, N'Électricité', N'kWh', 0.058, N'Électricité achetée, mix moyen France - 2023, France continentale, Base Carbone', 1, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'ENERGIE_GAZ')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'ENERGIE_GAZ', 2, 1, N'Gaz naturel', N'kWh PCI', 0.2392, N'Gaz naturel - 2022 (mix moyen consommation), France, Base Carbone', 2, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'DEPLACEMENT_DOMTRAV_VOITURE')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'DEPLACEMENT_DOMTRAV_VOITURE', 3, 1, N'Domicile-travail en voiture (km/an, tous salariés)', N'véhicule.km', 0.2311, N'Voiture - motorisation moyenne - 2018, France continentale, Base Carbone', 1, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'DEPLACEMENT_DOMTRAV_TRAIN')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'DEPLACEMENT_DOMTRAV_TRAIN', 3, 1, N'Domicile-travail en train (km/an, tous salariés)', N'passager.km', 0.03129, N'TER - 2018 - traction moyenne, France continentale, Base Carbone', 2, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'DEPLACEMENT_PRO_VOITURE')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'DEPLACEMENT_PRO_VOITURE', 3, 1, N'Déplacements professionnels en voiture (km/an)', N'véhicule.km', 0.2311, N'Voiture - motorisation moyenne - 2018, France continentale, Base Carbone', 3, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'DEPLACEMENT_PRO_TRAIN')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'DEPLACEMENT_PRO_TRAIN', 3, 1, N'Déplacements professionnels en train grande ligne (km/an)', N'passager.km', 0.00334, N'TGV - 2021, France continentale, Base Carbone', 4, 1);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [FacteursEmission] WHERE [Code] = N'DEPLACEMENT_PRO_AVION')
                INSERT INTO [FacteursEmission] ([Code], [Poste], [TypeDonnee], [Nom], [Unite], [ValeurKgCO2eParUnite], [Source], [Ordre], [Actif])
                VALUES (N'DEPLACEMENT_PRO_AVION', 3, 1, N'Déplacements professionnels en avion (km/an)', N'passager eq.km', 0.10268437, N'Avion passagers, moyen courrier, sans traînées, France continentale, Base Carbone', 5, 1);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [FacteursEmission]
                WHERE [Code] LIKE N'ACHAT_%' OR [Code] LIKE N'IMMO_%' OR [Code] LIKE N'ENERGIE_%' OR [Code] LIKE N'DEPLACEMENT_%';
                """);
        }
    }
}
