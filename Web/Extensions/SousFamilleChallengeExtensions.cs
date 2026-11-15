using Domain.Entities;

namespace Web.Extensions;

public static class SousFamilleChallengeExtensions
{
    public static string Libelle(this SousFamilleChallenge sousFamille) => sousFamille switch
    {
        SousFamilleChallenge.Sensibiliser => "Sensibiliser",
        SousFamilleChallenge.DuBilanALAction => "Passer du Bilan Carbone à l'action",
        SousFamilleChallenge.PiloterTrajectoireSnbc => "Piloter sa trajectoire SNBC 3",
        SousFamilleChallenge.ManagerEtEmbarquer => "Manager et embarquer",
        SousFamilleChallenge.DefisParPosteEmission => "Défis par poste d'émission",
        SousFamilleChallenge.IaEtTransition => "IA et transition",
        SousFamilleChallenge.InspireesDuGraine => "Inspirées du GRAINE",
        SousFamilleChallenge.ParticuliersEtReconversion => "Particuliers et reconversion",
        _ => sousFamille.ToString(),
    };
}
