namespace Application.Common.Interfaces;

// Vue d'ensemble des Cohortes (toutes confondues), base de tous les autres blocs du
// tableau de bord.
public sealed class QualiopiCohortesInfo
{
    public int NombreCohortesEnPreparation { get; set; } // inclut Proposee (jamais demarrees)
    public int NombreCohortesActives { get; set; }
    public int NombreCohortesTerminees { get; set; }
    public int NombreApprenantsTotal { get; set; } // utilisateurs distincts, toutes Cohortes confondues
}

// Niveau 3 Kirkpatrick ("Changement de pratique") via le taux de completion - cf.
// CLAUDE.md, "Mesure d'impact & KPI". Le taux apprenant est calcule uniquement sur les
// Cohortes Terminee (methode Miroir : on ne mesure que ce qui est acheve), en verifiant si
// chaque membre a une Preuve ValideeDefinitivement sur la DERNIERE etape de son Challenge.
public sealed class QualiopiCompletionInfo
{
    public int NombreCohortesDemarrees { get; set; } // Active + Terminee
    public int NombreCohortesTerminees { get; set; }
    public double? TauxCohortesMeneesATermePourcent { get; set; }

    public int NombreApprenantsCohortesTerminees { get; set; }
    public int NombreApprenantsAyantValideDerniereEtape { get; set; }
    public double? TauxCompletionApprenantPourcent { get; set; }
}

// Niveau 2 Kirkpatrick ("Acquisition de connaissances") via le taux de participation aux
// defis individuels - distinct de la completion (deposer une Preuve n'est pas la meme
// chose que la voir validee definitivement).
public sealed class QualiopiParticipationInfo
{
    public int NombrePreuvesAttendues { get; set; } // somme, par Cohorte demarree, de (membres x EtapeCourante)
    public int NombrePreuvesDeposees { get; set; }
    public double? TauxParticipationPourcent { get; set; }
}

// Suivi de l'execution (exigence Qualiopi) via l'emargement numerique.
public sealed class QualiopiAssiduiteInfo
{
    public int NombreSeancesProposees { get; set; } // Emargement.EnvoyeLe renseigne
    public int NombreSeancesSignees { get; set; } // Emargement.SigneLe renseigne
    public double? TauxPresencePourcent { get; set; }
    public decimal? TotalHeuresPresence { get; set; }
}

// Niveau 1 Kirkpatrick ("Satisfaction") - critere 7 du Referentiel National Qualite
// (Qualiopi). Score NPS standard (0-10, "Recommanderiez-vous...") agrege sur toutes les
// Cohortes - cf. Web.Data.SatisfactionReponse.
public sealed class QualiopiSatisfactionInfo
{
    public int NombreReponses { get; set; }
    public double? ScoreMoyenSur10 { get; set; }
    // NPS classique : % promoteurs (score 9-10) - % detracteurs (score 0-6), de -100 a 100.
    public double? NpsPourcent { get; set; }
    public double? NoteMoyenneContenus { get; set; }
    public double? NoteMoyenneAccompagnement { get; set; }
    public double? NoteMoyenneAdequationAttentes { get; set; }
}

// Point d'etape mi-parcours (methodologie bilan de competences) agrege sur toutes les
// Cohortes concernees.
public sealed class QualiopiMiParcoursInfo
{
    public int NombreReponses { get; set; }
    public double? NoteMoyenneAccompagnement { get; set; }
    public int ProjetPertinentOui { get; set; }
    public int ProjetPertinentPartiellement { get; set; }
    public int ProjetPertinentNon { get; set; }
}

// Progression des connaissances (Methode Miroir, cf. CLAUDE.md) : moyenne, sur les seules
// Cohortes disposant a la fois d'une campagne Amont ET Aval completee (on ne compare que
// ce qui a ete effectivement mesure aux deux bouts), du niveau auto-evalue global avant et
// apres le parcours.
public sealed class QualiopiProgressionConnaissancesInfo
{
    public int NombreCohortesAvecAmontEtAval { get; set; }
    public double? NiveauMoyenAmontSur10 { get; set; }
    public double? NiveauMoyenAvalSur10 { get; set; }
    public double? ProgressionMoyennePoints { get; set; } // Aval - Amont
}

// Traitement des reclamations (indicateur 32 du Referentiel National Qualite).
public sealed class QualiopiReclamationsInfo
{
    public int NombreTotal { get; set; }
    public int NombreRecues { get; set; }
    public int NombreEnCours { get; set; }
    public int NombreTraitees { get; set; }
    public double? TauxTraitementPourcent { get; set; }
    public double? DelaiMoyenTraitementJours { get; set; }
}

public sealed class QualiopiTableauDeBordInfo
{
    public QualiopiCohortesInfo Cohortes { get; set; } = new();
    public QualiopiCompletionInfo Completion { get; set; } = new();
    public QualiopiParticipationInfo Participation { get; set; } = new();
    public QualiopiAssiduiteInfo Assiduite { get; set; } = new();
    public QualiopiSatisfactionInfo Satisfaction { get; set; } = new();
    public QualiopiMiParcoursInfo MiParcours { get; set; } = new();
    public QualiopiProgressionConnaissancesInfo ProgressionConnaissances { get; set; } = new();
    public QualiopiReclamationsInfo Reclamations { get; set; } = new();
    public DateTime GenereLe { get; set; }
}

// Tableau de bord global (toutes Cohortes confondues) destine a la preparation et au
// passage d'un audit Qualiopi - distinct des blocs de stats deja presents sur la fiche
// Details d'UNE Cohorte (cf. CohortesController.Details), qui restent le point d'entree
// pour le detail nominatif. Ici, uniquement des agregats, jamais de donnee individuelle
// identifiante - coherent avec le principe Manifeste "Donnee = pilotage, jamais
// surveillance". Tout chiffre repose uniquement sur des donnees reellement enregistrees ;
// un bloc sans aucune donnee renvoie des compteurs a 0 et des moyennes nulles plutot qu'une
// valeur inventee.
public interface IQualiopiDashboardService
{
    Task<QualiopiTableauDeBordInfo> GetTableauDeBordAsync();
}
