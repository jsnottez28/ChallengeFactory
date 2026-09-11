# Fiche à saisir — Challenge « Cap Bas Carbone »

Contenu prêt à saisir dans le back-office : **Administration → Challenges → Créer un Challenge**,
puis **Architecture → Ajouter une étape** (×8). Les champs ci-dessous correspondent exactement
aux champs du formulaire (`Web/Views/Challenges/Save.cshtml` et `SaveEtape.cshtml`).

Aucune écriture en base n'a été faite par Claude — ce fichier est une préparation, pas une
migration ni un script SQL.

---

## Challenge

| Champ | Valeur |
|---|---|
| **Code** | `CHAL-CAP-BAS-CARBONE` *(à confirmer — identifiant stable libre, sert à l'import Excel)* |
| **Titre** | Cap Bas Carbone |
| **Slogan** | 9 semaines pour passer du bilan carbone à l'action, en équipe |
| **Nombre d'étapes** | 8 *(les 8 thématiques de contenu ; la cérémonie finale de la semaine 9 n'est pas une étape à part — elle a lieu à la clôture de l'étape 8, comme le fait déjà le système pour toute clôture de Challenge)* |
| **Mode** | BtoC *(confirmé — cf. note technique ci-dessous)* |
| **Thématique** | Décarbonation *(champ ajouté le 2026-09-11, migration `AddThematiqueChallenge` — distingue ce parcours des Challenges "Humain & Organisation" sur le catalogue public /formations)* |

> **Note technique — `Challenge.Mode` (`Domain/Entities/ModePlateforme.cs`) est un enum
> exclusif (`BtoB` ou `BtoC`, jamais les deux) : impossible d'ouvrir un même Challenge aux
> deux modes sans modifier le schéma, ce qui est hors périmètre de cette mission (règle
> "aucune migration/changement de schéma sans accord explicite"). Or `HomeController.Formations()`
> — la page publique vers laquelle pointe le bouton "Découvrir le parcours" de l'Accueil —
> ne liste **que** les Challenges `Mode == BtoC`. Pour que ce Challenge soit effectivement
> visible sur ce lien déjà publié, il doit donc être saisi en `BtoC`.
>
> Cela ne couvre cependant pas l'offre commerciale packagée vendue sur l'Accueil ("Pack
> Bilan + Challenge Cap Bas Carbone", "Challenge Cap Bas Carbone seul" — devis PME/ETI,
> cohorte fermée à l'organisation cliente) : cette offre-là est par nature BtoB. **Une
> seconde fiche Challenge, en `Mode = BtoB`**, sera donc nécessaire pour la vente aux
> entreprises — même titre/contenu pédagogique, cohorte restreinte à l'organisation
> cliente plutôt qu'ouverte au catalogue public. À saisir séparément le moment venu ; ce
> fichier ne prépare que la version `BtoC` du catalogue public.

### Description (champ HTML — à coller tel quel)

```html
<p>On a les chiffres, mais personne ne se sent concerné. Le plan d'action n'avance pas.</p>
<p>Pendant 9 semaines (8 thématiques + une semaine de cérémonie finale), des équipes
constituées par poste d'émission testent des actions de réduction, les chiffrent,
les font valider par leurs pairs et les présentent à la direction.</p>
<p><strong>Cible :</strong> tous les collaborateurs, en équipes par poste d'émission.</p>
<p><strong>Niveau :</strong> tous niveaux.</p>
<p><strong>À l'issue du parcours, chaque participant sait :</strong></p>
<ul>
  <li>lire le bilan carbone de son entreprise ;</li>
  <li>chiffrer un levier de réduction ;</li>
  <li>tester une action et en convaincre ses collègues ;</li>
  <li>présenter un plan d'action argumenté (tCO2e, coût, gain).</li>
</ul>
```

---

## Étapes (8)

Les titres de semaine venaient de la mission d'origine ; objectif pédagogique, compétence
cible et défi individuel ont été rédigés en cohérence avec la boucle CBL Engagement (S1-2)
→ Investigation (S3-4) → Action (S5-8) et avec l'illustration déjà publiée sur
`/la-methode` ("Cap Bas Carbone, en équipe" : défi S1 = lister les postes qui émettent le
plus, résultat S8 = action testée, chiffrée, présentée à la direction). Ce sont des choix
de conception pédagogique, pas des faits à vérifier — à ajuster librement en back-office
si une autre formulation convient mieux.

### Étape 1 — S1 : Notre empreinte à la loupe
- **Objectif pédagogique** : Comprendre à quoi correspond un bilan carbone et repérer ce qui, dans son propre poste, émet le plus.
- **Compétence cible** : Lire et interpréter les grandes lignes d'un bilan carbone (scopes, postes d'émission).
- **Défi individuel** : Lister les postes de son activité qui émettent le plus et noter ses hypothèses, à vérifier ensuite.

### Étape 2 — S2 : Pourquoi nous ?
- **Objectif pédagogique** : Relier l'enjeu de décarbonation de l'entreprise à son propre poste de travail, au-delà de l'obligation réglementaire.
- **Compétence cible** : Situer son activité quotidienne dans la trajectoire carbone de l'entreprise.
- **Défi individuel** : Formuler en une phrase ce que la décarbonation change concrètement pour son propre poste.

### Étape 3 — S3 : Enquête terrain
- **Objectif pédagogique** : Aller chercher les données réelles de son poste d'émission plutôt que de s'appuyer sur des estimations générales.
- **Compétence cible** : Collecter et documenter une donnée d'émission, source par source.
- **Défi individuel** : Recenser les sources réelles d'émission de son poste (factures, déplacements, consommations) et les données à remonter.

### Étape 4 — S4 : Chasseurs de causes
- **Objectif pédagogique** : Identifier les causes réelles des émissions de son poste, pas seulement les symptômes visibles.
- **Compétence cible** : Distinguer un levier de réduction réel d'une fausse bonne idée.
- **Défi individuel** : Identifier avec son équipe 2 à 3 causes principales des émissions de son poste et les leviers de réduction associés.

### Étape 5 — S5 : Prototype bas carbone
- **Objectif pédagogique** : Passer de l'idée à une action testable, même à petite échelle.
- **Compétence cible** : Construire et tester une action de réduction concrète sur son poste d'émission.
- **Défi individuel** : Tester une action de réduction pendant une semaine et noter ce qui fonctionne ou non.

### Étape 6 — S6 : Embarquer les autres
- **Objectif pédagogique** : Convaincre ses collègues d'adopter l'action testée, sans la leur imposer.
- **Compétence cible** : Mobiliser ses pairs autour d'un changement de pratique (savoir-être, posture).
- **Défi individuel** : Présenter son action testée à au moins deux collègues et recueillir leur retour.

### Étape 7 — S7 : Négocier le changement
- **Objectif pédagogique** : Chiffrer une action de réduction pour pouvoir la défendre auprès de la direction.
- **Compétence cible** : Construire un argumentaire chiffré (tCO2e, coût, gain) pour une décision de transition.
- **Défi individuel** : Chiffrer son action de réduction (tCO2e évitées, coût, gain) et préparer sa présentation.

### Étape 8 — S8 : Ancrer dans la durée
- **Objectif pédagogique** : Transformer une action ponctuelle en nouvelle pratique durable de l'équipe.
- **Compétence cible** : Pérenniser un changement de pratique au-delà du Challenge.
- **Défi individuel** : Présenter son action de réduction testée et chiffrée à la direction, et proposer comment la pérenniser.

---

## Semaine 9 — Cérémonie finale (hors étapes)

« Cérémonie finale et présentation des équipes à la direction. » Ne correspond à aucun champ
du formulaire Challenge/Étape actuel — se déroule naturellement à la clôture de l'étape 8.
[À CONFIRMER] si un contenu spécifique (script de cérémonie, support de présentation) doit être
préparé séparément (hors périmètre de ce fichier).

---

## Une fois saisi

1. Mettre à jour, dans `Web/Views/Home/Index.cshtml`, le lien du bloc « Cap Bas Carbone » pour
   pointer vers `/formations/{challengeId}` (l'ID réel attribué à la création), au lieu du lien
   générique actuel vers `/formations`.
2. Publier le Challenge (bouton « Publier » sur la page Architecture) pour qu'il apparaisse dans
   le catalogue public `/formations`.
