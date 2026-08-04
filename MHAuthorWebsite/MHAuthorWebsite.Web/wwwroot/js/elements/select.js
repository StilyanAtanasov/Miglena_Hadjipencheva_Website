"use strict";

export function initTomSelect(select) {
  if (select.tomselect) return;

  new TomSelect(select, {
    create: false,
    plugins: ["dropdown_input"],
    allowEmptyOption: true,
  });
}

document.querySelectorAll(`.select-main`).forEach(select => {
  initTomSelect(select);
});
