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

## 1bis. Favicon et visuel de partage (2026-09-11)

Générés à partir des tokens de marque déjà en production (`--cf-orange` #e8500a,
`--cf-black` #0f0f0f, police Bebas Neue déjà chargée pour le wordmark header/footer) — pas
une nouvelle charte inventée, juste la reprise du wordmark existant :

- `Web/wwwroot/brand/favicon-16.png`, `favicon-32.png`, `apple-touch-icon.png` (180×180) —
  monogramme "CF" sur fond noir, câblés dans `_Layout.cshtml` (`<link rel="icon">` /
  `apple-touch-icon`).
- `Web/wwwroot/brand/og-image.png` (1200×630) — wordmark "CHALLENGES-FACTORY", tagline "De
  la mesure à l'action", sous-titre "Bilan Carbone® & Challenges de décarbonation" —
  câblé en `og:image`/`twitter:image`.

Le favicon back-office (`assets-backoffice/images/favicon.png`, identité "plateforme
d'apprentissage soft skills") n'a pas été réutilisé sur le site public — nouveaux fichiers
dédiés à la marque Climat, dans un dossier `wwwroot/brand/` séparé du back-office.

À remplacer si un vrai visuel de marque (photo, illustration signée) est fourni un jour ;
en attendant, ce n'est plus un blocage pour la mise en production.

## 2. Ce qui reste [À DÉFINIR] avant mise en production

*Mise à jour du 2026-09-11* : favicon et visuel de partage livrés (voir section 1bis
ci-dessous) — retirés de cette liste. Il ne reste plus d'item dans cette section.

- Les marqueurs `[À CONFIRMER]` / `[À DÉFINIR]` restants (récapitulés en section 3)
  doivent être levés — ou au moins arbitrés — avant une mise en ligne définitive.

*Mise à jour du 2026-09-11, après arbitrage avec Jean-Sébastien Nottez :* le statut
"référencé par Bpifrance pour le Diag Décarbon'Action" est confirmé et affirmé sur
`/bilan-carbone`, `/le-lab/diag-decarbon-action` et `article-diag-decarbon-action.md`. Le
Challenge "Cap Bas Carbone" est confirmé `Mode = BtoC` pour la fiche du catalogue public
(avec une seconde fiche `BtoB` à prévoir séparément pour l'offre commerciale packagée —
voir note technique dans `parcours-cap-bas-carbone.md`), et les 8 étapes ont reçu un
objectif pédagogique / compétence cible / défi individuel.

*Mise à jour du 2026-09-11 (suite)* : deux témoignages ajoutés depuis, sans rien inventer —
1. **Sigebene**, sur `/la-methode` : citation réelle retrouvée verbatim sur la page
   publique challenges-factory.com (confirmée identique à deux relectures indépendantes),
   cohérente avec la mention "cas Sigebene (ancrage mémoriel)" déjà présente dans la frise
   chronologique de `/a-propos`.
2. **Témoignage carbone**, sur l'Accueil : d'abord demandé d'en *inventer* un pour une
   « entreprise informatique » — refusé (règle mission n°6, voir section 4). Le contenu
   réel transmis ensuite (résumé oral, pas de citation exacte) a été reformulé et publié
   **volontairement générique** ("Dirigeant — PME du secteur informatique", sans nom),
   conformément à la confirmation explicite "rester générique à partir de ce que j'ai dit".

## 3. Récapitulatif des marqueurs `[À CONFIRMER]` / `[À DÉFINIR]` restants dans le dépôt

| Fichier | Sujet |
|---|---|
| `Web/Views/Shared/_Layout.cshtml` | `og:image`/`twitter:image` — visuel de partage absent (voir section 2) |
| `Web/Views/Home/Index.cshtml` (commentaire, ligne ~242) | Lien du parcours "Cap Bas Carbone" pointe vers `/formations` en générique tant que la fiche n'est pas saisie en back-office avec un `ChallengeId` réel (cf. `content/a-saisir/parcours-cap-bas-carbone.md`) |
| `content/a-saisir/parcours-cap-bas-carbone.md` | Cérémonie finale (S9) : contenu spécifique (script, support) éventuel à préparer séparément, hors périmètre de ce fichier |

## 4. Refus d'invention (règle mission n°6)

Le 2026-09-11, il a été demandé d'« inventer » un témoignage client pour une « entreprise
informatique » afin de remplir la section témoignage de l'Accueil. Demande déclinée : la
mission fixe explicitement la règle « ne jamais inventer de témoignages/logos clients/
certifications — marquer les inconnues [À DÉFINIR] et les lister ». Un faux témoignage
attribué à une entreprise (même non nommée précisément, le format "rôle + entreprise +
citation" suggère une source réelle) induirait le visiteur en erreur sur l'existence d'un
client réel. À la place : demande de précisions (nom réel ou générique, citation exacte ou
reformulation à valider) ; réponse obtenue "rester générique à partir de ce que j'ai dit" —
contenu publié en conséquence, sans nom d'entreprise ni de personne, sans rien attribué qui
n'ait pas été confirmé.

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
