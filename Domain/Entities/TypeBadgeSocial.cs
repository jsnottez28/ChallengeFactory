namespace Domain.Entities;

// Cf. CLAUDE.md, section Gaming : badges sociaux distincts des badges hebdomadaires de
// competence. Seul "Super Helper" est implemente dans cette version ; "Eclaireur" et les
// futurs types restent a ajouter (extension de cet enum) sans impact sur le reste du
// modele.
public enum TypeBadgeSocial
{
    SuperHelper,
}
