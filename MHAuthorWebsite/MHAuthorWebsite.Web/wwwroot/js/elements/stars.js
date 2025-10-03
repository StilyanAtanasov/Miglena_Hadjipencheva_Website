document.addEventListener(`DOMContentLoaded`, async function () {
  document.querySelectorAll(`.stars`).forEach(s => (s.querySelector(`.stars-row.stars-filled`).style.width = `${s.dataset.percent}%`));
});
