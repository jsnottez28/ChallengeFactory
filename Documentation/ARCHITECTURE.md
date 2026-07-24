# Architecture — Aperçu et audit rapide

Résumé technique
- Application ASP.NET Core 8.0 (`net8.0`), template MVC + Identity.
- Persistance : EF Core avec `ApplicationDbContext` dans `Infrastructure/Persistence`.
- Migrations EF sous `Infrastructure/Migrations`.
- Projet `Web` référence `Application` et `Infrastructure`.

Points d'attention (audit rapide)

- Chaîne de connexion par défaut dans `Web/appsettings.json` utilise `(localdb)\\mssqllocaldb` (LocalDB), ce qui fonctionne sous Windows mais pas sur macOS.
  - Sur macOS, il faut utiliser une alternative : SQL Server dans Docker, PostgreSQL, SQLite pour le développement, ou une base distante (Azure SQL).

- `UserSecretsId` est défini dans `Web.csproj` — utiliser `dotnet user-secrets` pour stocker la connection string en développement.

- `ApplicationDbContext` hérite d'`IdentityDbContext` : la base de données doit inclure les tables d'Identity avant d'utiliser l'authentification.
- Cible : `net8.0` — vérifier que le SDK .NET 8 est installé localement.

Recommandations
- Remplacer la connection string `DefaultConnection` par une valeur utilisable sur macOS (voir `SETUP.md`).

- Documenter l'architecture des services (voir `SERVICES.md`).

- Ajouter des instructions pour exécuter SQL Server en container Docker, ou fournir un `docker-compose.yml` minimal.

- Vérifier les packages référencés pour compatibilité `net8.0`.

- Ajouter un README par projet (`Application`, `Infrastructure`, `Web`) si nécessaire.
