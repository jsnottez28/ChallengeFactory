using Application.Common.Interfaces;
using ClosedXML.Excel;
using Domain.Entities;

namespace Web.Services;

// Genere le tableur de collecte .xlsx d'un dossier client carbone : un onglet par poste
// ABC effectivement concerne (jamais les postes sans element), avec les colonnes de la
// "Matrice de collecte" du tableur Bilan Carbone(R) V9.0 (Libelle personnalise, Valeur,
// Unite, Type, Source, Hypotheses) + les colonnes de pilotage de la collecte (piece
// justificative, referent, statut). Les donnees deja connues (Pre-diagnostic) sont
// pre-remplies : la saisie peut ensuite etre recopiee telle quelle dans le tableur ABC.
public static class TableurCollecteGenerator
{
    private static readonly string[] TypesDonnee = ["Physique", "Comptable", "Extrapolée", "Statistique", "Approchée"];
    private static readonly string[] StatutsCollecte = ["À demander", "Demandé", "En attente", "Reçu", "Validé"];

    private static readonly string[] Entetes =
    [
        "Sous-poste", "Donnée à collecter", "Libellé personnalisé", "Valeur", "Unité", "Type",
        "Source", "Hypothèses", "Pièce justificative", "Référent", "Statut", "Commentaire", "Pourquoi cette donnée",
    ];

    private static readonly XLColor Vert = XLColor.FromHtml("#2E7D32");
    private static readonly XLColor VertClair = XLColor.FromHtml("#E8F5E9");
    private static readonly XLColor Orange = XLColor.FromHtml("#F28C28");

    public static byte[] Generer(string raisonSociale, DateTime dateGeneration, IReadOnlyList<ElementCollecteInfo> elements, string? lienEspaceClient = null)
    {
        using var classeur = new XLWorkbook();

        var parPoste = elements
            .GroupBy(e => e.Poste)
            .OrderBy(g => g.Key)
            .ToList();

        EcrireLisezMoi(classeur.Worksheets.Add("Lisez-moi"), raisonSociale, dateGeneration, lienEspaceClient);
        var suivi = classeur.Worksheets.Add("Suivi");

        foreach (var groupe in parPoste)
        {
            EcrirePoste(classeur.Worksheets.Add(NomOnglet(groupe.Key)), groupe.Key, groupe.ToList());
        }

        EcrireSuivi(suivi, parPoste.Select(g => (g.Key, g.Count())).ToList());

        using var flux = new MemoryStream();
        classeur.SaveAs(flux);
        return flux.ToArray();
    }

    // Nom d'onglet Excel : 31 caracteres max, sans : \ / ? * [ ].
    public static string NomOnglet(PosteCollecte poste)
    {
        var nom = QuestionnaireCollecteCatalogue.LibellePoste(poste);
        return nom.Length > 31 ? nom[..31] : nom;
    }

