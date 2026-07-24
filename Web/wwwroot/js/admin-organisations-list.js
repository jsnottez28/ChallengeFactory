(function () {
    'use strict';

    var filterInput = document.getElementById('organisationFilter');
    var tableBody = document.getElementById('organisationsTableBody');

    if (!filterInput || !tableBody) {
        return;
    }

    var rows = Array.prototype.slice.call(tableBody.querySelectorAll('tr[data-searchable]'));
    var noResultsRow = document.getElementById('organisationsNoResults');

    function applyFilter() {
        var terme = filterInput.value.trim().toLocaleLowerCase('fr-FR');
        var nombreVisibles = 0;

        rows.forEach(function (row) {
            var correspond = terme === '' || row.textContent.toLocaleLowerCase('fr-FR').indexOf(terme) !== -1;
            row.classList.toggle('d-none', !correspond);
            if (correspond) {
                nombreVisibles += 1;
            }
        });

        if (noResultsRow) {
            noResultsRow.classList.toggle('d-none', nombreVisibles !== 0);
        }
    }

    filterInput.addEventListener('input', applyFilter);
})();
