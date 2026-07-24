# CHALLENGES FACTORY — Contexte plateforme

## Quoi

CHALLENGES-FACTORY est une plateforme EdTech SaaS qui remplace la consommation
passive de contenus de formation par la résolution active de défis
(Challenge-Based Learning). Positionnement : le **"Strava des Soft Skills"**
— pas le "Netflix de la formation" (regarder), mais un outil pour **faire**.

Équation produit : **Défis + Cartes + Action**. L'utilisateur n'entre pas
dans un "cours" mais dans une **Cohorte** (ligue d'apprentissage, ~2 mois),
guidé au quotidien par un Agent Coach IA et encadré par des humains
(Tuteurs, Coachs).

Lancement commercial prévu : 1er septembre 2026.

---

## Les 4 principes non négociables (Le Manifeste)

Toute fonctionnalité développée doit respecter ces règles — elles priment
sur toute demande ponctuelle qui les contredirait :

1. **La preuve remplace le QCM.** Jamais de validation par quiz/QCM, même
   gamifié. Une compétence se valide par une **Preuve d'action** (photo,
   vidéo, témoignage écrit, capture d'écran) déposée sur le terrain.
2. **L'équipe avant l'individu.** Pas de leaderboard compétitif classant les
   individus les uns contre les autres. Le système de points valorise
   l'altruisme (aider un pair, valider les preuves des autres, donner un
   bon feedback) via des **"Points Augmentés"** — multiplicateurs pour
   comportement collectif, jamais pour compétition pure.
3. **L'humain valide, l'IA assiste.** L'IA (Agent Coach) est disponible
   24/7 pour la réactivité (FAQ, relances, pré-validation formelle), mais
   c'est toujours l'humain (pair, Tuteur, Coach) qui apporte la validation
   finale et la nuance. Ne jamais faire de l'IA le validateur final d'une
   compétence.
4. **Personnalisation, pas de "taille unique".** Le contenu se débloque
   progressivement selon le rythme du groupe/de l'individu ; pas d'accès
   massif à tout le catalogue d'un coup pour les offres d'entrée.
5. **Donnée = pilotage, jamais surveillance.** RGPD strict. La donnée sert à
   identifier qui a besoin d'aide, jamais à "fliquer" les utilisateurs.

---

## Objets métier centraux