    private static void EcrireLisezMoi(IXLWorksheet feuille, string raisonSociale, DateTime dateGeneration, string? lienEspaceClient)
    {
        feuille.Cell(1, 1).Value = "Tableur de collecte — Bilan Carbone®";
        feuille.Cell(1, 1).Style.Font.SetBold().Font.SetFontSize(16).Font.SetFontColor(Vert);
        feuille.Cell(2, 1).Value = raisonSociale;
        feuille.Cell(2, 1).Style.Font.SetBold().Font.SetFontSize(13);
        feuille.Cell(3, 1).Value = $"Généré le {dateGeneration.ToLocalTime():dd/MM/yyyy} par Challenges Factory";
        feuille.Cell(3, 1).Style.Font.SetItalic();

        var ligne = 5;

        if (!string.IsNullOrWhiteSpace(lienEspaceClient))
        {
            var celluleLien = feuille.Cell(ligne, 1);
            celluleLien.Value = "➜ Suivre ma collecte et retrouver ce fichier en ligne : mon Espace Client Bilan Carbone®";
            celluleLien.SetHyperlink(new XLHyperlink(lienEspaceClient));
            celluleLien.Style.Font.SetFontColor(XLColor.FromHtml("#1A73E8")).Font.SetUnderline().Font.SetBold();
            ligne += 2;
        }

        string[] lignes =
        [
            "Pourquoi ce tableur ?",
            "Un Bilan Carbone® mesure les émissions de gaz à effet de serre de votre activité, poste par poste, pour identifier où agir en priorité - c'est la première étape avant tout plan de décarbonation crédible.",
            "",
            "Comment le remplir",
            "1. Chaque onglet correspond à un poste du Bilan Carbone® (nomenclature de l'Association pour la transition Bas Carbone) - un court texte en tête d'onglet explique l'enjeu de ce poste, avec un exemple concret.",
            "2. Pour chaque ligne, renseignez la Valeur et l'Unité, puis indiquez d'où vient le chiffre (colonne Source) et vos éventuelles hypothèses.",
            "3. Colonne Type : Physique (relevé, facture en kWh, kg…), Comptable (montant en k€ HT), Extrapolée (calculée à partir d'un échantillon), Statistique (moyenne sectorielle), Approchée (estimation).",
            "4. Les lignes déjà remplies (en vert) proviennent de votre pré-diagnostic ou de vos réponses au questionnaire : merci de les vérifier et de les corriger si besoin plutôt que de les ressaisir.",
            "5. Vous pouvez ajouter des lignes (par exemple une ligne par site ou par fournisseur) en recopiant une ligne existante.",
            "6. Joignez les pièces justificatives indiquées (colonne \"Pièce justificative\") depuis votre espace client, ou envoyez-les par email à votre consultant.",
            "7. La colonne \"Pourquoi cette donnée\" rappelle ce qui a déclenché la demande (une réponse au questionnaire, une donnée du pré-diagnostic) - utile si vous avez un doute sur une ligne.",
            "",
            "Unités : les montants sont attendus en k€ HT (milliers d'euros hors taxe), comme dans le tableur Bilan Carbone® officiel.",
            "Confidentialité : ces données ne servent qu'à la réalisation de votre Bilan Carbone®.",
        ];

        for (var i = 0; i < lignes.Length; i++)
        {
            feuille.Cell(ligne + i, 1).Value = lignes[i];
        }

        feuille.Cell(ligne, 1).Style.Font.SetBold().Font.SetFontColor(Orange);
        feuille.Cell(ligne + 3, 1).Style.Font.SetBold().Font.SetFontColor(Orange);
        feuille.Column(1).Width = 140;
        feuille.Column(1).Style.Alignment.WrapText = true;
    }

    private static void EcrirePoste(IXLWorksheet feuille, PosteCollecte poste, List<ElementCollecteInfo> elements)
    {
        feuille.Cell(1, 1).Value = QuestionnaireCollecteCatalogue.LibellePoste(poste);
        feuille.Cell(1, 1).Style.Font.SetBold().Font.SetFontSize(14).Font.SetFontColor(Vert);

        var note = QuestionnaireCollecteCatalogue.NotePedagogique(poste);
        if (!string.IsNullOrWhiteSpace(note))
        {
            var celluleNote = feuille.Range(2, 1, 2, Entetes.Length).Merge().FirstCell();
            celluleNote.Value = note;
            celluleNote.Style.Font.SetItalic().Font.SetFontColor(XLColor.FromHtml("#5A5650"));
            celluleNote.Style.Alignment.WrapText = true;
            feuille.Row(2).Height = 32;
        }

        const int ligneEntete = 3;
        for (var c = 0; c < Entetes.Length; c++)
        {
            var cellule = feuille.Cell(ligneEntete, c + 1);
            cellule.Value = Entetes[c];
            cellule.Style.Font.SetBold().Font.SetFontColor(XLColor.White);
            cellule.Style.Fill.SetBackgroundColor(Vert);
            cellule.Style.Alignment.WrapText = true;
        }

        var ligne = ligneEntete + 1;
        foreach (var element in elements)
        {
            feuille.Cell(ligne, 1).Value = element.SousPoste;
            feuille.Cell(ligne, 2).Value = element.Donnee;
            if (element.Valeur.HasValue)
            {
                feuille.Cell(ligne, 4).Value = (double)element.Valeur.Value;
                feuille.Row(ligne).Cells(1, Entetes.Length).Style.Fill.SetBackgroundColor(VertClair);
            }
            feuille.Cell(ligne, 5).Value = element.Unite;
            feuille.Cell(ligne, 6).Value = element.TypeDonnee ?? string.Empty;
            feuille.Cell(ligne, 7).Value = element.Source ?? string.Empty;
            feuille.Cell(ligne, 9).Value = element.PieceJustificative;
            feuille.Cell(ligne, 10).Value = element.Referent;
            feuille.Cell(ligne, 11).Value = element.Valeur.HasValue ? "Reçu" : "À demander";
            feuille.Cell(ligne, 13).Value = element.Declencheur;
            ligne++;
        }

        // Lignes vides supplementaires (saisie multi-sites / multi-fournisseurs), couvertes
        // par les memes listes deroulantes.
        var derniereLigne = ligne + 20;

        feuille.Range(ligneEntete + 1, 6, derniereLigne, 6).CreateDataValidation().List($"\"{string.Join(",", TypesDonnee)}\"", true);
        feuille.Range(ligneEntete + 1, 11, derniereLigne, 11).CreateDataValidation().List($"\"{string.Join(",", StatutsCollecte)}\"", true);

        var largeurs = new[] { 28, 48, 24, 12, 14, 14, 30, 30, 40, 26, 14, 30, 40 };
        for (var c = 0; c < largeurs.Length; c++)
        {
            feuille.Column(c + 1).Width = largeurs[c];
        }

        feuille.Range(ligneEntete + 1, 1, derniereLigne, Entetes.Length).Style.Alignment.WrapText = true;
        feuille.Range(ligneEntete + 1, 1, derniereLigne, Entetes.Length).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
        feuille.Range(ligneEntete, 1, derniereLigne, Entetes.Length).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        feuille.Range(ligneEntete, 1, derniereLigne, Entetes.Length).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        feuille.SheetView.FreezeRows(ligneEntete);
    }

