"use strict";

import { showPopupAsync } from "../notification.js";

document.addEventListener(`DOMContentLoaded`, async function () {
  const input = document.getElementById(`show-legal-prompt`);
  if (!input || input.value !== `1`) return;

  
  setTimeout(() => {
    document.querySelector(`.swal2-popup.swal2-modal`).style.scale = 1.7;
  }, 300);

  await showPopupAsync({
    icon: `warning`,
    title: `Необходими съгласия`,
    text: `Има нова версия на правните документи. За да продължите, трябва да я приемете.`,
    confirmButtonColor: `rgb(58, 5, 58)`,
  });
});
