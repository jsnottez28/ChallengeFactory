using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedPiloteSnbcTransport : Migration
    {
        private const string TitreChallenge = "Piloter sa trajectoire SNBC 3 — Transport & logistique";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Pilote du parcours "Piloter sa trajectoire SNBC 3" (cf. CLAUDE.md, document
            // "Catalogue de formations Challenges Factory") : cadrage + le premier defi
            // sectoriel (Transport & logistique, 34% des emissions nationales), comme
            // priorise explicitement dans le document source avant de rediger les 46 autres
            // formations. Challenge complet (etapes + vraies Cartes de Competences), publie
            // en BtoC pour apparaitre sur /formations et beneficier du mecanisme existant de
            // demande d'embarquement.
            migrationBuilder.InsertData(
                table: "Challenges",
                columns: ["Titre", "Slogan", "Description", "NombreEtapes", "Mode", "Statut", "Thematique", "Format", "SousFamille", "Duree", "PublicVise", "Financement", "Prix", "DisponibleEnPresentiel", "CreeLe"],
                values: new object[]
                {
                    TitreChallenge,
                    "Fixez votre trajectoire carbone, puis testez-la sur le premier poste d'émission.",
                    "<p>La SNBC 3 fixe des budgets carbone nationaux pour 2024-2028 et 2029-2033. Ce Challenge forme en interne la personne qui va traduire ces budgets en trajectoire d'entreprise, puis la mettre à l'épreuve du terrain sur le transport et la logistique — premier poste d'émission, avec 34 % des émissions nationales.</p><p>Deux défis : d'abord fixer sa trajectoire et ses indicateurs, puis tester une action concrète sur sa flotte ou son activité de transport.</p>",
                    2,
                    (int)Domain.Entities.ModePlateforme.BtoC,
                    (int)Domain.Entities.StatutChallenge.Publie,
                    (int)Domain.Entities.ThematiqueChallenge.Decarbonation,
                    (int)Domain.Entities.FormatChallenge.Collectif,
                    (int)Domain.Entities.SousFamilleChallenge.PiloterTrajectoireSnbc,
                    "2 jours + 6 semaines sur le terrain",
                    "Référents et responsables d'actions carbone, transporteurs, PME avec flotte",
                    "OPCO",
                    "Sur devis",
                    true,
                    new System.DateTime(2026, 11, 15),
                });

            migrationBuilder.InsertData(
                table: "CartesCompetences",
                columns: ["Code", "Niveau", "TitreTheorie", "Objectif1", "Objectif2", "Objectif3", "Objectif4", "Citation", "TitreDefi", "ContextePro", "ContextePerso", "TonDefi", "Etape1", "Etape2", "Etape3", "Etape4", "Etape5", "Tip1", "Tip2", "Tip3", "CitationHumour", "CreeLe"],
                values: new object[,]
                {
                    {
                        "SNBC-C01",
                        (int)Domain.Entities.NiveauCarte.Intermediaire,
                        "Fixer sa trajectoire carbone",
                        "Comprendre les budgets carbone nationaux 2024-2028 et 2029-2033 fixés par la SNBC 3 (342 et 262 MtCO2e par an).",
                        "Décliner ces budgets en objectifs propres à l'entreprise pour 2028, 2033 et 2038.",
                        "Choisir 5 indicateurs d'activité reliés aux tCO2e, chacun porté par un responsable nommé.",
                        "Mettre en place les rituels de suivi qui mesurent chaque mois l'écart à la trajectoire.",
                        "Une trajectoire qu'on ne mesure pas n'existe pas.",
                        "Trajectoire 2028-2033-2038",
                        "Votre entreprise a reçu son Bilan Carbone® ou son pré-diagnostic : les postes d'émissions sont connus, mais personne n'a encore transformé ce chiffre en trajectoire datée, ni désigné qui doit suivre quoi.",
                        "Comme un budget familial : tant qu'il reste une ligne « divers » non affectée, personne ne sait vraiment où va l'argent. Une trajectoire sans indicateur nommé, c'est pareil.",
                        "D'ici la fin de la semaine, pose sur une seule page la trajectoire de ton entreprise pour 2028, 2033 et 2038, avec un responsable nommé pour chacun des 5 indicateurs retenus.",
                        "Récupère le dernier Bilan Carbone® (ou pré-diagnostic) et identifie les 3 postes d'émission les plus importants.",
                        "Reporte les budgets carbone nationaux à l'échelle de ton secteur et de ta taille d'entreprise.",
                        "Fixe, pour chaque poste prioritaire, une cible chiffrée à 2028, 2033 et 2038.",
                        "Choisis 5 indicateurs d'activité simples à suivre (litres de carburant, kWh, tonnes de déchets...) et relie-les aux tCO2e.",
                        "Nomme un responsable par indicateur et fixe la fréquence du point de suivi.",
                        "Mieux vaut 5 indicateurs suivis chaque mois que 20 indicateurs suivis une fois par an.",
                        "Relie toujours l'indicateur à une donnée que quelqu'un collecte déjà — n'invente pas une nouvelle collecte si une existe.",
                        "Présente la trajectoire sur une seule page : si la direction doit tourner la page pour comprendre, c'est qu'elle est trop compliquée.",
                        "Un plan sans pilote, c'est un avion en pilote automatique... vers une montagne.",
                        new System.DateTime(2026, 11, 15),
                    },
                    {
                        "SNBC-C02",
                        (int)Domain.Entities.NiveauCarte.Intermediaire,
                        "Décarboner sa flotte et ses tournées",
                        "Identifier les leviers de décarbonation du transport : électrification, report modal, optimisation des tournées.",
                        "Comprendre pourquoi le transport est le premier secteur émetteur national (34 % des émissions).",
                        "Construire la trajectoire carbone propre à sa flotte ou à son activité de transport.",
                        "Tester une action concrète sur le terrain et en mesurer l'effet avant/après.",
                        "On ne décarbone pas une flotte depuis un bureau.",
                        "Une action testée sur ma flotte",
                        "Ta direction veut réduire les émissions liées au transport, mais hésite entre changer les véhicules, revoir les tournées ou passer par un autre mode — sans donnée pour trancher.",
                        "Comme replanifier un road-trip : avant de changer de voiture, il vaut parfois mieux revoir l'itinéraire — ça coûte moins cher et ça marche tout de suite.",
                        "Dans les 6 semaines qui suivent la formation, teste une action concrète sur ta flotte ou ton activité de transport (tournée optimisée, test d'électrification, report modal) et mesure l'écart avant/après.",
                        "Cartographie tes trajets ou tournées actuels (kilomètres, carburant, taux de remplissage).",
                        "Identifie le levier le plus accessible à court terme : optimisation des tournées, électrification partielle, ou report modal.",
                        "Chiffre le gain attendu (litres, kWh, tCO2e) avant de lancer l'action.",
                        "Teste l'action pendant au moins 2 semaines sur un périmètre restreint (une tournée, un véhicule, une ligne).",
                        "Mesure le résultat réel et compare-le à ce qui était prévu, avec les pairs de ta Cohorte.",
                        "Commence par l'optimisation des tournées : c'est souvent le levier le moins cher et le plus rapide à tester.",
                        "N'attends pas d'avoir toute la flotte convertie pour mesurer — un seul véhicule pilote suffit pour apprendre.",
                        "Implique le conducteur ou le chauffeur dès le départ : c'est lui qui connaît les vraies contraintes du terrain.",
                        "La meilleure tournée, c'est celle qu'on ne fait pas deux fois.",
                        new System.DateTime(2026, 11, 15),
                    },
                });

            // ChallengeEtapes et la jointure vers les Cartes : passe par des sous-requetes sur
            // des valeurs uniques (Titre du Challenge, Code des Cartes) plutot que des Id
            // litteraux, les identifiants generes n'etant pas connus a l'ecriture de cette
            // migration.
            migrationBuilder.Sql($"""
                INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
                VALUES (
                    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'{EscapeSql(TitreChallenge)}'),
                    1,
                    N'Fixer la trajectoire de son entreprise et ses indicateurs de suivi',
                    N'Comprendre les budgets carbone 2028-2033-2038 de la SNBC 3 et les traduire en trajectoire propre à l''entreprise, avec des indicateurs d''activité reliés aux tCO2e.',
                    N'Piloter une trajectoire carbone réglementaire',
                    N'Produire la trajectoire 2028-2033-2038 de son entreprise et nommer un responsable pour chacun des 5 indicateurs terrain retenus.'
                );

                INSERT INTO [ChallengeEtapes] ([ChallengeId], [NumeroEtape], [TitreEtape], [ObjectifPedagogique], [CompetenceCible], [DefiIndividuel])
                VALUES (
                    (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'{EscapeSql(TitreChallenge)}'),
                    2,
                    N'Défi Transport & logistique',
                    N'Comprendre les leviers de décarbonation du transport (1er poste d''émission, 34 % des émissions nationales) : électrification, report modal, optimisation des tournées.',
                    N'Décarboner une flotte ou une activité de transport',
                    N'Construire la trajectoire de sa flotte et tester une action concrète (tournée optimisée, test d''électrification, report modal) sur le terrain, dans les 6 semaines suivant la formation.'
                );

                INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
                VALUES (
                    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'{EscapeSql(TitreChallenge)}') AND [NumeroEtape] = 1),
                    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'SNBC-C01')
                );

                INSERT INTO [ChallengeEtapeCartes] ([ChallengeEtapeId], [CarteCompetenceId])
                VALUES (
                    (SELECT [Id] FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'{EscapeSql(TitreChallenge)}') AND [NumeroEtape] = 2),
                    (SELECT [Id] FROM [CartesCompetences] WHERE [Code] = N'SNBC-C02')
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                DELETE FROM [ChallengeEtapeCartes] WHERE [CarteCompetenceId] IN (SELECT [Id] FROM [CartesCompetences] WHERE [Code] IN (N'SNBC-C01', N'SNBC-C02'));
                DELETE FROM [ChallengeEtapes] WHERE [ChallengeId] = (SELECT [Id] FROM [Challenges] WHERE [Titre] = N'{EscapeSql(TitreChallenge)}');
                DELETE FROM [CartesCompetences] WHERE [Code] IN (N'SNBC-C01', N'SNBC-C02');
                DELETE FROM [Challenges] WHERE [Titre] = N'{EscapeSql(TitreChallenge)}';
                """);
        }

        private static string EscapeSql(string valeur) => valeur.Replace("'", "''");
    }
}