    private static void EcrireSuivi(IXLWorksheet feuille, List<(PosteCollecte Poste, int Nombre)> postes)
    {
        feuille.Cell(1, 1).Value = "Suivi de la collecte";
        feuille.Cell(1, 1).Style.Font.SetBold().Font.SetFontSize(14).Font.SetFontColor(Vert);

        string[] entetes = ["Poste", "Données demandées", "Reçues", "Validées", "Avancement"];
        for (var c = 0; c < entetes.Length; c++)
        {
            var cellule = feuille.Cell(3, c + 1);
            cellule.Value = entetes[c];
            cellule.Style.Font.SetBold().Font.SetFontColor(XLColor.White);
            cellule.Style.Fill.SetBackgroundColor(Vert);
        }

        var ligne = 4;
        foreach (var (poste, nombre) in postes)
        {
            var onglet = NomOnglet(poste).Replace("'", "''");
            var plageStatut = $"'{onglet}'!K:K";
            feuille.Cell(ligne, 1).Value = QuestionnaireCollecteCatalogue.LibellePoste(poste);
            // -1 : l'en-tete "Donnée à collecter" (ligne 3) ; la ligne 1 n'a pas de colonne B.
            feuille.Cell(ligne, 2).FormulaA1 = $"COUNTA('{onglet}'!B:B)-1";
            feuille.Cell(ligne, 3).FormulaA1 = $"COUNTIF({plageStatut},\"Reçu\")+COUNTIF({plageStatut},\"Validé\")";
            feuille.Cell(ligne, 4).FormulaA1 = $"COUNTIF({plageStatut},\"Validé\")";
            feuille.Cell(ligne, 5).FormulaA1 = $"IF(B{ligne}=0,0,D{ligne}/B{ligne})";
            feuille.Cell(ligne, 5).Style.NumberFormat.Format = "0%";
            ligne++;
        }

        feuille.Cell(ligne, 1).Value = "Total";
        feuille.Cell(ligne, 1).Style.Font.SetBold();
        feuille.Cell(ligne, 2).FormulaA1 = $"SUM(B4:B{ligne - 1})";
        feuille.Cell(ligne, 3).FormulaA1 = $"SUM(C4:C{ligne - 1})";
        feuille.Cell(ligne, 4).FormulaA1 = $"SUM(D4:D{ligne - 1})";
        feuille.Cell(ligne, 5).FormulaA1 = $"IF(B{ligne}=0,0,D{ligne}/B{ligne})";
        feuille.Cell(ligne, 5).Style.NumberFormat.Format = "0%";

        feuille.Column(1).Width = 32;
        feuille.Columns(2, 5).Width = 18;
    }
}
