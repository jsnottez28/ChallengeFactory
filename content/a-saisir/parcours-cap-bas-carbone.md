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
| **Mode** | BtoB *(offre entreprise — [À CONFIRMER] si un mode BtoC doit aussi être ouvert)* |

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

Seuls les **titres de semaine** ont été fournis dans la mission — je n'ai pas inventé
d'objectif pédagogique, de compétence cible ni de défi individuel détaillé pour chacune :
ces trois champs sont marqués **[À DÉFINIR]** ci-dessous, à compléter avant publication.

### Étape 1 — S1 : Notre empreinte à la loupe
- **Objectif pédagogique** : [À DÉFINIR]
- **Compétence cible** : [À DÉFINIR]
- **Défi individuel** : [À DÉFINIR]

### Étape 2 — S2 : Pourquoi nous ?
- **Objectif pédagogique** : [À DÉFINIR]
- **Compétence cible** : [À DÉFINIR]
- **Défi individuel** : [À DÉFINIR]

### Étape 3 — S3 : Enquête terrain
- **Objectif pédagogique** : [À DÉFINIR]
- **Compétence cible** : [À DÉFINIR]
- **Défi individuel** : [À DÉFINIR]

### Étape 4 — S4 : Chasseurs de causes
- **Objectif pédagogique** : [À DÉFINIR]
- **Compétence cible** : [À DÉFINIR]
- **Défi individuel** : [À DÉFINIR]

### Étape 5 — S5 : Prototype bas carbone
- **Objectif pédagogique** : [À DÉFINIR]
- **Compétence cible** : [À DÉFINIR]
- **Défi individuel** : [À DÉFINIR]

### Étape 6 — S6 : Embarquer les autres
- **Objectif pédagogique** : [À DÉFINIR]
- **Compétence cible** : [À DÉFINIR]
- **Défi individuel** : [À DÉFINIR]

### Étape 7 — S7 : Négocier le changement
- **Objectif pédagogique** : [À DÉFINIR]
- **Compétence cible** : [À DÉFINIR]
- **Défi individuel** : [À DÉFINIR]

### Étape 8 — S8 : Ancrer dans la durée
- **Objectif pédagogique** : [À DÉFINIR]
- **Compétence cible** : [À DÉFINIR]
- **Défi individuel** : [À DÉFINIR]

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
