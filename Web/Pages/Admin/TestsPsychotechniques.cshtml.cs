using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Admin;

// Catalogue des tests psychotechniques disponibles (aujourd'hui : DISC uniquement) avec
// leur lien d'acces fixe - a copier-coller manuellement dans le champ "Defi individuel"
// d'une etape de Challenge, ou a envoyer manuellement en dehors de la plateforme. Le lien
// est le meme pour tous : la personne qui le suit doit etre connectee, son resultat est
// automatiquement lie a son compte (cf. IDiscService).
[Authorize(Policy = "Droit:TEST.CONSULTER")]
public class TestsPsychotechniquesModel : PageModel
{
    public List<TestPsychotechniqueItem> Tests { get; private set; } = [];

    public void OnGet()
    {
        Tests =
        [
            new TestPsychotechniqueItem
            {
                Nom = "Test DISC",
                Description = "25 questions, profil de personnalité Dominance / Influence / Stabilité / Conformité.",
                LienAcces = Url.Page("/Dashboard/TestDisc", null, null, Request.Scheme) ?? "/Dashboard/TestDisc",
            },
        ];
    }

    public sealed class TestPsychotechniqueItem
    {
        public string Nom { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LienAcces { get; set; } = string.Empty;
    }
}
