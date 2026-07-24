namespace Domain.Entities;

// Le prompt d'origine ne prevoyait que Debutant/Moyen/Expert, mais data.xlsx (source de
// verite reelle, feuille "cartes") contient aussi la valeur "Intermediaire" : l'enum est
// elargi pour rester importable sans erreur sur les donnees existantes.
public enum NiveauCarte
{
    Debutant,
    Intermediaire,
    Moyen,
    Expert
}
