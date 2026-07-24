(function () {
    'use strict';

    var codePostalInput = document.getElementById('organisationCodePostal');
    var villeInput = document.getElementById('organisationVille');
    var villeSuggestions = document.getElementById('organisationVillesSuggestions');
    var villeWarning = document.getElementById('organisationVilleWarning');

    if (!codePostalInput || !villeInput || !villeSuggestions || !villeWarning) {
        return;
    }

    var config = window.OrganisationFormConfig || {};
    var geoCommunesApiUrl = config.geoCommunesApiUrl || 'https://geo.api.gouv.fr/communes';

    var communesConnues = [];

    function normalise(valeur) {
        return (valeur || '').trim().toLocaleUpperCase('fr-FR');
    }

    function majSuggestions(noms) {
        villeSuggestions.innerHTML = '';
        noms.forEach(function (nom) {
            var option = document.createElement('option');
            option.value = nom;
            villeSuggestions.appendChild(option);
        });
    }

    function verifieCoherence() {
        if (communesConnues.length === 0) {
            villeWarning.classList.add('d-none');
            return;
        }

        var villeSaisie = normalise(villeInput.value);
        var correspond = villeSaisie === '' || communesConnues.some(function (nom) {
            return normalise(nom) === villeSaisie;
        });

        villeWarning.classList.toggle('d-none', correspond);
    }

    function chargeCommunes(codePostal) {
        fetch(geoCommunesApiUrl + '?codePostal=' + encodeURIComponent(codePostal) + '&fields=nom&format=json')
            .then(function (reponse) {
                return reponse.ok ? reponse.json() : [];
            })
            .then(function (communes) {
                communesConnues = (communes || []).map(function (commune) {
                    return commune.nom;
                });

                majSuggestions(communesConnues);

                if (communesConnues.length === 1 && villeInput.value.trim() === '') {
                    villeInput.value = communesConnues[0];
                }

                verifieCoherence();
            })
            .catch(function () {
                // API indisponible : on n'empeche pas la saisie, on desactive juste le controle.
                communesConnues = [];
                villeWarning.classList.add('d-none');
            });
    }

    codePostalInput.addEventListener('blur', function () {
        var codePostal = codePostalInput.value.trim();
        if (/^\d{5}$/.test(codePostal)) {
            chargeCommunes(codePostal);
        } else {
            communesConnues = [];
            majSuggestions([]);
            villeWarning.classList.add('d-none');
        }
    });

    villeInput.addEventListener('blur', verifieCoherence);
})();
