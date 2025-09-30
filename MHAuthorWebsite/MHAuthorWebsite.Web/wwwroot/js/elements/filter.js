"use strict";

window.addEventListener("DOMContentLoaded", function () {
  const filters = this.document.querySelectorAll(`.filter`);
  filters.forEach(f => {
    const filterForm = f.closest(`form`);
    f.addEventListener(`change`, function () {
      window.location.href = `${filterForm.action}?${this.dataset.paramName}=${encodeURIComponent(this.value)}`;
    });
  });
});
