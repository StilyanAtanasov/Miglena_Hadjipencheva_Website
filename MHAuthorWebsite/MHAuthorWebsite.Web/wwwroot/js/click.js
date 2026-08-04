"use strict";

document.addEventListener(`DOMContentLoaded`, function () {
  document.addEventListener(`click`, function (e) {
    const clickableElement = e.target.closest(`.clickable`);
    if (!clickableElement) return;
    if (e.target.closest(`a, button, input, textarea, select, label`)) return;

    let location = clickableElement.getAttribute(`data-details-url`);
    if (location == null) location = clickableElement.getAttribute(`data-url`);
    if (!location) return;

    window.location = location;
  });
});