- **Carte Apprenante** : l'unité atomique de compétence (soft skill unique).
  Structure fixe : Univers graphique, Objectif pédagogique, Mantra,
  Contexte/Exemple ("Le Pitch"), 3 Tips ("Hacks"), Punchline. Chaque carte
  possède un **code de référence** par thématique (ex : `MAN-C23` pour une
  carte "Management" sur le changement, `MAN-C15` pour l'OSBD) — ce code
  doit exister comme identifiant stable en base pour permettre de
  référencer une carte précise dans un parcours, un badge ou une preuve.
- **Ressource Directrice** : carte(s) poussée(s) par le système à une
  cohorte en fonction du défi/de la problématique choisie (ex : un défi
  "conflit d'équipe" pousse les cartes Recadrage/Faits, OSBD, Règles
  d'équipe, Reconnaissance).
- **Défi Individuel** : action solo sur le terrain → validée par dépôt
  d'une Preuve, validée à son tour par la Cohorte (les pairs) ou un
  Tuteur/Coach selon le niveau.
- **Défi Collectif (Squad)** : ne peut être résolu qu'en groupe (ex:
  "obtenir 3 feedbacks de collègues différents").
- **Cohorte** : groupe d'apprenants suivant un même défi/thématique sur une
  période donnée (ligue). Rythme le déblocage progressif des cartes.
- **Preuve** : livrable de validation (photo/vidéo/texte/capture). Objet
  central de tout le système de validation.
- **Points Standard / Points Augmentés** : système de scoring — les points
  augmentés récompensent l'entraide, pas la seule performance individuelle.
- **Kit de Lancement** : pack remis à chaque participant (carte de
  bienvenue, liste des équipes, codes d'accès, "Passeport"/scorecard).

---

## Gaming (ludification) & validation par la cohorte

### Ludification collaborative, jamais compétitive
- **Pas de leaderboard individuel classant les apprenants les uns contre
  les autres.** Toute mécanique de jeu doit renforcer la dynamique de
  groupe ("Tribu"/Squad), pas la compétition interpersonnelle.
- **Système XP à deux catégories nommées** (implémentation concrète des
  "Points Standard / Augmentés") :
  - **XP "Savoir"** : gagnés en validant une carte/ressource individuelle
    (ex : 50 XP par carte validée).
  - **XP "Entraide" / Points "Karma"** : gagnés en aidant un pair —
    relecture d'un script, réponse aux questions des autres, partage de
    bonnes pratiques (ex : 75 XP par action d'entraide, **volontairement
    supérieur** au XP "Savoir" pour inciter l'altruisme plus que la
    performance solo).
  - **Points "Assiduité"** : attribués aux équipes qui rendent leurs
    livrables collectifs à l'heure (distinct de la performance —
    récompense la régularité).
  - Toute nouvelle mécanique de points doit se rattacher à l'une de ces
    catégories ; ne pas créer de score générique/anonyme qui les mélange.
- **Règle d'or : aucun classement individuel de performance n'est
  publié.** Seul le **score collectif** de la cohorte/des équipes est
  affiché pour stimuler l'esprit de corps. Un classement individuel de
  points Karma/Entraide (qui a le plus aidé) est acceptable — c'est un
  classement de générosité, pas de performance — mais reste à valider
  au cas par cas contre le principe manifeste "pas de leaderboard".
- **Badges sociaux** (distincts des badges hebdomadaires de compétence) :
  ex. "Super Helper" (a le plus aidé ses pairs), "Éclaireur" (a partagé
  l'idée la plus innovante).
- **Scoring 360° (co-construit, jamais décidé par un seul acteur)** :
  - *Score Individuel* = validé par le pair/Buddy → note d'engagement.
  - *Score Collectif* = vote des autres équipes sur le meilleur livrable
    + bonus du Facilitateur/Chef de Projet.
- **Déblocage progressif** : les cartes/défis se débloquent étape par
  étape au rythme de la cohorte (semaine par semaine, alignée sur le
  cycle CBL — voir "Cycle de vie d'un Challenge" ci-dessus), pas en accès
  libre immédiat.
- **Badges hebdomadaires de compétence** : chaque semaine/sprint du
  parcours est associée à un badge nommé et à une compétence visée
  précise (ex : Semaine 2 = badge "L'Explorateur" / compétence "Écoute
  active"). Le badge se débloque à la validation de l'action terrain de
  la semaine, pas à la simple consultation des cartes. Distribution via
  n8n.
- **Notation entre pairs (Visio de Co-dev)** : lors des simulations en
  cohorte (jeu de rôle où des pairs incarnent des interlocuteurs
  "résistants/défensifs"), la posture de l'apprenant est **scorée par les
  pairs** selon des critères précis — c'est un mécanisme de scoring
  distinct de la validation de preuve (voir section suivante), à ne pas
  confondre : ici on évalue une performance simulée en direct, pas une
  preuve déposée a posteriori.
- **Progression de statut : Apprenant → Mentor.** Au-delà du "MasterGame"
  (niveau ultime validé en accompagnant une nouvelle cohorte, "Learning by
  Teaching"), la progression de statut doit être visible et valorisée
  comme un objectif de jeu à part entière, pas seulement comme un rôle
  administratif.

### Cycle de vie d'un Challenge (procédure de déploiement — 3 actes / 11 phases)

C'est le cadre de référence **canonique** pour tout Challenge déployé
(le parcours "Modulo-training" documenté par ailleurs en est une
instanciation particulière, pas la structure générale).

**ACTE 1 — Cadrage & Alignement (J-30)** : transformer une plainte
client en mission motivante.
1. *Réception de la demande* → livrable : Fiche "Demande Projet Client"
   (pain point, indicateurs actuels, impact business chiffré, cible/
   cohorte, cadrage temporel/urgence).
2. *Re-formulation* → livrable : Matrice de Reformulation (méthode du
   "Flip" : problème → enjeu de transformation positif, formule
   `[Verbe d'action] + [Objet du changement] + [Bénéfice final]`).
3. *Transformation en Challenge* → livrable : Fiche "Identité Challenge"
   (titre accrocheur, slogan, durée standard **8 semaines** de contenu
   sur **9 semaines calendaires**, promesse de fin).

**ACTE 2 — Design & Ingénierie (J-15)** : construire le parcours
sur-mesure.
4. *Diagnostic T0* → livrable : Rapport de Maturité Initial. Mesure sur
   3 axes : Savoir-Être (Soft), Savoir-Faire (Hard), Résultats (KPIs
   business). Outils : questionnaires perception, quiz/tests techniques,
   observation "Vis ma vie", audit de livrables, KPI business.
5. *Construction (Architecture CBL)* → livrable : Roadmap Challenge
   Détaillée. Le parcours de 8 semaines suit 3 phases CBL : **Engagement**
   (S1-S2, s'approprier), **Investigation** (S3-S4, comprendre le réel),
   **Action** (S5-S8, transformer et ancrer). Chaque semaine est
   scénarisée en 5 ingrédients : Défi de la Semaine (mission), Objectif
   Pédagogique, Livrable Collectif, Compétence Cible, Ressources (3-5
   cartes), Micro-défi Individuel.
6. *Sélection des contenus / micro-learning* → livrable : séquence
   éditoriale du "Daily Nudge" (règle du 1-3-1 : 1 notion + 3-5
   ressources + 1 défi terrain), rythme hebdo lun-ven (teasing → méthode
   → inspiration → action → quiz/synthèse).
7. *Planification opérationnelle* → livrable : Planning de Cohorte (voir
   mécanique du Tuilage ci-dessous).

**ACTE 3 — Déploiement & Impact (J-Jour à J+9 semaines)** : animer,
mesurer, pérenniser.
8. *Lancement (Kick-off)* → livrable : Kit de Lancement Challenge
   (carte de bienvenue, liste des équipes, codes d'accès, "Passeport"/
   scorecard). Format 45-60 min : mot du Sponsor, présentation méthode,
   constitution des équipes (Squads nommés), lancement du 1er défi.
9. *Pilotage hebdomadaire* → livrable : Tableau de Bord de Cohorte
   (météo d'équipe, top contributions, avancement, mur des
   célébrations). Voir sections Gaming et Validation par la cohorte.
10. *Mesure d'impact (Bilan T-Final)* → livrable : Rapport d'Impact
    Challenge (ROI). Méthode Miroir : on ne mesure à la fin QUE ce qui a
    été diagnostiqué en Phase 4 (mêmes indicateurs). Calcul du Delta
    (Δ) entre T0 et T-Final sur 3 niveaux : Performance Opérationnelle
    (Hard KPIs), Indicateurs Humains/Culturels (Soft KPIs), Acquis
    Pédagogiques.
11. *Capitalisation* → livrable : Dossier de Capitalisation (Knowledge
    Base) — cartes/historique restent accessibles en illimité après le
    Challenge, curation des "Pépites" (meilleures productions promues au
    rang de standard officiel), export de la base "Savoir Métier".

### La mécanique du "Tuilage" (J+7)

Principe central à respecter dans toute logique de planning/cohorte :
**le parcours dissocie l'apprentissage individuel (immédiat) de la
production collective (décalée d'une semaine)**. En semaine S, l'apprenant
travaille individuellement sur le **nouveau** thème S, tout en finalisant
**collectivement** le livrable du thème précédent S-1. Cela garantit un
temps de réflexion/collaboration avant le rendu collectif. Le planning de
cohorte doit systématiquement distinguer ces deux flux dans son modèle de
données (thème individuel courant ≠ thème du livrable collectif attendu
cette semaine-là).

**Boucle CBL hebdomadaire (le rythme apprenant, quel que soit le thème)** :
- **Temps 1 — Engagement** (début semaine) : réception du "Défi de la
  Semaine", consommation des ressources pour comprendre le sens.
- **Temps 2 — Investigation** (milieu semaine) : Micro-Défi Individuel
  sur le terrain + collaboration asynchrone sur le livrable collectif en
  cours (celui de S-1, cf. Tuilage).
- **Temps 3 — Action** (fin semaine) : dépôt du livrable collectif
  finalisé + Rituel Synchrone (visio) de partage/feedback/clôture.

### Validation des preuves par la cohorte (peer-to-peer)
C'est le mécanisme central de validation d'une compétence — à concevoir
avec soin fonctionnellement :

1. **Dépôt de la preuve** : l'apprenant dépose une preuve d'action (photo,
   vidéo, texte, capture d'écran) sur le défi individuel qu'il vient de
   réaliser.
2. **Pré-validation IA (niveau 1)** : l'Agent Coach IA vérifie uniquement
   la **conformité formelle** de la preuve (ex : présence effective d'une
   photo, netteté) — il ne juge jamais la qualité pédagogique du contenu.
   Cette étape filtre avant transmission humaine, elle ne remplace pas la
   validation humaine.
3. **Validation par les pairs (cœur du système)** :
   - Modèle **Binôme/Buddy** : chaque apprenant est associé à un pair
     (hebdomadaire ou pour la durée du défi). Le binôme échange en amont
     pour préparer le micro-défi, puis c'est le pair qui **valide** la
     réussite sur la plateforme (ex. "Je certifie que [prénom] a bien
     réalisé son feedback positif cette semaine").
   - Modèle **Cohorte élargie** : au-delà du binôme, tout membre de la
     cohorte peut être sollicité pour valider une preuve et laisser un
     feedback — cette activité de validation/feedback génère elle-même des
     Points Augmentés pour le validateur.
4. **Escalade Tuteur Master (niveau 2)** : les défis "standards" ne
   nécessitant pas d'expertise technique pointue peuvent être validés par
   un Tuteur Master plutôt que par les pairs (selon config du défi).
5. **Escalade Coach (niveau 3)** : intervient uniquement sur exception —
   décrochage critique, conflit, incompréhension majeure signalés par
   l'IA ou par un Tuteur — et reste seul habilité à la certification
   finale.

**Point d'implémentation important** : la fonctionnalité de validation
doit toujours pouvoir tracer *qui* a validé (pair/binôme, Tuteur, ou
Coach) et *à quel niveau*, car cela conditionne les Points Augmentés
attribués et la traçabilité RGPD/pilotage (jamais de surveillance, mais
la donnée de validation doit rester auditable).

---

## Mesure d'impact & KPI (fonctionnalité clé côté gestionnaire)

Le pilotage par la donnée est une fonctionnalité produit à part entière,
pas seulement un livrable de prestation ponctuel — le Rapport de Maturité
Initial (T0) et le Rapport d'Impact (T-Final) doivent pouvoir être générés
depuis la plateforme pour tout Challenge, en respectant la **Méthode
Miroir** : on ne compare en fin de parcours que les indicateurs
effectivement mesurés au départ.

**4 niveaux d'indicateurs (inspirés du modèle Kirkpatrick)** :
1. **Satisfaction** : taux de satisfaction, NPS, taux de complétion —
   n'atteste jamais à lui seul de l'apprentissage.
2. **Acquisition de connaissances** : score pré-test/post-test, taux de
   validation des défis.
3. **Changement de pratique** : % d'apprenants ayant mis en œuvre au
   moins une action concrète, nombre d'outils testés en situation réelle
   — c'est le niveau le plus différenciant pour Challenges Factory
   (grâce au système de preuves).
4. **Impact organisationnel** : évolution du climat d'équipe, réduction
   des tensions/risques psychosociaux, amélioration de la coopération.

**KPIs à exposer en priorité dans les tableaux de bord gestionnaire** :
taux de complétion, taux de participation aux défis, progression
pré-test/post-test, % d'apprenants ayant testé une pratique dans leur
travail, nombre d'actions mises en place par apprenant, taux de
satisfaction/NPS, amélioration perçue du climat d'équipe.

---

## Rôles utilisateurs (chaîne d'accompagnement tri-partite)

### Côté "apprenants" (learners)

| Rôle | Description | Fonctions clés |
|---|---|---|
| **Apprenant** | Utilisateur final en cohorte | Rejoindre/créer un défi, débloquer des cartes, déposer des preuves, valider les preuves des pairs (binôme/buddy), consulter son score, participer aux visios de cohorte |
| **Tuteur Master** | Apprenant avancé/Alumni certifié (niveau 2) | Animer le squad/chat de cohorte, valider les défis "standards" (niveau 1), signaler les cas complexes au Coach |
| **Agent Coach IA** | Non-humain, niveau 1 | Répondre aux questions (FAQ dynamique), relancer les inactifs ("nudge"), pré-valider la conformité formelle des preuves avant transmission humaine |

### Côté "gestionnaires" (management/admin)

| Rôle | Description | Fonctions clés |
|---|---|---|
| **Coach Challenges Factory** | Formateur expert salarié/partenaire certifié (niveau 3) | Pilotage par exception via tableau de bord (alertes IA de décrochage/conflit), anime visios de lancement et clôture, seul habilité à certifier/délivrer les attestations. Peut superviser jusqu'à ~400 apprenants |
| **Chef de Projet Factory** | Pilote opérationnel de la prestation, du cadrage à la capitalisation ("Gardien du Temps" / "Chef d'Orchestre") | Recueille la Fiche Demande Projet, produit la Roadmap et le Planning de Cohorte, anime le Kick-off, pilote hebdo (encouragements, relances des silencieux, pré-correction des livrables), produit le Rapport d'Impact et le Dossier de Capitalisation |
| **Ingénieur Pédagogique** | Conception du parcours (peut être la même personne que le Chef de Projet) | Scénarise l'architecture CBL (défi de la semaine, ressources, micro-défi), sélectionne/programme le micro-learning quotidien |
| **Sponsor Client** | Direction/manager côté client, non gestionnaire plateforme au sens strict mais acteur clé de la prestation B2B | Valide la faisabilité terrain des défis, légitime le projet au Kick-off, fournit les KPIs business sensibles pour le diagnostic T0 et le bilan final |
| **Gestionnaire / Admin entreprise (B2B)** | RH, manager côté client | Licence globale : restreindre l'affichage aux défis internes ou ouvrir au catalogue public ; tableau de bord "Gestion des Talents" (détection des profils moteurs, galerie de preuves comme preuve de savoir-faire) ; gestion des accès collaborateurs |
| **Studio de Création (offre Premium/Stratégique)** | Auteur interne côté client | Transformer procédures/méthodes internes en défis ludifiés via l'"Accélérateur Auteur IA" ; créer des parcours d'onboarding ou "Valeurs & Culture" |

**Règle importante** : selon l'offre B2B, la création de "Défis Individuels"
isolés peut être **désactivée** (offre Standard : tout doit profiter au
collectif) ou **activée** (offre Stratégique : autonomie complète
individuel + collectif). Vérifier l'offre/plan actif avant d'exposer cette
fonctionnalité.

---

## Architecture technique

- **Plateforme : full .NET.** L'ensemble de la plateforme applicative
  (apprenants + gestionnaires) est développé en **.NET 8 (ASP.NET Core)**,
  architecture microservices — notation des compétences transversales,
  appariement de modules, algorithmes de recommandation, gestion des
  cohortes, gaming/scoring, API. Conçue pour bascule vers infrastructure
  conteneurisée (Docker) selon la charge.
- **IA : solution Google.** Les briques IA (assistant Agent Coach,
  génération/adaptation de contenu, analyse sémantique) s'appuient sur les
  **solutions Google** (modèles Gemini / Google Cloud AI). Ne pas
  développer ou proposer d'intégration Mistral AI ou autre fournisseur LLM
  sans confirmation explicite.
- **Automatisation : n8n.** Les workflows d'orchestration (notifications,
  relances, badges, nudges) passent par **n8n** — objectif : zéro tâche
  admin manuelle à faible valeur ajoutée.
- **CRM/Marketing : solution gratuite pour l'instant.** Pas d'outil
  payant type HubSpot/Brevo à ce stade — utiliser une solution gratuite
  (ex : tier gratuit d'un CRM, Google Sheets/Forms, ou équivalent) pour
  le pipeline B2B et les notifications. Ne pas développer d'intégration
  poussée avec un outil CRM/marketing payant sans confirmation explicite ;
  privilégier une architecture qui permettra de swapper facilement vers
  une solution payante plus tard (ne pas coupler fortement le code métier
  à l'outil choisi).
- **Sécurité/Conformité** : RGPD strict, DPO interne, ISO 27001. Stockage
  objet cloud pour documents sensibles.

---

## ⚠️ Point de tension entre documents sources (non résolu)

Le `LE_MANIFESTE_CHALLENGE_FACTORY.docx` pose en principe fondateur le
**refus explicite du QCM** comme mécanisme de validation ("nous refusons
la validation par QCM, même gamifiés"). Le document
`PROCÉDURE_CHALLENGES_FACTORY_2_.docx` (ajouté plus récemment) propose à
l'inverse un système de preuve à plusieurs étages où le **Quiz/QCM
apparaît comme la première brique** ("nécessaire mais insuffisante"),
suivie de Challenge → Mini livrable → Feedback pair/manager → Badge, et
recommande des KPI comme le "score moyen QCM" ou le "score pré-test/
post-test".

**Ces deux sources se contredisent frontalement sur la place du QCM.**
Tant que ce point n'est pas tranché explicitement par toi, Claude Code ne
doit **pas** développer de mécanisme de QCM comme validateur de
compétence — le principe du Manifeste prévaut par défaut (règle "Ce qu'il
ne faut jamais faire" ci-dessous). Un QCM utilisé uniquement comme
**pré-test/post-test de connaissance** (mesure de progression, pas
validation de compétence) est en revanche compatible avec les deux
documents et peut être développé sans ambiguïté.

---

## Charte graphique (référence : challenges-factory.com)

Le site officiel **https://challenges-factory.com/** fait foi pour
l'identité visuelle et le ton. Tout écran/composant développé pour la
plateforme (apprenant ou gestionnaire) doit rester cohérent avec cette
charte.

### Identité visuelle
- **Palette** : hero en noir avec dégradé, système d'accent **orange**.
  ⚠️ *Le fetch texte du site ne restitue pas les codes hex exacts ni la
  police utilisée — à confirmer avec toi (via le CSS du thème WordPress,
  un export de charte, ou capture d'écran) avant qu'une valeur précise
  soit codée en dur quelque part.*
- **Nom de la marque toujours stylisé** : "CHALLENGES-FACTORY" (majuscules,
  tiret) dans le header/footer ; en usage courant "Challenges Factory".

### Ton et structure du contenu (confirmés par le site)
- **Punchline courte, contraste avant/après** : ex. *"Vos équipes
  apprennent en faisant"*, *"Fini les formations que l'on oublie le
  lendemain"*. Toujours partir d'un constat désirable (douleur) puis
  offrir un renversement positif — cohérent avec la méthode du "Flip"
  (Étape 2 de la procédure).
- **Blocs statistiques chiffrés en bandeau hero** : le site utilise
  activement ce pattern (`75%` rétention, `0€` coût d'absence, `9S`
  durée). **⚠️ Ceci contredit une règle précédemment établie** ("pas de
  blocs statistiques dans le contenu formation") — à trancher avec toi :
  soit la règle ne s'appliquait qu'aux *pages de formation individuelles*
  (pas au site vitrine/marketing), soit elle doit être levée. Par défaut,
  j'autorise ces blocs sur les pages marketing/vitrine (hero, sections
  "Le constat", offres) mais je continue à les éviter dans le contenu
  pédagogique des Cartes de Compétences, sauf confirmation contraire.
- **Structure "3 Actes"** : badges numérotés (ACTE 01/02/03) avec repère
  temporel (J-30, 8S, ROI) — reprend exactement le cadre "3 actes / 11
  phases" déjà documenté ; bonne confirmation de cohérence entre les
  documents source et le site public.
- **Cartes/tuiles avec émoji ou icône en tête** (🔍 S'engager, 🌍
  Investiguer, ⚡ Agir ; 💰 📊 🎯 🔒 pour les bénéfices DRH) — pattern
  systématique à réutiliser pour toute présentation de phase/bénéfice.
- **Citations clients encadrées** avec attribution (rôle + entreprise +
  mention "Cas Terrain Validé [année]").
- **Cartes tarifaires à 3 niveaux** avec un niveau "Recommandé" mis en
  avant (Starter / Programme PME / Licence Interne) — pattern à réutiliser
  si la plateforme expose un espace de souscription/upgrade B2B.
- **CTA constant et répété** : "📅 Cadrer mon Challenge →" — garder un CTA
  unique et cohérent plutôt que d'en varier la formulation selon les
  pages.

### Vocabulaire de marque à respecter strictement
- "Carte de Compétences" (site public) vs "Carte Apprenante" (documents
  internes) — **synonymes**, mais préférer "Carte de Compétences" dans
  tout ce qui est visible par l'utilisateur final ou le client B2B.
- "Coach Senior" (site) = "Coach Challenges Factory" (documents internes).
- "Squad" = équipe/cohorte restreinte au sein d'une cohorte plus large.
- "Méthode Miroir" : terme officiel pour la comparaison T0 vs T-Final —
  toujours utiliser ce nom, ne pas le paraphraser dans l'UI.

---

## Deux modes de la plateforme : BtoC (apprenant individuel) vs BtoB (entreprise)

La plateforme doit gérer **deux modes d'accès distincts**, avec des
parcours, permissions et surfaces d'écran différents. Toute fonctionnalité
développée doit préciser explicitement dans quel(s) mode(s) elle
s'applique — ne jamais supposer qu'une fonctionnalité conçue pour un mode
est valable pour l'autre sans vérification.

### Mode BtoC — apprenant individuel
- **Qui** : particulier qui s'inscrit lui-même, sans entreprise/licence
  derrière lui. Pas de Sponsor, pas de DRH, pas de Gestionnaire.
- **Accès catalogue** : catalogue public complet des parcours/Challenges
  disponibles (`/formations/`), inscription directe à une cohorte
  ouverte/publique.
- **Cohorte** : cohortes publiques mixant des apprenants d'entreprises et
  contextes différents (pas de cloisonnement par organisation).
- **Rôles actifs** : Apprenant, Tuteur Master, Agent Coach IA. **Pas** de
  rôle Coach Senior dédié, pas de Chef de Projet Factory dédié (le
  pilotage hebdo est assuré par l'Agent Coach IA + Tuteurs Master, sauf
  escalade).
- **Paiement/accès** : logique d'abonnement ou de paiement à l'unité par
  individu (à préciser avec toi — pas encore documenté dans les sources
  actuelles).
- **Pas de tableau de bord "Gestion des Talents"**, pas de Rapport
  d'Impact/ROI agrégé — l'apprenant BtoC voit seulement sa propre
  progression, ses propres preuves, son propre score.
- **Défis Individuels** : activés par défaut (pas de contrainte
  "tout doit profiter au collectif" imposée par une offre B2B Standard).

### Mode BtoB — entreprise (Starter / Programme PME / Licence Interne)
- **Qui** : passe par un contrat entreprise (une des 3 offres du site :
  Starter 3 900€/cycle, Programme PME 1 490€/mois, Licence Interne sur
  devis). Un Sponsor Client et/ou un Gestionnaire/Admin (RH, manager)
  encadrent le dispositif.
- **Accès catalogue** : **restreint et paramétrable** par le Gestionnaire
  — licence globale permettant de limiter l'affichage aux défis internes
  de l'entreprise ou d'ouvrir au catalogue public, selon l'offre.
- **Cohorte** : cohortes **cloisonnées par organisation** (les apprenants
  d'une entreprise ne se mélangent pas avec d'autres organisations, sauf
  paramétrage explicite).
- **Rôles actifs** : l'ensemble de la chaîne — Apprenant, Tuteur Master,
  Agent Coach IA, **Coach Senior/Challenges Factory**, **Chef de Projet
  Factory**, **Ingénieur Pédagogique**, **Sponsor Client**,
  **Gestionnaire/Admin entreprise**.
- **Facturation** : au niveau entreprise (contrat/cycle), pas par
  apprenant individuel.
- **Tableaux de bord exclusifs BtoB** : "Gestion des Talents" (détection
  des profils moteurs), Cartographie des compétences (livrable PDF, offre
  PME+), Rapport d'Impact/ROI (Méthode Miroir T0 → T-Final), Knowledge
  Base exportable (offre PME+), Dossier de Capitalisation.
- **Défis Individuels** : le paramétrage dépend de l'offre — **désactivé**
  sur l'offre Standard/Starter (tout doit profiter au collectif),
  **activé** sur l'offre Stratégique/Licence Interne (autonomie complète
  individuel + collectif). Vérifier l'offre active avant d'exposer cette
  fonctionnalité (règle déjà posée plus haut, rappelée ici pour le
  contexte BtoB).
- **Studio de Création** (offre Licence Interne) : les managers/formateurs
  internes deviennent facilitateurs certifiés et peuvent transformer leurs
  propres procédures/savoirs métier en défis via l'Accélérateur Auteur IA
  — fonctionnalité strictement réservée à ce mode/cette offre.

### Point d'implémentation transverse
Le choix du mode (BtoC / BtoB) doit être un attribut de premier niveau du
compte utilisateur et/ou de la cohorte (pas déduit indirectement d'une
autre donnée), car il conditionne : la visibilité du catalogue, la
présence ou non de rôles de delivery (Coach Senior, Chef de Projet,
Sponsor), l'activation des tableaux de bord agrégés, et les règles de
facturation. En cas de doute sur le comportement attendu pour une
fonctionnalité qui n'est pas couverte explicitement ci-dessus, demander
avant de développer plutôt que de supposer un comportement par défaut.

---

## Ce qu'il ne faut jamais faire

- Ne pas introduire de QCM/quiz comme mécanisme de **validation** de
  compétence (cf. point de tension ci-dessus — un QCM de mesure de
  progression pré/post reste acceptable).
- Ne pas créer de classement individuel compétitif (leaderboard toxique).
- Ne pas laisser l'IA délivrer une certification ou valider une compétence
  finale sans intervention humaine.
- Ne pas exposer d'accès illimité au catalogue complet sur les offres
  d'entrée (Challenge/Standard) — le déblocage doit rester progressif.
- Ne pas construire de fonctionnalité de tracking/surveillance individuelle
  qui s'écarte du pilotage pédagogique (risque RGPD + contraire au
  Manifeste).

---

## Notes pour Claude Code

- Toujours vérifier **quel type d'utilisateur** (apprenant / tuteur / coach
  / gestionnaire B2B) est concerné avant de développer une fonctionnalité —
  les permissions et la logique métier diffèrent fortement selon l'offre
  (Challenge / Premium / Prestige / BtoB Standard / Premium / Stratégique).
- Le contenu produit (textes, UI, notifications) est **toujours en
  français**.
- Se référer au `PROCÉDURE_CHALLENGES_FACTORY.docx`, au
  `PROCÉDURE_CHALLENGES_FACTORY_2_.docx` (procédure de déploiement
  complète en 11 phases — source la plus détaillée sur le processus), au
  `LE_MANIFESTE_CHALLENGE_FACTORY.docx` et au
  `Guide_de_Déploiement___Résoudre_les_Crises_d_Équipe_par_le_Modulo-training.docx`
  du projet pour le détail du déroulé pédagogique, les principes
  fondateurs et un exemple concret et complet de parcours CBL de bout en
  bout, avant de trancher une ambiguïté fonctionnelle.
- Ce fichier décrit le **scope produit cible** (vision plan d'affaires). Il
  peut y avoir un écart avec l'état réel du code à un instant T — en cas de
  doute sur ce qui est déjà implémenté, demander avant de supposer.
