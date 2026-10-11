namespace Domain.Entities;

// Suivi commercial d'un Pre-diagnostic carbone depose par un prospect - jamais visible du
// prospect lui-meme, uniquement cote Gestionnaire (cf. PreBilanCarboneController admin).
public enum StatutPreBilanCrm
{
    Nouveau,
    Contacte,
    Qualifie,
    Perdu,
    Gagne,
}
