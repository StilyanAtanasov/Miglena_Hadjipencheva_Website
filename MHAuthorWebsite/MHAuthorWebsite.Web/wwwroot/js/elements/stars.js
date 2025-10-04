"use strict";

document.addEventListener(`DOMContentLoaded`, async function () {
  document.querySelectorAll(`.stars`).forEach(s => (s.querySelector(`.stars-row.stars-filled`).style.width = `${+s.dataset.percent + 2}%`)); // 2 - Fine tuning the output
});
