using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Admin;

// Catalogue des tests psychotechniques disponibles avec leur lien d'acces fixe - a
// copier-coller manuellement dans le champ "Defi individuel" d'une etape de Challenge, ou
// a envoyer manuellement en dehors de la plateforme. Le lien est le meme pour tous et
// accessible sans compte : si la personne est connectee, son resultat est automatiquement
// lie a son compte et conserve en historique ; sinon elle voit son resultat a la volee avec
// une invitation a creer un compte (cf. IDiscService, IRiasecService, IBigFiveService,
// IScheinService, IKilmannService).
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
            new TestPsychotechniqueItem
            {
                Nom = "Test RIASEC",
                Description = "60 activités à cocher, profil d'intérêts professionnels selon le modèle de Holland (Réaliste, Investigateur, Artistique, Social, Entreprenant, Conventionnel).",
                LienAcces = Url.Page("/Dashboard/TestRiasec", null, null, Request.Scheme) ?? "/Dashboard/TestRiasec",
            },
            new TestPsychotechniqueItem
            {
                Nom = "Test Big Five",
                Description = "58 affirmations notées de 1 à 5 + 5 paires à choix forcé, profil de personnalité selon le modèle OCEAN (Névrosisme, Extraversion, Ouverture, Agréabilité, Conscienciosité), scores corrigés du biais d'acquiescence.",
                LienAcces = Url.Page("/Dashboard/TestBigFive", null, null, Request.Scheme) ?? "/Dashboard/TestBigFive",
            },
            new TestPsychotechniqueItem
            {
                Nom = "Test des ancres de carrière",
                Description = "45 affirmations notées de 1 à 5 + choix de 3 affirmations prioritaires, profil d'orientation de carrière selon le modèle de Schein (technique, managériale, autonomie, sécurité, créativité, dévouement à une cause, défi pur, qualité de vie, internationale).",
                LienAcces = Url.Page("/Dashboard/TestSchein", null, null, Request.Scheme) ?? "/Dashboard/TestSchein",
            },
            new TestPsychotechniqueItem
            {
                Nom = "Test Thomas-Kilmann",
                Description = "30 énoncés à choix forcé entre deux comportements, mode de gestion des conflits dominant selon la méthode Thomas-Kilmann (Rivaliser, Collaborer, Compromis, Éviter, Céder).",
                LienAcces = Url.Page("/Dashboard/TestKilmann", null, null, Request.Scheme) ?? "/Dashboard/TestKilmann",
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
