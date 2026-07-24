/**
 * ==================================================
 *  CONFIGURATION GLOBALE DATATABLES
 * ==================================================
 */
const DT_LANGUAGE = {
	search: "",
	searchPlaceholder: "Search",
	paginate: {
		previous: "<i class='fi fi-rr-angle-left'></i>",
		next: "<i class='fi fi-rr-angle-right'></i>",
		first: "<i class='fi fi-rr-angle-double-left'></i>",
		last: "<i class='fi fi-rr-angle-double-right'></i>"
	}
};

/**
 * ==================================================
 *  INIT GENERIQUE DATATABLE
 * ==================================================
 */
function initDataTable(selector, options = {}) {
	if (!$(selector).length) {
		return null;
	}

	const table = $(selector).DataTable({
		language: DT_LANGUAGE,

		// options spécifiques table
		...options,

		initComplete: function () {
			const wrapper = `${selector}_wrapper`;

			// 🔒 Anti-duplication DOM (search / pagination)
			if (!$(wrapper).data("initialized")) {
				$(wrapper)
					.data("initialized", true)
					.find("> .row.mt-2.justify-content-between")
					.first()
					.addClass("mx-2 py-2");
			}

			// callback custom éventuel
			if (typeof options.onInitComplete === "function") {
				options.onInitComplete.call(this);
			}
		}
	});

	return table;
}

/**
 * ==================================================
 *  SAFE INIT
 *  - pagination front uniquement
 *  - aucune troncature backend
 *  - ordre Django respecté
 * ==================================================
 */
function safeInitDataTable(selector, options = {}) {

	const defaultOptions = {
		paging: true,           // pagination active
		pageLength: 25,         // lignes par page
		lengthMenu: [25, 50, 100, 250],
		order: [],              // respecte l’ordre Django
		deferRender: true       // perfs si beaucoup de lignes
	};

	options = { ...defaultOptions, ...options };

	// 🔁 Anti double initialisation
	if ($.fn.DataTable.isDataTable(selector)) {
		return $(selector).DataTable();
	}

	return initDataTable(selector, options);
}

/**
 * ==================================================
 *  AUTO INIT (OPTIONNEL)
 *  <table id="data_td" data-datatable="true">
 * ==================================================
 */
document.addEventListener("DOMContentLoaded", function () {
	$("[data-datatable='true']").each(function () {
		if (this.id) {
			safeInitDataTable(`#${this.id}`);
		}
	});
});
