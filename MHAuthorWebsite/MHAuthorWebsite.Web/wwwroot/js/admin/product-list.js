"use strict";

import { showPopupAsync } from "../notification.js";

document.addEventListener(`DOMContentLoaded`, function () {
  // --- Delete product with confirmation ---
  document.querySelectorAll(`[data-action="delete-product"]`).forEach(b => {
    b.addEventListener(`click`, async function () {
      const form = b.closest(`form`);

      await showPopupAsync({
        icon: `warning`,
        title: `Изтриване на продукт`,
        text: `Сигурни ли сте, че искате да изтриете този продукт? Действието е необратимо!`,
        showCancelButton: true,
        confirmButtonColor: `rgb(39, 103, 231)`,
        cancelButtonColor: `rgb(255, 73, 73)`,
        onConfirm: () => form.submit(),
      });
    });
  });

  // --- End discount with confirmation ---
  document.querySelectorAll(`[data-action="end-discount"]`).forEach(b => {
    b.addEventListener(`click`, async function () {
      const form = b.closest(`form`);

      await showPopupAsync({
        icon: `warning`,
        title: `Премахване на промоция`,
        text: `Сигурни ли сте, че искате да премахнете промоцията за този продукт?`,
        showCancelButton: true,
        confirmButtonColor: `rgb(39, 103, 231)`,
        cancelButtonColor: `rgb(255, 73, 73)`,
        onConfirm: () => form.submit(),
      });
    });
  });
});
