// Composant carte flip (Carte de Competences) : clic/Entree/Espace fait pivoter la carte
// pour revéler l'autre face. Délégation d'événements pour fonctionner avec un nombre
// quelconque de cartes affichées sur une même page (liste admin, tableau de bord apprenant).
(function () {
    function basculer(carte) {
        carte.classList.toggle('is-flipped');
    }

    document.addEventListener('click', function (event) {
        if (event.target.closest('a, button')) {
            return;
        }

        var carte = event.target.closest('.carte-flip');
        if (carte) {
            basculer(carte);
        }
    });

    document.addEventListener('keydown', function (event) {
        if (event.key !== 'Enter' && event.key !== ' ') {
            return;
        }

        var carte = event.target.closest('.carte-flip');
        if (carte) {
            event.preventDefault();
            basculer(carte);
        }
    });
})();
