(function () {
    'use strict';

    var groupeCheckboxes = Array.prototype.slice.call(document.querySelectorAll('.js-groupe-checkbox'));
    var droitCheckboxes = Array.prototype.slice.call(document.querySelectorAll('.js-droit-checkbox'));
    var ressourceCheckboxes = Array.prototype.slice.call(document.querySelectorAll('.js-ressource-checkbox'));

    if (droitCheckboxes.length === 0) {
        return;
    }

    function droitsDeLaRessource(ressourceId) {
        return droitCheckboxes.filter(function (checkbox) {
            return checkbox.dataset.ressourceId === ressourceId;
        });
    }

    function majCaseRessource(ressourceCheckbox) {
        var droits = droitsDeLaRessource(ressourceCheckbox.dataset.ressourceId);
        var coches = droits.filter(function (checkbox) { return checkbox.checked; });

        ressourceCheckbox.checked = droits.length > 0 && coches.length === droits.length;
        ressourceCheckbox.indeterminate = coches.length > 0 && coches.length < droits.length;
    }

    function majToutesLesCasesRessource() {
        ressourceCheckboxes.forEach(majCaseRessource);
    }

    function appliquerEtatGroupes() {
        var groupesCoches = groupeCheckboxes
            .filter(function (checkbox) { return checkbox.checked; })
            .map(function (checkbox) { return checkbox.dataset.groupeId; });

        droitCheckboxes.forEach(function (checkbox) {
            var groupeId = checkbox.dataset.groupeId;
            var incluViaGroupe = groupeId !== '' && groupesCoches.indexOf(groupeId) !== -1;

            if (incluViaGroupe) {
                checkbox.checked = true;
                checkbox.disabled = true;
            } else {
                checkbox.disabled = false;
                checkbox.checked = checkbox.dataset.directChecked === 'true';
            }
        });

        majToutesLesCasesRessource();
    }

    groupeCheckboxes.forEach(function (checkbox) {
        checkbox.addEventListener('change', appliquerEtatGroupes);
    });

    droitCheckboxes.forEach(function (checkbox) {
        checkbox.addEventListener('change', function () {
            checkbox.dataset.directChecked = checkbox.checked ? 'true' : 'false';
            majToutesLesCasesRessource();
        });
    });

    ressourceCheckboxes.forEach(function (ressourceCheckbox) {
        ressourceCheckbox.addEventListener('change', function () {
            var droits = droitsDeLaRessource(ressourceCheckbox.dataset.ressourceId);

            droits.forEach(function (checkbox) {
                if (checkbox.disabled) {
                    return; // inclus via un groupe coche, non modifiable individuellement ici
                }

                checkbox.checked = ressourceCheckbox.checked;
                checkbox.dataset.directChecked = ressourceCheckbox.checked ? 'true' : 'false';
            });

            majCaseRessource(ressourceCheckbox);
        });
    });

    appliquerEtatGroupes();
})();
