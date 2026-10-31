// Formulaire admin d'article (/admin/articles) :
// - genere le slug depuis le titre tant qu'il n'a pas ete modifie a la main ;
// - redimensionne/recadre l'image de couverture en 1200x630 cote navigateur avant l'envoi
//   (aucune dependance de traitement d'image cote serveur - cf. ArticlesAdminController,
//   qui se contente de verifier que le fichier recu fait bien 1200x630) ;
// - affiche un apercu Markdown indicatif (conversion volontairement simple, le rendu
//   final est toujours recalcule cote serveur via Markdig - cf. ArticleService.RendreHtml).
(function () {
    var LARGEUR_CIBLE = 1200;
    var HAUTEUR_CIBLE = 630;

    var form = document.getElementById('articleForm');
    if (!form) {
        return;
    }

    // ---- Slug automatique ----
    var champTitre = document.getElementById('titre');
    var champSlug = document.getElementById('slug');
    var slugModifieManuellement = !!(champSlug && champSlug.value);

    function slugifier(texte) {
        return (texte || '')
            .normalize('NFD')
            .replace(/[̀-ͯ]/g, '')
            .toLowerCase()
            .replace(/[^a-z0-9]+/g, '-')
            .replace(/-{2,}/g, '-')
            .replace(/^-+|-+$/g, '');
    }

    if (champTitre && champSlug) {
        champTitre.addEventListener('input', function () {
            if (!slugModifieManuellement) {
                champSlug.value = slugifier(champTitre.value);
            }
        });
        champSlug.addEventListener('input', function () {
            slugModifieManuellement = champSlug.value.length > 0;
        });
    }

    // ---- Compteur meta description ----
    var champMeta = document.getElementById('meta');
    var compteurMeta = document.getElementById('metaCompteur');
    if (champMeta && compteurMeta) {
        var majCompteur = function () { compteurMeta.textContent = champMeta.value.length; };
        champMeta.addEventListener('input', majCompteur);
        majCompteur();
    }

    // ---- Redimensionnement de l'image de couverture (recadrage centre en 1200x630) ----
    var champImage = document.getElementById('image');
    var apercuImage = document.getElementById('imageApercu');

    function redimensionner(fichier) {
        return new Promise(function (resolve, reject) {
            var image = new Image();
            image.onload = function () {
                var ratioCible = LARGEUR_CIBLE / HAUTEUR_CIBLE;
                var ratioSource = image.width / image.height;

                var largeurRognee = image.width;
                var hauteurRognee = image.height;
                var decalageX = 0;
                var decalageY = 0;

                if (ratioSource > ratioCible) {
                    largeurRognee = image.height * ratioCible;
                    decalageX = (image.width - largeurRognee) / 2;
                } else {
                    hauteurRognee = image.width / ratioCible;
                    decalageY = (image.height - hauteurRognee) / 2;
                }

                var canvas = document.createElement('canvas');
                canvas.width = LARGEUR_CIBLE;
                canvas.height = HAUTEUR_CIBLE;
                var contexte = canvas.getContext('2d');
                contexte.drawImage(image, decalageX, decalageY, largeurRognee, hauteurRognee, 0, 0, LARGEUR_CIBLE, HAUTEUR_CIBLE);

                canvas.toBlob(function (blob) {
                    if (!blob) {
                        reject(new Error('Conversion impossible'));
                        return;
                    }
                    var extension = fichier.type === 'image/png' ? 'png' : 'jpg';
                    var typeMime = fichier.type === 'image/png' ? 'image/png' : 'image/jpeg';
                    var nom = 'couverture.' + extension;
                    resolve(new File([blob], nom, { type: typeMime }));
                }, fichier.type === 'image/png' ? 'image/png' : 'image/jpeg', 0.9);
            };
            image.onerror = reject;
            image.src = URL.createObjectURL(fichier);
        });
    }

    if (champImage) {
        champImage.addEventListener('change', function () {
            var fichier = champImage.files && champImage.files[0];
            if (!fichier) {
                return;
            }

            redimensionner(fichier).then(function (fichierRedimensionne) {
                var transfert = new DataTransfer();
                transfert.items.add(fichierRedimensionne);
                champImage.files = transfert.files;

                if (apercuImage) {
                    apercuImage.src = URL.createObjectURL(fichierRedimensionne);
                    apercuImage.classList.remove('d-none');
                }
            }).catch(function () {
                // Fichier illisible comme image (ex. format non supporte par le navigateur) :
                // on laisse le fichier original tel quel, le serveur refusera avec un
                // message explicite s'il ne respecte pas le format attendu.
            });
        });
    }

    // ---- Apercu Markdown (indicatif, conversion simple) ----
    var champContenu = document.getElementById('contenu');
    var apercuMarkdown = document.getElementById('apercuMarkdown');

    function echapperHtml(texte) {
        var div = document.createElement('div');
        div.textContent = texte;
        return div.innerHTML;
    }

    function convertirMarkdownSimple(markdown) {
        var lignes = (markdown || '').split('\n');
        var html = '';
        var dansListe = false;

        lignes.forEach(function (ligne) {
            var ligneEchappee = echapperHtml(ligne)
                .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
                .replace(/\*(.+?)\*/g, '<em>$1</em>')
                .replace(/\[(.+?)\]\((.+?)\)/g, '<a href="$2" target="_blank" rel="noopener">$1</a>');

            var estItemListe = /^[-*]\s+/.test(ligne);

            if (estItemListe && !dansListe) {
                html += '<ul>';
                dansListe = true;
            } else if (!estItemListe && dansListe) {
                html += '</ul>';
                dansListe = false;
            }

            if (estItemListe) {
                html += '<li>' + ligneEchappee.replace(/^[-*]\s+/, '') + '</li>';
            } else if (/^###\s+/.test(ligne)) {
                html += '<h3>' + ligneEchappee.replace(/^###\s+/, '') + '</h3>';
            } else if (/^##\s+/.test(ligne)) {
                html += '<h2>' + ligneEchappee.replace(/^##\s+/, '') + '</h2>';
            } else if (/^#\s+/.test(ligne)) {
                html += '<h1>' + ligneEchappee.replace(/^#\s+/, '') + '</h1>';
            } else if (ligne.trim().length === 0) {
                html += '';
            } else {
                html += '<p>' + ligneEchappee + '</p>';
            }
        });

        if (dansListe) {
            html += '</ul>';
        }

        return html;
    }

    if (champContenu && apercuMarkdown) {
        var majApercu = function () { apercuMarkdown.innerHTML = convertirMarkdownSimple(champContenu.value); };
        champContenu.addEventListener('input', majApercu);
        majApercu();
    }
})();
