"use strict";

import { initQuill } from "./editor.js";

document.addEventListener("DOMContentLoaded", async function (e) {
  const categorySelect = document.getElementById("selectProductType");
  const quill = await initQuill(true, true);

  RetrieveAttributes();
  categorySelect.addEventListener("change", RetrieveAttributes);

  document.querySelector("#addProductForm").addEventListener("submit", function (e) {
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

function RetrieveAttributes() {
  const attributesContainer = document.getElementById("productTypeAttributesContainer");
  const selectElement = document.getElementById(`selectProductType`);

  const selectedId = selectElement.value;

  if (!selectedId) {
    attributesContainer.innerHTML = "";
    return;
  }

  fetch(`/AdminProduct/GetCategoryTypeAttributes/${selectedId}`, {
    headers: {
      "X-Requested-With": "XMLHttpRequest",
    },
  })
    .then(response => {
      if (!response.ok) throw new Error("Грешка при зареждане на атрибутите.");
      return response.text();
    })
    .then(html => {
      attributesContainer.innerHTML = html;

      const $form = $("#addProductForm");
      $form.unbind();
      $form.removeData("validator");
      $form.removeData("unobtrusiveValidation");
      $.validator.unobtrusive.parse($form);
    })
    .catch(error => {
      console.error("Error:", error);
    });
}
