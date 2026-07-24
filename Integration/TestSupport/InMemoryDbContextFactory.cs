using Microsoft.EntityFrameworkCore;

namespace Integration.TestSupport;

internal static class InMemoryDbContextFactory
{
    // Une base InMemory par test (nom unique) : evite toute pollution entre tests qui
    // tournent en parallele (xUnit parallelise les classes de test par defaut).
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
