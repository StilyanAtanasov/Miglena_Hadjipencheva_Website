window.addEventListener("DOMContentLoaded", function () {
  const filter = this.document.getElementById(`filter`);
  const filterForm = filter.closest(`form`);
  filter.addEventListener(`change`, function () {
    window.location.href = `${filterForm.action}?${this.dataset.paramName}=${encodeURIComponent(this.value)}`;
  });
});
