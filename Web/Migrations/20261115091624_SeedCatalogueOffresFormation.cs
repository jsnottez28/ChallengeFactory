using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedCatalogueOffresFormation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var colonnes = new[] { "Titre", "Categorie", "LienAvecOffre", "Duree", "Public", "Financement", "Prix", "DisponibleEnChallenge", "DisponibleEnPresentiel", "Cible", "Statut", "LienPersonnalise", "Ordre", "EstPublie", "CreeLe" };

            // --- Sensibiliser ---
            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Atelier Fresque du Climat", (int)Domain.Entities.CategorieOffreFormation.Sensibiliser, "Déjà animé par Challenges Factory", "3 h", "Tous salariés", "OPCO", "Sur devis", false, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.Disponible, null, 1, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Fresque du Numérique", (int)Domain.Entities.CategorieOffreFormation.Sensibiliser, "Deuxième session après la Fresque du Climat", "3 h", "Équipes informatiques, bureau", "OPCO", "Sur devis", false, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 2, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Atelier 2tonnes", (int)Domain.Entities.CategorieOffreFormation.Sensibiliser, "Passer du constat aux actions individuelles", "3 h", "Tous salariés", "OPCO", "Sur devis", false, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 3, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Matinale dirigeants : Bilan Carbone, BEGES, donneurs d'ordres et SNBC 3", (int)Domain.Entities.CategorieOffreFormation.Sensibiliser, "Prospection vers le pré-diagnostic", "1 h 30", "Dirigeants PME et ETI", "OPCO", "Sur devis", false, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 4, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Atelier « Low-tech : défi d'équipe »", (int)Domain.Entities.CategorieOffreFormation.Sensibiliser, "Inspiré du GRAINE, format Challenge", "1 jour", "Équipes, CSE", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 5, true, new System.DateTime(2026, 11, 15) });

            // --- MonterEnCompetenceCarbone ---
            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Référent carbone / climat en entreprise", (int)Domain.Entities.CategorieOffreFormation.MonterEnCompetenceCarbone, "Répond au « plan sans pilote »", "3 jours", "Futur pilote du plan de transition", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 1, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Comprendre et lire un Bilan Carbone®", (int)Domain.Entities.CategorieOffreFormation.MonterEnCompetenceCarbone, "Après la remise du bilan", "1 jour", "Dirigeants, finance, achats", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 2, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Collecter les données carbone", (int)Domain.Entities.CategorieOffreFormation.MonterEnCompetenceCarbone, "Formalise l'étape « Mobiliser »", "1 jour", "Correspondants par service", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 3, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Répondre aux demandes carbone des donneurs d'ordres et aux appels d'offres", (int)Domain.Entities.CategorieOffreFormation.MonterEnCompetenceCarbone, "Cible les PME et ETI sous-traitantes", "1 jour", "Commerciaux, qualité, dirigeants de sous-traitants", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 4, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Achats responsables et scope 3 fournisseurs", (int)Domain.Entities.CategorieOffreFormation.MonterEnCompetenceCarbone, "Premier poste d'émission des PME", "1 jour", "Acheteurs", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 5, true, new System.DateTime(2026, 11, 15) });

            // --- PiloteTrajectoireSnbc ---
            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Tronc commun : de la SNBC 3 à la trajectoire de l'entreprise", (int)Domain.Entities.CategorieOffreFormation.PiloteTrajectoireSnbc, "Budgets carbone, objectifs 2028-2033-2038, indicateurs d'activité reliés aux tCO2e, rituels de suivi", "1 jour", "Référents, responsables d'actions", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 1, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Secteur Transport & logistique", (int)Domain.Entities.CategorieOffreFormation.PiloteTrajectoireSnbc, "Premier secteur émetteur (34 %) : électrification, report modal, optimisation des tournées", "1 jour", "Transporteurs, PME avec flotte", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 2, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Secteur Bâtiment & BTP", (int)Domain.Entities.CategorieOffreFormation.PiloteTrajectoireSnbc, "De 57 à 3 MtCO2e visés en 2050 : pompes à chaleur, rénovation, matériaux bas-carbone", "1 jour", "BTP, PME propriétaires de locaux", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 3, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Secteur Industrie & PME manufacturières", (int)Domain.Entities.CategorieOffreFormation.PiloteTrajectoireSnbc, "Électrification des procédés, efficacité énergétique, chaleur fatale", "1 jour", "Production, maintenance, achats", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 4, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Secteur Agriculture & agroalimentaire", (int)Domain.Entities.CategorieOffreFormation.PiloteTrajectoireSnbc, "Objectif 2030 maintenu à 67 MtCO2e ; haies et sols (lien GRAINE)", "1 jour", "Coopératives, IAA, négoce", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 5, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Secteur Déchets & économie circulaire", (int)Domain.Entities.CategorieOffreFormation.PiloteTrajectoireSnbc, "Prévention, tri, réemploi, écoconception", "1 jour", "Toutes PME", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 6, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Secteur Tertiaire, commerce & numérique", (int)Domain.Entities.CategorieOffreFormation.PiloteTrajectoireSnbc, "Énergie des locaux, déplacements, numérique, achats", "1 jour", "Services, ESN, distribution", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 7, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Suivi terrain accompagné", (int)Domain.Entities.CategorieOffreFormation.PiloteTrajectoireSnbc, "Revue du tableau de bord, écarts à la trajectoire, réajustement du plan", "6 classes virtuelles de 1 h 30 sur 6 mois", "Pilotes formés", "OPCO", "Sur devis", true, false, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 8, true, new System.DateTime(2026, 11, 15) });

            // --- ManagerEtEmbarquer ---
            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Manager la transition écologique de son équipe", (int)Domain.Entities.CategorieOffreFormation.ManagerEtEmbarquer, "Conduite du changement appliquée au climat", "2 jours", "Managers de proximité", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 1, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Accompagner les changements de comportements au travail", (int)Domain.Entities.CategorieOffreFormation.ManagerEtEmbarquer, "Inspiré du GRAINE ; leviers psychosociaux, freins, nudges", "2 jours", "Managers, RH, référents", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 2, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Ambassadeurs de la transition : convaincre sans moraliser", (int)Domain.Entities.CategorieOffreFormation.ManagerEtEmbarquer, "Compétence « embarquer ses collègues »", "1 jour", "Volontaires, membres du CSE", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 3, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Animer un collectif ou un atelier climat en interne", (int)Domain.Entities.CategorieOffreFormation.ManagerEtEmbarquer, "Facilitation, intelligence collective", "2 jours", "Référents, équipe RSE", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 4, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Devenir animateur Cap Bas Carbone", (int)Domain.Entities.CategorieOffreFormation.ManagerEtEmbarquer, "Licence du Challenge Cap Bas Carbone", "2 jours", "Animateurs internes d'ETI multisites, partenaires", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 5, true, new System.DateTime(2026, 11, 15) });

            // --- ThematiquesParPosteEmission ---
            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Énergie et sobriété des locaux", (int)Domain.Entities.CategorieOffreFormation.ThematiquesParPosteEmission, null, "½ jour", "Services généraux, tous salariés", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 1, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Mobilité des salariés et plan de mobilité employeur", (int)Domain.Entities.CategorieOffreFormation.ThematiquesParPosteEmission, null, "½ jour", "RH, services généraux", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 2, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Éco-conduite", (int)Domain.Entities.CategorieOffreFormation.ThematiquesParPosteEmission, null, "½ jour", "Conducteurs, commerciaux itinérants", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 3, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Fret et logistique bas-carbone", (int)Domain.Entities.CategorieOffreFormation.ThematiquesParPosteEmission, null, "½ jour", "Logistique, supply chain", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 4, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Numérique responsable", (int)Domain.Entities.CategorieOffreFormation.ThematiquesParPosteEmission, null, "½ jour", "Informatique, tous salariés", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 5, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Déchets et économie circulaire (poste d'émission)", (int)Domain.Entities.CategorieOffreFormation.ThematiquesParPosteEmission, null, "½ jour", "Production, services généraux", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 6, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Écoconception de l'offre", (int)Domain.Entities.CategorieOffreFormation.ThematiquesParPosteEmission, null, "1 jour", "Bureau d'études, marketing", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 7, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Alimentation bas carbone (restauration, événements, séminaires)", (int)Domain.Entities.CategorieOffreFormation.ThematiquesParPosteEmission, null, "½ jour", "RH, CSE, restauration collective", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 8, true, new System.DateTime(2026, 11, 15) });

            // --- IaEtTransition ---
            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "L'IA au service du bilan carbone et du reporting", (int)Domain.Entities.CategorieOffreFormation.IaEtTransition, null, "1 jour", "Référents carbone, RSE", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 1, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "IA frugale : utiliser l'IA générative en maîtrisant son empreinte", (int)Domain.Entities.CategorieOffreFormation.IaEtTransition, null, "½ jour", "Tous salariés", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 2, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Green IA : concevoir et déployer des projets d'IA sobres en entreprise", (int)Domain.Entities.CategorieOffreFormation.IaEtTransition, null, "1 jour", "DSI, chefs de projet, équipes informatiques", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 3, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Piloter son tableau de bord de trajectoire avec l'IA et le tableur", (int)Domain.Entities.CategorieOffreFormation.IaEtTransition, null, "1 jour", "Pilotes de trajectoire SNBC", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 4, true, new System.DateTime(2026, 11, 15) });

            // --- InspireesDuGraine ---
            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Conduire une démarche de transition écologique et sociale en PME", (int)Domain.Entities.CategorieOffreFormation.InspireesDuGraine, "Inspiré du module GRAINE « Accompagner une démarche de transition écologique et sociale »", "2 jours", "Dirigeants, RSE", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 1, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Évaluer l'impact de ses actions de transition", (int)Domain.Entities.CategorieOffreFormation.InspireesDuGraine, "Inspiré du module GRAINE « Savoir évaluer pour pérenniser ses projets »", "1 jour", "Référents, pilotes", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 2, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Communiquer sur ses engagements sans greenwashing", (int)Domain.Entities.CategorieOffreFormation.InspireesDuGraine, "Inspiré du module GRAINE « Rendre ses projets attractifs grâce à une communication efficace »", "1 jour", "Communication, marketing, dirigeants", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 3, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "S'adapter aux vagues de chaleur : santé et environnement au travail", (int)Domain.Entities.CategorieOffreFormation.InspireesDuGraine, "Inspiré du module GRAINE « Projet éducatif en santé-environnement »", "1 jour", "RH, QSE, CSE", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 4, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Biodiversité et renaturation des sites d'entreprise", (int)Domain.Entities.CategorieOffreFormation.InspireesDuGraine, "Inspiré du module GRAINE « Renaturation d'une cour d'école ou de crèche »", "1 jour", "Services généraux, RSE", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 5, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Haies, sols et stockage carbone", (int)Domain.Entities.CategorieOffreFormation.InspireesDuGraine, "Inspiré du module GRAINE « Bocages et haies, mieux comprendre pour agir collectivement »", "1 jour", "Exploitations agricoles, coopératives", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 6, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Eau : sobriété et pollutions dans l'entreprise", (int)Domain.Entities.CategorieOffreFormation.InspireesDuGraine, "Inspiré du module GRAINE « Les pollutions fantômes de l'eau »", "½ jour", "Production, QSE", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 7, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Former ses collègues à la transition (formateur interne)", (int)Domain.Entities.CategorieOffreFormation.InspireesDuGraine, "Inspiré du module GRAINE « Concevoir et animer une action de formation »", "2 jours", "Référents, ambassadeurs", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 8, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Facilitation graphique pour animer la transition", (int)Domain.Entities.CategorieOffreFormation.InspireesDuGraine, "Inspiré du module GRAINE « Facilitation graphique par le sketchnoting »", "1 jour", "Animateurs, managers", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 9, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Prendre la parole pour embarquer : posture et voix", (int)Domain.Entities.CategorieOffreFormation.InspireesDuGraine, "Inspiré du module GRAINE « Posture corporelle et vocale, animer avec plus d'aisance »", "1 jour", "Managers, ambassadeurs", "OPCO", "Sur devis", true, true, (int)Domain.Entities.ModePlateforme.BtoB, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 10, true, new System.DateTime(2026, 11, 15) });

            // --- ParticuliersEtReconversion ---
            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Bilan de compétences « métiers de la transition écologique »", (int)Domain.Entities.CategorieOffreFormation.ParticuliersEtReconversion, "Durée et prix à affiner avec le service administratif", "Jusqu'à 24 h", "Salariés, demandeurs d'emploi", "CPF", "Sur devis", true, false, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutOffreFormation.Disponible, "/bilan-de-competences", 1, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Bilan d'orientation jeunes vers les métiers verts", (int)Domain.Entities.CategorieOffreFormation.ParticuliersEtReconversion, "Partenariat à confirmer avec Orientaction", "Quelques séances", "Lycéens, étudiants", "Particuliers", "Sur devis", false, true, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 2, true, new System.DateTime(2026, 11, 15) });

            migrationBuilder.InsertData(
                table: "OffresFormation",
                columns: colonnes,
                values: new object[] { "Atelier « Découvrir les métiers de la transition »", (int)Domain.Entities.CategorieOffreFormation.ParticuliersEtReconversion, "Porte d'entrée vers le bilan de compétences", "2 h, en webinaire ou en collectif", "Personnes en questionnement", "Gratuit", "Sur devis", false, true, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutOffreFormation.SurDemande, null, 3, true, new System.DateTime(2026, 11, 15) });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [OffresFormation] WHERE [CreeLe] = '2026-11-15T00:00:00.0000000';");
        }
    }
}
