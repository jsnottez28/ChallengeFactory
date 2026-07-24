const initAppToggler = () => {
	
	const appTogglers = document.querySelectorAll(".app-toggler");
	const appMenubars = document.getElementById("appMenubar");

	appTogglers.forEach(toggler => {
		toggler.addEventListener("click", () => {
			toggler.classList.toggle("active");

			if (window.innerWidth >= 1480) {
				const currentValue = document.documentElement.getAttribute("data-app-sidebar");
				document.documentElement.setAttribute(
					"data-app-sidebar",
					currentValue === "full" ? "mini" : "full"
				);
			} else {
				if (appMenubars) {
					appMenubars.classList.toggle("open");
				}
			}
		});
	});

	if (appMenubars) {
		appMenubars.addEventListener("mouseenter", () => {
			if (document.documentElement.getAttribute("data-app-sidebar") === "mini") {
				document.documentElement.setAttribute("data-app-sidebar", "mini-hover");
			}
		});

		appMenubars.addEventListener("mouseleave", () => {
			if (document.documentElement.getAttribute("data-app-sidebar") === "mini-hover") {
				document.documentElement.setAttribute("data-app-sidebar", "mini");
			}
		});
	}
};


const passwordToggle = () => {
	document.querySelectorAll('.toggle-password').forEach(btn => {
		btn.addEventListener('click', () => {
			const input = btn.previousElementSibling;
			const isPassword = input.type === 'password';
			input.type = isPassword ? 'text' : 'password';
			btn.classList.toggle('active', isPassword);
		});
	});
}


const saerchList = () => {
	let listItems = [];

	// JSON load
	$.getJSON("assets/ajax/search.json", function(data) {
		listItems = data.listItems;
	});

	// Search functionality
	$("#searchInput").on("keyup", function() {
		let query = $(this).val().toLowerCase();
		let searchContainer = $("#searchContainer");
		searchContainer.empty();
		searchContainer.hide();
		
		$('#recentlyResults').hide();
		
		if (query.length === 0) {
			searchContainer.hide();
			$('#recentlyResults').show();
			return;
		}

		let matched = listItems.filter(item =>
			item.name.toLowerCase().includes(query) ||
			item.url.toLowerCase().includes(query)
		);

		if (matched.length > 0) {
			let grouped = {};
			matched.forEach(item => {
				if (!grouped[item.category]) grouped[item.category] = [];
				grouped[item.category].push(item);
			});

			for (let cat in grouped) {
				searchContainer.append(
					`<span class="text-uppercase text-2xs fw-semibold text-muted d-block mb-2">${cat}</span>`
				);
				let ul = $("<ul class='list-inline search-list'></ul>");
				grouped[cat].forEach(item => {
					ul.append(
						`<li>
							<a class="search-item" href="${item.url}">
								<i class="${item.icon}"></i> <span>${item.name}</span>
							</a>
						</li>`
					);
				});
				searchContainer.append(ul);
			}
			searchContainer.show();
		} else {
			searchContainer.append(`
				<div class="text-center pb-5 pt-4">
					<div class="avatar avatar-lg bg-danger-subtle shadow-secondary rounded-circle text-danger mb-3 m-auto">
						<i class="fi fi-rr-assessment"></i>
					</div>
					<h5 class="mb-1">No result found</h5>
					<div class="text-muted">Please try again with a different query</div>
				</div>
			`);
			searchContainer.show();
		}
	});
};


const currentYear = () => {
    const elements = document.querySelectorAll('.currentYear');
    const currentYear = new Date().getFullYear();

    elements.forEach(element => {
        element.textContent = currentYear;
    });
};


const setElementHeight = () => {
    const footer = document.querySelector('.footer-wrapper');
	if (footer) {
		const footerHeight = footer ? footer.offsetHeight : 0;
		document.documentElement.style.setProperty('--footer-height', `${footerHeight}px`);
	}
	
	const chatBox = document.querySelector('.chat-wrapper');
	if (chatBox) {
		const chatHeight = chatBox.offsetHeight;
		document.documentElement.style.setProperty('--chat-height', `${chatHeight}px`);
	}
	
};


