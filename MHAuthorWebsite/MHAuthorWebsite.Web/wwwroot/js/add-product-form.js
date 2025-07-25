"use strict";

import { initQuill } from "./editor.js";

document.addEventListener(`DOMContentLoaded`, async function (e) {
  const categorySelect = document.getElementById(`selectProductType`);
  const quill = await initQuill(true, true);

  RetrieveAttributes();
  categorySelect.addEventListener(`change`, RetrieveAttributes);

  document.querySelector(`#addProductForm`).addEventListener(`submit`, function (e) {
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
  const attributesContainer = document.getElementById(`productTypeAttributesContainer`);
  const selectElement = document.getElementById(`selectProductType`);

  const selectedId = selectElement.value;

  if (!selectedId) {
    attributesContainer.innerHTML = ``;
    return;
  }

  fetch(`/AdminProduct/GetCategoryTypeAttributes/${selectedId}`, {
    headers: {
      "X-Requested-With": `XMLHttpRequest`,
    },
  })
    .then(response => {
      if (!response.ok) throw new Error(`Грешка при зареждане на атрибутите.`);
      return response.text();
    })
    .then(html => {
      attributesContainer.innerHTML = html;

      const $form = $(`#addProductForm`);
      $form.unbind();
      $form.removeData(`validator`);
      $form.removeData(`unobtrusiveValidation`);
      $.validator.unobtrusive.parse($form);
    })
    .catch(error => {
      console.error(`Error:`, error);
    });
}

// --- Image preview ---
const imageInput = document.getElementById(`imageInput`);
const previewContainer = document.getElementById(`previewContainer`);

let selectedFiles = [];

imageInput.addEventListener(`change`, function () {
  const files = Array.from(this.files);

  files.forEach(file => {
    if (!file.type.startsWith(`image/`)) return;

    const reader = new FileReader();
    reader.onload = function (e) {
      const imgWrapper = document.createElement(`div`);
      imgWrapper.classList.add(`image-container`);

      const img = document.createElement(`img`);
      img.src = e.target.result;

      const removeBtn = document.createElement(`button`);
      removeBtn.type = `button`;
      removeBtn.classList = `removeBtn`;
      removeBtn.innerHTML = `<i class="fa-solid fa-file-slash"></i>`;
      removeBtn.addEventListener(`click`, () => {
        const index = selectedFiles.indexOf(file);
        if (index > -1) selectedFiles.splice(index, 1);
        imgWrapper.remove();
        updateFileInput();
      });

      imgWrapper.appendChild(img);
      imgWrapper.appendChild(removeBtn);
      previewContainer.appendChild(imgWrapper);
    };

    selectedFiles.push(file);
    reader.readAsDataURL(file);
  });

  this.value = ``; // Allow re-selecting same files
  updateFileInput();
});

function updateFileInput() {
  const dataTransfer = new DataTransfer();
  selectedFiles.forEach(f => dataTransfer.items.add(f));
  imageInput.files = dataTransfer.files;
}
