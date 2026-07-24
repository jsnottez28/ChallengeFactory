# Services — Principes et responsabilités

- Web (Projet `Web`): MVC + Identity, exposition des contrôleurs et pages Razor.
- Application (Projet `Application`): couche Application — DTOs, cas d'usage, interfaces de services.
- Infrastructure (Projet `Infrastructure`): implémentations techniques — EF Core, migrations, accès BDD.
- Domain (Projet `Domain`): entités métier, règles de validation et invariants.

Principes recommandés
- Séparer interfaces (Application) et implémentations (Infrastructure).
- Injecter les dépendances via le conteneur DI de ASP.NET Core (`Program.cs`).
- Centraliser la configuration des services dans des méthodes d'extension (ex: `AddInfrastructureServices`).
- Documenter chaque service important dans ce fichier en ajoutant sa responsabilité, API et contrats attendus.