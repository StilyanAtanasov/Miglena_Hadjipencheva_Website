document.addEventListener(`DOMContentLoaded`, async function () {
  document.querySelectorAll(`.stars`).forEach(s => (s.querySelector(`.stars-row.stars-filled`).style.width = `${+s.dataset.percent + 1.4}%`)); // 1.4 - Fine tuning the output
});
