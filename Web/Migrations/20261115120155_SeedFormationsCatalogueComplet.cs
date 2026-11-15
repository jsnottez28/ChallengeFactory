using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedFormationsCatalogueComplet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Les 46 formations restantes du catalogue (cf. CLAUDE.md, document "Catalogue
            // de formations Challenges Factory") completant le pilote deja publie
            // (SeedPiloteSnbcTransport). Memes principes : Challenge complet (au moins une
            // etape), vraies Cartes de Competences (mantra, pitch, defi, tips, punchline),
            // publie en BtoC ou BtoC selon la cible, pour beneficier tel quel du catalogue
            // /formations et du mecanisme existant de demande d'embarquement.
            migrationBuilder.InsertData(
                table: "Challenges",
                columns: ["Titre", "Slogan", "Description", "NombreEtapes", "Mode", "Statut", "Thematique", "Format", "SousFamille", "Duree", "PublicVise", "Financement", "Prix", "DisponibleEnPresentiel", "CreeLe"],
                values: new object[,]
                {
                { "Atelier Fresque du Climat + « premiers pas »", "Comprendre les causes du changement climatique, puis passer à l'action dès la sortie de l'atelier.", "<p>La Fresque du Climat est un atelier collaboratif qui fait comprendre, en 3 heures, les mécanismes du changement climatique à partir des données du GIEC. Chez Challenges Factory, on ne s'arrête pas au diagnostic : chaque équipe repart avec 3 idées d'actions concrètes, priorisées ensemble.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.Sensibiliser, "3 h", "Tous salariés", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Fresque du Numérique + défi numérique", "Mesurer l'empreinte du numérique dans son quotidien professionnel, puis réduire celle de son équipe.", "<p>Deuxième session après la Fresque du Climat : la Fresque du Numérique fait découvrir l'impact environnemental du numérique (fabrication des terminaux, data centers, réseaux). L'équipe en sort avec un premier défi numérique à tester.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.Sensibiliser, "3 h", "Équipes informatiques, bureau", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Atelier 2tonnes", "Repartir de l'atelier avec un engagement individuel et un engagement collectif, pas juste des chiffres.", "<p>L'atelier 2tonnes simule les choix de politiques climatiques à l'échelle d'un pays pour faire ressentir l'ampleur de l'effort nécessaire. Chaque participant en retire un engagement individuel ; l'équipe, un engagement collectif.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.Sensibiliser, "3 h", "Tous salariés", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Matinale dirigeants : où en est votre entreprise face à la SNBC 3 et à vos donneurs d'ordres ?", "Un auto-diagnostic pour savoir si votre entreprise est prête face aux exigences carbone qui montent.", "<p>Une matinale courte et directe pour les dirigeants de PME et ETI : où en êtes-vous par rapport à la SNBC 3 et aux exigences carbone de vos donneurs d'ordres ? Vous repartez avec un auto-diagnostic rempli et, si besoin, un rendez-vous de pré-diagnostic.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.Sensibiliser, "1 h 30", "Dirigeants PME et ETI", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Défi Low-tech d'équipe", "Concevoir et tester un vrai prototype low-tech sur le site, en équipe, en une journée.", "<p>Inspiré du réseau GRAINE, ce format challenge fait concevoir et tester un prototype low-tech (sobre, réparable, accessible) directement sur le site de l'entreprise, en une journée.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.Sensibiliser, "1 jour", "Équipes, CSE", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Pilote climat : porter le plan de transition dans l'entreprise", "Prendre en main le plan de transition de son entreprise et lancer 3 actions avant la fin du parcours.", "<p>Ce parcours forme la personne qui va porter le plan de transition carbone de l'entreprise au quotidien. Réparti en 3 sessions sur 2 mois, il alterne apports méthodologiques et mise en œuvre réelle entre les sessions.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.DuBilanALAction, "3 jours (en 3 fois sur 2 mois)", "Futur pilote du plan de transition", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Du Bilan Carbone® au plan d'action : choisir ses 3 premières actions", "Transformer un Bilan Carbone® en 3 actions chiffrées et arbitrées avec la direction, en une journée.", "<p>Après la remise du Bilan Carbone®, cette journée aide dirigeants, finance et achats à choisir et chiffrer leurs 3 premières actions, pour sortir du simple diagnostic.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.DuBilanALAction, "1 jour", "Dirigeants, finance, achats", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Défi collecte : réunir les données carbone en équipe", "Remplir et sourcer un tableur de collecte carbone pour son service, en 4 semaines.", "<p>Ce défi forme les correspondants de chaque service à collecter et sourcer leurs propres données carbone, étape « Mobiliser » du plan de transition.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.DuBilanALAction, "1 jour + 4 semaines de collecte", "Correspondants par service", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Répondre à son donneur d'ordres : de la demande carbone au dossier envoyé", "Compléter un vrai questionnaire client ou appel d'offres carbone, prêt à être envoyé.", "<p>Pour les commerciaux, qualité et dirigeants de PME et ETI sous-traitantes, cette journée transforme une demande carbone de donneur d'ordres en dossier réellement envoyé.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.DuBilanALAction, "1 jour", "Commerciaux, qualité, dirigeants de sous-traitants", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Défi achats : réduire le scope 3 avec ses fournisseurs", "Engager 3 fournisseurs prioritaires et ajouter un critère carbone à ses achats, en 6 semaines.", "<p>Le scope 3 (fournisseurs) est souvent le premier poste d'émission des PME. Ce défi fait agir directement les acheteurs sur leurs 3 fournisseurs les plus importants.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.DuBilanALAction, "1 jour + 6 semaines", "Acheteurs", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Piloter sa trajectoire SNBC 3 — Bâtiment & BTP", "Fixez votre trajectoire carbone, puis testez-la sur le bâtiment.", "<p>Ce Challenge forme en interne la personne qui va traduire les budgets SNBC 3 en trajectoire d'entreprise, puis la mettre à l'épreuve du terrain sur le secteur Bâtiment & BTP.</p>", 2, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.PiloterTrajectoireSnbc, "2 jours + 6 semaines sur le terrain", "BTP, PME propriétaires de locaux", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Piloter sa trajectoire SNBC 3 — Industrie & PME manufacturières", "Fixez votre trajectoire carbone, puis testez-la sur un procédé industriel.", "<p>Ce Challenge forme en interne la personne qui va traduire les budgets SNBC 3 en trajectoire d'entreprise, puis la mettre à l'épreuve du terrain sur le secteur Industrie & PME manufacturières.</p>", 2, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.PiloterTrajectoireSnbc, "2 jours + 6 semaines sur le terrain", "Production, maintenance, achats", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Piloter sa trajectoire SNBC 3 — Agriculture & agroalimentaire", "Fixez votre trajectoire carbone, puis testez-la sur une exploitation ou une filière agricole.", "<p>Ce Challenge forme en interne la personne qui va traduire les budgets SNBC 3 en trajectoire d'entreprise, puis la mettre à l'épreuve du terrain sur le secteur Agriculture & agroalimentaire.</p>", 2, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.PiloterTrajectoireSnbc, "2 jours + 6 semaines sur le terrain", "Coopératives, IAA, négoce", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Piloter sa trajectoire SNBC 3 — Déchets & économie circulaire", "Fixez votre trajectoire carbone, puis testez-la sur ses flux de déchets.", "<p>Ce Challenge forme en interne la personne qui va traduire les budgets SNBC 3 en trajectoire d'entreprise, puis la mettre à l'épreuve du terrain sur le secteur Déchets & économie circulaire.</p>", 2, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.PiloterTrajectoireSnbc, "2 jours + 6 semaines sur le terrain", "Toutes PME", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Piloter sa trajectoire SNBC 3 — Tertiaire, commerce & numérique", "Fixez votre trajectoire carbone, puis testez-la sur l'énergie, les déplacements ou le numérique.", "<p>Ce Challenge forme en interne la personne qui va traduire les budgets SNBC 3 en trajectoire d'entreprise, puis la mettre à l'épreuve du terrain sur le secteur Tertiaire, commerce & numérique.</p>", 2, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.PiloterTrajectoireSnbc, "2 jours + 6 semaines sur le terrain", "Services, ESN, distribution", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Suivi terrain accompagné", "Garder le cap sur sa trajectoire SNBC 3 grâce à un suivi régulier sur 6 mois.", "<p>Pour les pilotes déjà formés au cadrage et à un défi sectoriel, ce suivi de 6 classes virtuelles sur 6 mois permet de garder le cap, d'ajuster la trajectoire et de partager les difficultés avec d'autres pilotes.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.PiloterTrajectoireSnbc, "6 classes virtuelles de 1 h 30 sur 6 mois", "Pilotes formés", "OPCO", "Sur devis", false, new System.DateTime(2026, 11, 15) },
                { "Manager un défi bas-carbone dans son équipe", "Lancer un vrai défi bas-carbone dans son équipe entre les 2 journées de formation.", "<p>En 2 sessions sur 1 mois, ce parcours fait vivre au manager l'expérience de lancer et piloter un défi bas-carbone dans sa propre équipe.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.HumainEtOrganisation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.ManagerEtEmbarquer, "2 jours (en 2 fois sur 1 mois)", "Managers de proximité", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Faire changer les pratiques au travail : défi comportements", "Mesurer un geste ciblé avant et après pour prouver qu'un changement de comportement est possible.", "<p>Inspiré du GRAINE, ce défi s'attaque aux freins psychosociaux au changement de comportement au travail (énergie, déplacements, impressions...), avec une mesure avant/après.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.HumainEtOrganisation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.ManagerEtEmbarquer, "2 jours", "Managers, RH, référents", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Ambassadeurs : embarquer ses collègues sans moraliser", "Animer une vraie action collective auprès de ses collègues, sans donner de leçon.", "<p>Pour les volontaires et membres du CSE qui veulent embarquer leurs collègues, sans jamais tomber dans le discours moralisateur qui braque plus qu'il ne convainc.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.HumainEtOrganisation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.ManagerEtEmbarquer, "1 jour + 4 semaines", "Volontaires, membres du CSE", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Animer un atelier climat en interne", "Animer soi-même un vrai atelier climat auprès de ses collègues, et le débriefer.", "<p>Pour les référents et équipes RSE qui veulent devenir autonomes dans l'animation d'ateliers climat, avec une vraie mise en pratique devant de vrais collègues.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.HumainEtOrganisation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.ManagerEtEmbarquer, "2 jours", "Référents, équipe RSE", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Devenir animateur Cap Bas Carbone", "Co-animer un vrai cycle du Challenge Cap Bas Carbone et obtenir sa licence d'animation.", "<p>Pour les animateurs internes d'ETI multisites et les partenaires qui veulent déployer eux-mêmes le Challenge Cap Bas Carbone dans leur organisation.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.HumainEtOrganisation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.ManagerEtEmbarquer, "2 jours + 1 cycle accompagné", "Animateurs internes d'ETI multisites, partenaires", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Défi Énergie : baisser la consommation du site", "Le kWh le moins cher, c'est celui qu'on ne consomme jamais.", "<p>Défi court et ciblé, valable pour tous les secteurs : un lancement, 4 semaines sur le terrain, puis une restitution.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.DefisParPosteEmission, "½ jour + 4 semaines", "Services généraux, tous salariés", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Défi Mobilité : construire le plan de mobilité employeur", "On ne change pas les trajets de quelqu'un sans d'abord les comprendre.", "<p>Défi court et ciblé, valable pour tous les secteurs : un lancement, 4 semaines sur le terrain, puis une restitution.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.DefisParPosteEmission, "½ jour + 4 semaines", "RH, services généraux", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Défi Éco-conduite", "On ne conduit jamais pareil une fois qu'on a vu sa propre consommation chiffrée.", "<p>Défi court et ciblé, valable pour tous les secteurs : un lancement, 4 semaines sur le terrain, puis une restitution.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.DefisParPosteEmission, "½ jour + 4 semaines", "Conducteurs, commerciaux itinérants", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Défi Fret : optimiser ses flux logistiques", "Le camion à moitié vide pollue presque autant que le camion plein.", "<p>Défi court et ciblé, valable pour tous les secteurs : un lancement, 4 semaines sur le terrain, puis une restitution.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.DefisParPosteEmission, "½ jour + 4 semaines", "Logistique, supply chain", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Défi Numérique responsable", "Le matériel numérique le plus écologique est celui qu'on n'a pas encore remplacé.", "<p>Défi court et ciblé, valable pour tous les secteurs : un lancement, 4 semaines sur le terrain, puis une restitution.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.DefisParPosteEmission, "½ jour + 4 semaines", "Informatique, tous salariés", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Défi Déchets zéro gaspillage", "Trier mieux, c'est bien ; produire moins de déchets, c'est mieux.", "<p>Défi court et ciblé, valable pour tous les secteurs : un lancement, 4 semaines sur le terrain, puis une restitution.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.DefisParPosteEmission, "½ jour + 4 semaines", "Production, services généraux", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Défi Écoconception : revoir un produit ou un service", "Un produit bien conçu pense à sa fin de vie dès son premier croquis.", "<p>Défi court et ciblé, valable pour tous les secteurs : un lancement, 6 semaines sur le terrain, puis une restitution.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.DefisParPosteEmission, "1 jour + 6 semaines", "Bureau d'études, marketing", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Défi Alimentation bas carbone (restauration, séminaires)", "Un menu bas carbone ne doit jamais ressembler à une punition.", "<p>Défi court et ciblé, valable pour tous les secteurs : un lancement, 4 semaines sur le terrain, puis une restitution.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.DefisParPosteEmission, "½ jour + 4 semaines", "RH, CSE, restauration collective", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Atelier IA : automatiser sa collecte et son reporting carbone", "Repartir avec un assistant ou un modèle de reporting opérationnel sur ses propres données.", "<p>Pour remplir le lien « Compétences & IA » du site, aujourd'hui vide : on produit avec l'IA, pas seulement on en parle.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.IaEtTransition, "1 jour", "Référents carbone, RSE", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Défi IA frugale : réduire l'empreinte de ses usages d'IA générative", "Recenser ses usages d'IA générative et réduire leur empreinte réelle, en 4 semaines.", "<p>L'IA générative a elle-même une empreinte environnementale. Ce défi fait adopter des bonnes pratiques d'usage frugal, avec une estimation avant/après.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.IaEtTransition, "½ jour + 4 semaines", "Tous salariés", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Green IA : concevoir un projet d'IA sobre", "Revoir un projet d'IA de l'entreprise selon des critères de sobriété : modèle, hébergement, usage.", "<p>Pour DSI, chefs de projet et équipes informatiques qui conçoivent ou déploient des projets d'IA, cette journée fait intégrer la sobriété dès la conception.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.IaEtTransition, "1 jour", "DSI, chefs de projet, équipes informatiques", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Construire son tableau de bord de trajectoire avec l'IA et le tableur", "Repartir avec un tableau de bord alimenté par ses propres indicateurs de trajectoire SNBC.", "<p>Pour les pilotes de trajectoire SNBC déjà formés, cette journée fait construire, avec l'IA et le tableur, un vrai tableau de bord alimenté par leurs propres indicateurs.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.IaEtTransition, "1 jour", "Pilotes de trajectoire SNBC", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Lancer la démarche de transition écologique et sociale de sa PME", "Faire valider une feuille de route sur 12 mois en comité de direction.", "<p>Inspiré du module GRAINE « Accompagner une démarche de transition écologique et sociale », pour les dirigeants et équipes RSE qui veulent structurer leur démarche.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.HumainEtOrganisation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.InspireesDuGraine, "2 jours", "Dirigeants, RSE", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Mesurer l'impact réel de ses actions de transition", "Produire un bilan d'impact chiffré d'une action déjà menée.", "<p>Inspiré du module GRAINE « Savoir évaluer pour pérenniser ses projets », pour les référents et pilotes qui veulent objectiver l'impact de leurs actions.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.InspireesDuGraine, "1 jour", "Référents, pilotes", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Valoriser ses résultats mesurés sans greenwashing", "Produire un support de communication basé sur des résultats réellement chiffrés.", "<p>Inspiré du module GRAINE « Rendre ses projets attractifs grâce à une communication efficace », pour communiquer sans jamais tomber dans le greenwashing.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.HumainEtOrganisation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.InspireesDuGraine, "1 jour", "Communication, marketing, dirigeants", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Défi canicule : adapter ses postes de travail à la chaleur", "Diagnostiquer un site et mettre en place 3 mesures concrètes face à la chaleur.", "<p>Inspiré du module GRAINE « Projet éducatif en santé-environnement », pour RH, QSE et CSE confrontés aux vagues de chaleur de plus en plus fréquentes.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.HumainEtOrganisation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.InspireesDuGraine, "1 jour + 4 semaines", "RH, QSE, CSE", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Défi Biodiversité : renaturer un espace du site", "Débitumer ou végétaliser une zone du site avec les salariés, en chantier collectif.", "<p>Inspiré du module GRAINE « Renaturation d'une cour d'école ou de crèche », transposé à l'entreprise en chantier collectif.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.InspireesDuGraine, "1 jour + chantier collectif", "Services généraux, RSE, volontaires", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Défi Haies et sols : stocker du carbone sur son exploitation", "Chiffrer un projet de plantation ou de couverts en carbone réellement stocké.", "<p>Inspiré du module GRAINE « Bocages et haies, mieux comprendre pour agir collectivement », pour les exploitations agricoles et coopératives.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.InspireesDuGraine, "1 jour + terrain", "Exploitations agricoles, coopératives", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Défi Eau : réduire consommations et rejets", "Mesurer ses m³ consommés avant et après avoir traité une source de pollution.", "<p>Inspiré du module GRAINE « Les pollutions fantômes de l'eau », pour la production et le QSE.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.Decarbonation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.InspireesDuGraine, "½ jour + 4 semaines", "Production, QSE", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Former ses collègues à passer à l'action (formateur interne)", "Concevoir puis animer soi-même une séquence de formation-action en interne.", "<p>Inspiré du module GRAINE « Concevoir et animer une action de formation », pour les référents et ambassadeurs qui veulent former à leur tour.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.HumainEtOrganisation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.InspireesDuGraine, "2 jours", "Référents, ambassadeurs", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Facilitation graphique pour animer ses défis", "Repartir avec la fresque visuelle de son plan d'action, affichée dans l'entreprise.", "<p>Inspiré du module GRAINE « Facilitation graphique par le sketchnoting », pour les animateurs et managers qui veulent rendre leurs défis plus visuels.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.HumainEtOrganisation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.InspireesDuGraine, "1 jour", "Animateurs, managers", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Pitcher son action devant la direction : posture et voix", "Présenter son action filmée et débriefée, pour gagner en aisance devant un public exigeant.", "<p>Inspiré du module GRAINE « Posture corporelle et vocale, animer avec plus d'aisance », pour managers et ambassadeurs qui doivent convaincre une direction.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.HumainEtOrganisation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.InspireesDuGraine, "1 jour", "Managers, ambassadeurs", "OPCO", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Bilan de compétences « métiers de la transition écologique »", "Valider son projet professionnel grâce à des enquêtes métier et une immersion de terrain.", "<p>Un bilan de compétences entièrement tourné vers les métiers de la transition écologique : on ne se contente pas de tests, on va enquêter et s'immerger sur le terrain avant de valider son projet.</p>", 2, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.HumainEtOrganisation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.ParticuliersEtReconversion, "Jusqu'à 24 h", "Salariés, demandeurs d'emploi", "CPF", "Durée et prix à afficher après cadrage individuel", false, new System.DateTime(2026, 11, 15) },
                { "Bilan d'orientation jeunes vers les métiers verts", "Confronter son choix d'orientation à des rencontres réelles avec des professionnels des métiers verts.", "<p>Pour lycéens et étudiants en questionnement, un format court pour confronter ses idées d'orientation à la réalité des métiers verts, en lien avec le réseau GRAINE et Orientaction.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.HumainEtOrganisation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.ParticuliersEtReconversion, "Quelques séances", "Lycéens, étudiants", "Particuliers", "Sur devis", true, new System.DateTime(2026, 11, 15) },
                { "Atelier « Découvrir les métiers de la transition »", "Repartir avec 3 métiers à explorer et une première prise de contact planifiée.", "<p>Porte d'entrée gratuite vers le bilan de compétences, en webinaire ou en collectif, pour toute personne en questionnement sur son avenir professionnel.</p>", 1, (int)Domain.Entities.ModePlateforme.BtoC, (int)Domain.Entities.StatutChallenge.Publie, (int)Domain.Entities.ThematiqueChallenge.HumainEtOrganisation, (int)Domain.Entities.FormatChallenge.Collectif, (int)Domain.Entities.SousFamilleChallenge.ParticuliersEtReconversion, "2 h, en webinaire ou en collectif", "Personnes en questionnement", "Gratuit", "Gratuit", true, new System.DateTime(2026, 11, 15) }
                });

            migrationBuilder.InsertData(
                table: "CartesCompetences",
                columns: ["Code", "Niveau", "TitreTheorie", "Objectif1", "Objectif2", "Objectif3", "Citation", "TitreDefi", "ContextePro", "ContextePerso", "TonDefi", "Etape1", "Etape2", "Etape3", "Tip1", "Tip2", "Tip3", "CitationHumour", "CreeLe"],
                values: new object[,]
                {
                { "SENS-C01", (int)Domain.Entities.NiveauCarte.Debutant, "Comprendre pour agir", "Relier les causes du changement climatique aux activités quotidiennes de l'entreprise.", "Expérimenter l'intelligence collective à partir d'un jeu de cartes basé sur les données du GIEC.", "Transformer une prise de conscience collective en engagement concret, pas en culpabilité individuelle.", "On ne change pas ce qu'on ne comprend pas.", "3 idées qui comptent", "Après la Fresque, l'énergie collective retombe souvent en 48 heures si rien de concret n'est noté.", "Comme un brainstorming de famille sur les vacances : si personne n'écrit rien, chacun repart avec sa propre idée, et rien ne se passe.", "Avant de quitter la salle, ton équipe formule 3 idées d'actions concrètes et les classe de la plus facile à la plus impactante.", "Reclasse les causes découvertes pendant la Fresque par ordre d'importance pour ton métier.", "Propose, chacun, une action réaliste à son échelle.", "Votez en équipe pour garder les 3 meilleures et désignez qui les porte.", "Note les 3 idées sur un support visible de tous (mur, canal d'équipe) dans les 24 heures.", "Choisis des actions qui ne dépendent pas d'un budget ou d'une validation hiérarchique pour démarrer.", "Donne une date butoir à chaque idée, même approximative : « avant fin de mois » suffit.", "La meilleure fresque est celle qui finit encadrée au mur, pas glissée dans un tiroir.", new System.DateTime(2026, 11, 15) },
                { "SENS-C02", (int)Domain.Entities.NiveauCarte.Debutant, "L'empreinte cachée du numérique", "Identifier les postes d'émission du numérique : fabrication, data centers, réseaux, usages.", "Comprendre pourquoi la fabrication des terminaux pèse plus lourd que leur simple usage.", "Repérer les leviers de réduction accessibles à son équipe sans compétence technique.", "Le mail le plus écologique est celui qu'on n'envoie pas en pièce jointe à 50 personnes.", "Un premier défi numérique", "Le service informatique commande de nouveaux équipements chaque année, sans trajectoire de sobriété numérique définie.", "Comme un garde-manger qu'on ne trie jamais : on rachète plutôt que de vérifier ce qui existe déjà et qui pourrait resservir.", "Choisis avec ton équipe un geste numérique à tester pendant 4 semaines : allonger la durée de vie du matériel, nettoyer le stockage, ou revoir une habitude d'usage.", "Fais l'inventaire du matériel disponible et de son âge réel avant toute nouvelle demande d'achat.", "Mesure un indicateur simple avant de démarrer (Go de stockage, nombre d'appareils, âge moyen du parc).", "Teste le geste choisi pendant 4 semaines avec toute l'équipe.", "Vise d'abord le stockage : vider les boîtes mail et les espaces partagés est gratuit et immédiat.", "Repousse l'achat d'un nouvel écran ou ordinateur d'au moins 6 mois si l'usage le permet.", "Affiche le compteur de Go libérés dans l'espace d'équipe, ça motive plus qu'un discours.", "Le cloud, ce n'est pas un nuage : c'est un hangar plein de machines qui chauffent quelque part.", new System.DateTime(2026, 11, 15) },
                { "SENS-C03", (int)Domain.Entities.NiveauCarte.Debutant, "L'objectif 2 tonnes", "Comprendre pourquoi l'objectif climatique mondial se traduit en 2 tonnes de CO2e par personne et par an.", "Simuler des arbitrages de politique climatique pour ressentir le poids des décisions à grande échelle.", "Distinguer ce qui relève du choix individuel de ce qui relève du choix collectif ou politique.", "2 tonnes, ça se discute en groupe, pas en silence devant son empreinte carbone personnelle.", "Un engagement qui tient", "Après l'atelier, les bonnes résolutions s'évaporent souvent dès la première réunion qui déborde.", "Comme une résolution de nouvel an sur le sport : elle tient si on la note, qu'on la date, et qu'on la dit à quelqu'un.", "Formule un engagement individuel daté, et accorde-toi avec ton équipe sur un engagement collectif tout aussi daté.", "Repère dans l'atelier le levier qui te semble le plus accessible à ton échelle.", "Écris ton engagement individuel avec une date de point d'étape.", "Partage-le à voix haute avec ton équipe, qui fait de même avant de choisir son engagement collectif.", "Un engagement vague (« faire attention ») ne tient jamais : vise une action mesurable.", "Choisis un engagement collectif qui ne dépend pas d'une seule personne pour avancer.", "Fixe le point d'étape dans l'agenda tout de suite, pas « plus tard ».", "L'enfer est pavé de bonnes résolutions jamais mises à l'agenda.", new System.DateTime(2026, 11, 15) },
                { "SENS-C04", (int)Domain.Entities.NiveauCarte.Debutant, "Suis-je prêt face à la SNBC 3 ?", "Comprendre ce que la SNBC 3 change concrètement pour une PME ou une ETI.", "Identifier les signaux que ses propres donneurs d'ordres envoient déjà sur le sujet carbone.", "Savoir distinguer une obligation réglementaire d'une attente commerciale informelle.", "Le client qui demande votre bilan carbone aujourd'hui est celui qui l'exigera demain.", "Mon auto-diagnostic rempli", "Un dirigeant de PME reçoit de plus en plus de questionnaires carbone de ses clients, sans savoir s'il doit s'en inquiéter ou les ignorer.", "Comme un contrôle technique qu'on repousse : tant que rien n'est officiellement exigé, on remet à plus tard — jusqu'au jour où le client l'exige vraiment.", "Remplis l'auto-diagnostic en fin de matinale pour situer ton entreprise, et prends rendez-vous pour un pré-diagnostic si un point t'alerte.", "Liste les 3 derniers questionnaires ou demandes carbone reçus de clients ou donneurs d'ordres.", "Réponds aux questions de l'auto-diagnostic sans chercher à te justifier, juste en l'état réel.", "Identifie le point le plus urgent à traiter dans les 3 prochains mois.", "Ne confonds pas obligation légale (BEGES réglementaire) et attente commerciale (questionnaire client) : les deux comptent, mais pas au même rythme.", "Un auto-diagnostic honnête vaut mieux qu'un auto-diagnostic flatteur : c'est pour toi, pas pour un jury.", "Si un point t'inquiète, le pré-diagnostic gratuit permet de le vérifier avant d'investir.", "Mieux vaut un auto-diagnostic lucide un lundi matin qu'un audit client surprise un vendredi soir.", new System.DateTime(2026, 11, 15) },
                { "SENS-C05", (int)Domain.Entities.NiveauCarte.Debutant, "Penser low-tech", "Comprendre ce qui distingue une solution low-tech d'une solution high-tech ou d'une simple économie.", "Identifier un besoin réel du site qui pourrait être résolu simplement, sans gros investissement.", "Expérimenter la conception rapide en équipe, du besoin au prototype testable.", "La meilleure technologie est parfois celle qu'on n'a pas besoin d'acheter.", "Un prototype qui marche", "Le site a des petits problèmes du quotidien (chaleur, gaspillage, objet cassé) que personne ne prend le temps de résoudre simplement.", "Comme réparer soi-même une étagère plutôt que la racheter : ça prend un après-midi, mais ça marche, et on en est fier.", "En une journée, ton équipe conçoit et teste un vrai prototype low-tech répondant à un besoin réel du site.", "Repère un besoin concret et irritant du quotidien sur le site.", "Esquisse 2 ou 3 solutions simples, sans budget, avec ce qui existe déjà.", "Construis et teste le prototype retenu avant la fin de la journée.", "Préfère réparer ou détourner un objet existant plutôt qu'en commander un neuf.", "Un prototype moche qui marche vaut mieux qu'un beau plan qui reste sur le papier.", "Fais tester le prototype par quelqu'un qui n'a pas participé à sa conception : ça révèle vite s'il est vraiment utilisable.", "Le bricolage du vendredi après-midi a sauvé plus d'usines que le plan stratégique du lundi matin.", new System.DateTime(2026, 11, 15) },
                { "ACT-C01", (int)Domain.Entities.NiveauCarte.Debutant, "Porter le plan, pas juste le lire", "Comprendre la structure d'un plan de transition carbone (actions, responsables, échéances, indicateurs).", "Distinguer le rôle de pilote de celui de simple rapporteur de chiffres.", "Savoir embarquer les autres services sans autorité hiérarchique directe sur eux.", "Un plan de transition sans pilote, c'est une carte sans personne pour tenir le volant.", "3 actions lancées", "L'entreprise a reçu un plan de transition détaillé, mais personne n'a le temps ni le mandat clair pour le faire vivre au quotidien.", "Comme un plan d'épargne qu'on signe puis qu'on oublie : sans quelqu'un pour suivre les versements, il ne sert à rien.", "Entre les 3 sessions de ce parcours, choisis et lance 3 actions du plan de transition, que tu présenteras chiffrées (tCO2e, coût, délai) à la dernière session.", "Repère dans le plan de transition les 3 actions les plus rapides à lancer sans attendre un budget important.", "Identifie pour chacune un interlocuteur dans le service concerné et obtiens son accord de principe.", "Lance les 3 actions et note, dès le départ, comment tu vas mesurer leur effet.", "Commence par l'action la plus visible : un premier succès rapide convainc plus que 10 pages de plan.", "N'essaie pas de tout porter seul : trouve un relais dans chaque service concerné.", "Garde une trace écrite de chaque action lancée, même informelle — tu en auras besoin pour le bilan.", "Un bon pilote de transition passe plus de temps dans les couloirs qu'en réunion.", new System.DateTime(2026, 11, 15) },
                { "ACT-C02", (int)Domain.Entities.NiveauCarte.Debutant, "Du diagnostic à la décision", "Savoir lire les postes d'émission d'un Bilan Carbone® et repérer les plus actionnables à court terme.", "Chiffrer une action en tCO2e évitées, coût et délai de mise en œuvre.", "Faire arbitrer un plan d'action par la direction en une seule réunion.", "Un Bilan Carbone® qui reste dans un tiroir n'a jamais évité une tonne de CO2e.", "3 actions chiffrées", "Le Bilan Carbone® est arrivé, dense et technique : la direction ne sait pas par où commencer ni combien ça va coûter.", "Comme un relevé bancaire détaillé : tant que personne n'en tire 3 décisions concrètes, il ne sert qu'à constater.", "D'ici la fin de la journée, ton équipe repart avec 3 actions chiffrées en tCO2e, coût et délai, validées par la direction.", "Classe les postes d'émission du Bilan Carbone® du plus important au moins important.", "Pour les 3 premiers, estime le coût, le délai et le gain carbone de l'action la plus évidente.", "Présente les 3 actions à la direction et fais trancher en séance.", "Préfère une action imparfaite mais lancée cette année à une action parfaite repoussée à l'an prochain.", "Chiffrer même approximativement vaut mieux que ne pas chiffrer du tout : la précision s'affine en marchant.", "Fais trancher la direction en séance, pas par email une semaine plus tard — l'élan retombe vite.", "Un plan d'action sans chiffre, c'est une intention ; avec un chiffre, c'est un engagement.", new System.DateTime(2026, 11, 15) },
                { "ACT-C03", (int)Domain.Entities.NiveauCarte.Debutant, "Collecter sans se perdre", "Identifier les données nécessaires à un Bilan Carbone® fiable (énergie, déplacements, achats, déchets).", "Savoir où trouver ces données dans son propre service (factures, logiciels, carnets de bord).", "Sourcer chaque donnée pour qu'elle reste vérifiable par un tiers.", "Une donnée sans source, c'est une estimation qui s'ignore.", "Mon tableur rempli et sourcé", "Le Bilan Carbone® de l'entreprise avance mal faute de données fiables : chaque service pense que c'est à un autre de les fournir.", "Comme une déclaration d'impôts collective : si chacun attend que le voisin remplisse sa part, rien n'avance.", "Dans les 4 semaines qui suivent la formation, remplis le tableur de collecte pour ton service, avec la source de chaque donnée.", "Identifie dans le tableur les données qui concernent directement ton service.", "Retrouve la source de chacune (facture, export logiciel, carnet de bord) avant de la saisir.", "Signale clairement les données manquantes plutôt que de les estimer sans le dire.", "Commence par les données les plus faciles à obtenir (factures d'énergie) avant les plus complexes (déplacements professionnels).", "Toujours noter la source à côté de la donnée : ça évite de tout refaire au prochain Bilan Carbone®.", "Une donnée manquante signalée vaut mieux qu'une donnée inventée en silence.", "Un tableur bien sourcé aujourd'hui, c'est un Bilan Carbone® deux fois plus rapide demain.", new System.DateTime(2026, 11, 15) },
                { "ACT-C04", (int)Domain.Entities.NiveauCarte.Debutant, "Décoder la demande carbone", "Comprendre ce qu'un donneur d'ordres cherche réellement derrière un questionnaire carbone.", "Identifier dans l'entreprise les données déjà disponibles pour y répondre.", "Présenter une réponse carbone qui rassure sans surpromettre.", "Une bonne réponse carbone rassure un client, une mauvaise en fait fuir deux.", "Mon dossier envoyé", "Un donneur d'ordres envoie un questionnaire carbone dense ; l'entreprise sous-traitante craint de mal répondre ou de perdre le marché.", "Comme un dossier de prêt immobilier : plus on tarde à répondre complètement, plus le banquier doute.", "D'ici la fin de la journée, ton questionnaire client ou ton appel d'offres carbone est complété et prêt à partir.", "Lis la demande en entier avant de répondre, en repérant les questions obligatoires des questions bonus.", "Rassemble les données déjà disponibles (Bilan Carbone®, pré-diagnostic, actions en cours).", "Rédige une réponse honnête, y compris sur ce qui n'est pas encore fait, avec une échéance annoncée.", "Ne laisse jamais une question sans réponse : une case vide inquiète plus qu'une réponse imparfaite.", "Valorise les actions déjà en cours, même petites : un donneur d'ordres regarde la trajectoire, pas seulement l'état actuel.", "Fais relire le dossier par quelqu'un d'extérieur au service avant envoi.", "Le silence est la seule réponse qui perd systématiquement un marché.", new System.DateTime(2026, 11, 15) },
                { "ACT-C05", (int)Domain.Entities.NiveauCarte.Debutant, "Le scope 3, angle mort des PME", "Comprendre pourquoi les achats (scope 3) représentent souvent le premier poste d'émission d'une PME.", "Identifier ses fournisseurs les plus émetteurs ou les moins transparents sur le sujet.", "Ajouter un critère carbone simple et non bloquant à une grille d'achats existante.", "On ne décarbone jamais seul ce qu'on achète à quelqu'un d'autre.", "3 fournisseurs engagés", "L'entreprise a lancé son plan carbone en interne, mais ses fournisseurs continuent comme avant, sans qu'on leur ait jamais demandé.", "Comme choisir ses courses sans jamais regarder d'où vient le produit : le levier existe, personne ne l'a encore actionné.", "Dans les 6 semaines qui suivent la formation, engage 3 fournisseurs prioritaires sur le sujet carbone et ajoute un critère carbone à ta grille d'achats.", "Classe tes fournisseurs actuels par poids d'achat et identifie les 3 plus importants sur le plan carbone.", "Contacte-les pour une première discussion sur leurs propres engagements et données disponibles.", "Ajoute un critère carbone simple (ex. existence d'un bilan, distance de transport) à ta prochaine grille d'achat.", "Commence par un dialogue, pas un ultimatum : beaucoup de fournisseurs n'ont jamais été interrogés sur le sujet.", "Un critère carbone non éliminatoire au départ fait moins peur et s'impose plus facilement avec le temps.", "Documente chaque échange : la trajectoire compte autant que la mesure du jour.", "Le fournisseur le plus décarboné du marché, c'est souvent celui qu'on n'a jamais pensé à questionner.", new System.DateTime(2026, 11, 15) },
                { "SNBC-C03", (int)Domain.Entities.NiveauCarte.Debutant, "Décarboner son bâti", "Identifier les leviers de décarbonation du bâtiment : chauffage, rénovation, matériaux bas-carbone.", "Comprendre l'objectif national de réduction du secteur (de 57 à 3 MtCO2e visés en 2050).", "Construire un plan chauffage et rénovation chiffré pour un site ou un chantier.", "Le bâtiment le plus écologique est souvent celui qu'on rénove, pas celui qu'on construit.", "Un plan chauffage et rénovation chiffré", "La direction hésite entre rénover un site existant ou attendre une contrainte réglementaire pour agir.", "Comme retarder l'isolation de sa maison en se disant que l'hiver prochain sera plus doux : le coût grimpe à chaque report.", "D'ici la fin du défi, construis un plan chauffage et rénovation chiffré pour un site ou un chantier de ton entreprise.", "Fais l'état des lieux énergétique d'un site ou d'un chantier (chauffage, isolation, matériaux).", "Identifie 2 ou 3 actions réalistes (pompe à chaleur, isolation ciblée, matériau bas-carbone) et chiffre-les.", "Présente le plan chiffré à la direction avec un ordre de priorité.", "Commence par l'isolation avant de changer le système de chauffage : un bâtiment mal isolé gaspille toute énergie, même renouvelable.", "Priorise les travaux au meilleur rapport gain carbone / coût, pas les plus visibles.", "Implique le gestionnaire du site dès le diagnostic : il connaît les vraies contraintes du bâtiment.", "Un bâtiment bien isolé, c'est une facture de chauffage qui arrête de faire peur.", new System.DateTime(2026, 11, 15) },
                { "SNBC-C04", (int)Domain.Entities.NiveauCarte.Debutant, "Décarboner son procédé", "Identifier les leviers de décarbonation industrielle : électrification des procédés, efficacité énergétique, chaleur fatale.", "Comprendre comment auditer un procédé de production sous l'angle carbone.", "Lancer une action d'efficacité énergétique ou d'électrification sur un procédé réel.", "L'énergie la moins chère est toujours celle qu'on ne consomme pas.", "Un procédé audité, une action lancée", "L'atelier de production tourne depuis des années avec des réglages jamais réévalués depuis leur installation.", "Comme une voiture jamais révisée : elle roule, mais consomme bien plus qu'elle ne devrait.", "Dans les 6 semaines qui suivent la formation, audite un procédé de production et lance une action d'efficacité ou d'électrification.", "Choisis un procédé consommant beaucoup d'énergie et mesure sa consommation actuelle.", "Identifie un gisement d'économie (chaleur perdue, réglage à optimiser, étape électrifiable).", "Lance l'action retenue et mesure l'écart de consommation avant/après.", "La chaleur fatale (chaleur perdue d'un procédé) est souvent le gisement le plus sous-exploité : regarde en priorité où elle part.", "Implique les équipes de maintenance : elles connaissent les dérives réelles du procédé mieux que les plans d'origine.", "Mesure avant d'agir : sans mesure de départ, impossible de prouver le gain ensuite.", "Le meilleur ingénieur procédé, c'est souvent l'opérateur qui le fait tourner tous les jours.", new System.DateTime(2026, 11, 15) },
                { "SNBC-C05", (int)Domain.Entities.NiveauCarte.Debutant, "Décarboner sa filière", "Comprendre les leviers de décarbonation agricole : intrants, énergie, haies et stockage carbone des sols.", "Situer l'objectif national du secteur (67 MtCO2e maintenu comme cible 2030).", "Tester une action concrète sur un levier choisi (intrants, énergie, haies).", "Un sol vivant stocke plus de carbone qu'un sol qu'on ne fait que traverser.", "Une action testée sur le terrain", "La coopérative ou l'exploitation subit la pression carbone de ses clients sans savoir par quel levier commencer.", "Comme réduire sa consommation d'eau au jardin : on peut agir sur l'arrosage, le paillage ou le choix des plantes — encore faut-il choisir un levier à la fois.", "Dans les 6 semaines qui suivent la formation, teste une action concrète sur un levier choisi : intrants, énergie ou haies et couverts.", "Choisis, avec ton équipe, le levier le plus accessible cette saison (intrants, énergie, ou haies/couverts).", "Mesure l'état de départ (quantité d'intrants, consommation d'énergie, ou linéaire de haies).", "Teste l'action sur une parcelle ou un périmètre restreint avant généralisation.", "Commence petit : une parcelle pilote convainc plus qu'un plan pour toute l'exploitation.", "Les haies et couverts végétaux stockent du carbone tout en protégeant les sols : un double bénéfice facile à valoriser.", "Implique les exploitants dès le choix du levier : une action imposée de l'extérieur tient rarement.", "La meilleure politique agricole, c'est parfois une haie plantée au bon endroit.", new System.DateTime(2026, 11, 15) },
                { "SNBC-C06", (int)Domain.Entities.NiveauCarte.Debutant, "Décarboner ses déchets", "Comprendre la hiérarchie des leviers déchets : prévention, tri, réemploi, écoconception.", "Savoir peser et qualifier un flux de déchets avant d'agir dessus.", "Mettre en place une filière de réemploi ou de tri sur un flux identifié.", "Le meilleur déchet, c'est celui qu'on n'a jamais produit.", "Un flux pesé avant/après", "L'entreprise jette beaucoup, trie peu, et personne n'a jamais pesé ce qui sort réellement des bennes.", "Comme un frigo jamais vidé : on continue d'acheter sans voir ce qui finit à la poubelle.", "D'ici la fin du défi, pèse un flux de déchets avant et après avoir mis en place une filière de réemploi ou de tri.", "Choisis un flux de déchets visible et récurrent (emballages, chutes de production, invendus).", "Pèse ou quantifie ce flux sur une période de référence (une semaine).", "Mets en place une filière de tri ou de réemploi et repèse le même flux après quelques semaines.", "La prévention (ne pas produire le déchet) prime toujours sur le tri : commence par chercher à réduire avant de trier.", "Un partenaire de réemploi local vaut souvent mieux qu'une filière de recyclage lointaine.", "Affiche les résultats pesés : un chiffre concret motive plus qu'une consigne de tri affichée au mur.", "La meilleure poubelle, c'est celle qui reste à moitié vide.", new System.DateTime(2026, 11, 15) },
                { "SNBC-C07", (int)Domain.Entities.NiveauCarte.Debutant, "Décarboner son activité tertiaire", "Identifier les leviers propres au tertiaire : énergie des locaux, déplacements professionnels, usages numériques.", "Comprendre pourquoi ces 3 leviers sont souvent sous-estimés par rapport à l'industrie ou au transport.", "Tester une action mesurée sur l'un de ces 3 leviers.", "Un bureau n'a pas de cheminée, mais il a une empreinte carbone bien réelle.", "Une action testée et mesurée", "L'activité de service se pense « peu polluante » faute d'usine ou de flotte de camions, et personne n'a encore regardé de près.", "Comme se croire économe parce qu'on ne voit pas sa facture d'électricité : l'impact existe, juste caché dans les habitudes.", "Dans les 6 semaines qui suivent la formation, teste une action mesurée sur l'énergie, les déplacements ou le numérique de ton activité.", "Choisis le levier le plus pertinent pour ton activité (énergie des locaux, déplacements, ou numérique).", "Mesure l'état de départ avec un indicateur simple (kWh, kilomètres, Go de stockage).", "Teste une action ciblée pendant quelques semaines et mesure l'écart.", "Les déplacements professionnels sont souvent le levier le plus rapide à tester : une réunion en visio de moins par semaine se décide tout de suite.", "Implique les équipes concernées dans le choix du levier : l'adhésion compte plus que l'exhaustivité.", "Documente le résultat, même modeste : il sert de preuve pour la prochaine action.", "Le kilomètre le plus vert, c'est celui qu'on ne fait pas.", new System.DateTime(2026, 11, 15) },
                { "SNBC-C08", (int)Domain.Entities.NiveauCarte.Debutant, "Tenir le cap sur 6 mois", "Comprendre pourquoi une trajectoire carbone se perd souvent dans les 6 mois qui suivent sa création, faute de suivi.", "Savoir lire un écart entre trajectoire prévue et réalité terrain sans se décourager.", "Ajuster un plan d'action sans le refaire entièrement à chaque imprévu.", "Une trajectoire qu'on ne révise jamais devient vite une trajectoire qu'on a oubliée.", "Mon tableau de bord vivant", "Le pilote a lancé sa trajectoire et son premier défi sectoriel avec enthousiasme, mais seul face aux imprévus du quotidien, le suivi s'essouffle vite.", "Comme un programme de sport sans coach : motivant les 2 premières semaines, oublié à la troisième sans un point régulier.", "À chaque classe virtuelle, apporte ton tableau de bord à jour et un écart constaté à discuter avec les autres pilotes.", "Mets à jour ton tableau de bord avant chaque classe virtuelle, même partiellement.", "Identifie l'écart le plus significatif entre ce qui était prévu et ce qui s'est passé.", "Propose un ajustement concret du plan et confronte-le aux retours des autres pilotes.", "Un tableau de bord mis à jour à moitié vaut mieux qu'un tableau parfait jamais ouvert.", "Les autres pilotes de la classe virtuelle ont souvent déjà rencontré ton problème : pose la question avant de chercher seul.", "Un petit ajustement régulier évite le grand plan de rattrapage en fin d'année.", "Un pilote qui ne regarde son tableau de bord qu'une fois par an pilote à l'aveugle le reste du temps.", new System.DateTime(2026, 11, 15) },
                { "MNG-C01", (int)Domain.Entities.NiveauCarte.Debutant, "Manager par le défi, pas par la consigne", "Comprendre pourquoi un défi mobilise une équipe plus durablement qu'une consigne descendante.", "Savoir formuler un défi clair, mesurable et atteignable pour son équipe.", "Accompagner son équipe sans faire le défi à sa place.", "On ne mobilise pas une équipe avec une note de service.", "Mon défi lancé en équipe", "Le manager a reçu la consigne de « faire avancer le sujet carbone » dans son équipe, sans méthode pour le traduire en action concrète.", "Comme motiver une équipe de sport sans objectif de match : sans défi clair, l'entraînement s'étiole.", "Entre les 2 journées de ce parcours, lance un vrai défi bas-carbone dans ton équipe, et présente un premier bilan à la session suivante.", "Choisis avec ton équipe un défi bas-carbone concret, lié à son activité quotidienne.", "Fixe une échéance et un indicateur simple pour savoir si le défi est relevé.", "Anime un point d'étape à mi-parcours pour ajuster si besoin.", "Un défi d'équipe doit pouvoir se résumer en une phrase, sinon il est trop flou pour mobiliser.", "Laisse l'équipe choisir comment relever le défi, pas seulement quoi faire.", "Célèbre une victoire intermédiaire, même petite, avant la fin du défi.", "Un manager qui relève le défi à la place de son équipe n'a rien managé du tout.", new System.DateTime(2026, 11, 15) },
                { "MNG-C02", (int)Domain.Entities.NiveauCarte.Debutant, "Changer un geste, pas une personne", "Comprendre les principaux freins psychosociaux au changement de comportement (habitude, confort, scepticisme).", "Découvrir les nudges : des leviers simples qui orientent un comportement sans contrainte.", "Choisir un geste ciblé, mesurable avant et après l'action.", "On ne change pas les gens, on change l'environnement dans lequel ils agissent.", "Un geste mesuré avant/après", "La direction veut « changer les comportements » sur l'énergie ou les déplacements, mais les campagnes d'affichage classiques n'ont jamais rien changé durablement.", "Comme vouloir manger plus sainement en se contentant d'un poster dans la cuisine : ça ne suffit jamais sans un changement concret de l'environnement.", "Choisis un geste ciblé (énergie, déplacements, impressions) et mesure-le avant et après avoir testé un nudge simple pour le faire évoluer.", "Choisis un geste précis et facilement observable (ex. extinction des écrans, impressions recto-verso).", "Mesure l'état actuel pendant une semaine, sans rien changer.", "Introduis un nudge simple (rappel visuel, réglage par défaut) et remesure après quelques semaines.", "Un nudge agit sur l'environnement (réglage par défaut, visibilité) plutôt que sur la volonté des gens.", "Mesure toujours avant de changer quoi que ce soit, sinon impossible de prouver l'effet du nudge.", "Choisis un geste très concret : « économiser l'énergie » est trop vague pour être mesuré.", "Le meilleur nudge est souvent invisible : on ne se souvient même plus qu'on nous a aidés à bien faire.", new System.DateTime(2026, 11, 15) },
                { "MNG-C03", (int)Domain.Entities.NiveauCarte.Debutant, "Convaincre sans moraliser", "Comprendre pourquoi un discours moralisateur sur l'écologie braque plus qu'il ne convainc.", "Identifier des formats d'animation qui donnent envie plutôt qu'ils n'imposent.", "Mesurer l'adhésion réelle d'une action par le nombre de participants, pas par son propre enthousiasme.", "Celui qui fait la leçon perd son auditoire avant même d'avoir fini sa phrase.", "Une action animée, des participants comptés", "Le volontaire croit au sujet climatique, mais ses tentatives de sensibilisation sont perçues comme des leçons de morale par ses collègues.", "Comme conseiller un ami sur sa santé sans qu'il l'ait demandé : même juste, le conseil non sollicité braque plus qu'il n'aide.", "Dans les 4 semaines qui suivent la formation, anime une action collective auprès de tes collègues et compte le nombre réel de participants.", "Choisis un format d'animation participatif (jeu, défi, atelier) plutôt qu'une présentation descendante.", "Propose l'action sans obligation ni culpabilisation de ceux qui n'y participent pas.", "Compte et note le nombre réel de participants à l'action.", "Un format ludique attire toujours plus qu'un discours, même bref.", "Ne prends jamais la non-participation d'un collègue pour un désintérêt définitif : laisse la porte ouverte pour la prochaine fois.", "Valorise publiquement les participants plutôt que de pointer les absents.", "On embarque plus de monde avec un sourire et un défi qu'avec un index pointé.", new System.DateTime(2026, 11, 15) },
                { "MNG-C04", (int)Domain.Entities.NiveauCarte.Debutant, "Animer, pas seulement présenter", "Distinguer facilitation et présentation : l'animateur fait produire le groupe, il ne parle pas à sa place.", "Maîtriser 3 techniques simples d'intelligence collective (brainstorming cadré, vote, synthèse visuelle).", "Gérer une prise de parole qui dérape ou un groupe qui se désengage.", "Un bon animateur parle moins que son groupe, pas plus.", "Mon atelier réellement animé", "Le référent RSE connaît son sujet par cœur mais n'a jamais animé de groupe ; sa première tentative s'est transformée en exposé magistral.", "Comme un très bon joueur qui débute comme entraîneur : savoir jouer et savoir faire jouer sont deux compétences différentes.", "Anime réellement un atelier climat auprès de tes collègues et débriefe-le ensuite avec ton formateur.", "Prépare un déroulé d'atelier avec des temps de production du groupe, pas seulement d'écoute.", "Anime l'atelier en conditions réelles, devant un vrai groupe de collègues.", "Débriefe avec ton formateur ce qui a bien fonctionné et ce qui a dérapé.", "Prévois toujours plus de temps de production du groupe que de temps de parole de l'animateur.", "Si un groupe se désengage, relance par une question ouverte plutôt que par plus d'explications.", "Le silence après une question n'est pas un échec : laisse-le durer, la réponse arrive souvent après quelques secondes.", "Le meilleur compliment pour un animateur, c'est qu'on se souvienne de ce que le groupe a produit, pas de ce qu'il a dit.", new System.DateTime(2026, 11, 15) },
                { "MNG-C05", (int)Domain.Entities.NiveauCarte.Debutant, "Devenir passeur du Challenge", "Maîtriser en détail les 11 phases du cycle de vie d'un Challenge Cap Bas Carbone.", "Savoir adapter le Challenge à un contexte multisite sans en perdre la cohérence.", "Distinguer ce qui peut être personnalisé de ce qui doit rester fidèle à la méthode.", "On ne transmet bien que ce qu'on a soi-même pratiqué jusqu'au bout.", "Mon premier cycle co-animé", "L'ETI multisite veut déployer le Challenge dans plusieurs établissements sans dépendre à chaque fois d'un animateur externe.", "Comme former un moniteur de ski à partir d'un très bon skieur : il faut lui apprendre à transmettre, pas seulement à bien skier.", "Co-anime un cycle complet du Challenge Cap Bas Carbone avec un animateur Challenges Factory, puis obtiens ta licence d'animation.", "Observe un premier cycle animé par Challenges Factory avant de co-animer.", "Co-anime ensuite un cycle complet, phase par phase, avec un accompagnement rapproché.", "Reçois ta licence d'animation après validation du cycle co-animé.", "Garde toujours les ingrédients clés du Challenge (preuve, validation par les pairs, points augmentés) même en adaptant le contenu local.", "Documente les adaptations locales pour les partager avec les autres animateurs internes.", "Un animateur qui doute doit toujours pouvoir solliciter l'équipe Challenges Factory pendant l'accompagnement.", "La meilleure licence d'animation, c'est celle qu'on a gagnée en tremblant un peu la première fois.", new System.DateTime(2026, 11, 15) },
                { "POSTE-C01", (int)Domain.Entities.NiveauCarte.Debutant, "Chasser le gaspillage énergétique", "Identifier les principaux postes de consommation énergétique d'un site (chauffage, éclairage, équipements).", "Comprendre l'écart entre consommation nécessaire et consommation gaspillée.", "Mesurer les kWh avant et après une action de réduction.", "Le kWh le moins cher, c'est celui qu'on ne consomme jamais.", "kWh avant/après", "Le site consomme de l'énergie sans que personne n'ait jamais mesuré précisément où elle part.", "Comme laisser couler l'eau en se brossant les dents : le gaspillage est invisible tant qu'on ne regarde pas le compteur.", "En 4 semaines, mesure la consommation d'un poste énergétique du site avant et après une action de réduction.", "Repère un poste de consommation visible (éclairage d'une zone, chauffage d'une salle).", "Relève sa consommation actuelle sur une période de référence.", "Teste une action simple (extinction, réglage, horaire) et remesure après quelques semaines.", "Commence par l'éclairage et le chauffage des zones peu utilisées : le gain est souvent immédiat.", "Un simple changement d'horaire ou de réglage coûte zéro euro et donne un premier résultat rapide.", "Affiche le compteur de kWh économisés : ça motive plus qu'une consigne d'extinction.", "Le radiateur allumé dans une pièce vide est le meilleur argument pour convaincre.", new System.DateTime(2026, 11, 15) },
                { "POSTE-C02", (int)Domain.Entities.NiveauCarte.Debutant, "Repenser les trajets domicile-travail", "Comprendre les enjeux d'un plan de mobilité employeur (covoiturage, vélo, télétravail, transports en commun).", "Savoir mener une enquête simple auprès des salariés sur leurs trajets.", "Lancer 2 mesures concrètes issues de l'enquête.", "On ne change pas les trajets de quelqu'un sans d'abord les comprendre.", "Enquête menée, 2 mesures lancées", "L'entreprise veut agir sur la mobilité de ses salariés, mais ne connaît pas réellement leurs modes de trajet actuels.", "Comme vouloir réorganiser les courses d'une famille sans jamais lui demander ce qu'elle mange.", "En 4 semaines, mène une enquête mobilité auprès des salariés et lance 2 mesures concrètes issues des résultats.", "Prépare une enquête courte sur les trajets domicile-travail actuels des salariés.", "Diffuse-la et analyse les résultats pour repérer les leviers les plus demandés.", "Lance 2 mesures concrètes (covoiturage, prime vélo, jours de télétravail) issues de l'enquête.", "Une enquête courte obtient plus de réponses qu'un questionnaire exhaustif.", "Privilégie les mesures réclamées par les salariés eux-mêmes : l'adhésion est déjà là.", "Communique les résultats de l'enquête avant d'annoncer les mesures : ça légitime le choix.", "Le meilleur plan de mobilité est celui que les salariés ont eux-mêmes réclamé.", new System.DateTime(2026, 11, 15) },
                { "POSTE-C03", (int)Domain.Entities.NiveauCarte.Debutant, "Conduire pour consommer moins", "Identifier les gestes d'éco-conduite qui réduisent réellement la consommation de carburant.", "Comprendre l'effet cumulé de petits gestes sur un grand nombre de kilomètres.", "Mesurer sa consommation aux 100 km avant et après avoir adopté ces gestes.", "On ne conduit jamais pareil une fois qu'on a vu sa propre consommation chiffrée.", "Litres aux 100 km, avant/après", "Le commercial itinérant roule beaucoup sans jamais avoir mesuré l'effet de sa façon de conduire sur sa consommation.", "Comme cuisiner sans jamais peser les ingrédients : on peut toujours faire mieux, encore faut-il mesurer ce qu'on fait déjà.", "En 4 semaines, mesure ta consommation aux 100 km avant et après avoir testé 3 gestes d'éco-conduite.", "Note ta consommation actuelle aux 100 km sur tes trajets habituels.", "Adopte 3 gestes simples (anticipation, régime moteur, pression des pneus) pendant les 4 semaines.", "Remesure ta consommation et compare l'écart.", "La pression des pneus est le geste le plus simple et le plus souvent négligé.", "Anticiper les freinages économise plus de carburant que rouler lentement sur autoroute.", "Partage ton écart avant/après avec tes collègues conducteurs : la comparaison motive.", "Le pied le plus écologique est celui qui lève le pied avant le feu rouge, pas dessus.", new System.DateTime(2026, 11, 15) },
                { "POSTE-C04", (int)Domain.Entities.NiveauCarte.Debutant, "Optimiser avant d'électrifier", "Comprendre les leviers d'optimisation logistique : taux de remplissage, kilomètres évités, mutualisation.", "Savoir lire un taux de remplissage et un kilométrage à vide.", "Tester une optimisation de tournée ou de remplissage mesurable.", "Le camion à moitié vide pollue presque autant que le camion plein.", "Taux de remplissage, km évités", "Les tournées logistiques sont organisées comme depuis toujours, sans qu'on ait jamais vérifié leur taux de remplissage réel.", "Comme faire ses courses chaque jour pour un seul article : on pourrait regrouper, mais l'habitude l'emporte.", "En 4 semaines, teste une optimisation de tournée ou de remplissage et mesure les kilomètres évités.", "Mesure le taux de remplissage actuel d'une tournée type.", "Identifie une optimisation possible (regroupement, nouvel itinéraire, mutualisation).", "Teste-la et mesure les kilomètres ou le carburant économisés.", "Commence par les tournées les plus fréquentes : le gain s'y multiplie le plus vite.", "La mutualisation avec un autre service ou un partenaire proche est souvent le levier le plus rapide.", "Un logiciel d'optimisation de tournées n'est utile que si on mesure d'abord le problème à la main.", "Le meilleur trajet logistique, c'est celui qu'on ne fait qu'une fois au lieu de deux.", new System.DateTime(2026, 11, 15) },
                { "POSTE-C05", (int)Domain.Entities.NiveauCarte.Debutant, "Faire durer son matériel numérique", "Comprendre pourquoi la fabrication du matériel numérique pèse plus lourd que son usage quotidien.", "Identifier les leviers pour allonger la durée de vie du matériel.", "Nettoyer son stockage numérique et mesurer l'espace libéré.", "Le matériel numérique le plus écologique est celui qu'on n'a pas encore remplacé.", "Stockage nettoyé, durée de vie allongée", "Le parc informatique est renouvelé par habitude, sans vérifier si le matériel existant pourrait encore servir.", "Comme changer de téléphone parce qu'il est « vieux » alors qu'il fonctionne encore très bien.", "En 4 semaines, nettoie ton stockage numérique et repousse d'au moins un geste le remplacement de ton matériel.", "Fais l'inventaire de ton matériel actuel et de son âge réel.", "Nettoie tes espaces de stockage (mails, fichiers, cloud) et mesure l'espace libéré.", "Repousse d'un geste concret (réparation, mise à jour) le remplacement d'un équipement.", "Le stockage inutile (mails, doublons, vieux fichiers) est le geste le plus rapide à nettoyer.", "Une batterie remplacée coûte souvent 10 fois moins cher qu'un appareil neuf.", "Demande toujours s'il existe une option de réparation avant de commander du neuf.", "Le Go le plus propre, c'est celui qu'on a supprimé, pas celui qu'on a stocké ailleurs.", new System.DateTime(2026, 11, 15) },
                { "POSTE-C06", (int)Domain.Entities.NiveauCarte.Debutant, "Viser zéro gaspillage", "Comprendre la hiérarchie prévention, tri, réemploi appliquée aux déchets de production ou de bureau.", "Savoir peser un flux de déchets avant d'agir.", "Mesurer le taux de tri avant et après une action simple.", "Trier mieux, c'est bien ; produire moins de déchets, c'est mieux.", "Tonnes avant/après, taux de tri", "Les bennes se remplissent chaque semaine sans que personne n'ait jamais vérifié ce qu'elles contiennent vraiment.", "Comme un tri sélectif approximatif à la maison : on fait un effort, mais sans jamais vérifier si c'est vraiment efficace.", "En 4 semaines, mesure un flux de déchets avant et après avoir mis en place une action de tri ou de réduction.", "Choisis un flux de déchets visible et pèse-le sur une semaine de référence.", "Mets en place une action simple (bac de tri supplémentaire, consigne de réduction).", "Repèse le flux après quelques semaines et compare le taux de tri.", "Un bac de tri bien placé change plus de comportements qu'une affiche de consigne.", "Cherche d'abord à réduire le déchet à la source avant d'améliorer le tri.", "Affiche le résultat pesé : les chiffres convainquent plus que les bonnes intentions.", "La meilleure benne, c'est celle qui pèse le moins lourd à la pesée suivante.", new System.DateTime(2026, 11, 15) },
                { "POSTE-C07", (int)Domain.Entities.NiveauCarte.Debutant, "Concevoir en pensant à la fin de vie", "Comprendre les principes d'écoconception : matériaux, durabilité, fin de vie d'un produit ou service.", "Savoir calculer l'empreinte d'un produit avant et après modification.", "Modifier un produit ou service existant selon un critère d'écoconception.", "Un produit bien conçu pense à sa fin de vie dès son premier croquis.", "Empreinte recalculée après modification", "Le bureau d'études conçoit des produits performants, mais n'a jamais intégré de critère environnemental dans son cahier des charges.", "Comme construire une maison sans penser à son chauffage futur : on corrige après, bien plus cher qu'en amont.", "D'ici la fin du défi, modifie un produit ou service existant selon un critère d'écoconception et recalcule son empreinte.", "Choisis un produit ou service existant et calcule une première estimation de son empreinte.", "Identifie un critère d'écoconception à améliorer (matériau, durabilité, réparabilité).", "Modifie le produit ou le service selon ce critère et recalcule l'empreinte.", "Commence par le critère le plus simple à changer sans remettre en cause toute la conception.", "La durabilité et la réparabilité sont souvent les leviers les plus sous-exploités en écoconception.", "Implique le marketing dès la conception : un produit écoconçu mal présenté perd son avantage.", "Le meilleur produit écoconçu, c'est celui qu'on n'a pas besoin de remplacer avant longtemps.", new System.DateTime(2026, 11, 15) },
                { "POSTE-C08", (int)Domain.Entities.NiveauCarte.Debutant, "Servir bas carbone sans perdre en plaisir", "Comprendre les leviers de réduction carbone dans l'alimentation collective (produits, menus, gaspillage).", "Savoir revoir un menu ou un événement sans sacrifier la qualité perçue.", "Mesurer les tCO2e par repas avant et après une évolution de menu.", "Un menu bas carbone ne doit jamais ressembler à une punition.", "tCO2e par repas, avant/après", "La restauration collective ou les séminaires proposent des menus classiques, jamais pensés sous l'angle carbone.", "Comme organiser un repas de famille sans jamais varier le menu : on pourrait surprendre agréablement, mais l'habitude domine.", "En 4 semaines, revois un menu ou un événement et mesure les tCO2e par repas avant et après.", "Calcule une estimation carbone du menu ou de l'événement actuel.", "Propose une évolution (plus de végétal, produits locaux, moins de gaspillage) sans sacrifier le plaisir.", "Recalcule l'empreinte du nouveau menu et compare.", "Augmenter la part végétale d'un repas est souvent le levier le plus efficace et le moins coûteux.", "Présente le changement comme une amélioration gustative, pas comme une contrainte écologique.", "Mesure aussi le gaspillage alimentaire : réduire les quantités jetées compte autant que le contenu de l'assiette.", "Le meilleur menu bas carbone, c'est celui que personne ne devine en le mangeant.", new System.DateTime(2026, 11, 15) },
                { "IA-C01", (int)Domain.Entities.NiveauCarte.Debutant, "L'IA, assistante du reporting", "Comprendre les tâches de reporting carbone que l'IA générative peut réellement accélérer (synthèse, mise en forme, relance de collecte).", "Savoir rédiger une instruction (prompt) efficace pour un reporting carbone.", "Distinguer ce que l'IA peut produire seule de ce qui nécessite toujours une vérification humaine.", "L'IA rédige vite, mais c'est toujours l'humain qui valide le chiffre final.", "Mon assistant de reporting", "Le référent carbone passe des heures chaque mois à mettre en forme les mêmes données venues de services différents.", "Comme un assistant qui préparerait le premier jet d'un rapport : ça ne remplace pas la relecture, mais ça fait gagner un temps fou.", "D'ici la fin de la journée, repars avec un assistant ou un modèle de reporting opérationnel sur tes propres données carbone.", "Identifie une tâche répétitive de ton reporting (mise en forme, relance, synthèse).", "Construis un modèle d'instruction (prompt) réutilisable pour cette tâche.", "Teste-le sur tes propres données et vérifie la qualité du résultat avant de l'adopter.", "Ne confie jamais à l'IA un chiffre final sans vérification humaine : elle aide à produire, pas à certifier.", "Garde une trace de l'instruction qui fonctionne : elle te resservira chaque mois.", "Commence par la tâche la plus répétitive et la moins sensible pour gagner en confiance.", "Le meilleur assistant IA, c'est celui qui te fait gagner du temps sans jamais te faire perdre la main sur le chiffre.", new System.DateTime(2026, 11, 15) },
                { "IA-C02", (int)Domain.Entities.NiveauCarte.Debutant, "L'IA n'est pas gratuite pour la planète", "Comprendre pourquoi chaque requête à un modèle d'IA générative a un coût énergétique réel.", "Identifier les usages les plus coûteux (requêtes répétées, générations d'images, longs prompts inutiles).", "Adopter des réflexes simples pour un usage plus frugal de l'IA générative.", "Chaque question posée à une IA a un coût, même si on ne le voit jamais sur une facture.", "Usages recensés, empreinte estimée", "Les salariés utilisent de plus en plus l'IA générative au quotidien, sans savoir que cela a un coût environnemental réel.", "Comme laisser un robinet couler en se disant que l'eau est gratuite : l'absence de facture visible masque le vrai coût.", "En 4 semaines, recense tes usages d'IA générative et adopte 2 bonnes pratiques pour réduire leur empreinte.", "Recense tes usages actuels d'IA générative pendant une semaine (fréquence, type de requêtes).", "Identifie les usages les plus coûteux (requêtes répétées, générations inutiles) et adopte 2 bonnes pratiques.", "Réévalue tes usages après quelques semaines et estime l'écart.", "Reformule une requête imprécise plutôt que de la relancer plusieurs fois : ça réduit le nombre de requêtes.", "Réserve la génération d'images ou de vidéos aux usages qui en ont vraiment besoin.", "Privilégie un modèle plus petit et plus sobre quand la tâche ne demande pas la puissance maximale.", "La meilleure requête à l'IA, c'est parfois celle qu'on n'a pas eu besoin de reposer trois fois.", new System.DateTime(2026, 11, 15) },
                { "IA-C03", (int)Domain.Entities.NiveauCarte.Debutant, "Concevoir sobre dès le départ", "Comprendre que la taille du modèle, le lieu d'hébergement et la fréquence d'usage déterminent l'essentiel de l'empreinte d'un projet d'IA.", "Savoir questionner un besoin IA pour vérifier s'il justifie un modèle lourd.", "Identifier des alternatives plus sobres sans sacrifier la performance utile.", "Le projet d'IA le plus sobre est souvent celui qui n'avait pas besoin du plus gros modèle.", "Un projet revu, trois critères vérifiés", "L'équipe informatique s'apprête à déployer un projet d'IA sans avoir questionné sa taille, son hébergement ni sa fréquence d'usage réelle.", "Comme prendre un camion pour transporter une seule caisse : la puissance choisie dépasse largement le besoin réel.", "D'ici la fin de la journée, revois un projet d'IA de l'entreprise selon 3 critères : taille du modèle, hébergement, fréquence d'usage.", "Choisis un projet d'IA existant ou en cours de conception dans l'entreprise.", "Évalue-le sur les 3 critères : le modèle est-il surdimensionné, l'hébergement est-il choisi pour sa sobriété, l'usage est-il vraiment fréquent ?", "Propose une alternative plus sobre sur au moins un des 3 critères.", "Un modèle plus petit, bien choisi, suffit souvent largement à la tâche demandée.", "Vérifie où est hébergé le modèle utilisé : certains hébergeurs sont bien plus sobres que d'autres.", "Interroge toujours la fréquence réelle d'usage avant de dimensionner l'infrastructure.", "La meilleure architecture IA, c'est celle qui résiste à la question « en a-t-on vraiment besoin ? ».", new System.DateTime(2026, 11, 15) },
                { "IA-C04", (int)Domain.Entities.NiveauCarte.Debutant, "Un tableau de bord qui se met à jour tout seul (presque)", "Comprendre comment un tableur couplé à l'IA peut automatiser une partie de la mise à jour d'un tableau de bord.", "Identifier les indicateurs terrain à suivre pour sa propre trajectoire.", "Construire des formules et des instructions IA réutilisables, pas un fichier à refaire chaque mois.", "Le meilleur tableau de bord est celui qu'on met à jour en 10 minutes, pas en 2 heures.", "Mon tableau de bord alimenté", "Le pilote de trajectoire SNBC passe plus de temps à mettre en forme son tableau de bord qu'à analyser les écarts qu'il révèle.", "Comme refaire sa feuille de calcul budgétaire à la main chaque mois, alors qu'une formule suffirait à tout recalculer.", "D'ici la fin de la journée, repars avec un tableau de bord fonctionnel, alimenté par tes propres indicateurs terrain.", "Liste les 5 indicateurs terrain que tu dois suivre régulièrement.", "Construis les formules du tableur pour calculer automatiquement les écarts à la trajectoire.", "Ajoute une instruction IA réutilisable pour générer la synthèse mensuelle à partir de ce tableau.", "Automatise d'abord le calcul des écarts avant de penser à la mise en forme visuelle.", "Garde le tableau simple : un tableau de bord trop complexe finit par ne plus être mis à jour.", "Teste ton instruction IA sur un mois de données réelles avant de l'adopter définitivement.", "Un tableau de bord qu'on met à jour chaque mois vaut mieux qu'un tableau parfait qu'on abandonne au bout de deux.", new System.DateTime(2026, 11, 15) },
                { "GRA-C01", (int)Domain.Entities.NiveauCarte.Debutant, "Structurer avant de se lancer", "Comprendre les étapes clés d'une démarche de transition écologique et sociale réussie.", "Distinguer les actions rapides des actions structurantes sur 12 mois.", "Savoir faire valider une feuille de route par un comité de direction.", "Une démarche sans feuille de route finit toujours par se diluer dans l'urgence du quotidien.", "Ma feuille de route validée", "La PME multiplie les bonnes initiatives ponctuelles sur l'écologie et le social, sans vision d'ensemble ni validation formelle de la direction.", "Comme rénover une maison pièce par pièce sans plan d'ensemble : chaque chantier est utile, mais rien ne s'articule.", "D'ici la fin du parcours, rédige une feuille de route sur 12 mois et fais-la valider en comité de direction.", "Recense les actions déjà en cours sur le plan écologique et social de l'entreprise.", "Structure une feuille de route sur 12 mois avec des jalons trimestriels.", "Présente-la en comité de direction pour validation et arbitrage des moyens.", "Une feuille de route sur une seule page passe mieux en comité de direction qu'un document de 20 pages.", "Prévois des jalons trimestriels : ils permettent d'ajuster sans tout remettre en cause.", "Fais valider les moyens en même temps que les objectifs, pas après.", "La meilleure feuille de route, c'est celle qu'on ressort vraiment au comité suivant.", new System.DateTime(2026, 11, 15) },
                { "GRA-C02", (int)Domain.Entities.NiveauCarte.Debutant, "Évaluer pour mieux poursuivre", "Comprendre pourquoi une action sans bilan d'impact finit par être abandonnée faute de preuve de son utilité.", "Savoir mesurer une action sur 3 dimensions : tCO2e évitées, coût réel, adhésion des équipes.", "Distinguer un bilan honnête d'un bilan qui ne cherche qu'à se rassurer.", "Une action qu'on ne mesure jamais finit toujours par sembler inutile, même quand elle a marché.", "Mon bilan d'impact chiffré", "L'entreprise a mené une action de transition l'an dernier, mais personne n'a jamais vérifié si elle a vraiment eu l'effet espéré.", "Comme suivre un régime sans jamais remonter sur la balance : on espère que ça marche, sans jamais vérifier.", "D'ici la fin de la journée, produis un bilan d'impact chiffré (tCO2e, coût, adhésion) d'une action déjà menée dans ton entreprise.", "Choisis une action de transition déjà menée, même modeste.", "Rassemble les données disponibles sur son coût, son effet carbone et son accueil par les équipes.", "Rédige un bilan honnête, y compris si le résultat est mitigé.", "Un bilan mitigé mais honnête est plus utile qu'un bilan flatteur mais faux.", "Interroge directement les équipes concernées sur leur perception de l'action : c'est une donnée à part entière.", "Documente aussi ce qui n'a pas marché : ça évite de répéter la même erreur.", "Le meilleur bilan d'impact n'est pas celui qui flatte, c'est celui qui sert à mieux faire la prochaine fois.", new System.DateTime(2026, 11, 15) },
                { "GRA-C03", (int)Domain.Entities.NiveauCarte.Debutant, "Dire vrai, dire fort", "Comprendre ce qui distingue une communication légitime d'un greenwashing qui finit toujours par se retourner contre l'entreprise.", "Savoir choisir un chiffre réellement vérifiable pour appuyer une communication.", "Rédiger un message qui informe sans surpromettre.", "Un chiffre vérifiable convainc plus durablement qu'un slogan flatteur.", "Mon support basé sur des faits", "L'entreprise a de vrais résultats à communiquer, mais craint soit de trop en dire, soit de tomber dans l'exagération qu'on lui reprochera.", "Comme présenter ses résultats scolaires à ses parents : mieux vaut un bulletin honnête qu'une note gonflée qui se découvre vite.", "D'ici la fin de la journée, produis un support de communication (page web, post, réponse client) basé sur un résultat réellement chiffré de ton entreprise.", "Choisis un résultat déjà mesuré et vérifiable de ta démarche de transition.", "Rédige un message clair qui présente ce résultat sans l'exagérer ni le généraliser.", "Fais relire le message par quelqu'un qui n'a pas participé à l'action, pour vérifier qu'il ne surpromet pas.", "N'utilise jamais un mot comme « neutre » ou « zéro impact » sans preuve vérifiable derrière.", "Un chiffre précis et modeste rassure plus qu'un superlatif non sourcé.", "Mentionne aussi les limites de l'action : ça renforce la crédibilité du message.", "La meilleure communication verte est celle qu'on peut défendre chiffre en main face à un client sceptique.", new System.DateTime(2026, 11, 15) },
                { "GRA-C04", (int)Domain.Entities.NiveauCarte.Debutant, "Travailler quand il fait trop chaud", "Comprendre les risques santé réels liés aux vagues de chaleur sur un poste de travail (fatigue, baisse de vigilance, risques accrus).", "Savoir repérer les postes de travail les plus exposés à la chaleur sur un site.", "Identifier des mesures d'adaptation simples et peu coûteuses.", "Un poste de travail qui ignore la chaleur finit par ignorer aussi la sécurité.", "Diagnostic chaleur, 3 mesures en place", "Les vagues de chaleur se multiplient, mais l'organisation du travail n'a pas changé depuis des années.", "Comme continuer à faire du sport intensif en pleine canicule sans adapter son rythme : le corps finit par le payer.", "En 4 semaines, diagnostique un site face à la chaleur et mets en place 3 mesures concrètes.", "Repère les postes de travail les plus exposés à la chaleur (extérieur, proximité de machines, zones mal ventilées).", "Identifie 3 mesures simples (horaires décalés, points d'eau, zones d'ombre ou de fraîcheur).", "Mets-les en place et vérifie leur usage réel après quelques semaines.", "Décaler les horaires des tâches les plus pénibles coûte zéro euro et protège immédiatement.", "L'accès à l'eau fraîche doit être visible et facile, pas théorique.", "Implique directement les salariés les plus exposés dans le choix des mesures : ils connaissent les vrais points chauds du site.", "La meilleure mesure canicule, c'est celle que les salariés utilisent vraiment, pas celle qui est juste affichée.", new System.DateTime(2026, 11, 15) },
                { "GRA-C05", (int)Domain.Entities.NiveauCarte.Debutant, "Renaturer, même un petit bout", "Comprendre l'effet d'un espace renaturé sur la biodiversité locale et le confort thermique du site.", "Savoir organiser un chantier collectif simple, sans compétence paysagiste préalable.", "Choisir des essences locales adaptées plutôt que des plantations décoratives classiques.", "Un mètre carré végétalisé vaut mieux qu'un hectare de plan jamais commencé.", "Une zone transformée, avec les salariés", "Le site est entièrement bitumé ou minéral, sans aucun espace de nature, et personne n'a encore osé proposer de changer ça.", "Comme une cour d'école entièrement goudronnée : il suffit parfois d'un coin de terre retrouvé pour tout changer.", "Organise un chantier collectif pour débitumer ou végétaliser une zone du site avec des salariés volontaires.", "Repère une petite zone du site adaptée à la renaturation (coin de parking, talus, espace délaissé).", "Prépare le chantier avec des essences locales et du matériel simple.", "Organise le chantier collectif avec les salariés volontaires et documente le résultat.", "Commence petit : une seule zone réussie donne envie de recommencer ailleurs.", "Choisis des essences locales : elles demandent moins d'entretien et profitent mieux à la biodiversité locale.", "Fais du chantier un moment convivial, pas juste une corvée collective.", "La meilleure renaturation, c'est celle que les salariés viennent arroser d'eux-mêmes après le chantier.", new System.DateTime(2026, 11, 15) },
                { "GRA-C06", (int)Domain.Entities.NiveauCarte.Debutant, "Le sol, un puits de carbone oublié", "Comprendre comment les haies, bocages et couverts végétaux stockent du carbone dans les sols.", "Savoir estimer, même simplement, le carbone stocké par un linéaire de haies ou un couvert.", "Identifier le bon emplacement pour maximiser l'effet du projet (érosion, biodiversité, stockage).", "Une haie bien placée travaille pour le sol, le climat et la biodiversité en même temps.", "Mon projet chiffré en carbone stocké", "L'exploitation a arraché des haies par le passé pour agrandir les parcelles, et cherche aujourd'hui à inverser la tendance sans savoir par où commencer.", "Comme reconstruire un mur de pierres sèches après l'avoir démoli pour gagner de la place : on comprend après coup ce qu'il protégeait.", "D'ici la fin du défi, chiffre un projet de plantation de haies ou de couverts végétaux en carbone stocké.", "Repère les parcelles qui bénéficieraient le plus d'une haie ou d'un couvert (érosion, exposition au vent).", "Estime, même simplement, le linéaire ou la surface concernée et le carbone stocké attendu.", "Présente le projet chiffré à l'exploitation ou à la coopérative pour décision.", "Privilégie les essences locales, déjà adaptées au sol et au climat de la parcelle.", "Une haie protège aussi contre l'érosion et le vent : valorise ces bénéfices en plus du carbone.", "Un couvert végétal hivernal est souvent plus rapide à mettre en place qu'une haie pour un premier résultat.", "Le meilleur puits de carbone agricole est parfois celui qu'on a arraché par erreur il y a 20 ans.", new System.DateTime(2026, 11, 15) },
                { "POSTE-C09", (int)Domain.Entities.NiveauCarte.Debutant, "L'eau, une pollution souvent invisible", "Comprendre que les pollutions de l'eau sont souvent invisibles (rejets diffus, fuites, produits mal rincés).", "Savoir mesurer sa consommation d'eau sur un poste ou un process précis.", "Identifier et traiter une source de pollution ou de surconsommation.", "L'eau qui fuit en silence coûte aussi cher que celle qu'on gaspille à vue.", "m³ avant/après, une source traitée", "Le site consomme de l'eau et génère des rejets sans qu'on ait jamais cherché les fuites ou les pollutions diffuses.", "Comme un robinet qui goutte toute la nuit : invisible en plein jour, mais qui coûte cher sur l'année.", "En 4 semaines, mesure la consommation d'eau d'un poste et traite une source de pollution ou de fuite identifiée.", "Repère un poste ou un process consommant ou rejetant de l'eau de façon visible.", "Mesure sa consommation actuelle et identifie une fuite ou une pollution diffuse.", "Traite la source identifiée et remesure après quelques semaines.", "Les fuites sont souvent le gisement d'économie le plus simple et le plus rapide à traiter.", "Vérifie les rejets après nettoyage de matériel : c'est une pollution diffuse fréquente et sous-estimée.", "Implique la production dans le diagnostic : elle connaît les vraies habitudes du terrain.", "La meilleure économie d'eau, c'est la fuite qu'on a enfin réparée après des années.", new System.DateTime(2026, 11, 15) },
                { "GRA-C07", (int)Domain.Entities.NiveauCarte.Debutant, "Former comme on anime un défi", "Comprendre les principes d'une formation-action : apprendre en faisant, pas en écoutant.", "Savoir structurer une séquence pédagogique courte et actionnable.", "Distinguer transmettre un contenu et faire vivre une expérience.", "On retient 10 % de ce qu'on entend, et bien plus de ce qu'on a fait soi-même.", "Ma séquence conçue et animée", "Le référent maîtrise son sujet, mais n'a jamais conçu de formation : ses tentatives ressemblent plus à des présentations qu'à des formations-actions.", "Comme un expert qui raconte son métier sans jamais faire pratiquer ses élèves : ils retiennent l'anecdote, pas le geste.", "Conçois une séquence de formation-action, puis anime-la réellement auprès de tes collègues.", "Choisis un sujet que tu maîtrises et formule un objectif d'action, pas seulement de connaissance.", "Construis une séquence avec un temps de mise en pratique au moins aussi long que le temps d'apport.", "Anime la séquence en conditions réelles et récolte les retours des participants.", "Un objectif de formation-action commence toujours par un verbe d'action (faire, tester, produire), jamais « comprendre » seul.", "Prévois toujours plus de temps de pratique que de temps d'explication.", "Récolte les retours à chaud, juste après la séquence : c'est là qu'ils sont les plus sincères.", "La meilleure formation, c'est celle dont on se souvient par ce qu'on a fait, pas par ce qu'on a écouté.", new System.DateTime(2026, 11, 15) },
                { "GRA-C08", (int)Domain.Entities.NiveauCarte.Debutant, "Dessiner pour mieux faire comprendre", "Comprendre pourquoi une information visuelle se retient mieux qu'un tableau de texte.", "Maîtriser quelques pictogrammes et structures simples de sketchnoting, sans être dessinateur.", "Transformer un plan d'action textuel en fresque visuelle lisible par tous.", "On ne retient pas un plan d'action qu'on n'a jamais vraiment regardé.", "Ma fresque affichée", "Le plan d'action de l'équipe existe dans un document Word que personne ne relit plus après la réunion de lancement.", "Comme une notice de montage en texte dense : on finit par improviser faute d'avoir vraiment lu, alors qu'un schéma aurait suffi.", "D'ici la fin de la journée, transforme ton plan d'action en fresque visuelle et affiche-la dans l'entreprise.", "Choisis un plan d'action existant à transformer en visuel.", "Apprends et utilise quelques pictogrammes simples (flèches, bulles, icônes) pour structurer l'information.", "Dessine la fresque finale et affiche-la dans un lieu de passage de l'entreprise.", "Pas besoin de savoir bien dessiner : un pictogramme simple et clair vaut mieux qu'un dessin compliqué.", "Utilise toujours les mêmes codes visuels (couleur, forme) pour une même catégorie d'information.", "Affiche la fresque là où les équipes passent vraiment, pas dans un bureau fermé.", "La meilleure fresque, c'est celle que les gens s'arrêtent regarder sans qu'on le leur demande.", new System.DateTime(2026, 11, 15) },
                { "GRA-C09", (int)Domain.Entities.NiveauCarte.Debutant, "La forme compte autant que le fond", "Comprendre l'impact de la posture et de la voix sur la façon dont un message est reçu par une direction.", "Identifier ses propres tics de posture ou de voix qui affaiblissent un message pourtant solide.", "Structurer un pitch court et mémorable sur une action de transition.", "Une bonne action mal présentée convainc moins qu'une présentation bien menée.", "Mon pitch filmé et débriefé", "Le manager a un vrai résultat à présenter en comité de direction, mais perd ses moyens et son message dès qu'il doit le pitcher à l'oral.", "Comme un très bon musicien qui perd ses moyens sur scène faute d'avoir répété devant un public : le talent existe, la posture manque.", "D'ici la fin de la journée, présente ton action filmée devant le groupe, puis débriefe ta prestation avec le formateur.", "Prépare un pitch court (2-3 minutes) structuré autour de ton action de transition.", "Présente-le devant le groupe, filmé, dans les conditions les plus proches d'un vrai comité.", "Visionne et débriefe ta prestation : posture, voix, clarté du message.", "Un silence de 2 secondes avant de répondre à une question vaut mieux qu'une réponse précipitée et confuse.", "Travaille ta respiration avant de parler : elle stabilise la voix bien plus qu'on ne le croit.", "Prépare 3 chiffres clés à retenir par cœur : ils rassurent une direction mieux qu'un long discours.", "On ne se souvient jamais de la direction qui a bâillé, mais toujours de celle qui a été convaincue.", new System.DateTime(2026, 11, 15) },
                { "PART-C01", (int)Domain.Entities.NiveauCarte.Debutant, "Enquêter avant de choisir", "Comprendre pourquoi l'enquête métier révèle plus qu'une fiche métier théorique.", "Savoir préparer et mener un entretien avec un professionnel sans se contenter de questions superficielles.", "Classer ses pistes métier à partir de ce qu'on a vraiment appris sur le terrain.", "On ne choisit pas un métier qu'on n'a jamais vu de près.", "3 enquêtes menées", "La personne en reconversion a des pistes de métiers de la transition en tête, mais seulement à partir de ce qu'elle en imagine.", "Comme choisir un restaurant seulement sur sa façade : l'enquête terrain, c'est entrer et goûter avant de se décider.", "Mène au moins 3 enquêtes métier auprès de professionnels de la transition écologique et classe tes pistes à l'issue.", "Identifie 3 métiers de la transition qui t'intéressent, même vaguement.", "Prépare des questions concrètes sur le quotidien réel de chaque métier, pas seulement sur le diplôme requis.", "Mène les 3 enquêtes et note ce qui confirme ou infirme ton intérêt initial.", "Pose toujours une question sur une mauvaise journée type : elle révèle plus que la description officielle du poste.", "Note tes impressions juste après chaque enquête, à chaud, avant qu'elles ne s'estompent.", "N'hésite pas à solliciter un deuxième professionnel du même métier si le premier avis te surprend.", "La meilleure fiche métier, c'est toujours celle racontée par quelqu'un qui la vit.", new System.DateTime(2026, 11, 15) },
                { "PART-C02", (int)Domain.Entities.NiveauCarte.Debutant, "Valider sur le terrain, pas sur le papier", "Comprendre pourquoi une immersion révèle des aspects d'un métier qu'aucune enquête ne peut montrer.", "Savoir préparer une immersion utile (objectifs, questions à vérifier sur place).", "Transformer les enseignements de l'immersion en décision de projet.", "Le métier qu'on croit aimer sur le papier n'est validé que celui qu'on a vécu une journée.", "Mon projet validé ou ajusté", "La personne a une piste de métier préférée après ses enquêtes, mais hésite encore à s'engager dans une formation ou une reconversion.", "Comme essayer une voiture avant de l'acheter : la fiche technique ne remplace jamais le test sur route.", "Réalise une immersion sur le métier retenu, puis valide ou ajuste ton projet professionnel à l'issue.", "Organise une immersion d'au moins une journée dans le métier retenu.", "Prépare en amont les points précis à vérifier pendant l'immersion (rythme, gestes, relations).", "À l'issue, confronte ce que tu as vécu à ton projet initial et ajuste-le si besoin.", "Une immersion courte mais bien préparée vaut mieux qu'une immersion longue sans objectif clair.", "N'hésite pas à poser des questions gênantes pendant l'immersion : c'est le moment où elles ont le plus de valeur.", "Un projet ajusté après immersion n'est pas un échec : c'est l'immersion qui a fait son travail.", "Le meilleur projet professionnel, c'est celui qui a survécu à une vraie journée de terrain.", new System.DateTime(2026, 11, 15) },
                { "PART-C03", (int)Domain.Entities.NiveauCarte.Debutant, "S'orienter par la rencontre", "Découvrir la diversité des métiers verts, bien au-delà des représentations les plus connues.", "Savoir préparer une rencontre avec un professionnel pour en tirer de vrais enseignements.", "Confronter ses envies initiales à la réalité racontée par un professionnel.", "On ne s'oriente pas bien à partir de ce qu'on imagine d'un métier, mais de ce qu'on en apprend.", "Mon choix confronté à la réalité", "Le jeune a une idée d'orientation vers les métiers verts, mais fondée surtout sur des images d'Épinal ou les réseaux sociaux.", "Comme choisir des études seulement sur le nom du métier, sans jamais avoir parlé à quelqu'un qui l'exerce vraiment.", "Rencontre au moins un professionnel d'un métier vert qui t'intéresse et confronte ton idée initiale à ce qu'il te raconte.", "Identifie un métier vert qui t'attire, même vaguement défini.", "Prépare 3 questions concrètes sur le quotidien réel de ce métier.", "Rencontre un professionnel et note ce qui confirme ou change ta vision du métier.", "Pose des questions sur les études réellement suivies, pas seulement sur le métier final.", "N'hésite pas à rencontrer plusieurs professionnels d'un même métier : les parcours varient beaucoup.", "Accepte qu'une rencontre change ton idée de départ : c'est le but de la démarche.", "Le meilleur conseil d'orientation vient toujours de quelqu'un qui fait déjà le métier.", new System.DateTime(2026, 11, 15) },
                { "PART-C04", (int)Domain.Entities.NiveauCarte.Debutant, "Un premier pas vers la transition", "Découvrir un panorama large des métiers de la transition écologique, au-delà des idées reçues.", "Identifier des pistes correspondant à ses propres intérêts et contraintes.", "Planifier une première prise de contact concrète pour avancer après l'atelier.", "Le plus dur n'est pas de choisir un métier, c'est d'oser la première prise de contact.", "3 pistes et un premier contact planifié", "La personne s'interroge sur son avenir professionnel sans savoir par où commencer à chercher des pistes concrètes.", "Comme vouloir changer de ville sans jamais avoir regardé une carte : l'envie existe, le premier pas manque.", "D'ici la fin de l'atelier, repars avec 3 métiers de la transition à explorer et une première prise de contact planifiée.", "Découvre le panorama des métiers présentés pendant l'atelier.", "Choisis les 3 qui résonnent le plus avec tes intérêts et tes contraintes personnelles.", "Planifie une première prise de contact concrète (bilan de compétences, enquête métier, salon) avant de quitter l'atelier.", "Ne cherche pas à choisir un métier définitif dès cet atelier : l'objectif est d'ouvrir des pistes, pas de trancher.", "Planifie la prise de contact avec une date précise, pas un vague « bientôt ».", "Partage tes 3 pistes avec un proche : en parler aide souvent à préciser son envie.", "Le métier de demain commence souvent par un simple message envoyé aujourd'hui.", new System.DateTime(2026, 11, 15) }
                });

            // Etapes et jointures Etape<->Carte : sous-requetes sur Titre/Code (uniques),
            // les identifiants generes n'etant pas connus a l'ecriture de cette migration.
            // Certaines etapes reutilisent une Carte deja inseree par
            // SeedPiloteSnbcTransport (SNBC-C01, cadrage commun aux Challenges sectoriels).
            migrationBuilder.Sql(@"
INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Atelier Fresque du Climat + « premiers pas »'),
    1,
    N'Atelier Fresque du Climat + premiers pas',
    N'Comprendre les mécanismes du changement climatique à partir des données scientifiques du GIEC, en équipe.',
    N'Comprendre les enjeux climatiques pour agir',
    N'En fin d''atelier, chaque équipe formule et priorise 3 idées d''actions concrètes à tester dans son quotidien professionnel.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Fresque du Numérique + défi numérique'),
    1,
    N'Fresque du Numérique + défi numérique',
    N'Comprendre l''empreinte environnementale du numérique : fabrication des terminaux, data centers, réseaux.',
    N'Évaluer et réduire son empreinte numérique',
    N'L''équipe choisit un premier défi numérique à tester sur 4 semaines (durée de vie du matériel, stockage, usages).'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Atelier 2tonnes'),
    1,
    N'Atelier 2tonnes',
    N'Comprendre l''ampleur de l''effort de réduction nécessaire à l''échelle individuelle et collective (objectif 2 tonnes de CO2e par personne et par an).',
    N'Passer du constat à l''engagement personnel',
    N'Chaque participant formule un engagement individuel ; l''équipe formule un engagement collectif, les deux datés et suivis.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Matinale dirigeants : où en est votre entreprise face à la SNBC 3 et à vos donneurs d''ordres ?'),
    1,
    N'Matinale dirigeants SNBC 3 et donneurs d''ordres',
    N'Comprendre les obligations et attentes carbone qui montent (SNBC 3, BEGES, demandes des donneurs d''ordres).',
    N'Évaluer sa propre maturité carbone en tant que dirigeant',
    N'Chaque dirigeant remplit un auto-diagnostic et, s''il le souhaite, prend rendez-vous pour un pré-diagnostic carbone gratuit.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi Low-tech d''équipe'),
    1,
    N'Défi Low-tech d''équipe',
    N'Comprendre les principes low-tech (sobriété, réparabilité, accessibilité) et leur application en entreprise.',
    N'Concevoir une solution low-tech en équipe',
    N'L''équipe conçoit et teste un vrai prototype low-tech sur le site dans la journée (ex. récupérateur, isolation simple, outil réparé).'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Pilote climat : porter le plan de transition dans l''entreprise'),
    1,
    N'Prendre en main le plan de transition',
    N'Comprendre la structure d''un plan de transition carbone et le rôle du pilote qui le porte au quotidien.',
    N'Porter un plan de transition carbone',
    N'Entre les 3 sessions, le pilote lance 3 actions concrètes issues du plan et les présente, chiffrées, à la dernière session.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Du Bilan Carbone® au plan d''action : choisir ses 3 premières actions'),
    1,
    N'Choisir ses 3 premières actions',
    N'Savoir lire un Bilan Carbone® et identifier les postes d''émission les plus actionnables à court terme.',
    N'Arbitrer un premier plan d''action carbone',
    N'À l''issue de la journée, 3 actions sont chiffrées (tCO2e, coût, délai) et arbitrées avec la direction.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi collecte : réunir les données carbone en équipe'),
    1,
    N'Défi collecte de données carbone',
    N'Comprendre quelles données sont nécessaires à un Bilan Carbone® fiable et où les trouver dans son service.',
    N'Collecter et sourcer des données carbone fiables',
    N'Chaque correspondant rend, sous 4 semaines, un tableur de collecte rempli et sourcé pour son service.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Répondre à son donneur d''ordres : de la demande carbone au dossier envoyé'),
    1,
    N'Répondre à son donneur d''ordres',
    N'Comprendre ce qu''un donneur d''ordres cherche réellement derrière un questionnaire ou un appel d''offres carbone.',
    N'Répondre efficacement à une demande carbone client',
    N'Le participant complète un vrai questionnaire client ou appel d''offres, prêt à être envoyé en fin de journée.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi achats : réduire le scope 3 avec ses fournisseurs'),
    1,
    N'Défi achats scope 3',
    N'Comprendre pourquoi le scope 3 (achats et fournisseurs) pèse souvent plus lourd que les émissions directes de l''entreprise.',
    N'Intégrer un critère carbone dans sa politique d''achats',
    N'L''acheteur engage 3 fournisseurs prioritaires sur le sujet carbone et ajoute un premier critère carbone à sa grille d''achats.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Piloter sa trajectoire SNBC 3 — Bâtiment & BTP'),
    1,
    N'Fixer la trajectoire de son entreprise et ses indicateurs de suivi',
    N'Comprendre les budgets carbone 2028-2033-2038 de la SNBC 3 et les traduire en trajectoire propre à l''entreprise.',
    N'Piloter une trajectoire carbone réglementaire',
    N'Produire la trajectoire 2028-2033-2038 de son entreprise et nommer un responsable pour chacun des 5 indicateurs terrain retenus.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Piloter sa trajectoire SNBC 3 — Bâtiment & BTP'),
    2,
    N'Défi Bâtiment & BTP',
    N'Identifier les leviers de décarbonation du bâtiment : chauffage, rénovation, matériaux bas-carbone.',
    N'Décarboner une activité bâtiment',
    N'D''ici la fin du défi, construis un plan chauffage et rénovation chiffré pour un site ou un chantier de ton entreprise.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Piloter sa trajectoire SNBC 3 — Industrie & PME manufacturières'),
    1,
    N'Fixer la trajectoire de son entreprise et ses indicateurs de suivi',
    N'Comprendre les budgets carbone 2028-2033-2038 de la SNBC 3 et les traduire en trajectoire propre à l''entreprise.',
    N'Piloter une trajectoire carbone réglementaire',
    N'Produire la trajectoire 2028-2033-2038 de son entreprise et nommer un responsable pour chacun des 5 indicateurs terrain retenus.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Piloter sa trajectoire SNBC 3 — Industrie & PME manufacturières'),
    2,
    N'Défi Industrie & PME manufacturières',
    N'Identifier les leviers de décarbonation industrielle : électrification des procédés, efficacité énergétique, chaleur fatale.',
    N'Décarboner une activité industrie',
    N'Dans les 6 semaines qui suivent la formation, audite un procédé de production et lance une action d''efficacité ou d''électrification.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Piloter sa trajectoire SNBC 3 — Agriculture & agroalimentaire'),
    1,
    N'Fixer la trajectoire de son entreprise et ses indicateurs de suivi',
    N'Comprendre les budgets carbone 2028-2033-2038 de la SNBC 3 et les traduire en trajectoire propre à l''entreprise.',
    N'Piloter une trajectoire carbone réglementaire',
    N'Produire la trajectoire 2028-2033-2038 de son entreprise et nommer un responsable pour chacun des 5 indicateurs terrain retenus.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Piloter sa trajectoire SNBC 3 — Agriculture & agroalimentaire'),
    2,
    N'Défi Agriculture & agroalimentaire',
    N'Comprendre les leviers de décarbonation agricole : intrants, énergie, haies et stockage carbone des sols.',
    N'Décarboner une activité agriculture',
    N'Dans les 6 semaines qui suivent la formation, teste une action concrète sur un levier choisi : intrants, énergie ou haies et couverts.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Piloter sa trajectoire SNBC 3 — Déchets & économie circulaire'),
    1,
    N'Fixer la trajectoire de son entreprise et ses indicateurs de suivi',
    N'Comprendre les budgets carbone 2028-2033-2038 de la SNBC 3 et les traduire en trajectoire propre à l''entreprise.',
    N'Piloter une trajectoire carbone réglementaire',
    N'Produire la trajectoire 2028-2033-2038 de son entreprise et nommer un responsable pour chacun des 5 indicateurs terrain retenus.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Piloter sa trajectoire SNBC 3 — Déchets & économie circulaire'),
    2,
    N'Défi Déchets & économie circulaire',
    N'Comprendre la hiérarchie des leviers déchets : prévention, tri, réemploi, écoconception.',
    N'Décarboner une activité déchets',
    N'D''ici la fin du défi, pèse un flux de déchets avant et après avoir mis en place une filière de réemploi ou de tri.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Piloter sa trajectoire SNBC 3 — Tertiaire, commerce & numérique'),
    1,
    N'Fixer la trajectoire de son entreprise et ses indicateurs de suivi',
    N'Comprendre les budgets carbone 2028-2033-2038 de la SNBC 3 et les traduire en trajectoire propre à l''entreprise.',
    N'Piloter une trajectoire carbone réglementaire',
    N'Produire la trajectoire 2028-2033-2038 de son entreprise et nommer un responsable pour chacun des 5 indicateurs terrain retenus.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Piloter sa trajectoire SNBC 3 — Tertiaire, commerce & numérique'),
    2,
    N'Défi Tertiaire, commerce & numérique',
    N'Identifier les leviers propres au tertiaire : énergie des locaux, déplacements professionnels, usages numériques.',
    N'Décarboner une activité tertiaire,',
    N'Dans les 6 semaines qui suivent la formation, teste une action mesurée sur l''énergie, les déplacements ou le numérique de ton activité.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Suivi terrain accompagné'),
    1,
    N'Suivi terrain accompagné',
    N'Garder le cap sur la trajectoire fixée en revoyant régulièrement le tableau de bord et les écarts constatés.',
    N'Ajuster une trajectoire carbone dans la durée',
    N'Le tableau de bord est tenu à jour à chaque classe virtuelle, les écarts sont analysés et le plan est réajusté si besoin.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Manager un défi bas-carbone dans son équipe'),
    1,
    N'Manager un défi bas-carbone',
    N'Comprendre les ressorts de la pédagogie par le défi pour l''appliquer à sa propre équipe.',
    N'Lancer et piloter un défi d''équipe',
    N'Entre les 2 journées, le manager lance un vrai défi dans son équipe et présente un premier bilan à la deuxième session.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Faire changer les pratiques au travail : défi comportements'),
    1,
    N'Défi comportements au travail',
    N'Comprendre les leviers psychosociaux du changement de comportement (freins, nudges, habitudes).',
    N'Faire évoluer un comportement collectif au travail',
    N'Un geste ciblé (énergie, déplacements, impressions...) est mesuré avant et après l''action.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Ambassadeurs : embarquer ses collègues sans moraliser'),
    1,
    N'Ambassadeurs : embarquer sans moraliser',
    N'Comprendre pourquoi le discours moralisateur braque plus qu''il ne convainc en entreprise.',
    N'Embarquer ses collègues sur un sujet sensible',
    N'L''ambassadeur anime une action collective auprès de ses collègues et compte le nombre de participants réels.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Animer un atelier climat en interne'),
    1,
    N'Animer un atelier climat en interne',
    N'Maîtriser les techniques de facilitation et d''intelligence collective appliquées à un atelier climat.',
    N'Animer un atelier en autonomie',
    N'Le référent anime réellement un atelier auprès de collègues et le débriefe avec le formateur.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Devenir animateur Cap Bas Carbone'),
    1,
    N'Devenir animateur Cap Bas Carbone',
    N'Maîtriser en profondeur la mécanique du Challenge Cap Bas Carbone pour pouvoir l''animer en autonomie.',
    N'Animer un Challenge en autonomie, avec licence',
    N'L''animateur co-anime un cycle complet du Challenge et obtient sa licence d''animation à l''issue de l''accompagnement.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi Énergie : baisser la consommation du site'),
    1,
    N'Défi Énergie : baisser la consommation du site',
    N'Identifier les principaux postes de consommation énergétique d''un site (chauffage, éclairage, équipements).',
    N'Agir sur un poste d''émission ciblé',
    N'En 4 semaines, mesure la consommation d''un poste énergétique du site avant et après une action de réduction.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi Mobilité : construire le plan de mobilité employeur'),
    1,
    N'Défi Mobilité : construire le plan de mobilité employeur',
    N'Comprendre les enjeux d''un plan de mobilité employeur (covoiturage, vélo, télétravail, transports en commun).',
    N'Agir sur un poste d''émission ciblé',
    N'En 4 semaines, mène une enquête mobilité auprès des salariés et lance 2 mesures concrètes issues des résultats.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi Éco-conduite'),
    1,
    N'Défi Éco-conduite',
    N'Identifier les gestes d''éco-conduite qui réduisent réellement la consommation de carburant.',
    N'Agir sur un poste d''émission ciblé',
    N'En 4 semaines, mesure ta consommation aux 100 km avant et après avoir testé 3 gestes d''éco-conduite.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi Fret : optimiser ses flux logistiques'),
    1,
    N'Défi Fret : optimiser ses flux logistiques',
    N'Comprendre les leviers d''optimisation logistique : taux de remplissage, kilomètres évités, mutualisation.',
    N'Agir sur un poste d''émission ciblé',
    N'En 4 semaines, teste une optimisation de tournée ou de remplissage et mesure les kilomètres évités.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi Numérique responsable'),
    1,
    N'Défi Numérique responsable',
    N'Comprendre pourquoi la fabrication du matériel numérique pèse plus lourd que son usage quotidien.',
    N'Agir sur un poste d''émission ciblé',
    N'En 4 semaines, nettoie ton stockage numérique et repousse d''au moins un geste le remplacement de ton matériel.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi Déchets zéro gaspillage'),
    1,
    N'Défi Déchets zéro gaspillage',
    N'Comprendre la hiérarchie prévention, tri, réemploi appliquée aux déchets de production ou de bureau.',
    N'Agir sur un poste d''émission ciblé',
    N'En 4 semaines, mesure un flux de déchets avant et après avoir mis en place une action de tri ou de réduction.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi Écoconception : revoir un produit ou un service'),
    1,
    N'Défi Écoconception : revoir un produit ou un service',
    N'Comprendre les principes d''écoconception : matériaux, durabilité, fin de vie d''un produit ou service.',
    N'Agir sur un poste d''émission ciblé',
    N'D''ici la fin du défi, modifie un produit ou service existant selon un critère d''écoconception et recalcule son empreinte.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi Alimentation bas carbone (restauration, séminaires)'),
    1,
    N'Défi Alimentation bas carbone (restauration, séminaires)',
    N'Comprendre les leviers de réduction carbone dans l''alimentation collective (produits, menus, gaspillage).',
    N'Agir sur un poste d''émission ciblé',
    N'En 4 semaines, revois un menu ou un événement et mesure les tCO2e par repas avant et après.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Atelier IA : automatiser sa collecte et son reporting carbone'),
    1,
    N'Automatiser sa collecte et son reporting carbone',
    N'Comprendre comment l''IA générative peut accélérer la collecte et la mise en forme d''un reporting carbone.',
    N'Construire un assistant de reporting carbone opérationnel',
    N'Le référent repart avec un assistant ou un modèle de reporting fonctionnel sur ses propres données.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi IA frugale : réduire l''empreinte de ses usages d''IA générative'),
    1,
    N'Défi IA frugale',
    N'Comprendre l''empreinte environnementale de l''IA générative (entraînement des modèles, requêtes, data centers).',
    N'Adopter des usages d''IA générative plus sobres',
    N'Les usages sont recensés, de bonnes pratiques adoptées, et l''empreinte estimée avant et après.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Green IA : concevoir un projet d''IA sobre'),
    1,
    N'Concevoir un projet d''IA sobre',
    N'Comprendre les critères de sobriété d''un projet d''IA : taille du modèle, hébergement, fréquence d''usage réel.',
    N'Concevoir ou revoir un projet d''IA selon des critères de sobriété',
    N'Un projet d''IA de l''entreprise est revu selon des critères de sobriété (modèle, hébergement, usage).'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Construire son tableau de bord de trajectoire avec l''IA et le tableur'),
    1,
    N'Construire son tableau de bord avec l''IA',
    N'Comprendre comment combiner tableur et IA générative pour automatiser la mise à jour d''un tableau de bord carbone.',
    N'Construire un tableau de bord de trajectoire opérationnel',
    N'Le pilote repart avec un tableau de bord alimenté par ses propres indicateurs terrain.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Lancer la démarche de transition écologique et sociale de sa PME'),
    1,
    N'Lancer sa démarche de transition',
    N'Comprendre la structure d''une démarche de transition écologique et sociale à l''échelle d''une PME.',
    N'Structurer et faire valider une feuille de route',
    N'Une feuille de route sur 12 mois est rédigée puis validée en comité de direction.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Mesurer l''impact réel de ses actions de transition'),
    1,
    N'Mesurer l''impact réel d''une action',
    N'Comprendre les 3 dimensions d''un bilan d''impact : tCO2e, coût, adhésion des équipes.',
    N'Évaluer objectivement une action déjà menée',
    N'Un bilan d''impact chiffré est produit pour une action déjà menée par l''entreprise.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Valoriser ses résultats mesurés sans greenwashing'),
    1,
    N'Communiquer sans greenwashing',
    N'Comprendre la frontière entre valorisation légitime d''un résultat et greenwashing.',
    N'Produire une communication basée sur des faits chiffrés',
    N'Un support de communication (page web, post, réponse client) est produit, basé sur des résultats chiffrés.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi canicule : adapter ses postes de travail à la chaleur'),
    1,
    N'Défi canicule au travail',
    N'Comprendre les risques santé liés aux vagues de chaleur sur un poste de travail.',
    N'Adapter ses postes de travail à la chaleur',
    N'Un diagnostic chaleur d''un site est réalisé et 3 mesures concrètes sont mises en place.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi Biodiversité : renaturer un espace du site'),
    1,
    N'Défi Biodiversité en entreprise',
    N'Comprendre les enjeux de la renaturation en milieu professionnel (îlots de chaleur, biodiversité, bien-être).',
    N'Organiser un chantier collectif de renaturation',
    N'Une zone du site est débitumée ou végétalisée avec les salariés volontaires.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi Haies et sols : stocker du carbone sur son exploitation'),
    1,
    N'Défi Haies et sols',
    N'Comprendre le rôle des haies, bocages et couverts végétaux dans le stockage du carbone des sols.',
    N'Chiffrer un projet de stockage carbone sur son exploitation',
    N'Un projet de plantation ou de couverts est chiffré en carbone réellement stocké.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi Eau : réduire consommations et rejets'),
    1,
    N'Défi Eau',
    N'Comprendre les sources visibles et invisibles de consommation et de pollution de l''eau en milieu professionnel.',
    N'Réduire sa consommation d''eau et traiter une source de pollution',
    N'Les m³ consommés sont mesurés avant et après le traitement d''une source de pollution identifiée.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Former ses collègues à passer à l''action (formateur interne)'),
    1,
    N'Former ses collègues à l''action',
    N'Maîtriser les bases de l''ingénierie pédagogique appliquée à une action de formation interne.',
    N'Concevoir et animer une formation-action',
    N'Une séquence de formation-action est conçue puis réellement animée en interne.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Facilitation graphique pour animer ses défis'),
    1,
    N'Facilitation graphique de ses défis',
    N'Découvrir les bases du sketchnoting et de la facilitation graphique appliquées à l''animation de défis.',
    N'Rendre un plan d''action visuel et mémorable',
    N'Une fresque visuelle du plan d''action est produite et affichée dans l''entreprise.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Pitcher son action devant la direction : posture et voix'),
    1,
    N'Pitcher son action devant la direction',
    N'Travailler sa posture corporelle et vocale pour présenter une action avec aisance devant un public exigeant.',
    N'Pitcher une action de transition avec assurance',
    N'Chaque participant présente son action, filmé, puis débriefe sa prestation avec le groupe.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Bilan de compétences « métiers de la transition écologique »'),
    1,
    N'Explorer les métiers de la transition',
    N'Découvrir plusieurs métiers de la transition écologique par l''enquête directe auprès de professionnels.',
    N'Explorer un champ de métiers par l''enquête terrain',
    N'Mener au moins 3 enquêtes métier auprès de professionnels de la transition écologique et en tirer un premier classement de pistes.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Bilan de compétences « métiers de la transition écologique »'),
    2,
    N'Valider son projet par l''immersion',
    N'Confronter son projet professionnel à une immersion réelle dans le métier envisagé.',
    N'Valider un projet professionnel par le terrain',
    N'Une immersion est réalisée sur le métier retenu, et le projet professionnel est validé ou ajusté à l''issue.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Bilan d''orientation jeunes vers les métiers verts'),
    1,
    N'Bilan d''orientation jeunes vers les métiers verts',
    N'Découvrir la diversité des métiers verts et confronter ses représentations à la réalité du terrain.',
    N'Construire une orientation éclairée par la rencontre',
    N'Le choix d''orientation est confronté à des rencontres réelles avec des professionnels des métiers verts.'
);

INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
VALUES (
    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Atelier « Découvrir les métiers de la transition »'),
    1,
    N'Découvrir les métiers de la transition',
    N'Découvrir un panorama des métiers de la transition écologique et des voies d''accès possibles.',
    N'Identifier des pistes concrètes à explorer',
    N'Chaque participant repart avec 3 métiers à explorer et une première prise de contact planifiée.'
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Atelier Fresque du Climat + « premiers pas »') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'SENS-C01')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Fresque du Numérique + défi numérique') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'SENS-C02')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Atelier 2tonnes') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'SENS-C03')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Matinale dirigeants : où en est votre entreprise face à la SNBC 3 et à vos donneurs d''ordres ?') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'SENS-C04')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi Low-tech d''équipe') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'SENS-C05')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Pilote climat : porter le plan de transition dans l''entreprise') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'ACT-C01')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Du Bilan Carbone® au plan d''action : choisir ses 3 premières actions') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'ACT-C02')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi collecte : réunir les données carbone en équipe') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'ACT-C03')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Répondre à son donneur d''ordres : de la demande carbone au dossier envoyé') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'ACT-C04')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi achats : réduire le scope 3 avec ses fournisseurs') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'ACT-C05')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Piloter sa trajectoire SNBC 3 — Bâtiment & BTP') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'SNBC-C01')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Piloter sa trajectoire SNBC 3 — Bâtiment & BTP') AND [NumeroEtape] = 2),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'SNBC-C03')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Piloter sa trajectoire SNBC 3 — Industrie & PME manufacturières') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'SNBC-C01')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Piloter sa trajectoire SNBC 3 — Industrie & PME manufacturières') AND [NumeroEtape] = 2),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'SNBC-C04')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Piloter sa trajectoire SNBC 3 — Agriculture & agroalimentaire') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'SNBC-C01')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Piloter sa trajectoire SNBC 3 — Agriculture & agroalimentaire') AND [NumeroEtape] = 2),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'SNBC-C05')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Piloter sa trajectoire SNBC 3 — Déchets & économie circulaire') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'SNBC-C01')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Piloter sa trajectoire SNBC 3 — Déchets & économie circulaire') AND [NumeroEtape] = 2),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'SNBC-C06')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Piloter sa trajectoire SNBC 3 — Tertiaire, commerce & numérique') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'SNBC-C01')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Piloter sa trajectoire SNBC 3 — Tertiaire, commerce & numérique') AND [NumeroEtape] = 2),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'SNBC-C07')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Suivi terrain accompagné') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'SNBC-C08')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Manager un défi bas-carbone dans son équipe') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'MNG-C01')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Faire changer les pratiques au travail : défi comportements') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'MNG-C02')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Ambassadeurs : embarquer ses collègues sans moraliser') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'MNG-C03')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Animer un atelier climat en interne') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'MNG-C04')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Devenir animateur Cap Bas Carbone') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'MNG-C05')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi Énergie : baisser la consommation du site') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'POSTE-C01')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi Mobilité : construire le plan de mobilité employeur') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'POSTE-C02')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi Éco-conduite') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'POSTE-C03')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi Fret : optimiser ses flux logistiques') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'POSTE-C04')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi Numérique responsable') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'POSTE-C05')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi Déchets zéro gaspillage') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'POSTE-C06')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi Écoconception : revoir un produit ou un service') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'POSTE-C07')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi Alimentation bas carbone (restauration, séminaires)') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'POSTE-C08')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Atelier IA : automatiser sa collecte et son reporting carbone') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'IA-C01')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi IA frugale : réduire l''empreinte de ses usages d''IA générative') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'IA-C02')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Green IA : concevoir un projet d''IA sobre') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'IA-C03')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Construire son tableau de bord de trajectoire avec l''IA et le tableur') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'IA-C04')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Lancer la démarche de transition écologique et sociale de sa PME') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'GRA-C01')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Mesurer l''impact réel de ses actions de transition') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'GRA-C02')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Valoriser ses résultats mesurés sans greenwashing') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'GRA-C03')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi canicule : adapter ses postes de travail à la chaleur') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'GRA-C04')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi Biodiversité : renaturer un espace du site') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'GRA-C05')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi Haies et sols : stocker du carbone sur son exploitation') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'GRA-C06')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Défi Eau : réduire consommations et rejets') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'POSTE-C09')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Former ses collègues à passer à l''action (formateur interne)') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'GRA-C07')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Facilitation graphique pour animer ses défis') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'GRA-C08')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Pitcher son action devant la direction : posture et voix') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'GRA-C09')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Bilan de compétences « métiers de la transition écologique »') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'PART-C01')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Bilan de compétences « métiers de la transition écologique »') AND [NumeroEtape] = 2),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'PART-C02')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Bilan d''orientation jeunes vers les métiers verts') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'PART-C03')
);

INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
VALUES (
    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'Atelier « Découvrir les métiers de la transition »') AND [NumeroEtape] = 1),
    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'PART-C04')
);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
DELETE FROM [ChallengeEtapeCartes] WHERE [ChallengeEtapeId] IN (
    SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] IN (SELECT [Id] FROM [Challenges] WHERE [Titre] IN (N'Atelier Fresque du Climat + « premiers pas »', N'Fresque du Numérique + défi numérique', N'Atelier 2tonnes', N'Matinale dirigeants : où en est votre entreprise face à la SNBC 3 et à vos donneurs d''ordres ?', N'Défi Low-tech d''équipe', N'Pilote climat : porter le plan de transition dans l''entreprise', N'Du Bilan Carbone® au plan d''action : choisir ses 3 premières actions', N'Défi collecte : réunir les données carbone en équipe', N'Répondre à son donneur d''ordres : de la demande carbone au dossier envoyé', N'Défi achats : réduire le scope 3 avec ses fournisseurs', N'Piloter sa trajectoire SNBC 3 — Bâtiment & BTP', N'Piloter sa trajectoire SNBC 3 — Industrie & PME manufacturières', N'Piloter sa trajectoire SNBC 3 — Agriculture & agroalimentaire', N'Piloter sa trajectoire SNBC 3 — Déchets & économie circulaire', N'Piloter sa trajectoire SNBC 3 — Tertiaire, commerce & numérique', N'Suivi terrain accompagné', N'Manager un défi bas-carbone dans son équipe', N'Faire changer les pratiques au travail : défi comportements', N'Ambassadeurs : embarquer ses collègues sans moraliser', N'Animer un atelier climat en interne', N'Devenir animateur Cap Bas Carbone', N'Défi Énergie : baisser la consommation du site', N'Défi Mobilité : construire le plan de mobilité employeur', N'Défi Éco-conduite', N'Défi Fret : optimiser ses flux logistiques', N'Défi Numérique responsable', N'Défi Déchets zéro gaspillage', N'Défi Écoconception : revoir un produit ou un service', N'Défi Alimentation bas carbone (restauration, séminaires)', N'Atelier IA : automatiser sa collecte et son reporting carbone', N'Défi IA frugale : réduire l''empreinte de ses usages d''IA générative', N'Green IA : concevoir un projet d''IA sobre', N'Construire son tableau de bord de trajectoire avec l''IA et le tableur', N'Lancer la démarche de transition écologique et sociale de sa PME', N'Mesurer l''impact réel de ses actions de transition', N'Valoriser ses résultats mesurés sans greenwashing', N'Défi canicule : adapter ses postes de travail à la chaleur', N'Défi Biodiversité : renaturer un espace du site', N'Défi Haies et sols : stocker du carbone sur son exploitation', N'Défi Eau : réduire consommations et rejets', N'Former ses collègues à passer à l''action (formateur interne)', N'Facilitation graphique pour animer ses défis', N'Pitcher son action devant la direction : posture et voix', N'Bilan de compétences « métiers de la transition écologique »', N'Bilan d''orientation jeunes vers les métiers verts', N'Atelier « Découvrir les métiers de la transition »'))
);
DELETE FROM [ChallengeEtapes] WHERE [ChallengeId] IN (SELECT [Id] FROM [Challenges] WHERE [Titre] IN (N'Atelier Fresque du Climat + « premiers pas »', N'Fresque du Numérique + défi numérique', N'Atelier 2tonnes', N'Matinale dirigeants : où en est votre entreprise face à la SNBC 3 et à vos donneurs d''ordres ?', N'Défi Low-tech d''équipe', N'Pilote climat : porter le plan de transition dans l''entreprise', N'Du Bilan Carbone® au plan d''action : choisir ses 3 premières actions', N'Défi collecte : réunir les données carbone en équipe', N'Répondre à son donneur d''ordres : de la demande carbone au dossier envoyé', N'Défi achats : réduire le scope 3 avec ses fournisseurs', N'Piloter sa trajectoire SNBC 3 — Bâtiment & BTP', N'Piloter sa trajectoire SNBC 3 — Industrie & PME manufacturières', N'Piloter sa trajectoire SNBC 3 — Agriculture & agroalimentaire', N'Piloter sa trajectoire SNBC 3 — Déchets & économie circulaire', N'Piloter sa trajectoire SNBC 3 — Tertiaire, commerce & numérique', N'Suivi terrain accompagné', N'Manager un défi bas-carbone dans son équipe', N'Faire changer les pratiques au travail : défi comportements', N'Ambassadeurs : embarquer ses collègues sans moraliser', N'Animer un atelier climat en interne', N'Devenir animateur Cap Bas Carbone', N'Défi Énergie : baisser la consommation du site', N'Défi Mobilité : construire le plan de mobilité employeur', N'Défi Éco-conduite', N'Défi Fret : optimiser ses flux logistiques', N'Défi Numérique responsable', N'Défi Déchets zéro gaspillage', N'Défi Écoconception : revoir un produit ou un service', N'Défi Alimentation bas carbone (restauration, séminaires)', N'Atelier IA : automatiser sa collecte et son reporting carbone', N'Défi IA frugale : réduire l''empreinte de ses usages d''IA générative', N'Green IA : concevoir un projet d''IA sobre', N'Construire son tableau de bord de trajectoire avec l''IA et le tableur', N'Lancer la démarche de transition écologique et sociale de sa PME', N'Mesurer l''impact réel de ses actions de transition', N'Valoriser ses résultats mesurés sans greenwashing', N'Défi canicule : adapter ses postes de travail à la chaleur', N'Défi Biodiversité : renaturer un espace du site', N'Défi Haies et sols : stocker du carbone sur son exploitation', N'Défi Eau : réduire consommations et rejets', N'Former ses collègues à passer à l''action (formateur interne)', N'Facilitation graphique pour animer ses défis', N'Pitcher son action devant la direction : posture et voix', N'Bilan de compétences « métiers de la transition écologique »', N'Bilan d''orientation jeunes vers les métiers verts', N'Atelier « Découvrir les métiers de la transition »'));
DELETE FROM [CartesCompetences] WHERE [Code] IN (N'SENS-C01', N'SENS-C02', N'SENS-C03', N'SENS-C04', N'SENS-C05', N'ACT-C01', N'ACT-C02', N'ACT-C03', N'ACT-C04', N'ACT-C05', N'SNBC-C03', N'SNBC-C04', N'SNBC-C05', N'SNBC-C06', N'SNBC-C07', N'SNBC-C08', N'MNG-C01', N'MNG-C02', N'MNG-C03', N'MNG-C04', N'MNG-C05', N'POSTE-C01', N'POSTE-C02', N'POSTE-C03', N'POSTE-C04', N'POSTE-C05', N'POSTE-C06', N'POSTE-C07', N'POSTE-C08', N'IA-C01', N'IA-C02', N'IA-C03', N'IA-C04', N'GRA-C01', N'GRA-C02', N'GRA-C03', N'GRA-C04', N'GRA-C05', N'GRA-C06', N'POSTE-C09', N'GRA-C07', N'GRA-C08', N'GRA-C09', N'PART-C01', N'PART-C02', N'PART-C03', N'PART-C04');
DELETE FROM [Challenges] WHERE [Titre] IN (N'Atelier Fresque du Climat + « premiers pas »', N'Fresque du Numérique + défi numérique', N'Atelier 2tonnes', N'Matinale dirigeants : où en est votre entreprise face à la SNBC 3 et à vos donneurs d''ordres ?', N'Défi Low-tech d''équipe', N'Pilote climat : porter le plan de transition dans l''entreprise', N'Du Bilan Carbone® au plan d''action : choisir ses 3 premières actions', N'Défi collecte : réunir les données carbone en équipe', N'Répondre à son donneur d''ordres : de la demande carbone au dossier envoyé', N'Défi achats : réduire le scope 3 avec ses fournisseurs', N'Piloter sa trajectoire SNBC 3 — Bâtiment & BTP', N'Piloter sa trajectoire SNBC 3 — Industrie & PME manufacturières', N'Piloter sa trajectoire SNBC 3 — Agriculture & agroalimentaire', N'Piloter sa trajectoire SNBC 3 — Déchets & économie circulaire', N'Piloter sa trajectoire SNBC 3 — Tertiaire, commerce & numérique', N'Suivi terrain accompagné', N'Manager un défi bas-carbone dans son équipe', N'Faire changer les pratiques au travail : défi comportements', N'Ambassadeurs : embarquer ses collègues sans moraliser', N'Animer un atelier climat en interne', N'Devenir animateur Cap Bas Carbone', N'Défi Énergie : baisser la consommation du site', N'Défi Mobilité : construire le plan de mobilité employeur', N'Défi Éco-conduite', N'Défi Fret : optimiser ses flux logistiques', N'Défi Numérique responsable', N'Défi Déchets zéro gaspillage', N'Défi Écoconception : revoir un produit ou un service', N'Défi Alimentation bas carbone (restauration, séminaires)', N'Atelier IA : automatiser sa collecte et son reporting carbone', N'Défi IA frugale : réduire l''empreinte de ses usages d''IA générative', N'Green IA : concevoir un projet d''IA sobre', N'Construire son tableau de bord de trajectoire avec l''IA et le tableur', N'Lancer la démarche de transition écologique et sociale de sa PME', N'Mesurer l''impact réel de ses actions de transition', N'Valoriser ses résultats mesurés sans greenwashing', N'Défi canicule : adapter ses postes de travail à la chaleur', N'Défi Biodiversité : renaturer un espace du site', N'Défi Haies et sols : stocker du carbone sur son exploitation', N'Défi Eau : réduire consommations et rejets', N'Former ses collègues à passer à l''action (formateur interne)', N'Facilitation graphique pour animer ses défis', N'Pitcher son action devant la direction : posture et voix', N'Bilan de compétences « métiers de la transition écologique »', N'Bilan d''orientation jeunes vers les métiers verts', N'Atelier « Découvrir les métiers de la transition »');
");
        }
    }
}
