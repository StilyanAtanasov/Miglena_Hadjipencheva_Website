"use strict";

import { initQuill } from "./editor.js";

document.addEventListener(`DOMContentLoaded`, async function () {
  const quill = await initQuill(true);

  document
    .querySelector("#updateProductForm")
    .addEventListener("submit", function (e) {
      const plainText = quill.getText().trim();
      const descriptionInput = document.querySelector(`#descriptionInput`);
      const maxLength = parseInt(descriptionInput.dataset.textMaxLength);

      if (plainText.length > maxLength) {
        e.preventDefault();
        return;
      }

      const delta = quill.getContents();
      descriptionInput.value = JSON.stringify(delta);
    });
});