const initSelectPicker = () => {
	
	document.querySelectorAll('.select-status').forEach(dropdown => {
		const toggleButton = dropdown.querySelector('.dropdown-toggle');
		const items = dropdown.querySelectorAll('.dropdown-item');

		const updateButtonClassAndText = (text, selectedClass) => {
			// Remove btn-* except btn-sm, btn-lg
			toggleButton.classList.forEach(cls => {
				if (/^btn-/.test(cls) && !['btn-sm', 'btn-lg'].includes(cls)) {
					toggleButton.classList.remove(cls);
				}
			});

			if (selectedClass) {
				toggleButton.classList.add(...selectedClass.split(' '));
			}

			toggleButton.textContent = text;
		};

		// Handle default selection on page load
		const selectedItem = dropdown.querySelector('.dropdown-item[data-selected="true"]');
		if (selectedItem) {
			const defaultText = selectedItem.textContent.trim();
			const defaultClass = selectedItem.getAttribute('data-class');
			updateButtonClassAndText(defaultText, defaultClass);
		}

		// Handle selection on click
		items.forEach(item => {
			item.addEventListener('click', (e) => {
				e.preventDefault();
				items.forEach(i => i.removeAttribute('data-selected'));
				item.setAttribute('data-selected', 'true');

				const selectedText = item.textContent.trim();
				const selectedClass = item.getAttribute('data-class');
				updateButtonClassAndText(selectedText, selectedClass);
			});
		});
	});
};


function initSectionCheckboxSync() {
    document.querySelectorAll('.data-row-checkbox').forEach(function(section) {
        const masterCheckbox = section.querySelector('[data-row-checkbox]');
        const checkboxes = section.querySelectorAll('[data-checkbox]');

        if (!masterCheckbox || checkboxes.length === 0) return;

        masterCheckbox.addEventListener('change', function() {
            const checked = this.checked;
            checkboxes.forEach(function(cb) {
                cb.checked = checked;
            });
        });

        checkboxes.forEach(function(cb) {
            cb.addEventListener('change', function() {
                const allChecked = Array.from(checkboxes).every(c => c.checked);
                masterCheckbox.checked = allChecked;
            });
        });
    });
}


function initTooltips() {
	const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');

	tooltipTriggerList.forEach(el => {

		const tooltip = new bootstrap.Tooltip(el);

		// click par tooltip hide + focus remove
		el.addEventListener('click', () => {
			tooltip.hide();
			el.blur();
		});

	});
}


function initPopover() {
	var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'))
	var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
		return new bootstrap.Popover(popoverTriggerEl)
	})
}


