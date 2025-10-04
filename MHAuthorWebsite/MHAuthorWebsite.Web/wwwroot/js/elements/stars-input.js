"use strict";

document.addEventListener(`DOMContentLoaded`, () => {
  document.querySelectorAll(`.stars`).forEach(stars => {
    const input = stars.querySelector(".rating-input");
    const icons = stars.querySelectorAll(`.star-icon`);

    icons.forEach((icon, i) => {
      icon.addEventListener(`mouseover`, () => {
        icons.forEach((ic, j) => ic.classList.toggle(`fa-solid`, j <= i));
      });

      icon.addEventListener(`click`, () => {
        input.value = icon.dataset.index;
        icons.forEach((ic, j) => ic.classList.toggle(`fa-solid`, j <= i));
      });

      icon.addEventListener(`mouseout`, () => {
        const val = parseInt(input.value) || 0;
        icons.forEach((ic, j) => ic.classList.toggle(`fa-solid`, j < val));
      });
    });
  });
});
