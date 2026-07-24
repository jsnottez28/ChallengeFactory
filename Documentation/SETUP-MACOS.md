# Setup local (macOS) — Instructions rapides

PREREQUIS : 
- DOCKER
- DBEAVER --- Option TABLEPLUS - Natif Mac)


1) Installer .NET SDK (v8)
```bash
# Homebrew
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
brew install --cask dotnet-sdk
# Vérifier
dotnet --info
```

2) Installer l'outil EF Core (global)
```bash
dotnet tool install --global dotnet-ef
```

3) Choix de la base de données pour macOS
- Option A — SQL Server en Docker (recommandé si vous voulez rester sur SQL Server):
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Your_strong!Passw0rd" -p 1433:1433 --name fnccr-sql -d mcr.microsoft.com/mssql/server:2019-latest
```
Modifier `Web/appsettings.json` ou utiliser `User Secrets` :
```
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=FnccrCongres;User Id=sa;Password=Your_strong!Passw0rd"
}
```
- Option B — SQLite (pour développement léger) : modifier `Program.cs` pour utiliser `UseSqlite` et une connection string `Data Source=fnccr.db`.

4) Restaurer, build et appliquer migrations
```bash
cd /path/to/Fnccr.Congres
dotnet restore
dotnet build
# Appliquer migrations (startup project = Web)
dotnet ef database update --project Infrastructure --startup-project Web
```

5) Lancer l'application
```bash
dotnet watch run --project Web
```

6) Tester les emails en local
- Le projet supporte un mode de développement où les emails sont affichés dans la console.
- Pour utiliser MailHog (SMTP test local) :
```bash
docker run -d -p 1025:1025 -p 8025:8025 --name fnccr-mailhog mailhog/mailhog
```
- Dans `Web/appsettings.Development.json` ou votre `appsettings.Local.json`, configurez :
```json
"EmailSettings": {
  "UseConsoleEmail": false,
  "SmtpHost": "localhost",
  "SmtpPort": 1025,
  "EnableSsl": false,
  "SmtpUser": "",
  "SmtpPassword": "",
  "ExpediteurEmail": "no-reply@example.com",
  "ExpediteurNom": "FNCCR Congrès"
}
```
- Ouvrez l'interface MailHog à `http://localhost:8025` pour voir les emails de confirmation et mot de passe oublié.

Notes
- N'utilisez pas la chaîne `localdb` sur macOS.
- Pour stocker la connection string en développement :
```bash
dotnet user-secrets init --project Web
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=FnccrCongres;User Id=sa;Password=..."
```
- Pensez à sécuriser `SA` ou utiliser un utilisateur moins privilégié en prod.
