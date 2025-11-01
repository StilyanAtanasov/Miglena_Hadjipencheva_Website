"use strict";

window.addEventListener(`DOMContentLoaded`, function () {
  document.querySelectorAll(`.popup-btn-box`).forEach(p => {
    const popupName = p.id;
    const text = p.querySelector(`.popup-btn-text`);

    localStorage.getItem(`popup_${popupName}_closed`) != `true` && text.classList.remove(`invisible`);

    p.querySelector(`.popup-btn`).addEventListener(`click`, function () {
      localStorage.setItem(`popup_${popupName}_closed`, text.classList.toggle(`invisible`));
    });
  });
});
