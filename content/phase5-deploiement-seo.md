# Phase 5 — SEO, sitemap, audit de liens et préparation au déploiement

Document de travail (pas de contenu à saisir en base) — récapitule ce qui a été livré en
Phase 5 de la mission de repositionnement Climat, et ce qui reste à faire avant un
déploiement en production. Rien n'a été déployé : ce document prépare la procédure, il ne
l'exécute pas (cf. règle mission n°3 — pas de déploiement sans accord explicite).

## 1. Ce qui a été livré dans cette phase

- **Meta description** par page publique (`ViewData["MetaDescription"]`, avec repli
  générique si une vue l'omet).
- **Balise canonical** générée dynamiquement à partir du chemin de la requête, sur le
  domaine de production `https://challenges-factory.com` (en dur dans `_Layout.cshtml` —
  aucune valeur dans `appsettings*.json`, cf. règle mission n°8).
- **Open Graph** (`og:title`, `og:description`, `og:url`, `og:type`, `og:site_name`,
  `og:locale`) et **Twitter Card** (`summary`) sur chaque page.
- `Web/wwwroot/robots.txt` : autorise l'exploration des pages vitrine, exclut
  `/Administration/`, `/Admin/`, `/Identity/`, `/Notifications` ; référence le sitemap.
- `Web/wwwroot/sitemap.xml` : sitemap statique des pages vitrine (voir note ci-dessous sur
  pourquoi statique plutôt que généré).
- **Audit des liens internes** : tous les `asp-action` des vues publiques et du layout
  vérifiés contre les actions réelles de `HomeController` — aucun lien mort trouvé. Un
  point corrigé : `/bilan-carbone-eure-et-loir` (créée en Phase 4) n'était reliée depuis
  aucune page du site (orpheline, atteignable uniquement par URL directe) — un lien a été
  ajouté dans le footer, colonne "Entreprise".

## 2. Ce qui reste [À DÉFINIR] avant mise en production

- **Visuel de partage social** (`og:image` / `twitter:image`, format 1200×630) : aucun
  visuel n'existe aujourd'hui dans les assets publics du projet. À fournir une fois la
  charte graphique précisée (même point de tension que les codes couleur/police exacts,
  déjà signalé) — sans quoi les partages sur réseaux sociaux et messageries n'auront pas
  de vignette.
- **Favicon** : `_Layout.cshtml` (public) n'a actuellement aucune balise `<link
  rel="icon">`. Un favicon existe dans `Web/wwwroot/assets-backoffice/images/favicon.png`,
  mais il appartient à l'identité visuelle de l'ancien positionnement "plateforme
  d'apprentissage soft skills" (back-office uniquement) — je ne l'ai pas réutilisé pour le
  site public sans confirmation, pour ne pas figer une identité visuelle non validée pour
  la nouvelle marque Climat.
- Les marqueurs `[À CONFIRMER]` / `[À DÉFINIR]` déjà présents dans le contenu (récapitulés
  en section 3) doivent être levés — ou au moins arbitrés — avant une mise en ligne
  définitive, en particulier ceux visibles publiquement (témoignage client carbone absent,
  citation manquante).

## 3. Récapitulatif des marqueurs `[À CONFIRMER]` / `[À DÉFINIR]` restants dans le dépôt

| Fichier | Sujet |
|---|---|
| `Web/Views/Shared/_Layout.cshtml` | `og:image`/`twitter:image` — visuel de partage absent (voir section 2) |
| `Web/Views/Home/Index.cshtml` (commentaire, ligne ~242) | Lien du parcours "Cap Bas Carbone" pointe vers `/formations` en générique tant que la fiche n'est pas saisie en back-office avec un `ChallengeId` réel (cf. `content/a-saisir/parcours-cap-bas-carbone.md`) |
| `Web/Views/Home/Index.cshtml` (bloc commenté, ligne ~257-271) | Témoignage client "carbone" : structure prête, désactivée tant qu'aucune vraie citation n'est fournie — rien n'a été inventé |
| `Web/Views/Home/LeLabDiagDecarbonAction.cshtml` | Ne pas affirmer que Challenges Factory est référencé/habilité par Bpifrance pour le Diag Décarbon'Action tant que ce n'est pas confirmé (distinct du marqueur déjà levé sur la page `/bilan-carbone`, qui ne portait pas cette affirmation) |
| `content/a-saisir/parcours-cap-bas-carbone.md` | Mode BtoB seul vs. ouverture BtoC à trancher ; objectif pédagogique / compétence cible / défi individuel à définir pour chacune des 8 thématiques avant saisie back-office |
| `content/a-saisir/article-diag-decarbon-action.md` | Même point que `LeLabDiagDecarbonAction.cshtml` (statut de référencement Bpifrance) |

## 4. Procédure de déploiement (préparation — non exécutée)

À suivre lorsque la mise en production sera explicitement demandée :

1. `git checkout main && git pull` puis `git merge feature/offre-climat` (ou PR + revue),
   jamais de push direct sur `main` sans accord.
2. `dotnet build` puis `dotnet test` sur la branche fusionnée — doit rester vert (81/81 à
   la date de ce document).
3. Vérifier qu'aucune variable d'environnement / chaîne de connexion n'a été modifiée par
   erreur (`git diff` sur `appsettings*.json` doit être vide — jamais touché durant cette
   mission).
4. Lever ou arbitrer les marqueurs de la section 3, au moins ceux visibles publiquement.
5. Saisir en back-office (Admin > Challenges) le contenu de
   `content/a-saisir/parcours-cap-bas-carbone.md` pour obtenir un `ChallengeId` réel, puis
   remplacer le lien générique `/formations` par `/formations/{challengeId}` sur la page
   d'accueil (cf. commentaire dans `Index.cshtml`).
6. Une fois en ligne : soumettre `https://challenges-factory.com/sitemap.xml` à Google
   Search Console (et équivalent Bing Webmaster Tools) — la présence du fichier seule ne
   suffit pas à déclencher l'indexation.
7. Contrôle visuel (desktop + mobile) sur l'environnement de production réel avant
   annonce publique, même si déjà vérifié en local à chaque phase.
