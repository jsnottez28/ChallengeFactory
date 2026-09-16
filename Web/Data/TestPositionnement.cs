using Domain.Entities;

namespace Web.Data;

// En-tete d'une campagne de test de connaissances pour une Cohorte : au plus une par
// (Cohorte, Type) - cf. TestPositionnementService.EnvoyerAsync (idempotent, relance la
// meme campagne plutot que d'en creer une nouvelle). Couvre toujours l'ensemble des cartes
// du Challenge (toutes etapes confondues), pas seulement l'etape courante - c'est le meme
// referentiel de cartes qui est evalue en amont et en aval (Methode Miroir).
public class TestPositionnement
{
    public int Id { get; set; }

    public int CohorteId { get; set; }
    public Cohorte Cohorte { get; set; } = null!;

    public TypeTestPositionnement Type { get; set; }

    public DateTime EnvoyeLe { get; set; } = DateTime.UtcNow;

    public string EnvoyeParId { get; set; } = string.Empty;
    public ApplicationUser EnvoyePar { get; set; } = null!;
}