function initSidebarMenu() {

	/* ----------------------------
	   Default close
	---------------------------- */
	jQuery('.app-navbar .menubar > li.menu-arrow > a').next('.menu-inner').slideUp();
	jQuery('.app-navbar .menu-inner > li > a').next('.menu-inner').slideUp();

	/* ----------------------------
	   Click behavior
	---------------------------- */
	jQuery('.app-navbar .menubar > li.menu-arrow > a, .app-navbar .menu-inner > li > a')
		.off('click')
		.on('click', function (e) {

			if (jQuery(this).hasClass('open')) {
				jQuery(this).removeClass('open');
				jQuery(this).parent('li').children('.menu-inner').slideUp();
			} else {

				if (!e.ctrlKey) {
					jQuery(this).addClass('open');
				}

				if (jQuery(this).parent('li').children('.menu-inner').length > 0) {
					e.preventDefault();

					jQuery(this).next('.menu-inner').slideDown();
					jQuery(this).parent('li')
						.siblings('li')
						.find('a:first').removeClass('open')
						.end()
						.children('.menu-inner').slideUp();
				}
			}
		});

	/* ===================================================
	   ACTIVE PAGE + TAB-PANE SYNC
	=================================================== */

	var currentPath = window.location.pathname.replace(/^\/+/, '');
	if (!currentPath) currentPath = 'index.html';

	// reset
	jQuery('.app-navbar a').removeClass('active open');
	jQuery('.app-tab-content .tab-pane').removeClass('active show');
	jQuery('.app-menubar-tabs .menu-link').removeClass('active');

	var $activeLink = jQuery('.app-navbar a').filter(function () {

		var href = jQuery(this).attr('href');
		if (!href || href === '#' || href.startsWith('http')) return false;

		// normalize
		href = href.replace(/^\/+/, '');

		return (
			href === currentPath ||
			currentPath.endsWith(href)
		);

	}).first();

	if ($activeLink.length) {

		$activeLink.addClass('active');

		// open parent menus
		$activeLink
			.parents('.menu-inner').show()
			.prev('a').addClass('active open');

		$activeLink.parent('li').addClass('active');

		// activate tab-pane
		var $tabPane = $activeLink.closest('.tab-pane');
		$tabPane.addClass('active show');

		// activate top icon tab
		var tabId = $tabPane.attr('id');
		jQuery('.app-menubar-tabs .menu-link[href="#' + tabId + '"]').addClass('active');
	}
}


function initCheckable() {
    document.querySelectorAll('.checkable-wrapper').forEach(function(wrapper) {
        const checkAll = wrapper.querySelector('.checkable-check-all');
        const checkboxes = wrapper.querySelectorAll('.checkable-check-input');

        // Initialize checked state on load
        checkboxes.forEach(function(checkbox) {
            const item = checkbox.closest('.checkable-item');
            if (checkbox.checked && item) {
                item.classList.add('is-checked');
            }
        });

        // Handle "Select All"
        if (checkAll) {
            checkAll.addEventListener('change', function () {
                const isChecked = this.checked;
                checkboxes.forEach(function(checkbox) {
                    checkbox.checked = isChecked;
                    const item = checkbox.closest('.checkable-item');
                    if (item) {
                        item.classList.toggle('is-checked', isChecked);
                    }
                });
            });
        }

        // Handle individual checkbox toggle
        wrapper.addEventListener('change', function (e) {
            if (e.target.matches('.checkable-check-input')) {
                const item = e.target.closest('.checkable-item');
                if (item) {
                    item.classList.toggle('is-checked', e.target.checked);
                }

                // Update "Select All" state
                const allChecked = wrapper.querySelectorAll('.checkable-check-input:not(:checked)').length === 0;
                if (checkAll) {
                    checkAll.checked = allChecked;
                }
            }
        });
    });
}


function initEmailSidebarToggle() {
    const toggler = document.querySelector('.mail-sidebar-toggler');
    const sidebar = document.querySelector('.mail-sidebar');
    const overlay = document.querySelector('.sidebar-mobile-overlay');

    if (toggler && sidebar && overlay) {
        toggler.addEventListener('click', () => {
            sidebar.classList.toggle('open');
            overlay.classList.toggle('show', sidebar.classList.contains('open'));
        });

        overlay.addEventListener('click', () => {
            sidebar.classList.remove('open');
            overlay.classList.remove('show');
        });
    }
}


function initChatSidebarToggle() {
    const toggler = document.querySelector('.chat-sidebar-toggler');
    const sidebar = document.querySelector('.chat-sidebar');
    const overlay = document.querySelector('.sidebar-mobile-overlay');
    const btnClose = document.querySelector('.btn-close');

    if (toggler && sidebar && overlay) {
        toggler.addEventListener('click', () => {
            sidebar.classList.toggle('open');
            overlay.classList.toggle('show', sidebar.classList.contains('open'));
        });

        overlay.addEventListener('click', () => {
            sidebar.classList.remove('open');
            overlay.classList.remove('show');
        });
		
		btnClose.addEventListener('click', () => {
            sidebar.classList.remove('open');
            overlay.classList.remove('show');
        });
    }
}


