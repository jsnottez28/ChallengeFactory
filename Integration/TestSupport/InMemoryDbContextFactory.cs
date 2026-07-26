using Microsoft.EntityFrameworkCore;

namespace Integration.TestSupport;

internal static class InMemoryDbContextFactory
{
    // Une base InMemory par test (nom unique) : evite toute pollution entre tests qui
    // tournent en parallele (xUnit parallelise les classes de test par defaut).
    public static ApplicationDbContext Create()
    {
        return Create(Guid.NewGuid().ToString());
    }

    // Variante a nom explicite : permet d'ouvrir un DEUXIEME DbContext independant sur la
    // MEME base InMemory, pour verifier un scenario sans le cache d'identite d'un
    // DbContext partage (chaque requete HTTP reelle obtient son propre DbContext scope -
    // reutiliser le meme DbContext partout dans les tests peut masquer certains bugs, ou
    // au contraire, en simuler qui n'existent pas en production).
    public static ApplicationDbContext Create(string nomBase)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(nomBase)
            .Options;

        return new ApplicationDbContext(options);
    }
}