function initBookmarks() {
    document.addEventListener('click', (e) => {
        const bookmark = e.target.closest('.mail-item-bookmark');
        if (bookmark) {
            bookmark.classList.toggle('active');
        }
    });
}


const ThemeSwitcher = () => {
	'use strict';

	// Cookie helpers
	const getCookie = (name) => {
	  const match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
	  return match ? match[2] : null;
	};

	const setCookie = (name, value, days = 365) => {
	  const expires = new Date(Date.now() + days * 864e5).toUTCString();
	  document.cookie = `${name}=${value}; expires=${expires}; path=/`;
	};

	const getStoredTheme = () => getCookie('theme');
	const setStoredTheme = (theme) => setCookie('theme', theme);

	// Preferred theme
	const getPreferredTheme = () => {
	  const storedTheme = getStoredTheme();
	  if (storedTheme) return storedTheme;
	  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
	};

	// Apply theme
	const setTheme = (theme) => {
	  document.documentElement.setAttribute('data-bs-theme', theme);
	};

	// Page Ready
	$(document).ready(function () {
	  // ðŸ”¹ On load: apply saved/preferred theme
	  const preferredTheme = getPreferredTheme();
	  setTheme(preferredTheme);

	  // ðŸ”¹ Restore active state on button
	  if (preferredTheme === 'dark') {
		$('.theme-btn').addClass('active');
	  } else {
		$('.theme-btn').removeClass('active');
	  }

	  // ðŸ”¹ Click handler
	  $('.theme-btn').on('click', function () {
		$(this).toggleClass('active');

		let currentTheme = document.documentElement.getAttribute('data-bs-theme');
		let newTheme = currentTheme === 'dark' ? 'light' : 'dark';

		setTheme(newTheme);
		setStoredTheme(newTheme);
	  });
	});
};


function initSidebarPanel() {
	document.addEventListener('click', function(e) {
		const toggler = e.target.closest('.sidebar-panel-toggler');
		if (toggler) {
			const panel = document.querySelector('.app-sidebar-panel');
			if (panel) {
				panel.classList.toggle('show');
			}
		}

		const closeBtn = e.target.closest('.sidebar-close');
		if (closeBtn) {
			document.querySelectorAll('.app-sidebar-panel').forEach(panel => {
				panel.classList.remove('show');
			});
		}
	});
}


function initPriceSwitch() {
	const priceSwitch = document.querySelector("#priceSwitchCheck");

	if (priceSwitch) {
		priceSwitch.addEventListener("change", function () {
			const isYearly = this.checked;
			const monthlyPrices = document.querySelectorAll(".price-monthly");
			const yearlyPrices = document.querySelectorAll(".price-yearly");

			monthlyPrices.forEach(price => price.classList.toggle("d-none", isYearly));
			yearlyPrices.forEach(price => price.classList.toggle("d-none", !isYearly));
		});	
	}
}


document.addEventListener("DOMContentLoaded", () => {
    Waves.init();
	initAppToggler();
	passwordToggle();
	saerchList();
	setElementHeight();
	currentYear();
	initSectionCheckboxSync();
	initSelectPicker();
	initTooltips();
	initPopover();
	initCheckable();
	initSidebarMenu();
	initEmailSidebarToggle();
	initChatSidebarToggle();
	initBookmarks();
	ThemeSwitcher();
	initSidebarPanel();
	initPriceSwitch();
});


$(document).ready(function () {
	$('.dataTable').each(function() {
		const dtInstance = $(this).DataTable();
		dtInstance.on('draw.dt', function() {
			initSelectPicker();
			initSectionCheckboxSync();
		});
	});
	if (jQuery('.flatpickr-date').length > 0) {
		$(".flatpickr-date").flatpickr({
			enableTime: false,
			dateFormat: "Y-m-d H:i",
		});
	}
});